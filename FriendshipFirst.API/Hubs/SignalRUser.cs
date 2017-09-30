using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FriendshipFirst.API.Hubs
{
    public class SignalRUser
    {
        public string UserCode { get; set; }

        ///昵称
        public string NickName { get; set; }        

        /// <summary>
        /// 选择的卡组
        /// </summary>
        public string ChosenCardGroupCode { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 是否是庄家
        /// </summary>
        public bool IsBanker { get; set; } = false;

        /// <summary>
        /// 下注金额
        /// </summary>
        public decimal BetMoney { get; set; } = 0;

        public int PlayerStatus { get; set; }

        public int Position { get; set; } = -1;
    }
}