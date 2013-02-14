using System;
using System.ComponentModel;

namespace ADOUtils
{
	public static class DbFieldConversionUtils
	{
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