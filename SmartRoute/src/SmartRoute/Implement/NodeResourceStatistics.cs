using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
	public class NodeResourceStatistics
	{
		const string NETWORK_SENDIO = "NetworkSendIO";

		const string NETWORK_SENDBYTES = "NetworkSendBytes";

		const string NETWORK_RECEIVEIO = "NetworkReceiveIO";

		const string NETWORK_RECEIVEBYTES = "NetworkReceiveBytes";

		const string NETWORK_SENDIO_S = "NetworkSendIO/s";

		const string NETWORK_SENDBYTES_S = "NetworkSendBytes/s";

		const string NETWORK_RECEIVEIO_S = "NetworkReceiveIO/s";

		const string NETWORK_RECEIVEBYTES_S = "NetworkReceiveBytes/s";

		const string PROCESSED_MESSAGES = "ProcessedMessages";

		const string PROCESSING_MESSAGE = "ProcessingMessages";

		public Double NetworkSendIO
		{
			get { return this[NETWORK_SENDIO]; }
			set { this[NETWORK_SENDIO] = value; }
		}

		public Double NetworkSendIOPersecond
		{
			get { return this[NETWORK_SENDIO_S]; }
			set { this[NETWORK_SENDIO_S] = value; }
		}


		public Double NetworkSendBytes
		{
			get { return this[NETWORK_SENDBYTES]; }
			set { this[NETWORK_SENDBYTES] = value; }
		}

		public Double NetworkSendBytesPersecond
		{
			get { return this[NETWORK_SENDBYTES_S]; }
			set { this[NETWORK_SENDBYTES_S] = value; }
		}

		public Double NetworkReceiveIO
		{
			get { return this[NETWORK_RECEIVEIO]; }
			set { this[NETWORK_RECEIVEIO] = value; }
		}

		public Double NetworkReceiveBytes
		{
			get { return this[NETWORK_RECEIVEBYTES]; }
			set { this[NETWORK_RECEIVEBYTES] = value; }
		}

		public Double NetworkReceiveIOPersecond
		{
			get { return this[NETWORK_RECEIVEIO_S]; }
			set { this[NETWORK_RECEIVEIO_S] = value; }
		}

		public Double NetworkReceiveBytesPersecond
		{
			get { return this[NETWORK_RECEIVEBYTES_S]; }
			set { this[NETWORK_RECEIVEBYTES_S] = value; }
		}

		public double ProcessingMessages { get { return this[PROCESSING_MESSAGE]; } set { this[PROCESSING_MESSAGE] = value; } }

		public double ProcessedMessages { get { return this[PROCESSED_MESSAGES]; } set { this[PROCESSED_MESSAGES] = value; } }

		private Dictionary<string, Double> mProperties = new Dictionary<string, double>();

		public double this[string key]
		{
			get
			{
				double result;
				mProperties.TryGetValue(key, out result);
				return result;
			}
			set
			{
				mProperties[key] = value;
			}
		}

		public void FromProperties(Dictionary<string, double> properties)
		{
			if (properties != null)
				foreach (string key in properties.Keys)
					mProperties[key] = properties[key];
		}

		public Dictionary<string, double> ToProperties()
		{
			Dictionary<string, double> result = new Dictionary<string, double>();
			foreach (string key in mProperties.Keys)
			{
				result[key] = mProperties[key];
			}
			return result;
		}

		public long LastTime { get; set; }



	}
}
