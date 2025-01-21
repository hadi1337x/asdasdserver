using System.Runtime.CompilerServices;
using WorldFable_Server.Data;
using WorldFable_Server.Packets;

namespace WorldFable_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            StartListeningForCommands();
        }

        public static void ServerLog(string message)
        {
            Console.WriteLine($"(SERVER) {message}");
        }

        public static void ClientLog(string message)
        {
            Console.WriteLine($"(CLIENT) {message}");
        }

        public static void SQLLog(string message)
        {
            Console.WriteLine($"(SQL) {message}");
        }

        private static void StartListeningForCommands()
        {
            while (true)
            {
                string input = Console.ReadLine()?.Trim().ToLower();

                if (string.IsNullOrEmpty(input)) continue;

                switch (input)
                {
                    case "start":
                        SQL.checkSQLServer();
                        Server.ServerStart();
                        break;

                    case "stop":
                        ServerLog("Server stopped...");
                        break;

                    case "who":
                        ServerLog("Checking Players in World ...");
                        WorldFable_Server.Packets.Packets.ShowConnectedPlayers();
                        break;

                    case "help":
                        ClientLog("Available commands:");
                        ClientLog("start - Start the server");
                        ClientLog("stop - Stop the server");
                        ClientLog("status - Check the server status");
                        ClientLog("help - Show available commands");
                        break;

                    default:
                        ClientLog($"Unknown command: {input}");
                        break;
                }
            }
        }
    }
}
