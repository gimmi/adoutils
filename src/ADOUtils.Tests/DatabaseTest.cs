using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SharpTestsEx;

namespace ADOUtils.Tests
{
	[TestFixture]
	public class DatabaseTest
	{
		public const string Script = @"
CREATE TABLE Tbl(Id int NULL, Name nvarchar(255) NULL)
INSERT Tbl(Id, Name) VALUES(1, 'row 1')
INSERT Tbl(Id, Name) VALUES(2, 'row 2')
";
		private Database _target;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_target = new Database(TestUtils.CreateTestDbAndConnstr(Script));
		}

		[Test]
		public void Should_query_with_yield()
		{
			_target.Yield("SELECT Name FROM Tbl WHERE Id = @Id", new Dictionary<string, object> { { "Id", 2 } }).Select(x => x.GetString(0)).First().Should().Be.EqualTo("row 2");
			_target.Yield("SELECT Name FROM Tbl WHERE Id = @Id", new { Id = 2 }).Select(x => x.GetString(0)).First().Should().Be.EqualTo("row 2");
			_target.Yield("SELECT Name FROM Tbl WHERE Id = 2").Select(x => x.GetString(0)).First().Should().Be.EqualTo("row 2");
		}

		[Test]
		public void Should_query_with_read()
		{
			_target.Read("SELECT Name FROM Tbl WHERE Id = @Id", new Dictionary<string, object> { { "Id", 2 } }).Select(x => x["Name"]).First().Should().Be.EqualTo("row 2");
			_target.Read("SELECT Name FROM Tbl WHERE Id = @Id", new { Id = 2 }).Select(x => x["Name"]).First().Should().Be.EqualTo("row 2");
			_target.Read("SELECT Name FROM Tbl WHERE Id = 2").Select(x => x["Name"]).First().Should().Be.EqualTo("row 2");
		}

		[Test]
		public void Should_query_scalar()
		{
			_target.Scalar<int>("SELECT COUNT(*) FROM Tbl").Should().Be.EqualTo(2);
			_target.Scalar<int>("SELECT COUNT(*) FROM Tbl WHERE Id = @Id", new Dictionary<string, object> { { "Id", 2 } }).Should().Be.EqualTo(1);
			_target.Scalar<int>("SELECT COUNT(*) FROM Tbl WHERE Id = @Id", new { Id = 2 }).Should().Be.EqualTo(1);
		}

		[Test]
		public void Should_manage_connection()
		{
			GetConnField().Should().Be.Null();

			var outerConnection = _target.OpenConnection();

			GetConnField().State.Should().Be.EqualTo(ConnectionState.Open);

			var innerConnection = _target.OpenConnection();

			GetConnField().State.Should().Be.EqualTo(ConnectionState.Open);

			innerConnection.Close();

			GetConnField().State.Should().Be.EqualTo(ConnectionState.Open);

			outerConnection.Close();

			GetConnField().Should().Be.Null();
		}

		[Test]
		public void Should_manage_successful_transaction()
		{
			using (_target.OpenConnection())
			{
				GetTrField().Should().Be.Null();
				
				var transaction = _target.BeginTransaction();

				GetTrField().Should().Not.Be.Null();

				transaction.Commit();

				GetTrField().Should().Be.Null();
			}
		}

		[Test]
		public void Should_manage_failing_transaction()
		{
			using (_target.OpenConnection())
			{
				GetTrField().Should().Be.Null();
				
				var transaction = _target.BeginTransaction();

				GetTrField().Should().Not.Be.Null();

				transaction.Rollback();

				GetTrField().Should().Be.Null();
			}
		}

		[Test]
		public void Should_manage_nested_succesful_transaction()
		{
			using (_target.OpenConnection())
			{
				GetTrField().Should().Be.Null();
				
				var outerTransaction = _target.BeginTransaction();

				GetTrField().Should().Not.Be.Null();

				var innerTransaction = _target.BeginTransaction();

				GetTrField().Should().Not.Be.Null();

				innerTransaction.Commit();

				GetTrField().Should().Not.Be.Null();

				outerTransaction.Commit();

				GetTrField().Should().Be.Null();
			}
		}

		[Test]
		public void Should_manage_nested_failing_transaction()
		{
			using (_target.OpenConnection())
			{
				GetTrField().Should().Be.Null();
				
				var outerTransaction = _target.BeginTransaction();

				GetTrField().Should().Not.Be.Null();

				var innerTransaction = _target.BeginTransaction();

				GetTrField().Should().Not.Be.Null();

				innerTransaction.Rollback();

				GetTrField().Should().Be.Null();

				outerTransaction.Commit();

				GetTrField().Should().Be.Null();
			}
		}

		[Test]
		public void Should_open_transacion_with_connection()
		{
			var tran = _target.BeginTransaction();
			GetConnField().Should().Not.Be.Null();

			tran.Commit();
			GetConnField().Should().Be.Null();

			tran = _target.BeginTransaction();

			tran.Rollback();
			GetConnField().Should().Be.Null();
		}

		private SqlConnection GetConnField()
		{
			return (SqlConnection)_target.GetType().GetField("_conn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_target);
		}

		private SqlTransaction GetTrField()
		{
			return (SqlTransaction)_target.GetType().GetField("_tr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_target);
		}
	}
}