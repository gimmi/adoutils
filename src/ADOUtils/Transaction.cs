using System;

namespace ADOUtils
{
	public class Transaction : ITransaction
	{
		private readonly IConnection _connection;
		private readonly Action _commitAction;
		private readonly Action _rollbackAction;
		private bool _done = false;

		public Transaction(IConnection connection, Action commit, Action rollback)
		{
			_connection = connection;
			_commitAction = commit;
			_rollbackAction = rollback;
		}

		public virtual void Commit()
		{
			if (!_done)
			{
				_done = true;
				_commitAction.Invoke();
				_connection.Close();
			}
		}

		public virtual void Rollback()
		{
			if (!_done)
			{
				_done = true;
				_rollbackAction.Invoke();
				_connection.Close();
			}
		}

		public virtual void Dispose()
		{
			Rollback();
		}
	}
}