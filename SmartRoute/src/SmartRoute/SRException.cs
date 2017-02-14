using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    public class SRException : Exception
    {
        public SRException()
        {
            
        }
        public SRException(string message) : base(message) { }

        public SRException(string message, params object[] parameters) : base(string.Format(message, parameters)) { }

        public SRException(string message, Exception baseError) : base(message, baseError) { }

        public SRException(Exception baseError, string message, params object[] parameters) : base(string.Format(message, parameters), baseError) { }
    }
}
