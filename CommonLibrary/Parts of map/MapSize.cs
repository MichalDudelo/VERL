using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common_Library.Parts_of_map
{
    [DataContract]
    public class MapSize
    {
        [DataMember]
        private int _width;

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        [DataMember]
        private int _height;

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public MapSize(int width, int height)
        {
            _height = height;
            _width = width;
        }
    }
}
