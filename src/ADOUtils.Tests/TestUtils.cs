using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace ADOUtils.Tests
{
	public class TestUtils
	{
		/*
		 * Uses SQL server LocalDB
		 * Database file is located at: C:\Users\<user>\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\v11.0
		 * see http://msdn.microsoft.com/en-us/library/hh510202.aspx
		 * 
		 * For VS2010 you need to install:
		 * SQL Server 2012 express LocalDB
		 * http://support.microsoft.com/kb/2544514
		 */
		public const string DBName = "ADOUtilsTests";
		public const string Server = @"(localdb)\v11.0";
		private const string SqlServerConnStrTemplate = @"Server={0};Integrated Security=true;Initial Catalog={1}";
		private const string OdbcConnStrTemplate = @"Driver={{SQL Server Native Client 11.0}};Server={0};Database={1};Trusted_Connection=yes";

		public static void CreateTestDb()
		{
			SqlConnection.ClearAllPools();
			Execute("IF DB_ID('ADOUtilsTests') IS NOT NULL DROP DATABASE ADOUtilsTests", "master");
			Execute("CREATE DATABASE ADOUtilsTests", "master");
		}

		public static string SqlServerConnStr
		{
			get { return string.Format(SqlServerConnStrTemplate, Server, DBName); }
		}

		public static string OdbcConnStr
		{
			get { return string.Format(OdbcConnStrTemplate, Server, DBName); }
		}

		public static void Execute(string sql, string dbName = DBName)
		{
			var conn = new SqlConnection(string.Format(SqlServerConnStrTemplate, Server, dbName));
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