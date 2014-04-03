using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;
using Arena_Server.Infrastructure;
using Common_Library;
using Common_Library.Parts_of_map;
using System.IO;
using Common_Library.Infrastructure;
using ArenaServer.Infrastructure.EventArgs;
using System.Windows.Forms;
using Common_Library.XNA;
using Logger;



namespace TileEngine
{

    /// <summary>
    /// Constants used for changing turn time
    /// </summary>
    static class Constants
    {
        public const int longRoundMAX = 31;
        public const int longRoundMID25 = 25;
        public const int longRoundMID16 = 16;
        public const int longRoundMIN = 9;
        public const int longRoundMovePlus = 2;
        public const int longRoundMoveMinus = -2;

        public const int shortRoundMAX = 15;
        public const int shortRoundMID9 = 9;
        public const int shortRoundMID7 = 8;
        public const int shortRoundMIN = 4;
        public const int shortRoundMovePlus = 4;
        public const int shortRoundMoveMinus = -4;
    }


    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        Form globalForm;
        Map initialMap;
        Camera2D cam;
        ListBox gameplayListBox;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        TileMap myMap;
        string strLogPath = EventLog.GetApplicationPath() + "/GamePlayLog.txt";
        //RoundTimes
        int roundTimeMax;
        int roundTimeMidb;
        int roundTimeMids;
        int roundTimeMin;
        int movePlus;
        int moveMinus;

        int turnCounter = 0;
        int actualRound = 0;
        int totalRoundNumber;
        bool wantToAnimateTurn = false;
        double turnTime;
        int squaresAcross;
        int squaresDown;
        int numberOfCyclesForLoadAnimation = 16;


        public delegate void EndGameDelegate(object oSender, EventArgs Args);
        public static event EndGameDelegate EndGameEvent;


        public delegate void RefreshListBoxItem();
        public RefreshListBoxItem myListBoxDelegate;

        List<RobotAvatar> initialRobotAvatarList;
        Queue<KeyValuePair<SpriteAnimation, Move>> movesQueue = new Queue<KeyValuePair<SpriteAnimation, Move>>();
        Queue<List<Move>> turnMovesQueue = new Queue<List<Move>>();
        List<SpriteAnimation> robotList = new List<SpriteAnimation>();
        List<SpriteAnimation> smallItemsList = new List<SpriteAnimation>();
        List<SpriteAnimation> bigItemsList = new List<SpriteAnimation>();
        //List<SpriteAnimation> startingPositionList = new List<SpriteAnimation>();
        List<KeyValuePair<SpriteAnimation, Move>> currentRobotAnimationList = new List<KeyValuePair<SpriteAnimation, Move>>();
        Dictionary<string, double> scoreTable = new Dictionary<string, double>();


        SpriteAnimation currentWoundedRobot;




        /// <summary>
        /// Class which represents single item of gameplayListBox
        /// </summary>
        public class MyListBoxItem
        {
            public MyListBoxItem(System.Drawing.Color c, string rCD, string m, System.Drawing.Color c2, string m2)
            {
                ItemColor = c;
                ItemColor2 = c2;
                Message = m;
                Message2 = m2;
                RobotColorDescription = rCD;
            }
            public System.Drawing.Color ItemColor { get; set; }
            public System.Drawing.Color ItemColor2 { get; set; }
            public string Message { get; set; }
            public string RobotColorDescription { get; set; }
            public string Message2 { get; set; }
        }


        /// <summary>
        /// Function responsible for drawing single item (of type MyListBoxItem) of gameplayListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gameplayListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            MyListBoxItem item = gameplayListBox.Items[e.Index] as MyListBoxItem; // Get the current item and cast it to MyListBoxItem
            if (item != null)
            {
                e.Graphics.DrawString( // Draw the appropriate text in the ListBox
                    item.Message, // The message linked to the item
                    gameplayListBox.Font, // Take the font from the listbox
                    new System.Drawing.SolidBrush(item.ItemColor), // Set the color 
                    0, // X pixel coordinate
                    e.Index * gameplayListBox.ItemHeight // Y pixel coordinate.  Multiply the index by the ItemHeight defined in the listbox.
                );

                e.Graphics.DrawString( // Draw the appropriate text in the ListBox
                    item.Message2, // The message linked to the item
                    gameplayListBox.Font, // Take the font from the listbox
                    new System.Drawing.SolidBrush(item.ItemColor2), // Set the color 
                    ((int)(gameplayListBox.Width - item.Message2.Length * ((int)gameplayListBox.Font.Size) - 4)), // X pixel coordinate
                    e.Index * gameplayListBox.ItemHeight // Y pixel coordinate.  Multiply the index by the ItemHeight defined in the listbox.
                );
            }
            else
            {
                return;
                // The item isn't a MyListBoxItem, do something about it
            }
        }


        /// <summary>
        /// Constructor - takes 3 arguments complete initial list of Robot Avatars, bool variable determinig length of turn and total number of turns
        /// </summary>
        /// <param name="map"></param>
        /// <param name="robotAvatarList"></param>
        /// <param name="isExTime"></param>
        /// <param name="totalRN"></param>
        public Game1(Map map, List<RobotAvatar> robotAvatarList, double tTime, int totalRN)
        {
            initialMap = map;
            initialRobotAvatarList = robotAvatarList;
            turnTime = tTime;
            totalRoundNumber = totalRN;
            scoreTable.Clear();

            foreach (var robot in robotAvatarList)
                scoreTable.Add(robot.Login, 0);

            if (turnTime >= 1.0)
            {
                roundTimeMax = Constants.longRoundMAX;
                roundTimeMidb = Constants.longRoundMID25;
                roundTimeMids = Constants.longRoundMID16;
                roundTimeMin = Constants.longRoundMIN;
                movePlus = Constants.longRoundMovePlus;
                moveMinus = Constants.longRoundMoveMinus;
            }
            else
            {
                roundTimeMax = Constants.shortRoundMAX;
                roundTimeMidb = Constants.shortRoundMID9;
                roundTimeMids = Constants.shortRoundMID7;
                roundTimeMin = Constants.shortRoundMIN;
                movePlus = Constants.shortRoundMovePlus;
                moveMinus = Constants.shortRoundMoveMinus;
            }

            GamePlay.PlayerActionEvent += GamePlayService_PlayerActionEvent;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            movesQueue.Clear();
            squaresAcross = initialMap.MapWidth;
            squaresDown = initialMap.MapHeight;
            myMap = new TileMap(squaresAcross, squaresDown, initialMap);
        }


        /// <summary>
        /// Function responsible for updating gameplayListBox after each round
        /// </summary>
        /// <param name="moveList"></param>
        public void updateListBox(List<Move> moveList)
        {
            foreach (Move move in moveList)
            {
                var tempList = gameplayListBox.Items.Cast<MyListBoxItem>().ToList();
                var properItem = tempList.Find(it => (it.RobotColorDescription == move.Robot.Color && it.Message.Contains(move.Robot.Login)));

                if (move.Robot.Team != null)
                {
                    try
                    {
                        scoreTable[move.Robot.Login] += move.MyPay;
                        properItem.Message = (move.Robot.Login + "  " + scoreTable[move.Robot.Login].ToString());
                        properItem.Message2 = (move.Robot.Team);
                    }
                    catch (Exception e)
                    {
                        EventLog.WriteErrorToLog(strLogPath, e);
                    }
                }
                else
                {
                    try
                    {
                        scoreTable[move.Robot.Login] += move.MyPay;
                        properItem.Message = (move.Robot.Login + "  " + scoreTable[move.Robot.Login].ToString());
                        properItem.Message2 = "";
                    }
                    catch (Exception e)
                    {
                        EventLog.WriteErrorToLog(strLogPath, e);
                    }
                }

            }

            try
            {
                globalForm.Invoke(myListBoxDelegate);
            }
            catch (Exception e)
            {
                EventLog.WriteErrorToLog(strLogPath, e);
            }
        }


        /// <summary>
        /// Function that creates round region of the given region
        /// </summary>
        /// <param name="nLeftRect"></param>
        /// <param name="nTopRect"></param>
        /// <param name="nRightRect"></param>
        /// <param name="nBottomRect"></param>
        /// <param name="nWidthEllipse"></param>
        /// <param name="nHeightEllipse"></param>
        /// <returns></returns>

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn

        (
        int nLeftRect, // x-coordinate of upper-left corner
        int nTopRect, // y-coordinate of upper-left corner
        int nRightRect, // x-coordinate of lower-right corner
        int nBottomRect, // y-coordinate of lower-right corner
        int nWidthEllipse, // height of ellipse
        int nHeightEllipse // width of ellipse
        );

        //function call, it will return the rounded region of the given region
        public static System.Drawing.Region GetRoundedRegion(int controlWidth, int controlHeight)
        {
            return System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, controlWidth - 10, controlHeight - 10, 20, 20));
        }





        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            this.Window.AllowUserResizing = true;
            Form form = (Form)Control.FromHandle(Window.Handle);
            form.WindowState = FormWindowState.Maximized;
            form.Text = "VERL Visualization Round Number: " + actualRound.ToString();
            globalForm = form;
            globalForm.FormClosing += globaForm_FormClosing;
            gameplayListBox = new ListBox();
            gameplayListBox.DrawMode = DrawMode.OwnerDrawFixed;
            gameplayListBox.DrawItem += gameplayListBox_DrawItem;
            gameplayListBox.Location = new System.Drawing.Point(this.graphics.PreferredBackBufferWidth - 340, 10);
            gameplayListBox.Enabled = false;
            gameplayListBox.Name = "ListBox1";
            gameplayListBox.ItemHeight = 30;
            gameplayListBox.Font = new System.Drawing.Font("Console", 16, System.Drawing.FontStyle.Bold);
            gameplayListBox.Size = new System.Drawing.Size(340, 400);
            gameplayListBox.BackColor = System.Drawing.Color.Bisque;
            //the following Line of Code will round the edges of C# form
            gameplayListBox.Region = GetRoundedRegion(gameplayListBox.Width, gameplayListBox.Height);
            globalForm.Controls.Add(gameplayListBox);
            myListBoxDelegate = new RefreshListBoxItem(gameplayListBox.Refresh);
            base.Initialize();
            cam = new Camera2D(GraphicsDevice.Viewport);
        }


        /// <summary>
        /// Function responsible for handling message from server containing list of moves of each turn
        /// </summary>
        /// <param name="oSender"></param>
        /// <param name="playerMoveArgs"></param>
        private void GamePlayService_PlayerActionEvent(object oSender, PlayerMoveArgs playerMoveArgs)
        {
            List<Move> mvList = oSender as List<Move>;
            if (robotList.Count == 1 && mvList[0].MadeMove == MoveType.Disconnect)
                this.Exit();
            if (mvList != null)
                turnMovesQueue.Enqueue(mvList);

            updateListBox(mvList);
        }


        public void globaForm_FormClosing(object sender, FormClosingEventArgs args)
        {
            EndGameEvent(this, null);
            this.Exit();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Texture2D RobotTexDefault = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Characters/guard64.png", FileMode.Open, FileAccess.Read));
            Texture2D RobotTexBlue = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Characters/guard64_blue.png", FileMode.Open, FileAccess.Read));
            Texture2D RobotTexBrown = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Characters/guard64_brown.png", FileMode.Open, FileAccess.Read));
            Texture2D RobotTexGold = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Characters/guard64_gold.png", FileMode.Open, FileAccess.Read));
            Texture2D RobotTexGreen = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Characters/guard64_green.png", FileMode.Open, FileAccess.Read));
            Texture2D RobotTexPink = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Characters/guard64_pink.png", FileMode.Open, FileAccess.Read));
            Texture2D RobotTexRed = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Characters/guard64_red.png", FileMode.Open, FileAccess.Read));
            Texture2D RobotTexSilver = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Characters/guard64_silver.png", FileMode.Open, FileAccess.Read));
            Texture2D RobotTexViolet = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Characters/guard64_violet.png", FileMode.Open, FileAccess.Read));
            Texture2D RobotTexWhite = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Characters/guard64_white.png", FileMode.Open, FileAccess.Read));

            Texture2D TileTex = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/TileSets/tiles.png", FileMode.Open, FileAccess.Read));
            Texture2D SmallItemTex = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Items/small_item.png", FileMode.Open, FileAccess.Read));
            Texture2D BigItemTex = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Items/big_item.png", FileMode.Open, FileAccess.Read));
            //Texture2D StartPosTex = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Items/start_pos.png", FileMode.Open, FileAccess.Read));


            Tile.TileSetTexture = TileTex;


            foreach (SmallItem si in initialMap.SmallItemList)
            {
                SpriteAnimation smallItem;
                smallItem = new SpriteAnimation(SmallItemTex, null);
                smallItem.Position = new Vector2(si.X * 64, si.Y * 64);
                smallItemsList.Add(smallItem);
            }


            foreach (BigItem bi in initialMap.BigItemList)
            {
                SpriteAnimation bigItem;
                bigItem = new SpriteAnimation(BigItemTex, null);
                bigItem.Position = new Vector2(bi.X * 64, bi.Y * 64);
                bigItemsList.Add(bigItem);
            }


            //foreach (Position sp in initialMap.StartingPositionList)
            //{
            //    SpriteAnimation startingPosition;
            //    startingPosition = new SpriteAnimation(StartPosTex, null);
            //    startingPosition.Position = new Vector2(sp.X * 64, sp.Y * 64);
            //    startingPositionList.Add(startingPosition);
            //}

            foreach (RobotAvatar ra in initialRobotAvatarList)
            {
                if (ra.Color != null)
                {
                    SpriteAnimation robot;

                    #region switchOnRobotColor
                    switch (ra.Color)
                    {
                        case "blue":
                            robot = new SpriteAnimation(RobotTexBlue, ra.Login);
                            robot.Position = new Vector2(ra.RobotPosition.X * 64, ra.RobotPosition.Y * 64);
                            robotList.Add(robot);
                            gameplayListBox.Items.Add(new MyListBoxItem(System.Drawing.Color.SteelBlue, "blue", ra.Login, System.Drawing.Color.Black, ""));
                            break;
                        case "brown":
                            robot = new SpriteAnimation(RobotTexBrown, ra.Login);
                            robot.Position = new Vector2(ra.RobotPosition.X * 64, ra.RobotPosition.Y * 64);
                            robotList.Add(robot);
                            gameplayListBox.Items.Add(new MyListBoxItem(System.Drawing.Color.Sienna, "brown", ra.Login, System.Drawing.Color.Black, ""));
                            break;
                        case "gold":
                            robot = new SpriteAnimation(RobotTexGold, ra.Login);
                            robot.Position = new Vector2(ra.RobotPosition.X * 64, ra.RobotPosition.Y * 64);
                            robotList.Add(robot);
                            gameplayListBox.Items.Add(new MyListBoxItem(System.Drawing.Color.Goldenrod, "gold", ra.Login, System.Drawing.Color.Black, ""));
                            break;
                        case "green":
                            robot = new SpriteAnimation(RobotTexGreen, ra.Login);
                            robot.Position = new Vector2(ra.RobotPosition.X * 64, ra.RobotPosition.Y * 64);
                            robotList.Add(robot);
                            gameplayListBox.Items.Add(new MyListBoxItem(System.Drawing.Color.LimeGreen, "green", ra.Login, System.Drawing.Color.Black, ""));
                            break;
                        case "pink":
                            robot = new SpriteAnimation(RobotTexPink, ra.Login);
                            robot.Position = new Vector2(ra.RobotPosition.X * 64, ra.RobotPosition.Y * 64);
                            robotList.Add(robot);
                            gameplayListBox.Items.Add(new MyListBoxItem(System.Drawing.Color.HotPink, "pink", ra.Login, System.Drawing.Color.Black, ""));
                            break;
                        case "red":
                            robot = new SpriteAnimation(RobotTexRed, ra.Login);
                            robot.Position = new Vector2(ra.RobotPosition.X * 64, ra.RobotPosition.Y * 64);
                            robotList.Add(robot);
                            gameplayListBox.Items.Add(new MyListBoxItem(System.Drawing.Color.Crimson, "red", ra.Login, System.Drawing.Color.Black, ""));
                            break;
                        case "violet":
                            robot = new SpriteAnimation(RobotTexViolet, ra.Login);
                            robot.Position = new Vector2(ra.RobotPosition.X * 64, ra.RobotPosition.Y * 64);
                            robotList.Add(robot);
                            gameplayListBox.Items.Add(new MyListBoxItem(System.Drawing.Color.Indigo, "violet", ra.Login, System.Drawing.Color.Black, ""));
                            break;
                        case "white":
                            robot = new SpriteAnimation(RobotTexWhite, ra.Login);
                            robot.Position = new Vector2(ra.RobotPosition.X * 64, ra.RobotPosition.Y * 64);
                            robotList.Add(robot);
                            gameplayListBox.Items.Add(new MyListBoxItem(System.Drawing.Color.White, "white", ra.Login, System.Drawing.Color.Black, ""));
                            break;
                        case "silver":
                            robot = new SpriteAnimation(RobotTexSilver, ra.Login);
                            robot.Position = new Vector2(ra.RobotPosition.X * 64, ra.RobotPosition.Y * 64);
                            robotList.Add(robot);
                            gameplayListBox.Items.Add(new MyListBoxItem(System.Drawing.Color.DarkGray, "silver", ra.Login, System.Drawing.Color.Black, ""));
                            break;

                    }
                    #endregion
                }
                else
                {
                    SpriteAnimation robot;
                    robot = new SpriteAnimation(RobotTexDefault, ra.Login);
                    robot.Position = new Vector2(ra.RobotPosition.X * 64, ra.RobotPosition.Y * 64);
                    robotList.Add(robot);
                    gameplayListBox.Items.Add(new MyListBoxItem(System.Drawing.Color.SteelBlue, "blue", ra.Login, System.Drawing.Color.Black, ""));
                }

            }


            foreach (SpriteAnimation smallItem in smallItemsList)
            {
                smallItem.AddAnimation("IdleSmallItem", 0, 0, 64, 64, 1, 0.2f);
                smallItem.CurrentAnimation = "IdleSmallItem";
                smallItem.IsAnimating = true;
            }

            foreach (SpriteAnimation bigItem in bigItemsList)
            {
                bigItem.AddAnimation("IdleBigItem", 0, 0, 64, 64, 1, 0.2f);
                bigItem.CurrentAnimation = "IdleBigItem";
                bigItem.IsAnimating = true;
            }

            //foreach (SpriteAnimation startingPosition in startingPositionList)
            //{
            //    startingPosition.AddAnimation("IdleStartingPosition", 0, 0, 64, 64, 1, 0.2f);
            //    startingPosition.CurrentAnimation = "IdleStartingPosition";
            //    startingPosition.IsAnimating = true;
            //}

            foreach (SpriteAnimation robot in robotList)
            {
                robot.AddAnimation("WalkEast", 0, 64 * 1, 64, 64, 4, 0.05f);
                robot.AddAnimation("WalkNorth", 0, 64 * 3, 64, 64, 4, 0.05f);
                robot.AddAnimation("WalkSouth", 0, 64 * 2, 64, 64, 4, 0.05f);
                robot.AddAnimation("WalkWest", 0, 64 * 0, 64, 64, 4, 0.05f);

                robot.AddAnimation("IdleEast", 64, 64 * 1, 64, 64, 1, 0.2f);
                robot.AddAnimation("IdleNorth", 64 * 1, 64 * 3, 64, 64, 1, 0.2f);
                robot.AddAnimation("IdleSouth", 64 * 1, 64 * 2, 64, 64, 1, 0.2f);
                robot.AddAnimation("IdleWest", 64 * 2, 64 * 0, 64, 64, 1, 0.2f);
                robot.AddAnimation("IdleRest", 64 * 3, 64 * 7, 64, 64, 1, 0.2f);

                robot.AddAnimation("Appear", 0, 64 * 8, 64, 64, 4, 0.1f);
                robot.AddAnimation("Burning", 0, 64 * 6, 64, 64, 4, 0.2f);
                robot.AddAnimation("Rest", 0, 64 * 7, 64, 64, 4, 0.2f);

                robot.AddAnimation("ShootNorth", 0, 64 * 12, 64, 64, 4, 0.2f);
                robot.AddAnimation("ShootEast", 0, 64 * 10, 64, 64, 4, 0.2f);
                robot.AddAnimation("ShootSouth", 0, 64 * 11, 64, 64, 4, 0.2f);
                robot.AddAnimation("ShootWest", 0, 64 * 9, 64, 64, 4, 0.2f);

                robot.AddAnimation("WrongActionNorth", 0, 64 * 16, 64, 64, 4, 0.2f);
                robot.AddAnimation("WrongActionEast", 0, 64 * 14, 64, 64, 4, 0.2f);
                robot.AddAnimation("WrongActionWest", 0, 64 * 13, 64, 64, 4, 0.2f);
                robot.AddAnimation("WrongActionSouth", 0, 64 * 15, 64, 64, 4, 0.2f);

                robot.AddAnimation("HitNorth", 0, 64 * 20, 64, 64, 4, 0.2f);
                robot.AddAnimation("HitEast", 0, 64 * 18, 64, 64, 4, 0.2f);
                robot.AddAnimation("HitWest", 0, 64 * 17, 64, 64, 4, 0.2f);
                robot.AddAnimation("HitSouth", 0, 64 * 19, 64, 64, 4, 0.2f);

                robot.AddAnimation("HitIdleNorth", 0, 64 * 20, 64, 64, 1, 0.2f);
                robot.AddAnimation("HitIdleEast", 0, 64 * 18, 64, 64, 1, 0.2f);
                robot.AddAnimation("HitIdleWest", 0, 64 * 17, 64, 64, 1, 0.2f);
                robot.AddAnimation("HitIdleSouth", 0, 64 * 19, 64, 64, 1, 0.2f);

                robot.AddAnimation("WrongActionIdleNorth", 0, 64 * 16, 64, 64, 1, 0.2f);
                robot.AddAnimation("WrongActionIdleEast", 0, 64 * 14, 64, 64, 1, 0.2f);
                robot.AddAnimation("WrongActionIdleWest", 0, 64 * 13, 64, 64, 1, 0.2f);
                robot.AddAnimation("WrongActionIdleSouth", 0, 64 * 15, 64, 64, 1, 0.2f);

                robot.CurrentAnimation = "Appear";
                robot.IsAnimating = true;
            }

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        /// <summary>
        /// Function responsible for animating one tick of gameTime: changing robot positions and animations and state of items
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="turnCounter"></param>
        public void animateOneTick(GameTime gameTime, int turnCounter)
        {

            if (currentRobotAnimationList != null)
                foreach (KeyValuePair<SpriteAnimation, Move> kvp in currentRobotAnimationList)
                {

                    Vector2 moveVector = Vector2.Zero;
                    Vector2 moveDir = Vector2.Zero;
                    string animation = "";

                    Move move = kvp.Value;
                    SpriteAnimation currentRobot = kvp.Key;

                    switch (move.MadeMove)
                    {
                        #region MakeMoveCase

                        case MoveType.MakeMove:

                            switch (move.DirectionOfMove)
                            {

                                case Directions.Up:

                                    moveDir = new Vector2(0, moveMinus);
                                    animation = "WalkNorth";
                                    moveVector += new Vector2(0, moveMinus);

                                    if (moveDir.Length() != 0)
                                    {
                                        currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                                        if (currentRobot.CurrentAnimation != animation)
                                            currentRobot.CurrentAnimation = animation;

                                    }


                                    if (turnCounter == 0)
                                    {
                                        currentRobot.CurrentAnimation = "Idle" + currentRobot.CurrentAnimation.Substring(4);
                                    }
                                    break;


                                case Directions.Left:

                                    moveDir = new Vector2(moveMinus, 0);
                                    animation = "WalkWest";
                                    moveVector += new Vector2(moveMinus, 0);

                                    if (moveDir.Length() != 0)
                                    {
                                        currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                                        if (currentRobot.CurrentAnimation != animation)
                                            currentRobot.CurrentAnimation = animation;

                                    }



                                    if (turnCounter == 0)
                                    {
                                        currentRobot.CurrentAnimation = "Idle" + currentRobot.CurrentAnimation.Substring(4);
                                    }
                                    break;


                                case Directions.Right:

                                    moveDir = new Vector2(movePlus, 0);
                                    animation = "WalkEast";
                                    moveVector += new Vector2(movePlus, 0);

                                    if (moveDir.Length() != 0)
                                    {
                                        currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                                        if (currentRobot.CurrentAnimation != animation)
                                            currentRobot.CurrentAnimation = animation;

                                    }



                                    if (turnCounter == 0)
                                    {
                                        currentRobot.CurrentAnimation = "Idle" + currentRobot.CurrentAnimation.Substring(4);
                                    }
                                    break;

                                case Directions.Down:

                                    moveDir = new Vector2(0, movePlus);
                                    animation = "WalkSouth";
                                    moveVector += new Vector2(0, movePlus);

                                    if (moveDir.Length() != 0)
                                    {
                                        currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                                        if (currentRobot.CurrentAnimation != animation)
                                            currentRobot.CurrentAnimation = animation;


                                    }


                                    if (turnCounter == 0)
                                    {
                                        currentRobot.CurrentAnimation = "Idle" + currentRobot.CurrentAnimation.Substring(4);
                                    }

                                    break;

                            }
                            break;
                        #endregion
                        #region PunchCase
                        //case MoveType.Punch:

                        //    if (move.WoundedRobot != null)
                        //        currentWoundedRobot = robotList.Find(wounded => (wounded.RobotName.Equals(move.WoundedRobot.Login)));

                        //    switch (move.DirectionOfMove)
                        //    {
                        //        case Directions.Down:

                        //            if (turnCounter >= roundTimeMids && turnCounter <= roundTimeMax)
                        //            {
                        //                moveDir = new Vector2(0, movePlus);
                        //                animation = "WalkSouth";
                        //                moveVector += new Vector2(0, movePlus);

                        //                if (moveDir.Length() != 0)
                        //                {
                        //                    currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                        //                    if (currentRobot.CurrentAnimation != animation)
                        //                        currentRobot.CurrentAnimation = animation;


                        //                }


                        //            }
                        //            else if (turnCounter < roundTimeMids && turnCounter >= 0)
                        //            {
                        //                moveDir = new Vector2(0, moveMinus);
                        //                animation = "WalkSouth";
                        //                moveVector += new Vector2(0, moveMinus);

                        //                if (turnCounter >= roundTimeMin)
                        //                {
                        //                    if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                        //                        currentWoundedRobot.CurrentAnimation = "HitNorth";
                        //                }

                        //                if (moveDir.Length() != 0)
                        //                {
                        //                    currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                        //                    if (currentRobot.CurrentAnimation != animation)
                        //                        currentRobot.CurrentAnimation = animation;
                        //                }
                        //                if (turnCounter == 0)
                        //                {
                        //                    if (!currentRobot.CurrentAnimation.Contains("Hit"))
                        //                        currentRobot.CurrentAnimation = "IdleSouth";
                        //                    if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                        //                        currentWoundedRobot.CurrentAnimation = "HitIdleNorth";
                        //                }

                        //            }

                        //            break;


                        //        case Directions.Left:

                        //            if (turnCounter >= roundTimeMids && turnCounter <= roundTimeMax)
                        //            {
                        //                moveDir = new Vector2(moveMinus, 0);
                        //                animation = "WalkWest";
                        //                moveVector += new Vector2(moveMinus, 0);

                        //                if (moveDir.Length() != 0)
                        //                {
                        //                    currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                        //                    if (currentRobot.CurrentAnimation != animation)
                        //                        currentRobot.CurrentAnimation = animation;

                        //                }

                        //            }
                        //            else if (turnCounter < roundTimeMids && turnCounter >= 0)
                        //            {
                        //                moveDir = new Vector2(movePlus, 0);
                        //                animation = "WalkWest";
                        //                moveVector += new Vector2(movePlus, 0);

                        //                if (turnCounter >= roundTimeMin)
                        //                {
                        //                    if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                        //                        currentWoundedRobot.CurrentAnimation = "HitEast";
                        //                }

                        //                if (moveDir.Length() != 0)
                        //                {
                        //                    currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                        //                    if (currentRobot.CurrentAnimation != animation)
                        //                        currentRobot.CurrentAnimation = animation;

                        //                }
                        //                if (turnCounter == 0)
                        //                {
                        //                    if (!currentRobot.CurrentAnimation.Contains("Hit"))
                        //                        currentRobot.CurrentAnimation = "IdleWest";
                        //                    if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                        //                        currentWoundedRobot.CurrentAnimation = "HitIdleEast";
                        //                }

                        //            }


                        //            break;

                        //        case Directions.Right:
                        //            if (turnCounter >= roundTimeMids && turnCounter <= roundTimeMax)
                        //            {
                        //                moveDir = new Vector2(movePlus, 0);
                        //                animation = "WalkEast";
                        //                moveVector += new Vector2(movePlus, 0);

                        //                if (moveDir.Length() != 0)
                        //                {
                        //                    currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                        //                    if (currentRobot.CurrentAnimation != animation)
                        //                        currentRobot.CurrentAnimation = animation;

                        //                }


                        //            }
                        //            else if (turnCounter < roundTimeMids && turnCounter >= 0)
                        //            {
                        //                moveDir = new Vector2(moveMinus, 0);
                        //                animation = "WalkEast";
                        //                moveVector += new Vector2(moveMinus, 0);

                        //                if (turnCounter >= roundTimeMin)
                        //                {
                        //                    if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                        //                        currentWoundedRobot.CurrentAnimation = "HitWest";
                        //                }

                        //                if (moveDir.Length() != 0)
                        //                {
                        //                    currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                        //                    if (currentRobot.CurrentAnimation != animation)
                        //                        currentRobot.CurrentAnimation = animation;

                        //                }
                        //                if (turnCounter == 0)
                        //                {
                        //                    if (!currentRobot.CurrentAnimation.Contains("Hit"))
                        //                        currentRobot.CurrentAnimation = "IdleEast";
                        //                    if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                        //                        currentWoundedRobot.CurrentAnimation = "HitIdleWest";
                        //                }
                        //            }

                        //            break;

                        //        case Directions.Up:
                        //            if (turnCounter >= roundTimeMids && turnCounter <= roundTimeMax)
                        //            {
                        //                moveDir = new Vector2(0, moveMinus);
                        //                animation = "WalkNorth";
                        //                moveVector += new Vector2(0, moveMinus);

                        //                if (moveDir.Length() != 0)
                        //                {
                        //                    currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                        //                    if (currentRobot.CurrentAnimation != animation)
                        //                        currentRobot.CurrentAnimation = animation;

                        //                }

                        //            }
                        //            else if (turnCounter < roundTimeMids && turnCounter >= 0)
                        //            {
                        //                moveDir = new Vector2(0, movePlus);
                        //                animation = "WalkNorth";
                        //                moveVector += new Vector2(0, movePlus);

                        //                if (turnCounter >= roundTimeMin)
                        //                {
                        //                    if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                        //                        currentWoundedRobot.CurrentAnimation = "HitSouth";
                        //                }

                        //                if (moveDir.Length() != 0)
                        //                {
                        //                    currentRobot.MoveBy((int)moveDir.X, (int)moveDir.Y);

                        //                    if (currentRobot.CurrentAnimation != animation)
                        //                        currentRobot.CurrentAnimation = animation;

                        //                }
                        //                if (turnCounter == 0)
                        //                {
                        //                    if (!currentRobot.CurrentAnimation.Contains("Hit"))
                        //                        currentRobot.CurrentAnimation = "IdleNorth";
                        //                    if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                        //                        currentWoundedRobot.CurrentAnimation = "HitIdleSouth";
                        //                }
                        //            }


                        //            break;

                        //    }
                        //    break;
                        #endregion
                        #region RestCase
                        case MoveType.Rest:

                            if (turnCounter >= roundTimeMidb && turnCounter <= roundTimeMax)
                            {
                                currentRobot.CurrentAnimation = "Rest";

                            }
                            else if (turnCounter > 0 && turnCounter < roundTimeMidb)
                            {
                                currentRobot.CurrentAnimation = "IdleRest";

                            }
                            else if (turnCounter == 0)
                            {

                            }
                            break;
                        #endregion
                        #region ShootCase
                        case MoveType.Shoot:

                            if (move.WoundedRobot != null)
                                currentWoundedRobot = robotList.Find(wounded => (wounded.RobotName.Equals(move.WoundedRobot.Login)));

                            switch (move.DirectionOfMove)
                            {
                                case Directions.Up:
                                    if (turnCounter >= roundTimeMidb && turnCounter <= roundTimeMax)
                                    {
                                        currentRobot.CurrentAnimation = "ShootNorth";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitSouth";


                                    }
                                    else if (turnCounter > 0 && turnCounter < roundTimeMidb)
                                    {
                                        currentRobot.CurrentAnimation = "IdleNorth";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitIdleSouth";

                                    }
                                    else if (turnCounter == 0)
                                    {
                                        currentRobot.CurrentAnimation = "ShootNorth";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitIdleSouth";
                                    }
                                    break;

                                case Directions.Left:
                                    if (turnCounter >= roundTimeMidb && turnCounter <= roundTimeMax)
                                    {
                                        currentRobot.CurrentAnimation = "ShootWest";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitEast";

                                    }
                                    else if (turnCounter > 0 && turnCounter < roundTimeMidb)
                                    {
                                        currentRobot.CurrentAnimation = "IdleWest";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitIdleEast";

                                    }
                                    else if (turnCounter == 0)
                                    {
                                        currentRobot.CurrentAnimation = "ShootWest";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitIdleEast";
                                    }
                                    break;

                                case Directions.Right:
                                    if (turnCounter >= roundTimeMidb && turnCounter <= roundTimeMax)
                                    {
                                        currentRobot.CurrentAnimation = "ShootEast";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitWest";

                                    }
                                    else if (turnCounter > 0 && turnCounter < roundTimeMidb)
                                    {
                                        currentRobot.CurrentAnimation = "IdleEast";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitIdleWest";

                                    }
                                    else if (turnCounter == 0)
                                    {
                                        currentRobot.CurrentAnimation = "ShootEast";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitIdleWest";
                                    }
                                    break;

                                case Directions.Down:
                                    if (turnCounter >= roundTimeMidb && turnCounter <= roundTimeMax)
                                    {
                                        currentRobot.CurrentAnimation = "ShootSouth";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitNorth";

                                    }
                                    else if (turnCounter > 0 && turnCounter < roundTimeMidb)
                                    {
                                        currentRobot.CurrentAnimation = "IdleSouth";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitIdleNorth";

                                    }
                                    else if (turnCounter == 0)
                                    {
                                        currentRobot.CurrentAnimation = "ShootSouth";
                                        if (currentWoundedRobot != null && move.HostileMode && currentWoundedRobot.CurrentAnimation != "Burning")
                                            currentWoundedRobot.CurrentAnimation = "HitIdleNorth";
                                    }
                                    break;
                            }
                            break;
                        #endregion
                        #region PickSmallItemCase
                        case MoveType.PickSmallItem:

                            if (turnCounter == roundTimeMax)
                            {
                                SpriteAnimation smallItm;
                                smallItm = smallItemsList.Find(small => small.Position.X == (currentRobot.Position.X) && small.Position.Y == (currentRobot.Position.Y));
                                if (smallItm != null)
                                    smallItemsList.Remove(smallItm);
                                Draw(gameTime);

                            }
                            else if (turnCounter == 0)
                            {

                            }

                            break;
                        #endregion
                        #region PickBigItemCase
                        case MoveType.PickBigItem:
                            if (turnCounter == roundTimeMax)
                            {
                                SpriteAnimation bigItm;
                                bigItm = bigItemsList.Find(big => big.Position.X == (currentRobot.Position.X) && big.Position.Y == (currentRobot.Position.Y));
                                if (bigItm != null)
                                    bigItemsList.Remove(bigItm);
                                Draw(gameTime);

                            }
                            else if (turnCounter == 0)
                            {

                            }
                            break;

                        #endregion
                        #region DropBigItemCase
                        case MoveType.DropBigItem:

                            if (turnCounter == roundTimeMax)
                            {
                                Texture2D BigItemTex = Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("XNA/Textures/Items/big_item.png", FileMode.Open, FileAccess.Read));
                                SpriteAnimation bigItem;
                                bigItem = new SpriteAnimation(BigItemTex, null);
                                bigItem.Position = new Vector2(currentRobot.Position.X, currentRobot.Position.Y);
                                bigItemsList.Add(bigItem);
                                bigItem.AddAnimation("IdleBigItem", 0, 0, 64, 64, 1, 0.2f);
                                bigItem.CurrentAnimation = "IdleBigItem";
                                bigItem.IsAnimating = true;
                                Draw(gameTime);
                                //bigItem.Draw(spriteBatch, 0, 0);

                            }
                            else if (turnCounter == 0)
                            {

                            }

                            break;
                        #endregion
                        #region WrongActionCase
                        case MoveType.WrongAction:

                            switch (move.DirectionOfMove)
                            {
                                case Directions.Up:

                                    if (turnCounter >= roundTimeMidb && turnCounter <= roundTimeMax)
                                    {
                                        currentRobot.CurrentAnimation = "WrongActionNorth";

                                    }
                                    else if (turnCounter > 0 && turnCounter < roundTimeMidb)
                                    {
                                        currentRobot.CurrentAnimation = "WrongActionIdleNorth";

                                    }
                                    else if (turnCounter == 0)
                                    {

                                    }

                                    break;

                                case Directions.Left:

                                    if (turnCounter >= roundTimeMidb && turnCounter <= roundTimeMax)
                                    {
                                        currentRobot.CurrentAnimation = "WrongActionWest";

                                    }
                                    else if (turnCounter > 0 && turnCounter < roundTimeMidb)
                                    {
                                        currentRobot.CurrentAnimation = "WrongActionIdleWest";

                                    }
                                    else if (turnCounter == 0)
                                    {

                                    }

                                    break;

                                case Directions.Right:

                                    if (turnCounter >= roundTimeMidb && turnCounter <= roundTimeMax)
                                    {
                                        currentRobot.CurrentAnimation = "WrongActionEast";

                                    }
                                    else if (turnCounter > 0 && turnCounter < roundTimeMidb)
                                    {
                                        currentRobot.CurrentAnimation = "WrongActionIdleEast";

                                    }
                                    else if (turnCounter == 0)
                                    {

                                    }

                                    break;

                                case Directions.Down:

                                    if (turnCounter >= roundTimeMidb && turnCounter <= roundTimeMax)
                                    {
                                        currentRobot.CurrentAnimation = "WrongActionSouth";

                                    }
                                    else if (turnCounter > 0 && turnCounter < roundTimeMidb)
                                    {
                                        currentRobot.CurrentAnimation = "WrongActionIdleSouth";

                                    }
                                    else if (turnCounter == 0)
                                    {

                                    }

                                    break;

                            }

                            break;
                        #endregion
                        #region DisconnectRobotCase
                        case MoveType.Disconnect:
                            if (turnCounter == roundTimeMax)
                            {
                                SpriteAnimation disconnectedRobot;
                                disconnectedRobot = currentRobot;
                                if (disconnectedRobot != null)
                                    robotList.Remove(disconnectedRobot);
                                Draw(gameTime);

                            }
                            else if (turnCounter == 0)
                            {

                            }
                            break;

                        #endregion
                        #region BurningCase
                        case MoveType.Burn:
                            if (turnCounter == roundTimeMax)
                            {
                                currentRobot.CurrentAnimation = "Burning";
                            }
                            break;
                        #endregion
                    }
                }

            foreach (SpriteAnimation robot in robotList)
            {
                robot.Update(gameTime);

            }

            foreach (SpriteAnimation smallItem in smallItemsList)
            {
                smallItem.Update(gameTime);
            }

            foreach (SpriteAnimation bigItem in bigItemsList)
            {
                bigItem.Update(gameTime);
            }

            //foreach (SpriteAnimation startingPosition in startingPositionList)
            //{
            //    startingPosition.Update(gameTime);
            //}

            cam.Update();

        }



        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                this.Exit();



            if (turnCounter == 0 && !wantToAnimateTurn)
            {
                if (turnMovesQueue.Count > 0)
                {
                    currentRobotAnimationList.Clear();
                    var rList = turnMovesQueue.Dequeue();
                    if (totalRoundNumber > 0)
                        totalRoundNumber--;

                    foreach (Move mv in rList)
                    {
                        currentRobotAnimationList.Add(new KeyValuePair<SpriteAnimation, Move>((robotList.Find(robot => robot.RobotName.Equals(mv.Robot.Login))), mv));

                    }
                    turnCounter = roundTimeMax;
                    wantToAnimateTurn = true;
                    actualRound++;
                    globalForm.Text = "VERL Visualization Round Number: " + actualRound.ToString();

                }
            }


            if (turnCounter >= 0 && wantToAnimateTurn)
            {
                animateOneTick(gameTime, turnCounter);
                if (turnCounter == 0)
                    wantToAnimateTurn = false;
            }


            cam.Update();
            base.Update(gameTime);
            if (turnCounter > 0)
                turnCounter--;

            if (totalRoundNumber == 0 && turnMovesQueue.Count == 0 && wantToAnimateTurn == false)
            {
                var finalMsg = MessageBox.Show("End of Game!", "End of Game!", MessageBoxButtons.OK);
                if (finalMsg == DialogResult.OK)
                    this.Exit();
            }

        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SteelBlue);


            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, cam.Transform);

            for (int y = 0; y < squaresDown; y++)
            {
                for (int x = 0; x < squaresAcross; x++)
                {

                    spriteBatch.Draw(
                        Tile.TileSetTexture,
                        new Rectangle((x * 64), (y * 64), 64, 64),
                        Tile.GetSourceRectangle(myMap.Rows[y].Columns[x].TileID),
                        Color.White);
                }
            }


            foreach (SpriteAnimation smallItem in smallItemsList)
            {
                smallItem.Draw(spriteBatch, 0, 0);
            }

            foreach (SpriteAnimation bigItem in bigItemsList)
            {
                bigItem.Draw(spriteBatch, 0, 0);
            }

            //foreach (SpriteAnimation startingPosition in startingPositionList)
            //{
            //    startingPosition.Draw(spriteBatch, 0, 0);
            //}

            foreach (SpriteAnimation robot in robotList)
            {
                robot.Draw(spriteBatch, 0, 0);
            }

            if (numberOfCyclesForLoadAnimation-- == -1)
                foreach (SpriteAnimation robot in robotList)
                {
                    robot.CurrentAnimation = "IdleSouth";
                }

            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
