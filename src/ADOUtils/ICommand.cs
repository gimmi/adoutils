using System;
using System.Data;
using System.Data.Common;

namespace ADOUtils
{
	public interface ICommand : IDisposable
	{
		DbCommand DbCommand { get; }
	}
}
