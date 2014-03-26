using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileEngine
{
    static class Tile
    {
        static public Texture2D TileSetTexture;
        static public int TileStepX = 64;
        static public int TileStepY = 64;

        static public Rectangle GetSourceRectangle(int tileIndex)
        {
            if (tileIndex < 9)
                return new Rectangle(tileIndex * 64, 0, 64, 64);
            else
            {
                int x = tileIndex - 9;
                return new Rectangle(x * 64, 64, 64, 64);
            }
        }


    }
}
