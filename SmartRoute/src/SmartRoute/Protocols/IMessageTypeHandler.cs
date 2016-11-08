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

        string GetTypeName(Type type);

        Type GetType(string typeName);
    }

    public class MessageTypeHandler : IMessageTypeHandler
    {
        private System.Collections.Concurrent.ConcurrentDictionary<Type, string> mTypeNames = new System.Collections.Concurrent.ConcurrentDictionary<Type, string>();

        private System.Collections.Concurrent.ConcurrentDictionary<string, Type> mNameTypes = new System.Collections.Concurrent.ConcurrentDictionary<string, Type>();

        public Type GetType(string typeName)
        {
            Type result;
            if (!mNameTypes.TryGetValue(typeName, out result))
            {
                if (typeName == null)
                    throw new SRException("{0} type not found!", typeName);
                result = Type.GetType(typeName);
                if (result == null)
                    throw new SRException("{0} type not found!", typeName);

                mNameTypes[typeName] = result;
            }
            return result;
        }

        public Type ReadType(IBinaryReader reader)
        {
            string typeName = reader.ReadLine();
            return GetType(typeName);
        }

        public string GetTypeName(Type type)
        {
            string result;
            if (!mTypeNames.TryGetValue(type, out result))
            {
                TypeInfo info = type.GetTypeInfo();
                if (info.Name.IndexOf("System") >= 0)
                    result = info.FullName;
                else
                    result = string.Format("{0},{1}", info.FullName, info.Assembly.GetName().Name);
                mTypeNames[type] = result;
            }
            return result;
        }

        public void WriteType(object data, IBinaryWriter writer)
        {

            string name = GetTypeName(data.GetType());
            writer.WriteLine(name);
        }
    }

}
