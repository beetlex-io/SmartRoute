using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    interface INodeBroadcastListen
    {
        ILogHandler Loger { get; set; }

        void Open();

        void Remove(string node);

        EventHandler<NodeBroadcastListen.DiscoverEventArgs> Discover
        {
            get;
            set;
        }
    }
}
