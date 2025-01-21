using ENet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldFable_Server.Packets
{
    internal class Server
    {
        static Host _server = new Host();
        public static void ServerStart()
        {
            const ushort port = 6005;
            const int maxClients = 100;
            Library.Initialize();

            _server = new Host();
            Address address = new Address
            {
                Port = port
            };

            _server.Create(address, maxClients);
            Program.ServerLog($"ENet Server started on port {port}");

            Event netEvent;
            while (!Console.KeyAvailable)
            {
                if (_server.Service(15, out netEvent) > 0)
                {
                    switch (netEvent.Type)
                    {
                        case EventType.Connect:
                            Program.ClientLog($"Client connected - ID: {netEvent.Peer.ID}, IP: {netEvent.Peer.IP}");
                            Packets.HeartbeatTracker[netEvent.Peer.ID] = DateTime.Now;
                            Packets.AddPeer(netEvent.Peer.ID, netEvent.Peer);
                            Packets.SendPlayerUID(netEvent.Peer.ID, netEvent.Peer);
                            break;

                        case EventType.Disconnect:
                            Packets.RemovePeer(netEvent.Peer.ID);
                            break;
                        case EventType.Timeout:
                            Program.ClientLog($"Client disconnected - ID: {netEvent.Peer.ID}");
                            Packets.RemovePeer(netEvent.Peer.ID);
                            Packets.HeartbeatTracker.Remove(netEvent.Peer.ID);
                            break;

                        case EventType.Receive:
                            Packets.HandlePacket(ref netEvent);
                            netEvent.Packet.Dispose();
                            break;
                    }
                }
                Packets.CheckHeartbeatTimeouts();
            }

            _server.Flush();
            Library.Deinitialize();
        }
    }
}
