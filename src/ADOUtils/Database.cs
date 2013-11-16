using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace ADOUtils
{
	public class Database : IDatabase
	{
		private readonly DbProviderFactory _factory;
		private readonly string _connStr;
		private IDbConnection _conn;
		private IDbTransaction _tr;
		private bool _nestedTransactionRollback;

		public Database(string connStr) : this(connStr, DbProviderFactories.GetFactory("System.Data.SqlClient")) {}

		public Database(string connStr, DbProviderFactory factory)
		{
			_factory = factory;
			_connStr = connStr;
		}

		public virtual IConnection OpenConnection()
		{
			if(_conn != null)
			{
				return new Connection();
			}
			DbConnection conn = _factory.CreateConnection();
			conn.ConnectionString = _connStr;
			conn.Open();
			_conn = conn;
			return new Connection(CloseConnection);
		}

		private void CloseConnection()
		{
			if(_conn != null)
			{
				IDbConnection conn = _conn;
				_conn = null;
				conn.Close();
			}
		}

		public virtual ITransaction BeginTransaction()
		{
			IConnection connection = OpenConnection();
			if (_tr != null)
			{
				return new Transaction(connection, delegate { }, NotifyNestedTransactionRollback);
			}
			_tr = _conn.BeginTransaction();
			_nestedTransactionRollback = false;
			return new Transaction(connection, CommitTransaction, RollbackTransaction);
		}

		private void NotifyNestedTransactionRollback()
		{
			_nestedTransactionRollback = true;
		}

		private void CommitTransaction()
		{
			if(_tr != null)
			{
				if(_nestedTransactionRollback)
				{
					RollbackTransaction();
					throw new DataException("Cannot commit transaction when one of the nested transaction has been rolled back");
				}
				IDbTransaction tr = _tr;
				_tr = null;
				tr.Commit();
			}
		}

		private void RollbackTransaction()
		{
			if(_tr != null)
			{
				IDbTransaction tr = _tr;
				_tr = null;
				tr.Rollback();
			}
		}

		public virtual T Scalar<T>(string sql, object parameters)
		{
			return Scalar<T>(sql, ToDictionary(parameters));
		}

		public virtual T Scalar<T>(string sql)
		{
			return Scalar<T>(sql, new Dictionary<string, object>(0));
		}

		public virtual T Scalar<T>(string sql, IDictionary<string, object> parameters)
		{
			using (var cmd = CreateCommand())
			{
				cmd.DbCommand.CommandText = sql;
				AddParameters(cmd.DbCommand, parameters);
				object res = cmd.DbCommand.ExecuteScalar();
				return DbFieldConversionUtils.Convert<T>(res);
			}
		}

		public virtual ICommand CreateCommand()
		{
			IConnection conn = OpenConnection();
			IDbCommand cmd = _conn.CreateCommand();
			cmd.Transaction = _tr;
			return new Command(cmd, conn);
		}

		public virtual IEnumerable<IDataRecord> Query(string sql, object parameters = null)
		{
			return Query(sql, ToDictionary(parameters));
		}

		private IEnumerable<IDataRecord> Query(string sql, IDictionary<string, object> parameters)
		{
			using(var cmd = CreateCommand())
			{
				cmd.DbCommand.CommandText = sql;
				AddParameters(cmd.DbCommand, parameters);
				using (IDataReader rdr = cmd.DbCommand.ExecuteReader())
				{
					while(rdr.Read())
					{
						yield return rdr;
					}
				}
			}
		}

		public virtual int Exec(string sql, object parameters = null)
		{
			return Exec(sql, ToDictionary(parameters));
		}

		private int Exec(string sql, IDictionary<string, object> parameters)
		{
			using(var cmd = CreateCommand())
			{
				cmd.DbCommand.CommandText = sql;
				AddParameters(cmd.DbCommand, parameters);
				return cmd.DbCommand.ExecuteNonQuery();
			}
		}

		private static void AddParameters(IDbCommand cmd, IEnumerable<KeyValuePair<string, object>> parameters)
		{
			foreach(var pi in parameters)
			{
				IDataParameter par;
				if (pi.Value is IDataParameter)
				{
					par = pi.Value as IDataParameter;
				}
				else
				{
					par = cmd.CreateParameter();
					par.Value = pi.Value ?? DBNull.Value;
				}
				par.ParameterName = pi.Key;
				cmd.Parameters.Add(par);
			}
		}

		private static IDictionary<string, object> ToDictionary(object o)
		{
			return o as IDictionary<string, object> ?? TypeDescriptor.GetProperties(o).Cast<PropertyDescriptor>().ToDictionary(p => p.Name, p => p.GetValue(o));
		}

		public void Dispose()
		{
			RollbackTransaction();
			CloseConnection();
		}
	}
}