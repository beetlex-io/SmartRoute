using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeetleX.Buffers;
using BeetleX;
using System.Reflection;

namespace SmartRoute.Protocols
{
    public interface IMessageTypeHandler
    {
        Type ReadType(BeetleX.Buffers.IBinaryReader reader);

        void WriteType(object data, BeetleX.Buffers.IBinaryWriter writer);

        MessageType GetMessageType(string typeName);

        MessageType GetMessageType(Type type);
    }

    public class MessageType
    {

        public MessageType(string typeName)
        {
            Type = Type.GetType(typeName);
            if (Type == null)
                throw new SRException("{0} type not found!", typeName);
            Init();
        }

        public MessageType(Type type)
        {
            Type = type;
            Init();
        }

        private void Init()
        {
            TypeInfo info = Type.GetTypeInfo();
            if (info.FullName.IndexOf("System") >= 0)
                TypeName = info.FullName;
            else
                TypeName = string.Format("{0},{1}", info.FullName, info.Assembly.GetName().Name);
            Type[] interfaces = Type.GetInterfaces();
            IsCustomSerializer = interfaces.Contains(typeof(ISerializer));

        }

        public Type Type { get; private set; }

        public bool IsCustomSerializer { get; private set; }

        public string TypeName { get; set; }

        public ISerializer Create()
        {
            return (ISerializer)Activator.CreateInstance(Type);
        }
    }


    public class MessageTypeHandler : IMessageTypeHandler
    {
        private System.Collections.Concurrent.ConcurrentDictionary<Type, MessageType> mTypeNames = new System.Collections.Concurrent.ConcurrentDictionary<Type, MessageType>();

        private System.Collections.Concurrent.ConcurrentDictionary<string, MessageType> mNameTypes = new System.Collections.Concurrent.ConcurrentDictionary<string, MessageType>();

        public MessageType GetMessageType(Type type)
        {
            MessageType result;
            if (!mTypeNames.TryGetValue(type, out result))
            {
                result = new MessageType(type);
                mTypeNames[type] = result;
            }
            return result;
        }

        public MessageType GetMessageType(string typeName)
        {
            MessageType result;
            if (!mNameTypes.TryGetValue(typeName, out result))
            {
                result = new MessageType(typeName);
                mNameTypes[typeName] = result;
            }
            return result;
        }

        public Type ReadType(IBinaryReader reader)
        {
            string typeName = reader.ReadLine();
            return GetMessageType(typeName).Type;
        }


        public void WriteType(object data, IBinaryWriter writer)
        {

            string name = GetMessageType(data.GetType()).TypeName;
            writer.WriteLine(name);
        }


    }

}
