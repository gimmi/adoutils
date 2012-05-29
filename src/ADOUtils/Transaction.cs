using System;

namespace ADOUtils
{
	public class Transaction : IDisposable
	{
		private readonly Action[] _commitActions;
		private readonly Action[] _rollbackActions;
		private readonly Action[] _disposeActions;

		public Transaction(Action[] commit = null, Action[] rollback = null, Action[] dispose = null)
		{
			_commitActions = commit ?? new Action[] { delegate { } };
			_rollbackActions = rollback ?? new Action[] { delegate { } };
			_disposeActions = dispose ?? new Action[] { delegate { } };
		}

		public virtual void Commit()
		{
			foreach (var commitAction in _commitActions)
			{
				commitAction.Invoke();
			}
		}

		public virtual void Rollback()
		{
			foreach (var rollbackAction in _rollbackActions)
			{
				rollbackAction.Invoke();
			}
		}

		public virtual void Dispose()
		{
			foreach (var disposeAction in _disposeActions)
			{
				disposeAction.Invoke();
			}
		}
	}
}