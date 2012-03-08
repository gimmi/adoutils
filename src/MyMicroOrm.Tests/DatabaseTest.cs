using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace MyMicroOrm.Tests
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
	}
}