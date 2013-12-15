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
			mockRec.Setup(x => x["dbnull"]).Returns(DBNull.Value);
			mockRec.Setup(x => x["null"]).Returns(null);

			IDataRecord rec = mockRec.Object;
			rec.Get<string>("string").Should().Be.EqualTo("string");
			rec.Get<int>("int").Should().Be.EqualTo(123);
			rec.Get<Guid>("guid").Should().Be.EqualTo(new Guid("b8903b30-0862-4440-8058-ac721b1a2eda"));
			Executing.This(() => rec.Require<string>("null")).Should().Throw<NoNullAllowedException>();
		}

		[Test]
		public void Should_convert_string_values_from_db()
		{
			DbFieldConversionUtils.Convert<string>("string").Should().Be.EqualTo("string");
			DbFieldConversionUtils.Convert<string>(DBNull.Value).Should().Be.Null();
			DbFieldConversionUtils.Convert<string>(null).Should().Be.Null();
			DbFieldConversionUtils.Convert<string>("").Should().Be.EqualTo("");
			DbFieldConversionUtils.Convert<string>("  ").Should().Be.EqualTo("  ");
		}

		[Test]
		public void Should_convert_int_values_from_db()
		{
			DbFieldConversionUtils.Convert<int>(123).Should().Be.EqualTo(123);
			DbFieldConversionUtils.Convert<int>(0).Should().Be.EqualTo(0);
			DbFieldConversionUtils.Convert<int>(DBNull.Value).Should().Be.EqualTo(0);
			DbFieldConversionUtils.Convert<int>(null).Should().Be.EqualTo(0);
		}

		[Test]
		public void Should_convert_nullable_int_values_from_db()
		{
			DbFieldConversionUtils.Convert<int?>(123).Should().Be.EqualTo(123);
			DbFieldConversionUtils.Convert<int?>(0).Should().Be.EqualTo(0);
			DbFieldConversionUtils.Convert<int?>(DBNull.Value).Should().Not.Have.Value();
			DbFieldConversionUtils.Convert<int?>(null).Should().Not.Have.Value();
		}

		[Test]
		public void Should_convert_guid_values_from_db()
		{
			var guid = new Guid("b8903b30-0862-4440-8058-ac721b1a2eda");
			DbFieldConversionUtils.Convert<Guid>(guid).Should().Be.EqualTo(guid);
			DbFieldConversionUtils.Convert<Guid>(Guid.Empty).Should().Be.EqualTo(Guid.Empty);
			DbFieldConversionUtils.Convert<Guid>(DBNull.Value).Should().Be.EqualTo(Guid.Empty);
			DbFieldConversionUtils.Convert<Guid>(null).Should().Be.EqualTo(Guid.Empty);
		}

		[Test]
		public void Should_convert_nullable_guid_values_from_db()
		{
			var guid = new Guid("b8903b30-0862-4440-8058-ac721b1a2eda");
			DbFieldConversionUtils.Convert<Guid?>(guid).Should().Be.EqualTo(guid);
			DbFieldConversionUtils.Convert<Guid?>(Guid.Empty).Should().Be.EqualTo(Guid.Empty);
			DbFieldConversionUtils.Convert<Guid?>(DBNull.Value).Should().Not.Have.Value();
			DbFieldConversionUtils.Convert<Guid?>(null).Should().Not.Have.Value();
		}

		[Test]
		public void Should_convert_between_different_numeric_types()
		{
			DbFieldConversionUtils.Convert<Int32>(3.14).Should().Be.EqualTo(3);
			DbFieldConversionUtils.Convert<Int32>(3.55).Should().Be.EqualTo(4);
			DbFieldConversionUtils.Convert<Int32>((UInt16)5).Should().Be.EqualTo(5);
			DbFieldConversionUtils.Convert<Double>(5).Should().Be.EqualTo(5);
		}

		[Test]
		public void Should_convert_char()
		{
			DbFieldConversionUtils.Convert<char>(DBNull.Value).Should().Be.EqualTo('\0');
			DbFieldConversionUtils.Convert<char>(null).Should().Be.EqualTo('\0');
			DbFieldConversionUtils.Convert<char>('A').Should().Be.EqualTo('A');
			DbFieldConversionUtils.Convert<char>("A").Should().Be.EqualTo('A');
			DbFieldConversionUtils.Convert<char>(0).Should().Be.EqualTo('\0');
			Executing.This(() => DbFieldConversionUtils.Convert<char>("AA")).Should().Throw<FormatException>()
				.And.Exception.Message.Should().Contain("String must be exactly one character long.");
			Executing.This(() => DbFieldConversionUtils.Convert<char>("")).Should().Throw<FormatException>()
				.And.Exception.Message.Should().Contain("String must be exactly one character long.");

			DbFieldConversionUtils.Convert<char?>(DBNull.Value).Should().Not.Have.Value();
			DbFieldConversionUtils.Convert<char?>(null).Should().Not.Have.Value();
			DbFieldConversionUtils.Convert<char?>("A").Should().Be.EqualTo('A');
		}
	}
}