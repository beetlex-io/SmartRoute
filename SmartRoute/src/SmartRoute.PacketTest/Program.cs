using BeetleX;
using BeetleX.Clients;
using BeetleX.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.PacketTest
{
    public class Program : ServerHandlerBase
    {
        private static IServer mServer;

        private static IClient mClient;

        public static void Main(string[] args)
        {

            NetConfig config = new NetConfig();
            mServer = ServerFactory.CreateTcpServer<Program, SmartRoute.Protocols.Packet>(config);
            mClient = ServerFactory.CreateTcpClient<Protocols.ClientPacket>("192.168.1.241", 10100);
            mServer.Open();
            Console.Read();
        }

        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            base.SessionPacketDecodeCompleted(server, e);
            //Console.WriteLine(e.Message);
            // server.Send(e.Message, e.Session);
            mClient.Send(e.Message);
        }

        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            base.SessionReceive(server, e);
        }

    }
}
