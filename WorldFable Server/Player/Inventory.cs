using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldFable_Server
{
    public class PlayerInventory
    {
        public int InventoryID { get; set; }
        public int InventorySize { get; set; }
    }
    public class InventoryItem
    {
        public short ItemID { get; set; } 
        public byte ItemCount { get; set; } 
        public int InventoryID { get; set; }  

        public PlayerInventory Inventory { get; set; }
    }

}
