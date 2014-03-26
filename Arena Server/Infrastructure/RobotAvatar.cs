using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common_Library.Parts_of_map;
using Common_Library.Infrastructure;
using Common_Library;
namespace Arena_Server.Infrastructure
{
    public class RobotAvatar
    {
        private string _login;
        private int _errorNumber = 0;
      //  private Move move;
        private int healRounds = 0;
        private string _team;
        private Position _initialPosition;

        public Position InitialPosition
        {
            get { return _initialPosition; }
            set { _initialPosition = value; }
        }

        public string Team
        {
            get { return _team; }
            set { _team = value; }
        }

        public int HealRounds
        {
            get { return healRounds; }
            set { healRounds = value; }
        }
        private bool isHealed = true;

        public bool IsHealed
        {
            get { return isHealed; }
            set { isHealed = value; }
        }

        public int ErrorNumber
        {
            get { return _errorNumber; }
            set { _errorNumber = value; }
        }

        public string Login
        {
            get { return _login; }
            set { _login = value; }
        }
        private Position _robotPosition;

        public Position RobotPosition
        {
            get { return _robotPosition; }
            set { _robotPosition = value; }
        }
        private int _healthPoints = 5;
     
        public int HealthPoints
        {
            get { return _healthPoints; }
            set { _healthPoints = value; }
        }
        private string _color;

        public string Color
        {
            get { return _color; }
            set { _color = value; }
        }
        private bool _hasBigItem = false;

        public bool HasBigItem
        {
            get { return _hasBigItem; }
            set { _hasBigItem = value; }
        }
        private int _smallItem = 0;

        public int SmallItem
        {
            get { return _smallItem; }
            set { _smallItem = value; }
        }

        private Map _currentMapForRobot;

        public Map CurrentMapForRobot
        {
            get { return _currentMapForRobot; }
            set { _currentMapForRobot = value; }
        }


        public RobotAvatar(string login, string Color, string Team)
        {
            _login = login;
            _color = Color;
            _team = Team;
            
        }

        public void Reset()
        {
            this.IsHealed = true;
            this.RobotPosition = this.InitialPosition;
            this.HealthPoints = 4;
            this.HasBigItem = false;
            this.ErrorNumber = 0;
            this.SmallItem = 0;
            this.HealRounds = 0;
        }
        public override string ToString()
        {
            return Login;
        }
    }
}
