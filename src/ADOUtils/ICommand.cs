using System;
using System.Data;

namespace ADOUtils
{
	public interface ICommand : IDisposable
	{
		IDbCommand DbCommand { get; }
	}
}