using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ADOUtils
{
	public class Database : IDatabase
	{
		private readonly DbProviderFactory _factory;
		private readonly Action<string> _log;
		private readonly string _connStr;
		private IDbConnection _conn;
		private IDbTransaction _tr;
		private bool _nestedTransactionRollback;

		public Database(string connStr) : this(connStr, DbProviderFactories.GetFactory("System.Data.SqlClient"), null) {}

		public Database(string connStr, DbProviderFactory factory, Action<string> log)
		{
			_factory = factory;
			_log = log;
			_connStr = connStr;
		}

		public virtual Connection OpenConnection()
		{
			if(_conn != null)
			{
				return new Connection();
			}
			if(_log != null)
			{
				_log.Invoke("Opening connection");
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
				if(_log != null)
				{
					_log.Invoke("Closing connection");
				}
				IDbConnection conn = _conn;
				_conn = null;
				conn.Close();
			}
		}

		public virtual Transaction BeginTransaction()
		{
			Connection connection = OpenConnection();
			if (_tr != null)
			{
				return new Transaction(connection, delegate { }, NotifyNestedTransactionRollback, NotifyNestedTransactionRollback);
			}
			if (_log != null)
			{
				_log.Invoke("Beginning transaction");
			}
			_tr = _conn.BeginTransaction();
			_nestedTransactionRollback = false;
			return new Transaction(connection, CommitTransaction, RollbackTransaction, RollbackTransaction);
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
					throw new DataException("Cannot commit transaction when one of the nested transaction has been rolled back");
				}
				if (_log != null)
				{
					_log.Invoke("Committing transaction");
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
				if (_log != null)
				{
					_log.Invoke("Rolling back transaction");
				}
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
			using(OpenConnection())
			{
				if (_log != null)
				{
					_log.Invoke(string.Concat("Executing scalar: ", SqlToString(sql, parameters)));
				}
				IDbCommand cmd = _conn.CreateCommand();
				cmd.Transaction = _tr;
				cmd.CommandText = sql;
				AddParameters(cmd, parameters);
				object res = cmd.ExecuteScalar();
				return DbFieldConversionUtils.Convert<T>(res);
			}
		}

		public virtual IEnumerable<IDataRecord> Read(string sql)
		{
			return Read(sql, new Dictionary<string, object>(0));
		}

		public virtual IEnumerable<IDataRecord> Read(string sql, object parameters)
		{
			return Read(sql, ToDictionary(parameters));
		}

		public virtual IEnumerable<IDataRecord> Read(string sql, IDictionary<string, object> parameters)
		{
			return Yield(sql, parameters).Select(CachedDataRecord.Build).ToList();
		}

		public virtual IEnumerable<IDataRecord> Yield(string sql, object parameters)
		{
			return Yield(sql, ToDictionary(parameters));
		}

		public virtual IEnumerable<IDataRecord> Yield(string sql)
		{
			return Yield(sql, new Dictionary<string, object>(0));
		}

		public virtual IEnumerable<IDataRecord> Yield(string sql, IDictionary<string, object> parameters)
		{
			using(OpenConnection())
			{
				if (_log != null)
				{
					_log.Invoke(string.Concat("Executing reader: ", SqlToString(sql, parameters)));
				}
				IDbCommand cmd = _conn.CreateCommand();
				cmd.Transaction = _tr;
				cmd.CommandText = sql;
				AddParameters(cmd, parameters);
				using(IDataReader rdr = cmd.ExecuteReader())
				{
					while(rdr.Read())
					{
						yield return rdr;
					}
					if (_log != null)
					{
						_log.Invoke(string.Concat("Closing reader: ", SqlToString(sql, parameters)));
					}
				}
			}
		}

		private string SqlToString(string sql, IEnumerable<KeyValuePair<string, object>> parameters)
		{
			return string.Concat("`", sql , "` { ", string.Join(", ", parameters.Select(kv => string.Concat(kv.Key, " = `", kv.Value, "`"))), " }");
		}

		public virtual int Exec(string sql)
		{
			return Exec(sql, new Dictionary<string, object>(0));
		}

		public virtual int Exec(string sql, object parameters)
		{
			return Exec(sql, ToDictionary(parameters));
		}

		public virtual int Exec(string sql, IDictionary<string, object> parameters)
		{
			using(OpenConnection())
			{
				if (_log != null)
				{
					_log.Invoke(string.Concat("Executing: ", SqlToString(sql, parameters)));
				}
				IDbCommand cmd = _conn.CreateCommand();
				cmd.Transaction = _tr;
				cmd.CommandText = sql;
				AddParameters(cmd, parameters);
				return cmd.ExecuteNonQuery();
			}
		}

		private static void AddParameters(IDbCommand cmd, IEnumerable<KeyValuePair<string, object>> parameters)
		{
			foreach(var pi in parameters)
			{
				IDbDataParameter par = cmd.CreateParameter();
				par.ParameterName = pi.Key;
				par.Value = pi.Value ?? DBNull.Value;
				cmd.Parameters.Add(par);
			}
		}

		private static IDictionary<string, object> ToDictionary(object o)
		{
			return TypeDescriptor.GetProperties(o).Cast<PropertyDescriptor>().ToDictionary(p => p.Name, p => p.GetValue(o));
		}

		public void Dispose()
		{
			RollbackTransaction();
			CloseConnection();
		}
	}
}