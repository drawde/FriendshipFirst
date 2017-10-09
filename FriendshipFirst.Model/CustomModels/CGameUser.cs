using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendshipFirst.Model.CustomModels
{
    public class CGameUser
    {
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string HeadImg { get; set; }
        public string OpenID { get; set; }
        public string UserCode { get; set; }
        public decimal Balance { get; set; }
        public decimal BetMoney { get; set; }
        public bool IsBanker { get; set; }
        public int PlayerStatus { get; set; }
        public string RoundCode { get; set; }
        public decimal WinMoney { get; set; }
        public string GameCode { get; set; }
        public DateTime AddTime { get; set; }
        public string NextRoundCode { get; set; }
        public bool IsActivity { get; set; }
        public int GameStatus { get; set; }
        public int RoomIndex { get; set; }
    }
}
