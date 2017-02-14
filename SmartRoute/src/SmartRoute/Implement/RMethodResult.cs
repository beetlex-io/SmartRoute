using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    class RMethodResult
    {
        public bool IsVoid { get; set; }

        public object Value { get; set; }

        public Exception Exception { get; set; }
    }
}
