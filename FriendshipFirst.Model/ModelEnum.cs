using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendshipFirst.Model
{
    public enum InvitationStatus
    {
        无 = 0,
        已使用 = 2,
        未使用 = 1
    }

    public enum DataSourceEnum
    {
        API = 1,
        SignalR = 2,
        GameControler = 3,
        Web = 4,
        Admin = 5
    }

    /// <summary>
    /// 游戏状态
    /// </summary>
    public enum GameStatusEnum
    {
        无 = 0,
        初始化 = 1,
        已开始 = 2,
        结算中 = 3,
        已结算 = 4,
    }

    /// <summary>
    /// 玩家状态
    /// </summary>
    public enum PlayerStatusEnum
    {
        未准备 = 1,
        已下注 = 2,
        已结算 = 3,
    }

    public enum TableStatusEnum
    {
        正常 = 1,
        关闭 = 2,
    }

    public enum GameStyleEnum
    {
        庄家模式 = 1,
        自由模式 = 2,
    }
}
