using System.Data.Common;

namespace ADOUtils
{
	public class Command : ICommand
	{
		private readonly IConnection _connection;

		public Command(DbCommand dbCommand, IConnection connection)
		{
			DbCommand = dbCommand;
			_connection = connection;
		}

		public DbCommand DbCommand { get; }

		public void Dispose()
		{
			_connection.Dispose();
		}
	}
}
