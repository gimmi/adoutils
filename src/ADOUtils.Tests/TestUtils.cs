using System.Data.SqlClient;

namespace ADOUtils.Tests
{
	public class TestUtils
	{
		private const string ConnStrTemplate = @"Data Source=.\SQLEXPRESS;Initial Catalog={0};Integrated Security=True";
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