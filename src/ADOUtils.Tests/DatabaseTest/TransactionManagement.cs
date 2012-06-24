using System.Data;
using NUnit.Framework;
using SharpTestsEx;

namespace ADOUtils.Tests.DatabaseTest
{
	[TestFixture]
	public class TransactionManagement
	{
		public const string TblScript = @"
CREATE TABLE Tbl(IntValue int NULL)
";
		private Database _target;

		[SetUp]
		public void SetUp()
		{
			TestUtils.CreateTestDb();
			TestUtils.Execute(TblScript);

			_target = new Database(TestUtils.ConnStr);
		}

		[Test]
		public void Should_commit_only_when_explicitly_commit()
		{
			using(var tx = _target.BeginTransaction())
			{
				_target.Exec("INSERT INTO Tbl(IntValue) VALUES(1)");
				tx.Commit();
			}

			using(var tx = _target.BeginTransaction())
			{
				_target.Exec("INSERT INTO Tbl(IntValue) VALUES(2)");
				tx.Rollback();
			}

			using(var tx = _target.BeginTransaction())
			{
				_target.Exec("INSERT INTO Tbl(IntValue) VALUES(3)");
			}

			_target.Scalar<int>("SELECT SUM(IntValue) FROM Tbl").Should().Be.EqualTo(1);
		}

		[Test]
		public void Should_commit_operations_from_nested_transactions()
		{
			using(var outer = _target.BeginTransaction())
			{
				_target.Exec("INSERT INTO Tbl(IntValue) VALUES(1)");
	
				using (var inner = _target.BeginTransaction())
				{
					_target.Exec("INSERT INTO Tbl(IntValue) VALUES(2)");
					inner.Commit();
				}

				using (var inner = _target.BeginTransaction())
				{
					_target.Exec("INSERT INTO Tbl(IntValue) VALUES(3)");

					using (var innerInner = _target.BeginTransaction())
					{
						_target.Exec("INSERT INTO Tbl(IntValue) VALUES(4)");
						innerInner.Commit();
					}

					inner.Commit();
				}
				outer.Commit();
			}

			_target.Scalar<int>("SELECT COUNT(*) FROM Tbl").Should().Be.EqualTo(4);
		}

		[Test]
		public void Should_revert_nested_transactions_when_rolling_back_outer_transaction()
		{
			using (var outer = _target.BeginTransaction())
			{
				_target.Exec("INSERT INTO Tbl(IntValue) VALUES(1)");

				using (var inner = _target.BeginTransaction())
				{
					_target.Exec("INSERT INTO Tbl(IntValue) VALUES(2)");
					inner.Commit();
				}

				using (var inner = _target.BeginTransaction())
				{
					_target.Exec("INSERT INTO Tbl(IntValue) VALUES(3)");

					using (var innerInner = _target.BeginTransaction())
					{
						_target.Exec("INSERT INTO Tbl(IntValue) VALUES(4)");
						innerInner.Commit();
					}

					inner.Commit();
				}

				_target.Exec("INSERT INTO Tbl(IntValue) VALUES(5)");

				outer.Rollback();
			}

			_target.Scalar<int>("SELECT COUNT(*) FROM Tbl").Should().Be.EqualTo(0);
		}

		[Test]
		public void Should_throw_exception_and_save_nothing_when_commit_outer_transaction_and_at_least_one_inner_transaction_has_been_rollback()
		{
			using (var outer = _target.BeginTransaction())
			{
				_target.Exec("INSERT INTO Tbl(IntValue) VALUES(1)");

				using (var inner = _target.BeginTransaction())
				{
					_target.Exec("INSERT INTO Tbl(IntValue) VALUES(2)");
					inner.Rollback();
				}

				_target.Exec("INSERT INTO Tbl(IntValue) VALUES(3)");

				Executing.This(() => outer.Commit()).Should().Throw<DataException>()
					.And.Exception.Message.Should().Be.EqualTo("Cannot commit transaction when one of the nested transaction has been rolled back");
			}

			_target.Scalar<int>("SELECT COUNT(*) FROM Tbl").Should().Be.EqualTo(0);
		}

		[Test]
		public void Should_safely_call_commit_and_rollback_multiple_times()
		{
			using (var outer = _target.BeginTransaction())
			{
				_target.Exec("INSERT INTO Tbl(IntValue) VALUES(1)");

				using (var inner = _target.BeginTransaction())
				{
					_target.Exec("INSERT INTO Tbl(IntValue) VALUES(2)");
					inner.Commit();
					inner.Rollback();
					inner.Commit();
				}

				_target.Exec("INSERT INTO Tbl(IntValue) VALUES(3)");

				outer.Commit();
				outer.Rollback();
				outer.Commit();
			}

			_target.Scalar<int>("SELECT COUNT(*) FROM Tbl").Should().Be.EqualTo(3);
		}
	}
}