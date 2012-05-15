using System.Data.SqlClient;

namespace ADOUtils.Tests
{
	public class TestUtils
	{
		private const string ConnStrTemplate = @"Data Source=.\SQLEXPRESS;Initial Catalog={0};Integrated Security=True";

		public static string CreateTestDbAndConnstr(string script)
		{
			SqlConnection.ClearAllPools();
			Execute("IF DB_ID('ADOUtilsTests') IS NOT NULL DROP DATABASE ADOUtilsTests");
			Execute("CREATE DATABASE ADOUtilsTests");
			Execute("USE ADOUtilsTests\n" + script);
			return string.Format(ConnStrTemplate, "ADOUtilsTests");
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