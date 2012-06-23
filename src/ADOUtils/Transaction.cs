using System;

namespace ADOUtils
{
	public class Transaction : IDisposable
	{
		private readonly Connection _connection;
		private readonly Action _commitActions;
		private readonly Action _rollbackActions;
		private readonly Action _disposeActions;

		public Transaction(Connection connection, Action commit = null, Action rollback = null, Action dispose = null)
		{
			_connection = connection;
			_commitActions = commit ?? delegate { };
			_rollbackActions = rollback ?? delegate { };
			_disposeActions = dispose ?? delegate { };
		}

		public virtual void Commit()
		{
			_commitActions.Invoke();
			_connection.Close();
		}

		public virtual void Rollback()
		{
			_rollbackActions.Invoke();
			_connection.Close();
		}

		public virtual void Dispose()
		{
			_disposeActions.Invoke();
			_connection.Dispose();
		}
	}
}