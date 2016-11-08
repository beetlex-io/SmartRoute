using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    public class NodeFactory
    {
        public static INode Default
        {
            get
            {
                return Node.Default;
            }
        }
        public static INode CreateNode(string cluster, string tokenKey)
        {
            Node node = new SmartRoute.Node();
            node.Cluster = cluster;
            node.TokenKey = tokenKey;
            return node;
        }
    }
}
