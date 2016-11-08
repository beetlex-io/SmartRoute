using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    public class EventSubscriber : ISubscriber
    {

        private Dictionary<Type, IMessageHandler> mHandlers = new Dictionary<Type, IMessageHandler>();

        public string Name
        {
            get; set;
        }

        public INode Node
        {
            get; set;
        }

        public void Dispose()
        {
            Node = null;
        }


        interface IMessageHandler
        {
            void Invoke(Message message);
        }


        class MessageHandler<T> : IMessageHandler
        {
            public Action<Message, T> Handler { get; set; }

            public void Invoke(Message message)
            {
                if (Handler != null)
                {
                    Handler(message, (T)message.Data);
                }
            }
        }


        public void Process(INode node, Message message)
        {
            message.Subscriber = this;
            object[] state = new object[] { node, message };
            if (message.IsLocal)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(OnProcess, state);
            }
            else
            {
                OnProcess(state);
            }
        }

        public void Register<T>(Action<Message, T> handler)
        {
            MessageHandler<T> item = new MessageHandler<T>();
            item.Handler = handler;
            mHandlers[typeof(T)] = item;
        }

        private void OnProcess(object state)
        {

            INode node = (INode)((object[])state)[0];
            Message message = (Message)((object[])state)[1];
            message.Track("subscriber process message");
            try
            {
                Type key = message.Data.GetType();
                IMessageHandler handler;
                if (mHandlers.TryGetValue(key, out handler))
                {
                    message.Track("subscriber process message DynamicInvoke");
                    handler.Invoke(message);
                }
                else
                {
                    string error = string.Format("{0} message({1}) handler notfound!", Name, message);
                    message.ProcessError(new SRException(error));
                    Node.Loger.Process(LogType.ERROR, error);
                }

            }
            catch (Exception e_)
            {
                string error = string.Format("{0} process message({1}) error {2}", Name, message, e_.Message);
                message.ProcessError(new SRException(error, e_));
                Node.Loger.Process(LogType.ERROR, error);
            }
            message.EndTrack("subscriber process message completed");
        }

        public void Publish(Message message)
        {
            Node.Publish(message);
        }

        public void Publish(string consumer, object data, ReceiveMode mode = ReceiveMode.Eq)
        {
            Message msg = new Message();
            msg.Pulisher = this.Name;
            msg.Consumers = consumer;
            msg.Data = data;
            Publish(msg);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
