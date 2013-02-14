using System.Data;

namespace ADOUtils
{
	public static class ExtensionMethods
	{
		public static T Get<T>(this IDataRecord record, string name)
		{
			return DbFieldConversionUtils.Convert<T>(record[name]);
		}

		public static T Get<T>(this IDataRecord record, string name, T def)
		{
			return DbFieldConversionUtils.Convert(record[name], def);
		}

		public static T Require<T>(this IDataRecord record, string name)
		{
			return DbFieldConversionUtils.Convert<T>(record[name], () => { throw new NoNullAllowedException(string.Concat("Unexpected NULL value for field ", name)); });
		}
	}
}