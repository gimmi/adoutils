using System;
using System.ComponentModel;
using System.Data;

namespace MyMicroOrm
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

		public static T Convert<T>(object value, T def)
		{
			return Convert(value, () => def);
		}

		public static T Convert<T>(object value)
		{
			return Convert<T>(value, delegate {
				throw new NoNullAllowedException("Unexpected NULL value from database");
			});
		}

		private static T Convert<T>(object value, Func<T> defFn)
		{
			if (value == DBNull.Value)
			{
				return defFn();
			}
			Type conversionType = typeof(T);
			if(conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				if(value == null)
				{
					return Activator.CreateInstance<T>();
				}
				conversionType = new NullableConverter(conversionType).UnderlyingType;
			}
			return (T)System.Convert.ChangeType(value, conversionType);
		}
	}
}