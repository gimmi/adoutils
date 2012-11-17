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
         */
        private const string ConnStrTemplate = @"Server=(localdb)\v11.0;Integrated Security=true;Initial Catalog={0}";
		private const string DBName = "ADOUtilsTests";

		public static void CreateTestDb()
		{
			SqlConnection.ClearAllPools();
			Execute("IF DB_ID('ADOUtilsTests') IS NOT NULL DROP DATABASE ADOUtilsTests", "master");
			Execute("CREATE DATABASE ADOUtilsTests", "master");
		}

		public static string ConnStr
		{
			get { return string.Format(ConnStrTemplate, DBName); }
		}

		public static void Execute(string sql, string dbName = DBName)
		{
			var conn = new SqlConnection(string.Format(ConnStrTemplate, dbName));
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