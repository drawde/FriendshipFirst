//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace FriendshipFirst.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class FF_GameRecord :BaseEntity
    {
        public long ID { get; set; }
        public string RoundCode { get; set; }
        public string UserCode { get; set; }
        public decimal BetMoney { get; set; }
        public bool IsBanker { get; set; }
        public int PlayerStatus { get; set; }
        public System.DateTime AddTime { get; set; }
        public decimal WinMoney { get; set; }
        public decimal Balance { get; set; }
    }
}