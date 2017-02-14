using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    class RParameterInfo
    {
        public RParameterInfo(System.Reflection.ParameterInfo info)
        {
            ParameterInfo = info;
            Type = info.ParameterType;
        }
        public System.Reflection.ParameterInfo ParameterInfo
        { get; set; }

        public Type Type { get; set; }
    }
}
