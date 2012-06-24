using System;

namespace ADOUtils
{
	public class Transaction : IDisposable
	{
		private readonly Connection _connection;
		private Action _commitAction;
		private Action _rollbackAction;

		public Transaction(Connection connection, Action commit, Action rollback)
		{
			_connection = connection;
			_commitAction = commit;
			_rollbackAction = rollback;
		}

		public virtual void Commit()
		{
			Action commitAction = _commitAction;
			_commitAction = _rollbackAction = delegate {};
			commitAction.Invoke();
			_connection.Close();
		}

		public virtual void Rollback()
		{
			Action rollbackAction = _rollbackAction;
			_rollbackAction = _commitAction = delegate {};
			rollbackAction.Invoke();
			_connection.Close();
		}

		public virtual void Dispose()
		{
			Rollback();
		}
	}
}