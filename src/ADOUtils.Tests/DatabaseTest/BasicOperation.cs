using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace ADOUtils.Tests.DatabaseTest
{
	[TestFixture]
	public class BasicOperation
	{
		public const string TblScript = @"
CREATE TABLE Tbl(IntValue int NULL, StringValue nvarchar(255) NULL, DateValue datetime NULL, GuidValue uniqueidentifier NULL)
INSERT Tbl(IntValue, StringValue, DateValue, GuidValue) VALUES(1, 'string 1', '2012-06-09 18:33', 'A71C8E84-B1A1-4CED-81B7-F551704A33E7')
INSERT Tbl(IntValue, StringValue, DateValue, GuidValue) VALUES(2, 'string 2', '2012-06-09 18:34', 'A9610CE3-7013-4C78-9C32-452D5A3CE450')
";
		public const string SPScript = @"
CREATE PROCEDURE SP
	@Param1 nvarchar(max),
	@Param2 int
AS BEGIN
	SELECT @param1 AS Param1, @Param2 AS Param2
END
";
		private Database _target;

		[TestFixtureSetUp]
		public void SetUp()
		{
			TestUtils.CreateTestDb();
			TestUtils.Execute(TblScript);
			TestUtils.Execute(SPScript);

			_target = new Database(TestUtils.ConnStr);
		}

		[Test]
		public void Should_query_with_yield()
		{
			_target.Yield("SELECT StringValue FROM Tbl WHERE IntValue = @IntValue", new Dictionary<string, object> { { "IntValue", 2 } }).Select(x => x.GetString(0)).First().Should().Be.EqualTo("string 2");
			_target.Yield("SELECT StringValue FROM Tbl WHERE IntValue = @IntValue", new { IntValue = 2 }).Select(x => x.GetString(0)).First().Should().Be.EqualTo("string 2");
			_target.Yield("SELECT StringValue FROM Tbl WHERE IntValue = 2").Select(x => x.GetString(0)).First().Should().Be.EqualTo("string 2");
		}

		[Test]
		public void Should_query_with_read()
		{
			_target.Read("SELECT StringValue FROM Tbl WHERE IntValue = @IntValue", new Dictionary<string, object> { { "IntValue", 2 } }).Select(x => x["StringValue"]).First().Should().Be.EqualTo("string 2");
			_target.Read("SELECT StringValue FROM Tbl WHERE IntValue = @IntValue", new { IntValue = 2 }).Select(x => x["StringValue"]).First().Should().Be.EqualTo("string 2");
			_target.Read("SELECT StringValue FROM Tbl WHERE IntValue = 2").Select(x => x["StringValue"]).First().Should().Be.EqualTo("string 2");
		}

		[Test]
		public void Should_query_scalar()
		{
			_target.Scalar<int>("SELECT COUNT(*) FROM Tbl").Should().Be.EqualTo(2);
			_target.Scalar<int>("SELECT COUNT(*) FROM Tbl WHERE IntValue = @IntValue", new Dictionary<string, object> { { "IntValue", 2 } }).Should().Be.EqualTo(1);
			_target.Scalar<int>("SELECT COUNT(*) FROM Tbl WHERE IntValue = @IntValue", new { IntValue = 2 }).Should().Be.EqualTo(1);
		}

		[Test]
		public void Should_exec()
		{
			_target.Exec("UPDATE Tbl SET StringValue = StringValue").Should().Be.EqualTo(2);
			_target.Exec("UPDATE Tbl SET StringValue = StringValue WHERE 1 = @par", new { par = 2 }).Should().Be.EqualTo(0);
		}

		[Test]
		public void Should_pass_and_read_various_datatypes_as_expected()
		{
			var actual = _target.Yield("SELECT * FROM Tbl WHERE IntValue = @IntValue AND StringValue = @StringValue AND DateValue = @DateValue AND GuidValue = @GuidValue", new {
				IntValue = 1, StringValue = "string 1", DateValue = new DateTime(2012, 6, 9, 18, 33, 0), GuidValue = new Guid("A71C8E84-B1A1-4CED-81B7-F551704A33E7")
			}).Select(r => new {
				IntValue = r["IntValue"], StringValue = r["StringValue"], DateValue = r["DateValue"], GuidValue = r["GuidValue"]
			}).Single();

			actual.IntValue.Should().Be.OfType<int>().And.Be.EqualTo(1);
			actual.StringValue.Should().Be.OfType<string>().And.Be.EqualTo("string 1");
			actual.DateValue.Should().Be.OfType<DateTime>().And.Be.EqualTo(new DateTime(2012, 6, 9, 18, 33, 0));
			actual.GuidValue.Should().Be.OfType<Guid>().And.Be.EqualTo(new Guid("A71C8E84-B1A1-4CED-81B7-F551704A33E7"));
		}

		[Test]
		public void Should_manage_connection()
		{
			_target.FieldValue<IDbConnection>("_conn").Should().Be.Null();

			IConnection outerConnection = _target.OpenConnection();

			_target.FieldValue<IDbConnection>("_conn").State.Should().Be.EqualTo(ConnectionState.Open);

			IConnection innerConnection = _target.OpenConnection();

			_target.FieldValue<IDbConnection>("_conn").State.Should().Be.EqualTo(ConnectionState.Open);

			innerConnection.Close();

			_target.FieldValue<IDbConnection>("_conn").State.Should().Be.EqualTo(ConnectionState.Open);

			outerConnection.Close();

			_target.FieldValue<IDbConnection>("_conn").Should().Be.Null();
		}

		[Test]
		public void Should_manage_failing_transaction()
		{
			using(_target.OpenConnection())
			{
				_target.FieldValue<IDbTransaction>("_tr").Should().Be.Null();

				ITransaction transaction = _target.BeginTransaction();

				_target.FieldValue<IDbTransaction>("_tr").Should().Not.Be.Null();

				transaction.Rollback();

				_target.FieldValue<IDbTransaction>("_tr").Should().Be.Null();
			}
		}

		[Test]
		public void Should_open_transacion_with_connection()
		{
			ITransaction tran = _target.BeginTransaction();
			_target.FieldValue<IDbConnection>("_conn").Should().Not.Be.Null();

			tran.Commit();
			_target.FieldValue<IDbConnection>("_conn").Should().Be.Null();

			tran = _target.BeginTransaction();

			tran.Rollback();
			_target.FieldValue<IDbConnection>("_conn").Should().Be.Null();
		}

		[Test]
		public void Should_execute_stored_procedure()
		{
			var actual = _target.Yield("EXEC SP @Param1, @Param2", new { Param1 = "p1", Param2 = 2 }).Select(r => new {
				Param1 = r["Param1"], 
				Param2 = r["Param2"]
			}).Single();

			actual.Param1.Should().Be.EqualTo("p1");
			actual.Param2.Should().Be.EqualTo(2);
		}

		[Test]
		public void Should_give_access_to_raw_command()
		{
			using (var cmd = _target.CreateCommand())
			{
				var sqlCmd = (SqlCommand)cmd.DbCommand;
				sqlCmd.CommandText = "Select @IntValue";
				sqlCmd.Parameters.AddWithValue("IntValue", 123);
				sqlCmd.ExecuteScalar().Should().Be.EqualTo(123);
			}
		}

		[Test]
		public void Should_put_raw_command_in_transaction()
		{
			using(_target.BeginTransaction())
			{
				using (var cmd = _target.CreateCommand())
				{
					var sqlCmd = (SqlCommand)cmd.DbCommand;
					sqlCmd.CommandText = "INSERT INTO Tbl(IntValue) VALUES(123)";
					sqlCmd.ExecuteNonQuery();
				}
			}

			_target.Scalar<int>("SELECT COUNT(*) FROM Tbl WHERE IntValue = 123").Should().Be.EqualTo(0);
		}
	}
}