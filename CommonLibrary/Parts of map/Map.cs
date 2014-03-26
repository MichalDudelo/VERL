using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Common_Library.Parts_of_map;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Common_Library
{
    [Serializable]
    public class Map
    {

        private int mapWidth; 
        private int mapHeight;
        private List<Wall> wallList;
        private List<Floor> floorList;
        private List<BigItem> bigItemList;
        private List<SmallItem> smallItemList;
        private Segment[,] globalMap;
        private List<Position> startingPositionList;
        private int _startingPositionNumber = 0;
        private int colorPalette = 1;

        public int ColorPalette
        {
            get { return colorPalette; }
            set { colorPalette = value; }
        }

        public int MapWidth
        {
            get { return mapWidth; }
            set { mapWidth = value; }
        }

        public int MapHeight
        {
            get { return mapHeight; }
            set { mapHeight = value; }
        }

        public int StartingPositionNumber
        {
            get { return _startingPositionNumber; }
            set { _startingPositionNumber = value; }
        }

        public List<Position> StartingPositionList
        {
            get { return startingPositionList; }
            set { startingPositionList = value; }
        }

       
        public List<Wall> WallList
        {
            get { return wallList; }
            set { wallList = value; }
        }
        
        public List<Floor> FloorList
        {
            get { return floorList; }
            set { floorList = value; }
        }
        
        public List<BigItem> BigItemList
        {
            get { return bigItemList; }
            set { bigItemList = value; }
        }
        

        public List<SmallItem> SmallItemList
        {
            get { return smallItemList; }
            set { smallItemList = value; }
        }

      
        public Segment[,] GlobalMap
        {
            get { return globalMap; }
            set { globalMap = value; }
        }

        public Map Copy()
        {
            var cpy = this.SerializeMap();
            return Map.Deserialize(cpy);
        }

        public Map(int width, int height)
        {
            MapWidth = width;
            MapHeight = height;
            bigItemList = new List<BigItem>();
            smallItemList = new List<SmallItem>();
            startingPositionList = new List<Position>();
            globalMap = new Segment[height, width];
        }

        public byte[] SerializeMap()
        {
            IFormatter formatter = new BinaryFormatter();
            var ms = new MemoryStream();
            formatter.Serialize(ms, this);
            return ms.ToArray();
        }

        public static Map Deserialize(byte[] SerialiazedMap)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream st = new MemoryStream(SerialiazedMap);
            return (Map)formatter.Deserialize(st);
        }

        public Map getSmallerPartForRobot(Position robotPosition)
        {
            //to think!!
            int yStartMapOffset = (robotPosition.Y - 8) < 0 ? 0 : (robotPosition.Y - 8);
            int xStartMapOffset = (robotPosition.X - 8) < 0 ? 0 : (robotPosition.X - 8);
            int yEndMapOffset = (robotPosition.Y + 8) >= MapHeight ? (MapHeight -1) : (robotPosition.Y + 8);
            int xEndMapOffset = (robotPosition.X + 8) >= MapWidth ? (MapWidth -1) : (robotPosition.X + 8);
            int k = -1, l = -1;
            int XsmallMapRobotPosition = 8 - xStartMapOffset;
            int YsmallMapRobotPosition = 8 - yStartMapOffset; 
            Map smallerMap = new Map(17, 17);
            Segment[,] smallerGlobalMap = new Segment[17, 17];

            for (int i = yStartMapOffset; i <= yEndMapOffset; i++)
            {
                l = -1;
                k++;
                for (int j = xStartMapOffset; j <= xEndMapOffset; j++)
                {
                    l++;
                    if (globalMap[i, j].HasBigItem) smallerMap.BigItemList.Add(new BigItem(j, i));
                    if (globalMap[i, j].IsStartingPosition) smallerMap.StartingPositionList.Add(new Position(j, i));
                    if (globalMap[i, j].SmallItemNumber > 0) smallerMap.smallItemList.Add(new SmallItem(j, i));
                    smallerGlobalMap[ (k), (l)] = globalMap[i, j].Copy();
                    
                }
                
            }
           
            smallerMap.GlobalMap = smallerGlobalMap;
            return smallerMap;
 
        }
    }
}