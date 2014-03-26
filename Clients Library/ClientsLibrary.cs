using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Clients_Library;
using Common_Library.IService_Contracts;
using Autofac;
using Common_Library.Parts_of_map;
using Common_Library;
using Common_Library.Infrastructure;
using Clients_Library.Infrastructure;
using System.Reflection;
namespace Clients_Library
{
    public class ClientsLibrary
    {
        public string ipAddress;

        public delegate void InitialMessageReciveDelegate(object oSender, InitialMessageRecivedArgs oEventArgs);
        public  event InitialMessageReciveDelegate InitialMessageReciveEvent;

        public delegate void MessageReciveDelegate(object oSender, MessageRecivedArgs oEventArgs);
        public event MessageReciveDelegate GamePlayMessageReciveEvent;

        public delegate void GameStartDelegate(object oSender, GamePlayArgs oEventArgs);
        public  event GameStartDelegate GameStartEvent;

        public delegate void GameEndDelegate(object oSender, GamePlayArgs oEventArgs);
        public  event GameEndDelegate GameEndEvent;

        public delegate void RoundEndDelegate(object oSender, GameRoundArgs oEventArgs);
        public  event RoundEndDelegate RoundEndEvent;


        public delegate void RoundStartDelegate(object oSender, GameRoundArgs oEventArgs);
        public  event RoundStartDelegate RoundStartEvent;

        private ClientChannel<IGamePlayService> _gamePlayChannel;
        
        public CallbackHandler _callbackHandler;
        /// <summary>
        /// Clients library constructor which gets ip adress of arena server and initializes connection with server.
        /// It registers all events for callbacks from server ( messgaes send by the server to the client )
        /// </summary>
        /// <param name="ip"></param>
        public ClientsLibrary(string ip)
        {
            ipAddress = ip;
            //_callbackHandler = ClientServiceLocator.Instance.Find<CallbackHandler>();
            InitializeServerConnection();
            CallbackHandler.RegisterLoginAction((map, response) => InitialMessageReciveEvent(this, new InitialMessageRecivedArgs(map,response)));
            CallbackHandler.RegisterStartGameAction((roundNumber, team) => GameStartEvent(this, new GamePlayArgs(roundNumber, team, false)));
            CallbackHandler.RegisterEndGameAction(finalScore => GameEndEvent(this, new GamePlayArgs(finalScore, null, true)));
            CallbackHandler.RegisterGamePlayAction((map, response) => GamePlayMessageReciveEvent(this, new MessageRecivedArgs(map, response)));
            CallbackHandler.RegisterBeginRoundAction((roundNumber,possibleMoveList) => RoundStartEvent(this, new GameRoundArgs(roundNumber,possibleMoveList )));
            CallbackHandler.RegisterEndRoundAction(roundNumber => RoundEndEvent(this, new GameRoundArgs(roundNumber)));
        }

        /// <summary>
        /// Initializes connection with the server by creating tcp connection and Client channel with contract IGamePlayService
        /// </summary>
        public void InitializeServerConnection()
        {
            var builder = new ContainerBuilder();
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress endpointGameplay = new EndpointAddress("net.tcp://"+ipAddress+":8733/GamePlayService/");
            binding.Security.Mode = SecurityMode.None;
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            builder.Register(c => new InstanceContext(new CallbackHandler()));
            builder.Register(c => new DuplexChannelFactory<IGamePlayService>(c.Resolve<InstanceContext>(), binding, endpointGameplay)).AsSelf();
            builder.Register(c => new ClientChannel<IGamePlayService>(c.Resolve<DuplexChannelFactory<IGamePlayService>>().CreateChannel()));
            ClientServiceLocator.Setup(builder.Build(), true);
        }


       

        public bool MakeMove(Directions direction, int roundNumber)
        {
            try
            {
            
            _gamePlayChannel.Proxy.MakeMove(direction, roundNumber);
            }
            catch (EndpointNotFoundException e)
            {
                Console.WriteLine("Endpoind not found");
                return false;
            }
            catch (FaultException e)
            {
                Console.WriteLine("Fault exception");
                return false;
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout exception");
                return false;
            }
            return true;
        }

        public bool Rest(int roundNumber)
        {
            try
            {
                
                _gamePlayChannel.Proxy.Rest(roundNumber);
            }
            catch (EndpointNotFoundException e)
            {
                Console.WriteLine("Endpoind not found");
                return false;
            }
            catch (FaultException e)
            {
                Console.WriteLine("Fault exception");
                return false;
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout exception");
                return false;
            }
            return true;
        }


        public bool DropBigItem(int roundNumber)
        {
            try
            {

                _gamePlayChannel.Proxy.DropBigItem(roundNumber);
            }
            catch (EndpointNotFoundException e)
            {
                Console.WriteLine("Endpoind not found");
                return false;
            }
            catch (FaultException e)
            {
                Console.WriteLine("Fault exception");
                return false;
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout exception");
                return false;
            }
            return true;
        }

        public bool PickBigItem(int roundNumber)
        {
            try
            {

                _gamePlayChannel.Proxy.PickBigItem(roundNumber);
            }
            catch (EndpointNotFoundException e)
            {
                Console.WriteLine("Endpoind not found");
                return false;
            }
            catch (FaultException e)
            {
                Console.WriteLine("Fault exception");
                return false;
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout exception");
                return false;
            }
            return true;
        }

        public bool PickSmallItem(int roundNumber)
        {
            try
            {

                _gamePlayChannel.Proxy.PickSmallItem(roundNumber);
            }
            catch (EndpointNotFoundException e)
            {
                Console.WriteLine("Endpoind not found");
                return false;
            }
            catch (FaultException e)
            {
                Console.WriteLine("Fault exception");
                return false;
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout exception");
                return false;
            }
            return true;
        }


        public bool Shoot(Directions direction, int roundNumber)
        {
            try
            {

                _gamePlayChannel.Proxy.Shoot(direction, roundNumber);
            }
            catch (EndpointNotFoundException e)
            {
                Console.WriteLine("Endpoind not found");
                return false;
            }
            catch (FaultException e)
            {
                Console.WriteLine("Fault exception");
                return false;
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout exception");
                return false;
            }
            return true;
        }

        public bool Punch(Directions direction, int roundNumber)
        {
            try
            {

                _gamePlayChannel.Proxy.Punch(direction, roundNumber);
            }
            catch (EndpointNotFoundException e)
            {
                Console.WriteLine("Endpoind not found");
                return false;
            }
            catch (FaultException e)
            {
                Console.WriteLine("Fault exception");
                return false;
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout exception");
                return false;
            }
            return true;
        }

        public bool Login(string NickName)
        {
            try
            {
                 _gamePlayChannel = ClientServiceLocator.Instance.Find<ClientChannel<IGamePlayService>>();
               
                _gamePlayChannel.Proxy.Login(NickName);


            }
            catch (EndpointNotFoundException e)
            {
                Console.WriteLine("Endpoind not found");
                return false;
            }
            catch (FaultException e)
            {
                Console.WriteLine("Fault exception");
                return false;
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout exception");
                return false;
            }
           
            return true;
        }
    }
}
