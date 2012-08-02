using System.Data;

namespace ADOUtils
{
	public class Command : ICommand
	{
		private readonly IDbCommand _dbCommand;
		private readonly IConnection _connection;

		public Command(IDbCommand dbCommand, IConnection connection)
		{
			_dbCommand = dbCommand;
			_connection = connection;
		}

		public IDbCommand DbCommand
		{
			get { return _dbCommand; }
		}

		public void Dispose()
		{
			_connection.Dispose();
		}
	}
}