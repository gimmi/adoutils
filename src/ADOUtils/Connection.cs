using System;

namespace ADOUtils
{
	public class Connection : IDisposable
	{
		private readonly Action _action;

		public Connection(Action action = null)
		{
			_action = action ?? delegate {};
		}

		public virtual void Close()
		{
			_action.Invoke();
		}

		public virtual void Dispose()
		{
			_action.Invoke();
		}
	}
}