using System;
using System.Collections.Generic;

namespace ADOUtils
{
	[Obsolete("Use System.Collections.Concurrent.ConcurrentDictionary instead, with GetOrAdd method.")]
	public class IdentityMap<T>
	{
		private readonly Dictionary<object, T> _map = new Dictionary<object, T>();

		public T GetOrBuild(object id, Func<T> buildFn)
		{
			if(!_map.ContainsKey(id))
			{
				T built = buildFn.Invoke();
				if(!_map.ContainsKey(id))
				{
					_map.Add(id, built);
				}
			}
			return _map[id];
		}
	}
}