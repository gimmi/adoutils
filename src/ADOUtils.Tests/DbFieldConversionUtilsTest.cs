using System;
using System.Data;
using Moq;
using NUnit.Framework;
using SharpTestsEx;

namespace ADOUtils.Tests
{
	[TestFixture]
	public class DbFieldConversionUtilsTest
	{
		[Test]
		public void Should_convert_fields_from_datarecord()
		{
			var mockRec = new Mock<IDataRecord>();
			mockRec.Setup(x => x["string"]).Returns("string");
			mockRec.Setup(x => x["int"]).Returns(123);
			mockRec.Setup(x => x["guid"]).Returns(new Guid("b8903b30-0862-4440-8058-ac721b1a2eda"));
			mockRec.Setup(x => x["nullguid"]).Returns(DBNull.Value);

			IDataRecord rec = mockRec.Object;
			rec.Get<string>("string").Should().Be.EqualTo("string");
			rec.Get<int>("int").Should().Be.EqualTo(123);
			rec.Get<int?>("int").Should().Be.EqualTo(123);
			rec.Get<Guid>("guid").Should().Be.EqualTo(new Guid("b8903b30-0862-4440-8058-ac721b1a2eda"));
			rec.Get<Guid?>("guid").Should().Be.EqualTo(new Guid("b8903b30-0862-4440-8058-ac721b1a2eda"));
			rec.Get<Guid?>("nullguid", null).Should().Not.Have.Value();
			Executing.This(() => rec.Get<Guid>("nullguid")).Should().Throw<NoNullAllowedException>();
		}

		[Test]
		public void Should_convert_values_from_db()
		{
			DbFieldConversionUtils.Convert<string>("string").Should().Be.EqualTo("string");
			DbFieldConversionUtils.Convert(DBNull.Value, "def").Should().Be.EqualTo("def");
			DbFieldConversionUtils.Convert<int>(123).Should().Be.EqualTo(123);
			DbFieldConversionUtils.Convert<int?>(123).Should().Be.EqualTo(123);
			DbFieldConversionUtils.Convert<Guid>(new Guid("b8903b30-0862-4440-8058-ac721b1a2eda")).Should().Be.EqualTo(new Guid("b8903b30-0862-4440-8058-ac721b1a2eda"));
			DbFieldConversionUtils.Convert<Guid?>(new Guid("b8903b30-0862-4440-8058-ac721b1a2eda")).Should().Be.EqualTo(new Guid("b8903b30-0862-4440-8058-ac721b1a2eda"));
			DbFieldConversionUtils.Convert<Guid?>(DBNull.Value, null).Should().Not.Have.Value();
			Executing.This(() => DbFieldConversionUtils.Convert<Guid>(DBNull.Value)).Should().Throw<NoNullAllowedException>();
		}

		[Test]
		public void Should_treat_plain_null_as_a_valid_value()
		{
			DbFieldConversionUtils.Convert<string>(null).Should().Be.Null();
			DbFieldConversionUtils.Convert(null, "def").Should().Be.Null();
			Executing.This(() => DbFieldConversionUtils.Convert(null, 123)).Should().Throw<InvalidCastException>();
			DbFieldConversionUtils.Convert<int?>(null).Should().Not.Have.Value();
			DbFieldConversionUtils.Convert<int?>(null, 123).Should().Not.Have.Value();
			DbFieldConversionUtils.Convert<Guid?>(null).Should().Not.Have.Value();
		}
	}
}