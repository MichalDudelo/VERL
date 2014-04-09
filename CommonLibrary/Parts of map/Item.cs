using System;


namespace Common_Library.Parts_of_map
{
    [Serializable]
    public class Item
    {

        public override bool Equals(object obj)
        {
            Item i = obj as Item;
            return (i == null) ? false : this.X == i.X && this.Y == i.Y;
        }

        public int X;
        public int Y;

        public Item()
        {

        }
    }

}