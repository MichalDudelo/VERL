using Common_Library.Parts_of_map;
using System;

namespace Common_Library.Parts_of_map
{
    [Serializable]
    public class Segment
    {
        bool isStartingPosition;
        int smallItemNumber;
        bool hasBigItem;
        Robot robot;

        public Robot Robot
        {
            get { return robot; }
            set { robot = value; }
        }
        int x;
        int y;

        public int X
        {
            get { return x; }
            set { x = value; }
        }
       

        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        String color;

        public String Color
        {
            get { return color; }
            set { color = value; }
        }


        public int SmallItemNumber
        {
            get { return smallItemNumber; }
            set { smallItemNumber = value; }
        }

        public bool IsStartingPosition
        {
            get { return isStartingPosition; }
            set { isStartingPosition = value; }
        }

        public bool HasBigItem
        {
            get { return hasBigItem; }
            set { hasBigItem = value; }
        }
       

        public Segment()
        {
            isStartingPosition = false;
            
            hasBigItem = false;
            smallItemNumber = 0;
            color = "Transparent";
        }
        public Segment Copy()
        {
            var seg = (Segment)this.MemberwiseClone();
            if (this.robot != null)
                seg.robot = new Robot(this.robot.HealthPoints, this.robot.RobotPosition, this.robot.RobotColor);
            else
                seg.robot = null;
            return seg;
        }
    }
}