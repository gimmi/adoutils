using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace ADOUtils.Tests
{
    public class SQLiteTest
    {
        private Database _target;

        private SQLiteConnection _inMemoryConnectionRetainer;
        public const string TblScript = @"
            CREATE TABLE Tbl(NumValue numeric, StringValue text);
            INSERT INTO Tbl(NumValue, StringValue) VALUES(1, 'string 1');
            INSERT INTO Tbl(NumValue, StringValue) VALUES(2, 'string 2');
        ";

        [SetUp]
        public void SetUp()
        {
            var connStr = $"FullUri=file:{Guid.NewGuid():N}?mode=memory&cache=shared";
            _inMemoryConnectionRetainer = new SQLiteConnection(connStr).OpenAndReturn();
            new SQLiteCommand(TblScript, _inMemoryConnectionRetainer).ExecuteNonQuery();

            _target = new Database(connStr, SQLiteFactory.Instance);
        }

        [Test]
        public void Should_query()
        {
            _target.Query("SELECT StringValue FROM Tbl WHERE NumValue = @NumValue", new Dictionary<string, object> { { "NumValue", 2 } }).Select(x => x["StringValue"]).First().Should().Be.EqualTo("string 2");
            _target.Query("SELECT StringValue FROM Tbl WHERE NumValue = @NumValue", new { NumValue = 2 }).Select(x => x["StringValue"]).First().Should().Be.EqualTo("string 2");
            _target.Query("SELECT StringValue FROM Tbl WHERE NumValue = 2").Select(x => x["StringValue"]).First().Should().Be.EqualTo("string 2");
        }

        [Test]
        public void Should_use_dictionary_values_as_parameters()
        {
            IDictionary<string, object> parameters = new Dictionary<string, object> { { "NumValue", 2 } };
            _target.Query("SELECT StringValue FROM Tbl WHERE NumValue = @NumValue", parameters).Select(x => x["StringValue"]).First().Should().Be.EqualTo("string 2");
        }

        [Test]
        public void Should_tolerate_null_parameters()
        {
            _target.Query("SELECT StringValue FROM Tbl WHERE NumValue = 2").Select(x => x["StringValue"]).First().Should().Be.EqualTo("string 2");
        }

        [Test]
        public void Should_query_scalar()
        {
            _target.Scalar<int>("SELECT COUNT(*) FROM Tbl").Should().Be.EqualTo(2);
            _target.Scalar<int>("SELECT COUNT(*) FROM Tbl WHERE NumValue = @NumValue", new Dictionary<string, object> { { "NumValue", 2 } }).Should().Be.EqualTo(1);
            _target.Scalar<int>("SELECT COUNT(*) FROM Tbl WHERE NumValue = @NumValue", new { NumValue = 2 }).Should().Be.EqualTo(1);
        }

        [Test]
        public void Should_return_default_value_for_type_when_scalar_return_no_rows()
        {
            _target.Scalar<int>("SELECT NumValue FROM Tbl WHERE 1 = 2").Should().Be.EqualTo(0);
            _target.Scalar<string>("SELECT StringValue FROM Tbl WHERE 1 = 2").Should().Be.Null();
            _target.Scalar<int?>("SELECT NumValue FROM Tbl WHERE 1 = 2").Should().Not.Have.Value();
        }

        [TearDown]
        public void TearDown()
        {
            _inMemoryConnectionRetainer.Close();
        }

		[Test]
		public void Should_exec()
		{
			_target.Exec("UPDATE Tbl SET StringValue = StringValue").Should().Be.EqualTo(2);
			_target.Exec("UPDATE Tbl SET StringValue = StringValue WHERE 1 = @par", new Dictionary<string, object> { { "par", 2 } }).Should().Be.EqualTo(0);
			_target.Exec("UPDATE Tbl SET StringValue = StringValue WHERE 1 = @par", new { par = 2 }).Should().Be.EqualTo(0);
		}
    }
}
