using System.Collections.Generic;
using System.Data;
using System.Linq;
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
			TestUtils.CreateTestDb();
			TestUtils.Execute(Script);

			_target = new Database(TestUtils.ConnStr);
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
		public void Should_exec()
		{
			_target.Exec("UPDATE Tbl SET Name = Name").Should().Be.EqualTo(2);
			_target.Exec("UPDATE Tbl SET Name = Name WHERE 1 = @par", new { par = 2 }).Should().Be.EqualTo(0);
		}

		[Test]
		public void Should_manage_connection()
		{
			_target.FieldValue<IDbConnection>("_conn").Should().Be.Null();

			Connection outerConnection = _target.OpenConnection();

			_target.FieldValue<IDbConnection>("_conn").State.Should().Be.EqualTo(ConnectionState.Open);

			Connection innerConnection = _target.OpenConnection();

			_target.FieldValue<IDbConnection>("_conn").State.Should().Be.EqualTo(ConnectionState.Open);

			innerConnection.Close();

			_target.FieldValue<IDbConnection>("_conn").State.Should().Be.EqualTo(ConnectionState.Open);

			outerConnection.Close();

			_target.FieldValue<IDbConnection>("_conn").Should().Be.Null();
		}

		[Test]
		public void Should_manage_successful_transaction()
		{
			using(_target.OpenConnection())
			{
				_target.FieldValue<IDbTransaction>("_tr").Should().Be.Null();

				Transaction transaction = _target.BeginTransaction();

				_target.FieldValue<IDbTransaction>("_tr").Should().Not.Be.Null();

				transaction.Commit();

				_target.FieldValue<IDbTransaction>("_tr").Should().Be.Null();
			}
		}

		[Test]
		public void Should_manage_failing_transaction()
		{
			using(_target.OpenConnection())
			{
				_target.FieldValue<IDbTransaction>("_tr").Should().Be.Null();

				Transaction transaction = _target.BeginTransaction();

				_target.FieldValue<IDbTransaction>("_tr").Should().Not.Be.Null();

				transaction.Rollback();

				_target.FieldValue<IDbTransaction>("_tr").Should().Be.Null();
			}
		}

		[Test]
		public void Should_manage_nested_succesful_transaction()
		{
			using(_target.OpenConnection())
			{
				_target.FieldValue<IDbTransaction>("_tr").Should().Be.Null();

				Transaction outerTransaction = _target.BeginTransaction();

				_target.FieldValue<IDbTransaction>("_tr").Should().Not.Be.Null();

				Transaction innerTransaction = _target.BeginTransaction();

				_target.FieldValue<IDbTransaction>("_tr").Should().Not.Be.Null();

				innerTransaction.Commit();

				_target.FieldValue<IDbTransaction>("_tr").Should().Not.Be.Null();

				outerTransaction.Commit();

				_target.FieldValue<IDbTransaction>("_tr").Should().Be.Null();
			}
		}

		[Test]
		public void Should_manage_nested_failing_transaction()
		{
			using(_target.OpenConnection())
			{
				_target.FieldValue<IDbTransaction>("_tr").Should().Be.Null();

				Transaction outerTransaction = _target.BeginTransaction();

				_target.FieldValue<IDbTransaction>("_tr").Should().Not.Be.Null();

				Transaction innerTransaction = _target.BeginTransaction();

				_target.FieldValue<IDbTransaction>("_tr").Should().Not.Be.Null();

				innerTransaction.Rollback();

				_target.FieldValue<IDbTransaction>("_tr").Should().Be.Null();

				outerTransaction.Commit();

				_target.FieldValue<IDbTransaction>("_tr").Should().Be.Null();
			}
		}

		[Test]
		public void Should_open_transacion_with_connection()
		{
			Transaction tran = _target.BeginTransaction();
			_target.FieldValue<IDbConnection>("_conn").Should().Not.Be.Null();

			tran.Commit();
			_target.FieldValue<IDbConnection>("_conn").Should().Be.Null();

			tran = _target.BeginTransaction();

			tran.Rollback();
			_target.FieldValue<IDbConnection>("_conn").Should().Be.Null();
		}
	}
}