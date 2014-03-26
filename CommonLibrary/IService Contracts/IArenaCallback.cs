using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Common_Library.Infrastructure;

namespace Common_Library.IService_Contracts
{

    public interface IArenaCallback
    {
        [OperationContract(IsOneWay = true)]
        void reciveInitialData(byte[] initialMap, InitialServerResponse response);
        
        [OperationContract(IsOneWay = true)]
        void reciveGamePlayData(byte[] initialMap, GamePlayServerResponse response);

        [OperationContract(IsOneWay = true)]
        void gameRoundStart(int roundNumber, List<PossibleAction> possibleActionList);

        [OperationContract(IsOneWay=true)]
        void gameRoundEnd(int roundNumber);

        [OperationContract(IsOneWay = true)]
        void gamePlayEnd(double finalScore);

        [OperationContract(IsOneWay=true)]
        void gamePlayStarted(int initialGameNumber, string team);
    }
}
