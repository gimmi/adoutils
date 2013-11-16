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
		int Exec(string sql, object parameters = null);
		ICommand CreateCommand();
		IEnumerable<IDataRecord> Query(string sql, object parameters = null);
	}
}