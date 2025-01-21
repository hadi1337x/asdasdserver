using ENet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WorldFable_Server.Data;

namespace WorldFable_Server.Packets
{
    public class Packets
    {
        public static Dictionary<uint, DateTime> HeartbeatTracker = new Dictionary<uint, DateTime>();
        const int HeartbeatTimeout = 10;
        static List<Worlds> worldsList = new List<Worlds>();
        public static Dictionary<uint, Peer> allPeers = new Dictionary<uint, Peer>();
        public static List<Player> connectedPlayers = new List<Player> ();

        public static void AddPeer(uint peerId, Peer peer)
        {
            allPeers[peerId] = peer;
        }

        public static void RemovePeer(uint peerId)
        {
            allPeers.Remove(peerId);
            connectedPlayers.RemoveAll(player => player.PeerID == peerId);
        }

        public static List<Peer> GetAllPeers()
        {
            return allPeers.Values.ToList();
        }
        public static Player GetPlayerByPeerID(uint id)
        {
            return connectedPlayers.FirstOrDefault(player => player.PeerID == id);
        }
        public static Peer GetPeerById(uint peerId)
        {
            if (allPeers.TryGetValue(peerId, out Peer peer))
            {
                return peer;
            }
            return peer;
        }
        public static void ShowConnectedPlayers()
        {
            if (connectedPlayers.Count == 0)
            {
                Console.WriteLine("No players are currently connected.");
            }
            else
            {
                Console.WriteLine("Currently connected players:");
                foreach (var player in connectedPlayers)
                {
                    Console.WriteLine($"Player ID: {player.PlayerID}, Player Name: {player.DisplayName}, Player World Name: {player.CurrentWorld}");
                }
            }
        }


        public static void HandlePacket(ref Event netEvent)
        {
            byte[] data = new byte[netEvent.Packet.Length];
            netEvent.Packet.CopyTo(data);

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                PacketId packetId = (PacketId)reader.ReadByte();

                if (packetId == PacketId.Heartbeat)
                {
                    HeartbeatTracker[netEvent.Peer.ID] = DateTime.Now;
                    Console.WriteLine($"Heartbeat received from client ID: {netEvent.Peer.ID}");
                }
                if (packetId == PacketId.Register)
                {
                    string username = reader.ReadString();
                    string password = reader.ReadString();

                    SQL.CreatePlayer(username, password, username);
                }
                if (packetId == PacketId.Login)
                {
                    string username = reader.ReadString();
                    string password = reader.ReadString();

                    string result = SQL.CheckLog(username, password);
                    SendLoginResult(result,username, netEvent.Peer);
                }
                if (packetId == PacketId.RequestPlayerData)
                {
                    string tankIdName = reader.ReadString();
                    Player playerData = SQL.GetPlayerData(tankIdName);

                    SendPlayerData(playerData, netEvent.Peer);

                    playerData.PeerID = netEvent.Peer.ID;

                    connectedPlayers.Add(playerData);
                }
                if (packetId == PacketId.Join_World)
                {
                    string worldName = reader.ReadString();
                    sendWorld(worldName, netEvent.Peer, netEvent.Peer.ID);
                }
                if (packetId == PacketId.PlayerMovement)
                {
                    uint movingPlayerId = reader.ReadUInt32();
                    string worldName = reader.ReadString();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    HandlePlayerMovement(movingPlayerId, worldName, x, y, netEvent.Peer);
                }
                if (packetId == PacketId.Spawn_LocalPlayer)
                {
                    uint playerId = reader.ReadUInt32();
                    Player localPlayerSpawn = GetPlayerByPeerID(playerId);
                    Console.WriteLine(localPlayerSpawn.CurrentWorld);
                    HandlePlayerJoin(localPlayerSpawn.CurrentWorld, playerId, netEvent.Peer);
                }
            }
        }
        private static void HandlePlayerMovement(uint playerId,string worldName, float x, float y, Peer peer)
        {
            Vector2 newPosition = new Vector2(x, y);
            Player playerToBeUpdated = GetPlayerByPeerID(playerId);

            playerToBeUpdated.x = x;
            playerToBeUpdated.y = y;

            Console.WriteLine($"Player {playerId} moved to {x}, {y}");

            foreach (var player in connectedPlayers)
            {
                if (player.CurrentWorld == worldName)
                {
                    Console.WriteLine($"Sent player movement {player.DisplayName}");
                    SendPlayerPositionUpdate(playerId, newPosition, GetPeerById(player.PeerID));
                }
            }
        }
        private static void SendPlayerPositionUpdate(uint playerId, Vector2 position, Peer peer)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketId.PlayerMovement);
                writer.Write(playerId);
                writer.Write(position.X);
                writer.Write(position.Y);

                SendPacket(peer, stream.ToArray());
            }
        }
        public static void UpdatePlayerWorld(uint playerId, string worldName)
        {
            Player playerToBeUpdated = GetPlayerByPeerID(playerId);

            if (playerToBeUpdated != null)
            {
                playerToBeUpdated.CurrentWorld = worldName;

                Program.ServerLog($"Updated Player ID: {playerToBeUpdated.PlayerID} - New World: {worldName}");
            }
            else
            {
                Program.ServerLog($"Player with ID {playerId} not found.");
            }
        }
        public static void sendWorld(string Name, Peer peer, uint PeerID)
        {
            Worlds world = null;

            for (int i = 0; i < worldsList.Count; i++)
            {
                if (worldsList[i].Name == Name)
                {
                    world = worldsList[i];
                    break;
                }
            }

            if (world == null)
            {
                world = new Worlds(Name, 100, 60);
                worldsList.Add(world);
            }
            UpdatePlayerWorld(PeerID, Name);
            SendWorldItems(peer, world.Items);
        }
        private static void HandlePlayerJoin(string worldName, uint playerId, Peer peer)
        {
            Player playerToBeSpawned = GetPlayerByPeerID(playerId);
            playerToBeSpawned.x = 50;
            playerToBeSpawned.y = 36;

            SendPlayerSpawn(worldName, playerId, playerToBeSpawned.x, playerToBeSpawned.y, peer);

            foreach (var player in connectedPlayers)
            {
                if (player.PeerID != playerId && player.CurrentWorld == worldName)
                {
                    Player otherPlayers = GetPlayerByPeerID(player.PeerID);
                    SendOtherPlayerSpawn(worldName, playerId, otherPlayers.x, otherPlayers.y, GetPeerById(player.PeerID));
                    Console.WriteLine($"Sent Spawn Other Players to {otherPlayers.DisplayName}");
                }
            }
            int inventorySlot = SQL.GetInventorySizeById(playerToBeSpawned.InventoryID);

            SendInventorySize(inventorySlot, peer);

            Console.WriteLine($"Player {playerId} joined with peer ID {peer.ID} in world {playerToBeSpawned.CurrentWorld}");
        }
        private static void SendPlayerSpawn(string worldName, uint playerId, float x, float y, Peer peer)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketId.Spawn_LocalPlayer);
                writer.Write(playerId);
                writer.Write(worldName);
                writer.Write(x);
                writer.Write(y);

                SendPacket(peer, stream.ToArray());
            }
        }
        private static void SendInventorySize(int size, Peer peer)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketId.GetInventorySize);
                writer.Write(size);

                SendPacket(peer, stream.ToArray());
            }
        }

        private static void SendOtherPlayerSpawn(string worldName, uint playerId, float x, float y, Peer peer)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketId.Spawn_OtherPlayer);
                writer.Write(playerId);
                writer.Write(worldName);
                writer.Write(x);
                writer.Write(y);

                SendPacket(peer, stream.ToArray());
            }
        }
        private static void SendWorldItems(Peer client, List<WorldItem> worldItems)
        {
            byte[] data = ConvertWorldItemsToByteArray(worldItems);
            SendPacketToClient(client, PacketId.Join_World, data);
        }
        private static byte[] ConvertWorldItemsToByteArray(List<WorldItem> worldItems)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                foreach (var item in worldItems)
                {
                    stream.Write(BitConverter.GetBytes(item.foreground), 0, 4);
                    stream.Write(BitConverter.GetBytes(item.background), 0, 4);
                }
                return stream.ToArray();
            }
        }
        private static void SendPacketToClient(Peer client, PacketId packetId, byte[] data)
        {
            byte[] packetData = new byte[1 + data.Length];
            packetData[0] = (byte)packetId;
            Array.Copy(data, 0, packetData, 1, data.Length);

            Packet packet = new Packet();
            packet.Create(packetData);
            client.Send(0, ref packet);
        }
        public static void CheckHeartbeatTimeouts()
        {
            DateTime now = DateTime.Now;
            foreach (var entry in HeartbeatTracker.ToList())
            {
                if ((now - entry.Value).TotalSeconds > HeartbeatTimeout)
                {
                    Console.WriteLine($"Client ID {entry.Key} timed out due to missing heartbeat.");
                    HeartbeatTracker.Remove(entry.Key);
                }
            }
        }
        public static void SendPlayerUID(uint playerId, Peer peer)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketId.PlayerUID);
                writer.Write(playerId);

                SendPacket(peer, stream.ToArray());
                Console.WriteLine($"Sent Player UID {playerId}");
            }
        }
        public static void SendPlayerData(Player playerData, Peer peer)
        {
            if (playerData == null)
            {
                Console.WriteLine("Player data is null. Cannot send.");
                return;
            }

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketId.RequestPlayerData);

                writer.Write(playerData.PlayerID);
                writer.Write(playerData.TankIDName ?? string.Empty);
                writer.Write(playerData.TankIDPass ?? string.Empty);
                writer.Write(playerData.DisplayName ?? string.Empty);
                writer.Write(playerData.Country ?? string.Empty);
                writer.Write(playerData.AdminLevel);
                writer.Write(playerData.CurrentWorld ?? string.Empty);
                writer.Write(playerData.x);
                writer.Write(playerData.y);

                writer.Write(playerData.ClothHair);
                writer.Write(playerData.ClothShirt);
                writer.Write(playerData.ClothPants);
                writer.Write(playerData.ClothFeet);
                writer.Write(playerData.ClothFace);
                writer.Write(playerData.ClothHand);
                writer.Write(playerData.ClothBack);
                writer.Write(playerData.ClothMask);
                writer.Write(playerData.ClothNecklace);

                writer.Write(playerData.CanWalkInBlocks);
                writer.Write(playerData.CanDoubleJump);
                writer.Write(playerData.IsInvisible);
                writer.Write(playerData.IsBanned);
                writer.Write(playerData.BanTime);

                writer.Write(playerData.InventoryID);

                SendPacket(peer, stream.ToArray());
                Console.WriteLine("Sent Player Data to Peer");
            }
        }

        public static void SendLoginResult(string result,string username, Peer peer)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketId.Login);
                writer.Write(result);
                writer.Write(username);

                SendPacket(peer, stream.ToArray());
                Console.WriteLine($"Sent Login Result {result}");
            }
        }
        public static void SendPacket(Peer peer, byte[] data)
        {
            Packet packet = default;
            packet.Create(data);
            peer.Send(0, ref packet);
        }
        public enum PacketId : byte
        {
            Heartbeat = 1,
            PlayerUID = 2,
            Register = 3,
            Login = 4,
            RequestPlayerData = 5,
            Join_World = 6,
            Spawn_LocalPlayer = 7,
            PlayerMovement = 8,
            Spawn_OtherPlayer = 9,
            GetInventorySize = 10
        }
    }
}
