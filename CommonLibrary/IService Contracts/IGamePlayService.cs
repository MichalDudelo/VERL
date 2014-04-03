using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common_Library.Parts_of_map;

namespace Common_Library.IService_Contracts
{
    [ServiceContract(CallbackContract = typeof(IArenaCallback))]
    public interface IGamePlayService
    {
        [OperationContract ]
        void Login(string user);
 
        [OperationContract]
         void MakeMove(Directions direction, int roundNumber);
        
        [OperationContract]
        void PickBigItem(int roundNumber);
        
        [OperationContract]
        void DropBigItem(int roundNumber);

        [OperationContract]
        void PickSmallItem(int roundNumber);

        //[OperationContract]
        //void Punch(Directions direction,int roundNumber);

        [OperationContract]
        void Shoot(Directions direction,int roundNumber);

        [OperationContract]
        void Rest(int roundNumber);

       
        
        
    }
}
