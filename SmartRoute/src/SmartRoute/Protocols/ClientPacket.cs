using BeetleX;
using BeetleX.Buffers;
using BeetleX.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.Protocols
{
    public class ClientPacket : BeetleX.Clients.IClientPacket
    {

        public ClientPacket()
        {
            TypeHandler = new MessageTypeHandler();
        }

        private System.Collections.Concurrent.ConcurrentDictionary<Type, string> mTypeNames = new System.Collections.Concurrent.ConcurrentDictionary<Type, string>();

        public IMessageTypeHandler TypeHandler
        {
            get; set;
        }

        public EventClientPacketCompleted Completed
        {
            get;
            set;
        }

        private int mSize = 0;

        public IClientPacket Clone()
        {
            ClientPacket result = new Protocols.ClientPacket();
            result.TypeHandler = TypeHandler;
            return result;
        }

        public void Decode(IClient client, IBinaryReader reader)
        {
            START:
            try
            {
                object data;
                lock (reader.Stream)
                {
                    if (mSize == 0)
                    {
                        if (reader.Length < 4)
                            return;
                        mSize = reader.ReadInt32();
                    }
                    if (reader.Length < mSize)
                        return;

                    Type type = TypeHandler.ReadType(reader);
                    int bodySize = reader.ReadInt32();
                    data = reader.Stream.Deserialize(bodySize, type);
                    Message msg = data as Message;
                    if (msg != null)
                    {
                        msg.Track("message decode start");
                        msg.IsLocal = false;
                        int datasize = reader.ReadInt32();
                        Type dataType = TypeHandler.GetType(msg.DataType);
                        msg.Data = reader.Stream.Deserialize(datasize, dataType);
                        msg.Track("message decode completed");
                    }
                    mSize = 0;
                }
                try
                {
                    if (Completed != null)
                    {
                        Completed(client, data);
                    }
                }
                catch (Exception e_)
                {
                    client.ProcessError(e_, "client packet process object error!");
                }
                goto START;
            }
            catch (Exception e_)
            {
                client.ProcessError(e_, "client packet decode error!");
                client.DisConnect();
            }
        }

        public void Dispose()
        {

        }

        public void Encode(object data, IClient client, IBinaryWriter writer)
        {
            Message msg = data as Message;
            if (msg != null)
            {
                msg.Track("message GetType");
                msg.DataType = TypeHandler.GetTypeName(msg.Data.GetType());
            }
            using (IWriteBlock msgsize = writer.Allocate4Bytes())
            {
                int length = (int)writer.Length;
                TypeHandler.WriteType(data, writer);
                using (IWriteBlock bodysize = writer.Allocate4Bytes())
                {
                    int bodyStartlegnth = (int)writer.Length;
                    ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(writer.Stream, data);
                    bodysize.SetData((int)writer.Length - bodyStartlegnth);
                }
                if (msg != null)
                {
                    msg.Track("message write body");
                    using (IWriteBlock datasize = writer.Allocate4Bytes())
                    {
                        int dataStartlength = (int)writer.Length;
                        ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(writer.Stream, msg.Data);
                        datasize.SetData((int)writer.Length - dataStartlength);
                    }
                    msg.Track("message write body completed!");
                    msg.Track("message size:" + writer.Length);
                }
                msgsize.SetData((int)writer.Length - length);
               
            }
        }
    }
}
