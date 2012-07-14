using System;
using System.Collections.Generic;
using System.Data;

namespace ADOUtils
{
	public interface IDatabase : IDisposable
	{
		IConnection OpenConnection();
		ITransaction BeginTransaction();
		T Scalar<T>(string sql);
		T Scalar<T>(string sql, object parameters);
		T Scalar<T>(string sql, IDictionary<string, object> parameters);
		IEnumerable<IDataRecord> Read(string sql);
		IEnumerable<IDataRecord> Read(string sql, object parameters);
		IEnumerable<IDataRecord> Read(string sql, IDictionary<string, object> parameters);
		IEnumerable<IDataRecord> Yield(string sql);
		IEnumerable<IDataRecord> Yield(string sql, object parameters);
		IEnumerable<IDataRecord> Yield(string sql, IDictionary<string, object> parameters);
		int Exec(string sql);
		int Exec(string sql, object parameters);
		int Exec(string sql, IDictionary<string, object> parameters);
	}
}