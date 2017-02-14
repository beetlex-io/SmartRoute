using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace SmartRoute
{
    class RMethodInfo
    {

        public RMethodInfo(System.Reflection.MethodInfo method, object target)
        {
            Target = target;
            Parameters = new List<RParameterInfo>();
            Method = method;
            Name = method.Name;
            foreach (System.Reflection.ParameterInfo p in method.GetParameters())
            {
                Parameters.Add(new RParameterInfo(p));
            }

        }

        public System.Reflection.MethodInfo Method { get; set; }

        public string Name { get; set; }

        public IList<RParameterInfo> Parameters
        {
            get;
            private set;
        }

        public object Target { get; set; }

        public bool IsVoid { get; set; }

        public RMethodResult Invoke(object[] parameters)
        {
            RMethodResult result = new SmartRoute.RMethodResult();
            result.IsVoid = IsVoid;
            try
            {
                result.Value = Method.Invoke(Target, parameters);
            }
            catch (Exception e_)
            {
                result.Exception = e_;
            }
            return result;
        }
    }
}
