using System;
using System.Data;

namespace ADOUtils
{
	public class Command : IDisposable
	{
		private readonly Connection _connection;
		private readonly IDbCommand _command;

		public Command(Connection connection, IDbCommand command)
		{
			_connection = connection;
			_command = command;
		}

		public IDbCommand DbCommand
		{
			get { return _command; }
		}

		public void Dispose()
		{
			_connection.Dispose();
		}
	}
}