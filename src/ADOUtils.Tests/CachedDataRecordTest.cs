using System.Data;
using NUnit.Framework;
using System.Linq;
using SharpTestsEx;

namespace ADOUtils.Tests
{
	public class CachedDataRecordTest
	{
		private IDataRecord _target;

		[SetUp]
		public void SetUp()
		{
			TestUtils.CreateTestDb();
			var db = new Database(TestUtils.ConnStr);
			using (db.OpenConnection())
			{
				_target = db.Yield("SELECT 1 AS Id, 'row 1' AS Name, 'row 1 2' AS Name").Select(CachedDataRecord.Build).First();
			}
		}

		[Test]
		public void Should_keep_only_the_first_value_with_multiple_column_with_the_same_name()
		{
			_target["Name"].Should().Be.EqualTo("row 1");
		}

		[Test]
		public void Should_treat_column_name_as_case_insensitive()
		{
			_target["Id"].Should().Be.EqualTo(1);
			_target["ID"].Should().Be.EqualTo(1);
			_target["id"].Should().Be.EqualTo(1);
		}
	}
}