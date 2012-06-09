using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace ADOUtils
{
	public class Database : IDisposable
	{
		private readonly DbProviderFactory _factory;
		private readonly string _connStr;
		private IDbConnection _conn;
		private IDbTransaction _tr;

		public Database(string connStr) : this("System.Data.SqlClient", connStr) {}

		public Database(string providerName, string connStr) : this(DbProviderFactories.GetFactory(providerName), connStr) {}

		public Database(DbProviderFactory factory, string connStr)
		{
			_factory = factory;
			_connStr = connStr;
		}

		public virtual Connection OpenConnection()
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
				IDbConnection conn = _conn;
				_conn = null;
				conn.Close();
			}
		}

		public virtual Transaction BeginTransaction()
		{
			if(_tr != null)
			{
				return new Transaction(rollback: new Action[] { RollbackTransaction });
			}
			Connection connection = OpenConnection();
			_tr = _conn.BeginTransaction();
			return new Transaction(new Action[] { CommitTransaction, connection.Close }, new Action[] { RollbackTransaction, connection.Close }, new Action[] { RollbackTransaction, connection.Close });
		}

		private void CommitTransaction()
		{
			if(_tr != null)
			{
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
			using(OpenConnection())
			{
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
				}
			}
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