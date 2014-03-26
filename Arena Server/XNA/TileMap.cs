using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Common_Library;
using Common_Library.Parts_of_map;

namespace TileEngine
{
    class MapRow
    {
        public List<MapCell> Columns = new List<MapCell>();
    }

    class TileMap
    {
        public List<MapRow> Rows = new List<MapRow>();
        public int MapWidth;
        public int MapHeight;
        private Texture2D mouseMap;
        
        
        public TileMap(int width, int height, Map initialMap)
        {
            MapWidth = width;
            MapHeight = height;


            for (int y = 0; y < MapHeight; y++)
            {
                MapRow thisRow = new MapRow();
                for (int x = 0; x < MapWidth; x++)
                {
                    thisRow.Columns.Add(new MapCell(0));
                }
                Rows.Add(thisRow);
            }



            for (int i = 0; i < MapHeight; i++)
            {
                for (int j = 0; j < MapWidth; j++)
                {
                        if(initialMap.GlobalMap[i, j] != null)
                        switch (initialMap.GlobalMap[i, j].Color)
                        {
                            case "Default":
                                if (initialMap.GlobalMap[i, j] is Wall)
                                    Rows[i].Columns[j].TileID = 0;
                                else
                                    Rows[i].Columns[j].TileID = 9;
                                break;

                            case "Red":
                                if (initialMap.GlobalMap[i, j] is Wall)
                                    Rows[i].Columns[j].TileID = 1;
                                else
                                    Rows[i].Columns[j].TileID = 10;
                                break;

                            case "Blue":
                                if (initialMap.GlobalMap[i, j] is Wall)
                                    Rows[i].Columns[j].TileID = 2;
                                else
                                    Rows[i].Columns[j].TileID = 11;
                                break;

                            case "Green":
                                if (initialMap.GlobalMap[i, j] is Wall)
                                    Rows[i].Columns[j].TileID = 3;
                                else
                                    Rows[i].Columns[j].TileID = 12;
                                break;

                            case "Yellow":
                                if (initialMap.GlobalMap[i, j] is Wall)
                                    Rows[i].Columns[j].TileID = 4;
                                else
                                    Rows[i].Columns[j].TileID = 13;
                                break;

                            case "Orange":
                                if (initialMap.GlobalMap[i, j] is Wall)
                                    Rows[i].Columns[j].TileID = 5;
                                else
                                    Rows[i].Columns[j].TileID = 14;
                                break;

                            case "Violet":
                                if (initialMap.GlobalMap[i, j] is Wall)
                                    Rows[i].Columns[j].TileID = 6;
                                else
                                    Rows[i].Columns[j].TileID = 15;
                                break;
                            case"Black":
                                if (initialMap.GlobalMap[i, j] is Wall)
                                    Rows[i].Columns[j].TileID = 8;
                                else
                                    Rows[i].Columns[j].TileID = 17;
                                break;

                        }                                            
                }
            }

            // End Create Sample Map Data
        }
        public Point WorldToMapCell(Point worldPoint, out Point localPoint)
        {
            Point mapCell = new Point(
               (int)(worldPoint.X / mouseMap.Width),
               ((int)(worldPoint.Y / mouseMap.Height)) * 2
               );

            int localPointX = worldPoint.X % mouseMap.Width;
            int localPointY = worldPoint.Y % mouseMap.Height;

            int dx = 0;
            int dy = 0;

            uint[] myUint = new uint[1];

            if (new Rectangle(0, 0, mouseMap.Width, mouseMap.Height).Contains(localPointX, localPointY))
            {
                mouseMap.GetData(0, new Rectangle(localPointX, localPointY, 1, 1), myUint, 0, 1);

                if (myUint[0] == 0xFF0000FF) // Red
                {
                    dx = -1;
                    dy = -1;
                    localPointX = localPointX + (mouseMap.Width / 2);
                    localPointY = localPointY + (mouseMap.Height / 2);
                }

                if (myUint[0] == 0xFF00FF00) // Green
                {
                    dx = -1;
                    localPointX = localPointX + (mouseMap.Width / 2);
                    dy = 1;
                    localPointY = localPointY - (mouseMap.Height / 2);
                }

                if (myUint[0] == 0xFF00FFFF) // Yellow
                {
                    dy = -1;
                    localPointX = localPointX - (mouseMap.Width / 2);
                    localPointY = localPointY + (mouseMap.Height / 2);
                }

                if (myUint[0] == 0xFFFF0000) // Blue
                {
                    dy = +1;
                    localPointX = localPointX - (mouseMap.Width / 2);
                    localPointY = localPointY - (mouseMap.Height / 2);
                }
            }

            mapCell.X += dx;
            mapCell.Y += dy - 2;

            localPoint = new Point(localPointX, localPointY);

            return mapCell;
        }

        public Point WorldToMapCell(Point worldPoint)
        {
            Point dummy;
            return WorldToMapCell(worldPoint, out dummy);
        }
    }
}
