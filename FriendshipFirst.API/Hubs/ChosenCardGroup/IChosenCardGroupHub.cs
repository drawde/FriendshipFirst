using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FriendshipFirst.API.Hubs.ChosenCardGroup
{
    interface IChosenCardGroupHub: IHub
    {        
        void IAmReady(string param);
    }
}