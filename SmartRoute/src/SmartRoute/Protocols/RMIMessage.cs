using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeetleX.Buffers;

namespace SmartRoute.Protocols
{
    class RMIMessage : ISerializer
    {
        public string Name { get; set; }

        public string Method { get; set; }

        public string DeserializeError { get; set; }

        public object[] Parameters { get; set; }

        public void Deserialize(IBinaryReader reader)
        {
            Name = reader.ReadShortUTF();
            Method = reader.ReadShortUTF();
            int count = reader.ReadInt16();
            RInterfaceInfo interfaceinfo = null;
            RMethodInfo methodinfo = null;
            if (RInterfaceFactory.Default.TryGet(Name, out interfaceinfo) && interfaceinfo.TryGet(Method, out methodinfo) && methodinfo.Parameters.Count == count)
            {
                Parameters = new object[count];
                for (int i = 0; i < count; i++)
                {
                    int size = reader.ReadInt32();
                    if (size > 0)
                    {
                        Parameters[i] = reader.Stream.Deserialize(size, methodinfo.Parameters[i].Type);
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    int size = reader.ReadInt32();
                    byte[] data = new byte[size];
                    reader.Read(data, 0, size);
                }
                DeserializeError = string.Format("{0}.{1} method not found!", Name, Method);
            }

        }

        public void Serialize(IBinaryWriter writer)
        {
            writer.WriteShortUTF(Name);
            writer.WriteShortUTF(Method);
            writer.Write((short)Parameters.Length);
            for (int i = 0; i < Parameters.Length; i++)
            {
                object data = Parameters[i];
                if (data == null)
                {
                    writer.Write(0);
                }
                else
                {
                    
                    using (IWriteBlock block = writer.Allocate4Bytes())
                    {
                        int start = (int)writer.Length;
                        data.Serialize(writer.Stream);
                        block.SetData((int)writer.Length - start);
                    }
                }
            }
        }
    }
}
