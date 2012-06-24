using Moq;
using NUnit.Framework;
using SharpTestsEx;

namespace ADOUtils.Tests
{
	[TestFixture]
	public class TransactionTest
	{
		private Transaction _target;
		private Mock<Connection> _connection;
		private int _commitCount;
		private int _rollbackCount;

		[SetUp]
		public void SetUp()
		{
			_connection = new Mock<Connection>(null);
			_commitCount = 0;
			_rollbackCount = 0;
			_target = new Transaction(_connection.Object, () => ++_commitCount, () => ++_rollbackCount);
		}

		[Test]
		public void Should_invoke_commit_action()
		{
			_target.Commit();

			_commitCount.Should().Be.EqualTo(1);
			_rollbackCount.Should().Be.EqualTo(0);
			_connection.Verify(x=>x.Close(), Times.Once());
		}

		[Test]
		public void Should_invoke_rollback_action()
		{
			_target.Rollback();

			_commitCount.Should().Be.EqualTo(0);
			_rollbackCount.Should().Be.EqualTo(1);
			_connection.Verify(x=>x.Close(), Times.Once());
		}

		[Test]
		public void Should_rollback_when_disposing_and_no_calls_to_commit_or_rollback_made()
		{
			_target.Rollback();

			_commitCount.Should().Be.EqualTo(0);
			_rollbackCount.Should().Be.EqualTo(1);
			_connection.Verify(x=>x.Close(), Times.Once());
		}
	}
}