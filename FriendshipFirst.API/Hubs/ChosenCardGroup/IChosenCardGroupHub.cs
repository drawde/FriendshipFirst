using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FriendshipFirst.API.Hubs.ChosenCardGroup
{
    interface IChosenCardGroupHub: IHub
    {        
        string IAmReady(string param);
        string IAmCancelReady(string param);
    }
}