using System;

namespace ADOUtils
{
	public class Transaction : IDisposable
	{
		private readonly Connection _connection;
		private readonly Action _commitAction;
		private readonly Action _rollbackAction;
		private readonly Action _disposeAction;

		public Transaction(Connection connection, Action commit, Action rollback, Action dispose)
		{
			_connection = connection;
			_commitAction = commit;
			_rollbackAction = rollback;
			_disposeAction = dispose;
		}

		public virtual void Commit()
		{
			_commitAction.Invoke();
			_connection.Close();
		}

		public virtual void Rollback()
		{
			_rollbackAction.Invoke();
			_connection.Close();
		}

		public virtual void Dispose()
		{
			_disposeAction.Invoke();
			_connection.Dispose();
		}
	}
}