using System;
using System.Collections.Generic;
using System.Data;

namespace MyMicroOrm
{
	internal class CachedDataRecord : IDataRecord
	{
		private readonly IDictionary<string, object> _row;

		public CachedDataRecord(IDictionary<string, object> row)
		{
			_row = row;
		}

		object IDataRecord.this[string name]
		{
			get { return _row[name]; }
		}

		#region unimplemented

		public int FieldCount
		{
			get { throw new NotImplementedException(); }
		}

		object IDataRecord.this[int i]
		{
			get { throw new NotImplementedException(); }
		}

		public string GetName(int i)
		{
			throw new NotImplementedException();
		}

		public string GetDataTypeName(int i)
		{
			throw new NotImplementedException();
		}

		public Type GetFieldType(int i)
		{
			throw new NotImplementedException();
		}

		public object GetValue(int i)
		{
			throw new NotImplementedException();
		}

		public int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		public int GetOrdinal(string name)
		{
			throw new NotImplementedException();
		}

		public bool GetBoolean(int i)
		{
			throw new NotImplementedException();
		}

		public byte GetByte(int i)
		{
			throw new NotImplementedException();
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public char GetChar(int i)
		{
			throw new NotImplementedException();
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public Guid GetGuid(int i)
		{
			throw new NotImplementedException();
		}

		public short GetInt16(int i)
		{
			throw new NotImplementedException();
		}

		public int GetInt32(int i)
		{
			throw new NotImplementedException();
		}

		public long GetInt64(int i)
		{
			throw new NotImplementedException();
		}

		public float GetFloat(int i)
		{
			throw new NotImplementedException();
		}

		public double GetDouble(int i)
		{
			throw new NotImplementedException();
		}

		public string GetString(int i)
		{
			throw new NotImplementedException();
		}

		public decimal GetDecimal(int i)
		{
			throw new NotImplementedException();
		}

		public DateTime GetDateTime(int i)
		{
			throw new NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		public bool IsDBNull(int i)
		{
			throw new NotImplementedException();
		}

		#endregion

		public static IDataRecord Build(IDataRecord rec)
		{
			var row = new Dictionary<string, object>(rec.FieldCount, StringComparer.InvariantCultureIgnoreCase);
			for(int i = 0; i < rec.FieldCount; i++)
			{
				var name = rec.GetName(i);
				if(!row.ContainsKey(name))
				{
					row.Add(name, rec[i]);
				}
			}
			return new CachedDataRecord(row);
		}
	}
}