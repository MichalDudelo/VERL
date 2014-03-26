using System;


namespace Common_Library.Parts_of_map
{
    [Serializable]
    public class Item
    {
        enum type 
        {one = 1, four = 4, eight = 8}

        public int X;
        public int Y;

        public Item()
        {

        }
    }

}