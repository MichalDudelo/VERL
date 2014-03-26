using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common_Library.Parts_of_map
{
    [Serializable]
    public class Robot
    {
        [DataMember]
        private int _healthPoints;

        public int HealthPoints
        {
            get { return _healthPoints; }
            set { _healthPoints = value; }
        }
         [DataMember]
        private Position _robotPosition;

        public Position RobotPosition
        {
            get { return _robotPosition; }
            set { _robotPosition = value; }
        }
         [DataMember]
        private string _robotColor;

        public string RobotColor
        {
            get { return _robotColor; }
            set { _robotColor = value; }
        }

        public Robot(int healthPoints, Position robotPosition, string Color)
        {
            this._healthPoints = healthPoints;
            this.RobotPosition = robotPosition;
            this._robotColor = Color;
        }

       
    }
}
