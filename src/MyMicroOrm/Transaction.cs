using System;

namespace MyMicroOrm
{
	public class Transaction : IDisposable
	{
		private readonly Action _commit;
		private readonly Action _rollback;
		private readonly Action _dispose;

		public Transaction(Action commit = null, Action rollback = null, Action dispose = null)
		{
			_commit = commit ?? delegate { };
			_rollback = rollback ?? delegate { };
			_dispose = dispose ?? delegate { };
		}

		public virtual void Commit()
		{
			_commit.Invoke();
		}

		public virtual void Rollback()
		{
			_rollback.Invoke();
		}

		public virtual void Dispose()
		{
			_dispose.Invoke();
		}
	}
}