using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
	public class ClusterInfo
	{

		public ClusterInfo(string name)
		{
			Nodes = new List<NodeInfo>();
			Name = name;
		}

		public string Name { get; set; }


		public IList<NodeInfo> Nodes
		{
			get;
			private set;
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendFormat("*{0} Cluster Info\r\n", Name);
			foreach (NodeInfo node in Nodes)
			{
				sb.AppendFormat("*-[{0}]{1}\r\n", node.IsLocal ? "Local" : "Remote", node.ID);
				Dictionary<string, Double> stati = node.Statistics.ToProperties();
				foreach (string key in stati.Keys)
				{
					sb.AppendFormat("*----{0}\t\t:{1}\r\n", key, stati[key]);
				}
			}
			return sb.ToString();
		}

	}
	public class NodeInfo
	{
		public NodeInfo(string id, bool islocal)
		{
			ID = id;
			this.IsLocal = islocal;
		}

		public string ID { get; set; }

		public bool IsLocal { get; set; }

		public NodeResourceStatistics Statistics
		{
			get;
			set;
		}
	}
}
