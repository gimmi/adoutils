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
		private readonly Action<string> _log;
		private readonly string _connStr;
		private IDbConnection _conn;
		private IDbTransaction _tr;
		private bool _nestedTransactionRollback;

		public Database(string connStr) : this(connStr, DbProviderFactories.GetFactory("System.Data.SqlClient"), null) {}

		public Database(string connStr, DbProviderFactory factory) : this(connStr, factory, null) {}

		[Obsolete("ADOUtils logging support will be removed in future versions, so dont use it anymore.")]
		public Database(string connStr, DbProviderFactory factory, Action<string> log)
		{
			_factory = factory;
			_log = log;
			_connStr = connStr;
		}

		public virtual IConnection OpenConnection()
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

		public virtual ITransaction BeginTransaction()
		{
			IConnection connection = OpenConnection();
			if (_tr != null)
			{
				return new Transaction(connection, delegate { }, NotifyNestedTransactionRollback);
			}
			if (_log != null)
			{
				_log.Invoke("Beginning transaction");
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
			using (var cmd = CreateCommand())
			{
				if (_log != null)
				{
					_log.Invoke(string.Concat("Executing scalar: ", SqlToString(sql, parameters)));
				}
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

		[Obsolete("Use Query(...).AsDisconnected().ToList() instead.")]
		public virtual IEnumerable<IDataRecord> Read(string sql)
		{
			return Query(sql, new Dictionary<string, object>(0)).AsDisconnected().ToList();
		}

		[Obsolete("Use Query(...).AsDisconnected().ToList() instead.")]
		public virtual IEnumerable<IDataRecord> Read(string sql, object parameters)
		{
			return Query(sql, ToDictionary(parameters)).AsDisconnected().ToList();
		}

		[Obsolete("Use Query(...).AsDisconnected().ToList() instead.")]
		public virtual IEnumerable<IDataRecord> Read(string sql, IDictionary<string, object> parameters)
		{
			return Query(sql, parameters).AsDisconnected().ToList();
		}

		[Obsolete("Use Query(...) method instead.")]
		public virtual IEnumerable<IDataRecord> Yield(string sql, object parameters)
		{
			return Query(sql, ToDictionary(parameters));
		}

		[Obsolete("Use Query(...) method instead.")]
		public virtual IEnumerable<IDataRecord> Yield(string sql)
		{
			return Query(sql, new Dictionary<string, object>(0));
		}

		[Obsolete("Use Query(...) method instead.")]
		public virtual IEnumerable<IDataRecord> Yield(string sql, IDictionary<string, object> parameters)
		{
			return Query(sql, parameters);
		}

		public virtual IEnumerable<IDataRecord> Query(string sql, object parameters = null)
		{
			return Query(sql, ToDictionary(parameters));
		}

		private IEnumerable<IDataRecord> Query(string sql, IDictionary<string, object> parameters)
		{
			using(var cmd = CreateCommand())
			{
				if (_log != null)
				{
					_log.Invoke(string.Concat("Executing reader: ", SqlToString(sql, parameters)));
				}
				cmd.DbCommand.CommandText = sql;
				AddParameters(cmd.DbCommand, parameters);
				using (IDataReader rdr = cmd.DbCommand.ExecuteReader())
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

		public virtual int Exec(string sql, object parameters = null)
		{
			return Exec(sql, ToDictionary(parameters));
		}

		private int Exec(string sql, IDictionary<string, object> parameters)
		{
			using(var cmd = CreateCommand())
			{
				if (_log != null)
				{
					_log.Invoke(string.Concat("Executing: ", SqlToString(sql, parameters)));
				}
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