using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileEngine
{
    class MapCell
    {
        public int TileID { get; set; }

        public MapCell(int tileID)
        {
            TileID = tileID;
        }
    }
}
