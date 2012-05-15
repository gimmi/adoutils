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

		public void Close()
		{
			_action.Invoke();
		}

		public void Dispose()
		{
			_action.Invoke();
		}
	}
}