using System.Collections.Generic;
using System.Data.Common;
using NUnit.Framework;
using SharpTestsEx;

namespace ADOUtils.Tests.DatabaseTest
{
	[TestFixture]
	public class Logging
	{
		private Database _target;
		private List<string> _logs;

		[SetUp]
		public void SetUp()
		{
			TestUtils.CreateTestDb();
			_logs = new List<string>();
			_target = new Database(TestUtils.ConnStr, DbProviderFactories.GetFactory("System.Data.SqlClient"), s => _logs.Add(s));
		}

		[Test]
		public void Should_log_connection()
		{
			_target.OpenConnection().Close();

			_logs.Should().Have.SameSequenceAs(new[] {
				"Opening connection", 
				"Closing connection"
			});
		}

		[Test]
		public void Should_log_succesful_transaction()
		{
			using(_target.OpenConnection())
			{
				_logs.Clear();
				_target.BeginTransaction().Commit();

				_logs.Should().Have.SameSequenceAs(new[] {
					"Beginning transaction",
					"Committing transaction"
				});
			}
		}

		[Test]
		public void Should_log_unsuccessful_transaction()
		{
			using(_target.OpenConnection())
			{
				_logs.Clear();
				_target.BeginTransaction().Rollback();

				_logs.Should().Have.SameSequenceAs(new[] {
					"Beginning transaction",
					"Rolling back transaction"
				});
			}
		}

		[Test]
		public void Should_log_query_execution()
		{
			using (_target.OpenConnection())
			{
				_logs.Clear();
				_target.Exec("SELECT @Par AS P", new { Par = "hello" });

				_logs.Should().Have.SameSequenceAs(new[] {
					"Executing: `SELECT @Par AS P` { Par = `hello` }"
				});
			}
		}

		[Test]
		public void Should_log_query_reader()
		{
			using (_target.OpenConnection())
			{
				_logs.Clear();
				_target.Read("SELECT @Par AS P", new { Par = "hello" });

				_logs.Should().Have.SameSequenceAs(new[] {
					"Executing reader: `SELECT @Par AS P` { Par = `hello` }",
					"Closing reader: `SELECT @Par AS P` { Par = `hello` }"
				});
			}
		}

		[Test]
		public void Should_log_query_scalar()
		{
			using (_target.OpenConnection())
			{
				_logs.Clear();
				_target.Scalar<string>("SELECT @Par AS P", new { Par = "hello" });

				_logs.Should().Have.SameSequenceAs(new[] {
					"Executing scalar: `SELECT @Par AS P` { Par = `hello` }"
				});
			}
		}

		[Test]
		public void Should_work_when_no_logger_defined()
		{
			var db = new Database(TestUtils.ConnStr);

			using (db.OpenConnection())
			{
				using(var tx = db.BeginTransaction())
				{
					db.Read("SELECT 1");
					db.Scalar<int>("SELECT 1");
					db.Exec("SELECT 1");
					tx.Commit();
				}
			}
		}
	}
}