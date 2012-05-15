using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace ADOUtils
{
	public class Database : IDisposable
	{
		private readonly string _connStr;
		private SqlConnection _conn;
		private SqlTransaction _tr;

		public Database(string connStr)
		{
//			DbProviderFactories.GetFactory();
			_connStr = connStr;
		}

		public virtual Connection OpenConnection()
		{
			if (_conn != null)
			{
				return new Connection();
			}
			var conn = new SqlConnection(_connStr);
			conn.Open();
			_conn = conn;
			return new Connection(CloseConnection);
		}

		private void CloseConnection()
		{
			if (_conn != null)
			{
				var conn = _conn;
				_conn = null;
				conn.Close();
			}
		}

		public virtual Transaction BeginTransaction()
		{
			if (_tr != null)
			{
				return new Transaction(rollback: RollbackTransaction);
			}
			_tr = _conn.BeginTransaction();
			return new Transaction(CommitTransaction, RollbackTransaction, RollbackTransaction);
		}

		private void CommitTransaction()
		{
			if (_tr != null)
			{
				var tr = _tr;
				_tr = null;
				tr.Commit();
			}
		}

		private void RollbackTransaction()
		{
			if (_tr != null)
			{
				var tr = _tr;
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
				var cmd = new SqlCommand(sql, _conn, _tr);
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
				var cmd = new SqlCommand(sql, _conn, _tr);
				AddParameters(cmd, parameters);
				using (IDataReader rdr = cmd.ExecuteReader())
				{
					while(rdr.Read())
					{
						yield return rdr;
					}
				}
			}
		}

		public virtual int Exec(string sql, object parameters)
		{
			return Exec(sql, ToDictionary(parameters));
		}

		public virtual int Exec(string sql, IDictionary<string, object> parameters)
		{
			using(OpenConnection())
			{
				var cmd = new SqlCommand(sql, _conn, _tr);
				AddParameters(cmd, parameters);
				return cmd.ExecuteNonQuery();
			}
		}

		private static void AddParameters(SqlCommand cmd, IDictionary<string, object> parameters)
		{
			foreach(var pi in parameters)
			{
				cmd.Parameters.AddWithValue(pi.Key, pi.Value ?? DBNull.Value);
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