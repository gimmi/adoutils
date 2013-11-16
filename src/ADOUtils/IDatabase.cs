using System;
using System.Collections.Generic;
using System.Data;

namespace ADOUtils
{
	public interface IDatabase : IDisposable
	{
		IConnection OpenConnection();
		ITransaction BeginTransaction();
        ICommand CreateCommand();
        T Scalar<T>(string sql, object parameters = null);
		int Exec(string sql, object parameters = null);
		IEnumerable<IDataRecord> Query(string sql, object parameters = null);
	}
}