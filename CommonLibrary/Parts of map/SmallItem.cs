using System;

namespace Common_Library.Parts_of_map
{
    [Serializable]
    public class SmallItem : Item
    {
        public SmallItem(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
