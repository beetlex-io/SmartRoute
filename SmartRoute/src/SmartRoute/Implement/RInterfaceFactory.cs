using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
	class RInterfaceFactory
	{
		private Dictionary<string, RInterfaceInfo> mInterfaces = new Dictionary<string, RInterfaceInfo>();

		public void Register(Type type, object impl)
		{
			string name = type.Name;
			if (mInterfaces.ContainsKey(name))
			{
				throw new SRException(string.Format("{0} interface has been registered", name));
			}
			mInterfaces[name] = new SmartRoute.RInterfaceInfo(type, impl);
		}


		public bool TryGet(string key, out RInterfaceInfo info)
		{
			return mInterfaces.TryGetValue(key, out info);
		}

		private static RInterfaceFactory mDefault = null;

		public static RInterfaceFactory Default
		{
			get
			{
				if (mDefault == null)
					mDefault = new RInterfaceFactory();
				return mDefault;
			}
		}
	}
}
