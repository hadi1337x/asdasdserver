using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WorldFable_Server
{
    [ProtoContract]
    public class WorldItem
    {
        [ProtoMember(1)]
        public int foreground = 0;
        [ProtoMember(2)]
        public int background = 0;

    }

    [ProtoContract]
    public class Worlds
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public int Width { get; set; }
        [ProtoMember(3)]
        public int Height { get; set; }

        [ProtoMember(4)]
        public List<WorldItem> Items { get; set; }

        public Worlds()
        {
            Items = new List<WorldItem>();
        }

        public Worlds(string Name, int Width, int Height)
        {
            this.Name = Name;
            this.Width = Width;
            this.Height = Height;

            Items = new List<WorldItem>();

            GenerateWorld();
        }

        public void GenerateWorld()
        {
            Random random = new Random();
            for (int i = Width * Height - 1; i >= 0; i--)
            {
                WorldItem item = new WorldItem();

                if (i >= 3800 && i < 5400 && random.Next(50) == 0)
                    item.foreground = 2;
                else if (i >= 3700 && i < 5400)
                {
                    if (i > 5000)
                    {
                        if (i % 7 == 0)
                            item.foreground = 4;
                        else
                            item.foreground = 1;
                    }
                    else
                        item.foreground = 1;
                }
                else if (i >= 5400)
                    item.foreground = 3;

                if (i >= 3700)
                    item.background = 14;

                if (i == 3650)
                {
                    int x = i % Width;
                    int y = i / Width;
                    item.foreground = 5;
                }
                else if (i >= 3600 && i < 3700)
                    item.foreground = 0;

                if (i == 3750)
                    item.foreground = 3;

                Items.Add(item);
            }
        }
        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize(stream, this);
                return stream.ToArray();
            }
        }

        public static Worlds Deserialize(byte[] data)
        {
            using (MemoryStream stram = new MemoryStream(data))
            {
                return Serializer.Deserialize<Worlds>(stram);
            }
        }
    }
}
