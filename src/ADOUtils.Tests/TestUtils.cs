using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace ADOUtils.Tests
{
	public class TestUtils
	{
		/*
		 * Uses SQL server LocalDB
		 * Database file is located at: C:\Users\<user>\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB
		 * see http://msdn.microsoft.com/en-us/library/hh510202.aspx
		 *
		 * For VS2010 you need to install:
		 * SQL Server 2012 express LocalDB
		 * http://support.microsoft.com/kb/2544514
		 */
		private const string ConnStrTemplate = "Data Source=tcp:127.0.0.1,1433;User ID=sa;Password=Passw0rd;Initial Catalog=";
		private const string DBName = "ADOUtilsTests";

		public static void CreateTestDb()
		{
			SqlConnection.ClearAllPools();
			Execute("IF DB_ID('ADOUtilsTests') IS NOT NULL DROP DATABASE ADOUtilsTests", "master");
			Execute("CREATE DATABASE ADOUtilsTests", "master");
		}

		public static string ConnStr => ConnStrTemplate + DBName;

		public static void Execute(string sql, string dbName = DBName)
		{
			var conn = new SqlConnection(ConnStrTemplate + dbName);
			conn.Open();
			try
			{
				new SqlCommand(sql, conn).ExecuteNonQuery();
			}
			finally
			{
				conn.Close();
			}
		}
	}
}
