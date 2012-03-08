using System.Data.SqlClient;

namespace MyMicroOrm.Tests
{
	public class TestUtils
	{
		private const string ConnStrTemplate = @"Data Source=.\SQLEXPRESS;Initial Catalog={0};Integrated Security=True";

		public static string CreateTestDbAndConnstr(string script)
		{
			SqlConnection.ClearAllPools();
			Execute("IF DB_ID('MyMicroOrmTests') IS NOT NULL DROP DATABASE MyMicroOrmTests");
			Execute("CREATE DATABASE MyMicroOrmTests");
			Execute("USE MyMicroOrmTests\n" + script);
			return string.Format(ConnStrTemplate, "MyMicroOrmTests");
		}

		private static void Execute(string sql)
		{
			var conn = new SqlConnection(string.Format(ConnStrTemplate, "master"));
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