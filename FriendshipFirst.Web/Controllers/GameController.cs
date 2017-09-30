using FriendshipFirst.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FriendshipFirst.Web.Controllers
{
    public class GameController : Controller
    {
        [OAuth]
        public ActionResult Saloon()
        {
            return View();
        }

        [OAuth]
        public ActionResult ChosenCardGroup()
        {
            return View();
        }
    }
}