using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Library.Infrastructure
{
    public enum MoveConsequence
    {
        ShotAndHitPlayer,
        ShotAndHitWall,
        ShootAndNothingHappen,
        InvalidMove_ShotPlayerWithNoHealthPoints,
       //PunchedPlayer,
       // InvalidMove_PunchedEmptySpace,
       //InvalidMove_PunchedPlayerWithNoHealthPoints,
        MovedToAnotherPlace,
        InvalidMove,
        PickedBigItem,
        InvalidMove_PickingBigItem,
        DroppedBigItem,
        InvalidMove_DroppingBigItem,
        PickedSmallItem,
        InvalidMove_PickingSmallItem,
        Rest,
        TimeOut,
        NULL

        //strzelilem i zabralem zycie
        //strzelilem w sciane
        //uderzylem robota
        //uderzylem i nic sie nie stalo(w pusta przestrzen)
        //uderzylem w robota a on nie mial punktow zycia
        //ruszylem sie na pole
        //invalid move
        //podnioslem duzy przedmiot
        //invalid move- podniesienie przedmiotu
        //upuscilem duzy przedmiot
        //invalid move - upuszczam tam gdzie juz jest przedmiot
        //zebralem maly przedmiot
        //odpoczywalem
        
        
        
    }
}
