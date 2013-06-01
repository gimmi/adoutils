using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace ADOUtils.Tests
{
	[TestFixture]
	public class OdbcProviderTest
	{
		private Database _target;

		[SetUp]
		public void SetUp()
		{
			TestUtils.CreateTestDb();

			_target = new Database(TestUtils.OdbcConnStr, DbProviderFactories.GetFactory("System.Data.Odbc"), null);
		}

		[Test]
		public void Should_query_with_parameters()
		{
			var actual = _target.Query("SELECT IntValue = @IntValue, StringValue = @StringValue, DateValue = @DateValue, GuidValue = @GuidValue", new {
				StringValue = "456",
				IntValue = 123,
				DateValue = new DateTime(2013, 6, 1),
				GuidValue = new Guid("6F8E7DAD-87F6-46D0-A1E8-177AED5FA3F4")
			}).AsDisconnected().Single();

			actual.Get<int>("IntValue").Should().Be.EqualTo(123);
			actual.Get<string>("StringValue").Should().Be.EqualTo("456");
			actual.Get<DateTime>("DateValue").Should().Be.EqualTo(new DateTime(2013, 6, 1));
			actual.Get<Guid>("GuidValue").Should().Be.EqualTo(new Guid("6F8E7DAD-87F6-46D0-A1E8-177AED5FA3F4"));
		}
	}
}