using System;
using System.ComponentModel;
using System.Data;

namespace ADOUtils
{
	public static class DbFieldConversionUtils
	{
		public static T Get<T>(this IDataRecord record, string name)
		{
			return Convert<T>(record[name]);
		}

		public static T Get<T>(this IDataRecord record, string name, T def)
		{
			return Convert(record[name], def);
		}

		public static T Require<T>(this IDataRecord record, string name)
		{
			return Convert<T>(record[name], () => {
				throw new NoNullAllowedException(string.Concat("Unexpected NULL value for field ", name));
			});
		}

		public static T Convert<T>(object value)
		{
			return Convert(value, default(T));
		}

		public static T Convert<T>(object value, T def)
		{
			return Convert(value, () => def);
		}

		public static T Convert<T>(object value, Func<T> defFn)
		{
			if (value == DBNull.Value || value == null)
			{
				return defFn.Invoke();
			}
			Type type = typeof (T);
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
			{
				type = new NullableConverter(type).UnderlyingType;
			}
			return (T) System.Convert.ChangeType(value, type);
		}
	}
}