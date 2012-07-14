using System;

namespace ADOUtils
{
	public interface IConnection : IDisposable
	{
		void Close();
	}
}