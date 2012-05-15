using NUnit.Framework;
using SharpTestsEx;

namespace ADOUtils.Tests
{
	[TestFixture]
	public class IdentityMapTest
	{
		private IdentityMap<TestEntity> _target;

		[SetUp]
		public void SetUp()
		{
			_target = new IdentityMap<TestEntity>();
		}

		[Test]
		public void Should_return_the_same_instance_given_the_same_id()
		{
			var entity1 = new TestEntity();

			_target.GetOrBuild(1, () => entity1).Should().Be.SameInstanceAs(entity1);
			_target.GetOrBuild(1, delegate {
				Assert.Fail();
				return null;
			}).Should().Be.SameInstanceAs(entity1);
		}

		[Test]
		public void Should_return_the_ame_instance_in_recursive_construction()
		{
			var entity1 = new TestEntity();
			var entity2 = new TestEntity();

			_target.GetOrBuild(1, delegate {
				_target.GetOrBuild(1, () => entity2);
				return entity1;
			}).Should().Be.SameInstanceAs(entity2);
		}

		private class TestEntity {}
	}
}