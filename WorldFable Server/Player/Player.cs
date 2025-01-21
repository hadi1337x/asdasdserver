using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldFable_Server
{
    public class Player
    {
        public uint PeerID { get; set; }
        public int PlayerID { get; set; }  
        public string? TankIDName { get; set; }  
        public string? TankIDPass { get; set; }  
        public string? DisplayName { get; set; }  
        public string? Country { get; set; }  
        public int AdminLevel { get; set; }  
        public string? CurrentWorld { get; set; }

        // Position
        public float x {  get; set; }  
        public float y { get; set; }

        // Wearables 
        public int ClothHair { get; set; }  
        public int ClothShirt { get; set; }  
        public int ClothPants { get; set; }  
        public int ClothFeet { get; set; }  
        public int ClothFace { get; set; }  
        public int ClothHand { get; set; }  
        public int ClothBack { get; set; } 
        public int ClothMask { get; set; }  
        public int ClothNecklace { get; set; }  

        // PlayerStates
        public bool CanWalkInBlocks { get; set; }
        public bool CanDoubleJump { get; set; } 
        public bool IsInvisible { get; set; }  
        public bool IsBanned { get; set; } 
        public long BanTime { get; set; }  

        // Player Inventory
        public int InventoryID { get; set; } 
    }
}
