using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    public interface ISubscriber : IDisposable
    {

        INode Node { get; set; }

        string Name { get; set; }

        void Process(INode node, Message message);

        void Publish(Message message);

    }
}
