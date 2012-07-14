using System;

namespace ADOUtils
{
	public interface ITransaction : IDisposable
	{
		void Commit();
		void Rollback();
	}
}