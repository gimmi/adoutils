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

		public static T Convert<T>(object value)
		{
			if (value == DBNull.Value || value == null)
			{
				return default(T);
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