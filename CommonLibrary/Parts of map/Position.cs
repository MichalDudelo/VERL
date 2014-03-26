using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common_Library.Parts_of_map
{
    [Serializable]
    public class Position
    {
        [DataMember]
        private int x;

        public int X
        {
        get { return x; }
        set { x = value; }
        }
        [DataMember]
        private int y;

        public int Y
        {
          get { return y; }
          set { y = value; }
        }

        public bool Used = false;

        public Position Copy()
        {
            return (Position)this.MemberwiseClone();
        }

        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Used = false;
        }
        public override string ToString()
        {
            return "X: " + this.x + " Y: " + this.y;
        }
    }
}
