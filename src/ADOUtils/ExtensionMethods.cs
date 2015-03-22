using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

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

		public static IEnumerable<IDataRecord> AsDisconnected(this IEnumerable<IDataRecord> records)
		{
			return records.Select(DisconnectedDataRecord.Build);
		}

		public static void AddPars(this IDbCommand cmd, object parameters)
		{
			var kvps = parameters as IDictionary<string, object> ?? TypeDescriptor.GetProperties(parameters)
				.Cast<PropertyDescriptor>()
				.ToDictionary(p => p.Name, p => p.GetValue(parameters));
			foreach (var kvp in kvps)
			{
				IDataParameter par;
				if (kvp.Value is IDataParameter)
				{
					par = kvp.Value as IDataParameter;
				}
				else
				{
					par = cmd.CreateParameter();
					par.Value = kvp.Value ?? DBNull.Value;
				}
				par.ParameterName = kvp.Key;
				cmd.Parameters.Add(par);
			}
		}
	}
}