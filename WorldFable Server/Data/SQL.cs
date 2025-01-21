using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldFable_Server.Data
{
    public class SQL
    {
        public static string connectionString = "Server=DESKTOP-8O0GVI9;Database=WorldFable;User Id=admin;Password=123@ADMIN;";
        public static bool checkSQLServer()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Program.SQLLog("SQL Server Connection Successful!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Program.SQLLog($"SQL Server Connection Failed: {ex.Message}");
                return false;
            }
        }
        public static void CreatePlayer(string username, string password, string displayName)
        {
            username = username.ToLower();

            int inventoryID = CreateInventory();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = @"
                INSERT INTO [WorldFable].[dbo].[Players] 
                ([tankIDName], [tankIDPass], [displayName], [country], [adminLevel], [currentWorld],
                 [cloth_hair], [cloth_shirt], [cloth_pants], [cloth_feet], [cloth_face], [cloth_hand],
                 [cloth_back], [cloth_mask], [cloth_necklace], [canWalkInBlocks], [canDoubleJump], 
                 [isInvisible], [isBanned], [banTime], [inventoryID])
                VALUES 
                (@tankIDName, @tankIDPass, @displayName, @country, @adminLevel, @currentWorld,
                 @cloth_hair, @cloth_shirt, @cloth_pants, @cloth_feet, @cloth_face, @cloth_hand,
                 @cloth_back, @cloth_mask, @cloth_necklace, @canWalkInBlocks, @canDoubleJump, 
                 @isInvisible, @isBanned, @banTime, @inventoryID)";

                    cmd.Parameters.AddWithValue("@tankIDName", username);
                    cmd.Parameters.AddWithValue("@tankIDPass", password);
                    cmd.Parameters.AddWithValue("@displayName", displayName);
                    cmd.Parameters.AddWithValue("@country", "");
                    cmd.Parameters.AddWithValue("@adminLevel", 0);
                    cmd.Parameters.AddWithValue("@currentWorld", "");
                    cmd.Parameters.AddWithValue("@cloth_hair", 0);
                    cmd.Parameters.AddWithValue("@cloth_shirt", 0);
                    cmd.Parameters.AddWithValue("@cloth_pants", 0);
                    cmd.Parameters.AddWithValue("@cloth_feet", 0);
                    cmd.Parameters.AddWithValue("@cloth_face", 0);
                    cmd.Parameters.AddWithValue("@cloth_hand", 0);
                    cmd.Parameters.AddWithValue("@cloth_back", 0);
                    cmd.Parameters.AddWithValue("@cloth_mask", 0);
                    cmd.Parameters.AddWithValue("@cloth_necklace", 0);
                    cmd.Parameters.AddWithValue("@canWalkInBlocks", false);
                    cmd.Parameters.AddWithValue("@canDoubleJump", false);
                    cmd.Parameters.AddWithValue("@isInvisible", false);
                    cmd.Parameters.AddWithValue("@isBanned", false);
                    cmd.Parameters.AddWithValue("@banTime", DBNull.Value);
                    cmd.Parameters.AddWithValue("@inventoryID", inventoryID);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static int CreateInventory()
        {
            int inventoryID = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = @"
                INSERT INTO [WorldFable].[dbo].[PlayerInventories] ([inventorySize])
                VALUES (@inventorySize);
                SELECT SCOPE_IDENTITY();";

                    cmd.Parameters.AddWithValue("@inventorySize", 20);

                    inventoryID = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return inventoryID;
        }
        public static string CheckLog(string username, string password)
        {
            string isValid = "false";

            string query = "SELECT COUNT(1) FROM Players WHERE tankIDName = @username AND tankIDPass = @password";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        int count = Convert.ToInt32(command.ExecuteScalar());

                        if (count > 0)
                        {
                            isValid = "true";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            return isValid;
        }
        public static Player GetPlayerData(string tankIDName)
        {
            Player player = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = @"
                        SELECT [playerID], [tankIDName], [tankIDPass], [displayName], [country],
                               [adminLevel], [currentWorld], [cloth_hair], [cloth_shirt], 
                               [cloth_pants], [cloth_feet], [cloth_face], [cloth_hand], 
                               [cloth_back], [cloth_mask], [cloth_necklace], [canWalkInBlocks], 
                               [canDoubleJump], [isInvisible], [isBanned], [banTime], [inventoryID]
                        FROM [WorldFable].[dbo].[Players]
                        WHERE [tankIDName] = @tankIDName";

                    cmd.Parameters.AddWithValue("@tankIDName", tankIDName);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            player = new Player
                            {
                                PlayerID = reader.GetInt32(reader.GetOrdinal("playerID")),
                                TankIDName = reader.GetString(reader.GetOrdinal("tankIDName")),
                                TankIDPass = reader.GetString(reader.GetOrdinal("tankIDPass")),
                                DisplayName = reader.GetString(reader.GetOrdinal("displayName")),
                                Country = reader.GetString(reader.GetOrdinal("country")),
                                AdminLevel = reader.GetInt32(reader.GetOrdinal("adminLevel")),
                                CurrentWorld = reader.GetString(reader.GetOrdinal("currentWorld")),
                                ClothHair = reader.GetInt32(reader.GetOrdinal("cloth_hair")),
                                ClothShirt = reader.GetInt32(reader.GetOrdinal("cloth_shirt")),
                                ClothPants = reader.GetInt32(reader.GetOrdinal("cloth_pants")),
                                ClothFeet = reader.GetInt32(reader.GetOrdinal("cloth_feet")),
                                ClothFace = reader.GetInt32(reader.GetOrdinal("cloth_face")),
                                ClothHand = reader.GetInt32(reader.GetOrdinal("cloth_hand")),
                                ClothBack = reader.GetInt32(reader.GetOrdinal("cloth_back")),
                                ClothMask = reader.GetInt32(reader.GetOrdinal("cloth_mask")),
                                ClothNecklace = reader.GetInt32(reader.GetOrdinal("cloth_necklace")),
                                CanWalkInBlocks = reader.GetBoolean(reader.GetOrdinal("canWalkInBlocks")),
                                CanDoubleJump = reader.GetBoolean(reader.GetOrdinal("canDoubleJump")),
                                IsInvisible = reader.GetBoolean(reader.GetOrdinal("isInvisible")),
                                IsBanned = reader.GetBoolean(reader.GetOrdinal("isBanned")),
                                BanTime = reader.IsDBNull(reader.GetOrdinal("banTime"))
                                            ? 0 : reader.GetInt64(reader.GetOrdinal("banTime")),
                                InventoryID = reader.GetInt32(reader.GetOrdinal("inventoryID"))
                            };
                        }
                    }
                }
            }

            return player;
        }
        public static int GetInventorySizeById(int inventoryId)
        {
            // SQL query to fetch the inventory size based on the inventory ID
            string query = "SELECT [inventorySize] FROM [dbo].[PlayerInventories] WHERE [inventoryID] = @inventoryID";

            // Set up the connection and command
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@inventoryID", inventoryId); // Add the inventoryID parameter to the query

                try
                {
                    // Open the connection
                    connection.Open();

                    // Execute the query and get the result
                    object result = command.ExecuteScalar(); // ExecuteScalar returns the first column of the first row

                    if (result != DBNull.Value && result != null)
                    {
                        return Convert.ToInt32(result); // Return the inventory size as an int
                    }
                    else
                    {
                        Console.WriteLine($"No inventory found with ID {inventoryId}");
                        return -1; // Return -1 if no inventory was found
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., connection issues, SQL errors)
                    Console.WriteLine("An error occurred: " + ex.Message);
                    return -1; // Return -1 in case of error
                }
            }
        }
    }
}
