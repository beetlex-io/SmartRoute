using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
namespace SmartRoute
{
	public class SwitchSubscriber : ISubscriber
	{

		private System.Threading.Timer mBroadcastServiceTimer = null;

		private string mSwitchName;

		public SwitchSubscriber(INode node, string name = "SMARTROUTE_SWITCH")
		{
			mSwitchName = name;
			Name = Guid.NewGuid().ToString("N") + "@" + name;
			Node = node;
			mDefaultEventSubscriber = node.Register<EventSubscriber>(Name);
			mDefaultEventSubscriber.Register<Protocols.SyncServiceInfo>(OnSyncService);
			mBroadcastServiceTimer = new System.Threading.Timer(OnBroadcastService, null, 1000, 1000);
		}

		private Dictionary<string, RemoteService> mRemoteServices = new Dictionary<string, RemoteService>();

		private List<ServiceEventSubscriber> mLocalServices = new List<ServiceEventSubscriber>();

		private EventSubscriber mDefaultEventSubscriber;

		private void OnSyncService(Message message, Protocols.SyncServiceInfo e)
		{
			bool create = false;
			foreach (Protocols.ServiceSubscriberItem item in e.Items)
			{
				RemoteService service = GetRemoteService(item.Service, out create);
				service.Register(item.Name, Node);
				if (create)
				{
					if (ServiceRegisted != null)
					{
						ServiceRegisted(this, service);
					}
				}
			}
		}

		private void OnBroadcastService(object state)
		{
			mBroadcastServiceTimer.Change(-1, -1);
			try
			{
				int count = mLocalServices.Count;
				if (count > 0)
				{
					Protocols.SyncServiceInfo services = new Protocols.SyncServiceInfo();
					for (int i = 0; i < count; i++)
						services.Items.Add(mLocalServices[i].GetInfo());
					string receive = @".+@" + mSwitchName;
					Message msg = new Message();
					msg.Data = services;
					msg.Pulisher = Name;
					msg.Consumers = receive;
					msg.Mode = ReceiveMode.Regex;
					Publish(msg);
				}
			}
			catch (Exception e_)
			{
				Node.Loger.Process(LogType.ERROR, "bradcast service error {0}", e_.Message);
			}
			finally
			{
				mBroadcastServiceTimer.Change(1000, 1000);
			}
		}

		private RemoteService GetRemoteService(string service, out bool create)
		{
			lock (mRemoteServices)
			{
				RemoteService result;
				if (mRemoteServices.ContainsKey(service))
				{
					result = mRemoteServices[service];
					create = false;
				}
				else
				{
					create = true;
					result = new RemoteService();
					result.ServiceName = service;
					mRemoteServices[service] = result;

				}
				return result;
			}

		}

		private RemoteServiceSubscriber GetRemoteServiceSubscriber(string service)
		{
			bool create = false;
			RemoteService remoteservice = GetRemoteService(service, out create);
			RemoteServiceSubscriber subscriber = GetRemoteServiceSubscriber(remoteservice);
			if (subscriber == null)
				throw new SRException("{0} service subscriber notfound!", service);
			return subscriber;
		}

		public string Name
		{
			get; set;
		}

		public INode Node
		{
			get; set;
		}

		public EventSubscriber GetService(string serviceName)
		{
			ServiceEventSubscriber item = mLocalServices.Find(p => p.Name == serviceName);
			if (item == null)
			{
				item = Node.Register<ServiceEventSubscriber>(Guid.NewGuid().ToString("N") + "@" + serviceName);
				item.Service = serviceName;
				mLocalServices.Add(item);
				Node.Loger.Process(LogType.INFO, "registed service {0}:{1}", serviceName, item.Name);
			}
			return item;
		}

		public object PublishToService(string service, object data)
		{
			RemoteServiceSubscriber subscriber = GetRemoteServiceSubscriber(service);

			Message msg = new Message();
			msg.Track("publish to server start!");
			msg.Data = data;
			msg.Consumers = subscriber.Name;
			msg.Mode = ReceiveMode.Eq;
			msg.Pulisher = Name;
			msg.Subscriber = this;
			object result = Node.PublishSync(msg);
			msg.EndTrack("publish to service completed!", Node);
			return result;
		}

		public T PublishToService<T>(string service, object data)
		{
			return (T)PublishToService(service, data);

		}

		public void PublishToServicee(string service, object data)
		{
			RemoteServiceSubscriber subscriber = GetRemoteServiceSubscriber(service);
			Message msg = new Message();
			msg.Data = data;
			msg.Consumers = subscriber.Name;
			msg.Pulisher = Name;
			Node.Publish(msg);
		}

		public void Dispose()
		{
			if (mBroadcastServiceTimer != null)
				mBroadcastServiceTimer = null;
		}

		public void Process(INode node, Message message)
		{

		}

		protected virtual RemoteServiceSubscriber GetRemoteServiceSubscriber(RemoteService remoteService)
		{
			return remoteService.GetServiceSubscriber(Node);
		}

		public void Publish(Message message)
		{
			Node.Publish(message);
		}

		public Events.EventServiceRegisted ServiceRegisted { get; set; }

		#region RMI
		public void Register<T>(object service)
		{
			Type type = typeof(T);
			if (!type.GetTypeInfo().IsInterface)
				throw new SRException("<T> not a interface");
			if (!(service is T))
				throw new SRException("service not implemented <T>!");
			RInterfaceFactory.Default.Register(type, service);
			GetService(type.Name).Register<Protocols.RMIMessage>(OnRMI);
		}
		public Object MethodInvoke(string name, string method, params object[] parameters)
		{
			Protocols.RMIMessage msg = new Protocols.RMIMessage();
			msg.Name = name;
			msg.Method = method;
			msg.Parameters = parameters;
			object result = PublishToService(name, msg);
			if (result is Protocols.Error)
			{
				throw new SRException(string.Format("Invoke {0}.{1} error {2}!", name, method, ((Protocols.Error)result).Message));
			}
			if (result is Protocols.Success)
				return null;
			return result;

		}
		public T MethodInvoke<T>(string name, string method, params object[] parameters)
		{
			return (T)MethodInvoke(name, method, parameters);
		}
		private void OnRMI(Message message, Protocols.RMIMessage rmimsg)
		{
			if (!string.IsNullOrEmpty(rmimsg.DeserializeError))
			{
				message.Reply(new Protocols.Error() { Message = rmimsg.DeserializeError });
			}
			else
			{
				RInterfaceInfo interfaceinfo = null;
				RMethodInfo methodinfo = null;
				RInterfaceFactory.Default.TryGet(rmimsg.Name, out interfaceinfo);
				interfaceinfo.TryGet(rmimsg.Method, out methodinfo);
				//Node.Loger.Process(LogType.DEBUG, "{0}.{1} invoke", rmimsg.Name, rmimsg.Method);
				RMethodResult result = methodinfo.Invoke(rmimsg.Parameters);

				if (result.Exception != null)
				{
					Protocols.Error error = new Protocols.Error() { Message = result.Exception.Message, StackTrace = result.Exception.StackTrace };
					message.Reply(error);
					//Node.Loger.Process(LogType.DEBUG, "{0}.{1} invoke error {2}", rmimsg.Name, rmimsg.Method, result.Exception.Message);
				}
				else
				{
					if (result.IsVoid || result.Value == null)
						message.Reply(new Protocols.Success());
					else
						message.Reply(result.Value);
					//Node.Loger.Process(LogType.DEBUG, "{0}.{1} invoke completed!", rmimsg.Name, rmimsg.Method);
				}
			}
		}
		#endregion
	}
}
