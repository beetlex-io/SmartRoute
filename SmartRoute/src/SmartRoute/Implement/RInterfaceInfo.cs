using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
namespace SmartRoute
{
	class RInterfaceInfo
	{

		public RInterfaceInfo(Type itype, object impl)
		{
			this.Target = impl;
			this.Type = itype;
			LoadMethodInfo();
		}

		private void LoadMethodInfo()
		{
			foreach (System.Reflection.MethodInfo m in this.Type.GetRuntimeMethods())
			{
				RMethodInfo method = new RMethodInfo(m, this.Target);
				method.Target = Target;
				if (mMethods.ContainsKey(method.Name))
				{
					throw new SRException(string.Format("{0}.{1} has been registered", Type.Name, m.Name));
				}
				mMethods[method.Name] = method;
			}
		}

		public Type Type { get; set; }

		private Dictionary<string, RMethodInfo> mMethods = new Dictionary<string, RMethodInfo>();

		public string Name { get; set; }

		public object Target { get; set; }

		public bool TryGet(string name, out RMethodInfo result)
		{
			return mMethods.TryGetValue(name, out result);

		}
	}
}
