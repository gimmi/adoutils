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
		private DbConnection _conn;
		private DbTransaction _tr;
		private bool _nestedTransactionRollback;

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
			var conn = _factory.CreateConnection();
			conn.ConnectionString = _connStr;
			conn.Open();
			_conn = conn;
			return new Connection(CloseConnection);
		}

		private void CloseConnection()
		{
			if(_conn != null)
			{
				var conn = _conn;
				_conn = null;
				conn.Close();
			}
		}

		public virtual ITransaction BeginTransaction()
		{
			var connection = OpenConnection();
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
				var tr = _tr;
				_tr = null;
				tr.Commit();
			}
		}

		private void RollbackTransaction()
		{
			if(_tr != null)
			{
				var tr = _tr;
				_tr = null;
				tr.Rollback();
			}
		}

		public virtual T Scalar<T>(string sql, object parameters = null, int? timeout = null)
		{
			using var cmd = CreateCommand(timeout);
			cmd.DbCommand.CommandText = sql;
			AddParameters(cmd.DbCommand, ToDictionary(parameters));
			var res = cmd.DbCommand.ExecuteScalar();
			return DbFieldConversionUtils.Convert<T>(res);
		}

		public virtual ICommand CreateCommand(int? timeout = null)
		{
			var conn = OpenConnection();
			var cmd = _conn.CreateCommand();
			cmd.Transaction = _tr;
			if (timeout.HasValue)
			{
				cmd.CommandTimeout = timeout.Value;
			}
			return new Command(cmd, conn);
		}

		public virtual IEnumerable<IDataRecord> Query(string sql, object parameters = null, int? timeout = null)
		{
			using var cmd = CreateCommand(timeout);
			cmd.DbCommand.CommandText = sql;
			AddParameters(cmd.DbCommand, ToDictionary(parameters));
			using var rdr = cmd.DbCommand.ExecuteReader();
			while(rdr.Read())
			{
				yield return rdr;
			}
		}

		public virtual int Exec(string sql, object parameters = null, int? timeout = null)
		{
			using var cmd = CreateCommand(timeout);
			cmd.DbCommand.CommandText = sql;
			AddParameters(cmd.DbCommand, ToDictionary(parameters));
			return cmd.DbCommand.ExecuteNonQuery();
		}

		private static void AddParameters(DbCommand cmd, IEnumerable<KeyValuePair<string, object>> parameters)
		{
			foreach(var (key, value) in parameters)
			{
				IDataParameter par;
				if (value is IDataParameter parameter)
				{
					par = parameter;
				}
				else
				{
					par = cmd.CreateParameter();
					par.Value = value ?? DBNull.Value;
				}
				par.ParameterName = key;
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
