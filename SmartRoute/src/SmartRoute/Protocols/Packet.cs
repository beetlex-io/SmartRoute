using BeetleX;
using BeetleX.Buffers;
using BeetleX.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.Protocols
{
    public class Packet : BeetleX.IPacket
    {
        public Packet()
        {
            TypeHandler = new MessageTypeHandler();
        }

        private PacketDecodeCompletedEventArgs mCompletedEventArgs = new PacketDecodeCompletedEventArgs();

        public EventHandler<PacketDecodeCompletedEventArgs> Completed
        {
            get;
            set;
        }

        public IMessageTypeHandler TypeHandler
        {
            get;
            set;
        }

        private int mSize = 0;

        public IPacket Clone()
        {
            Packet result = new Protocols.Packet();
            result.TypeHandler = TypeHandler;
            return result;
        }

        public void Decode(ISession session, IBinaryReader reader)
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
                        msg.Track("message decode");
                        msg.IsLocal = false;
                        int datasize = reader.ReadInt32();
                        Type dataType = TypeHandler.GetType(msg.DataType);
                        msg.Data = reader.Stream.Deserialize(datasize, dataType);
                    }

                    mSize = 0;
                }
                try
                {
                    if (Completed != null)
                    {
                        Completed(this, mCompletedEventArgs.SetInfo(session, data));
                    }
                }
                catch (Exception e_)
                {
                    session.Server.Error(e_, session, "session packet process object error!");
                }
                if (reader.Length == 0)
                    return;
                goto START;
            }
            catch (Exception e_)
            {
                session.Server.Error(e_, session, "session packet decode error!");
                session.Dispose();
            }
        }

        private void OnEncode(object data, IBinaryWriter writer)
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
                }
                msgsize.SetData((int)writer.Length - length);

            }
        }

        public byte[] Encode(object data, IServer server)
        {
            byte[] result = null;
            using (XStream stream = new XStream(server.BufferPool))
            {
                IBinaryWriter writer = ServerFactory.CreateWriter(stream, server.Config.Encoding,
                 server.Config.LittleEndian);
                OnEncode(data, writer);
                stream.Position = 0;
                result = new byte[stream.Length];
                stream.Read(result, 0, result.Length);

            }
            return result;
        }

        public ArraySegment<byte> Encode(object data, IServer server, byte[] buffer)
        {
            using (XStream stream = new XStream(server.BufferPool))
            {
                IBinaryWriter writer = ServerFactory.CreateWriter(stream, server.Config.Encoding,
                  server.Config.LittleEndian);
                OnEncode(data, writer);
                stream.Position = 0;
                int count = (int)writer.Length;
                stream.Read(buffer, 0, count);
                return new ArraySegment<byte>(buffer, 0, count);

            }
        }

        public void Encode(object data, ISession session, IBinaryWriter writer)
        {
            OnEncode(data, writer);
        }

        public void Dispose()
        {

        }
    }
}
