using System;

namespace Common_Library.Parts_of_map
{
    [Serializable]
    public class BigItem : Segment
    {
        public BigItem(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
