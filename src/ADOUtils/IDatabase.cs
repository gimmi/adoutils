using System;
using System.Collections.Generic;
using System.Data;

namespace ADOUtils
{
	public interface IDatabase : IDisposable
	{
		IConnection OpenConnection();
		ITransaction BeginTransaction();
		ICommand CreateCommand(int? timeout = null);
		T Scalar<T>(string sql, object parameters = null, int? timeout = null);
		int Exec(string sql, object parameters = null, int? timeout = null);
		IEnumerable<IDataRecord> Query(string sql, object parameters = null, int? timeout = null);
	}
}