using FriendshipFirst.Common;
using FriendshipFirst.Common.Enum;
using FriendshipFirst.Common.JsonModel;
using FriendshipFirst.DAL;
using FriendshipFirst.DAL.Impl;
using FriendshipFirst.Model;
using FriendshipFirst.Model.CustomModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendshipFirst.BLL
{
    public class GameRecordBll : BaseBLL<FF_GameRecord>
    {
        private IRepository<FF_GameRecord> _repository = new Repository<FF_GameRecord>();
        private GameRecordBll()
        {
        }
        public static GameRecordBll Instance = new GameRecordBll();

        public APIResultBase GetRecord(string roundCode, string userCode)
        {
            var where = LDMFilter.True<FF_GameRecord>();
            where = where.And(c => c.RoundCode == roundCode && c.UserCode == userCode);
            var lst = _repository.GetList(where, "AddTime", false).Result;
            if (lst != null && lst.TotalItemsCount > 0)
            {
                return JsonModelResult.PackageSuccess(lst.Items.FirstOrDefault());
            }
            return JsonModelResult.PackageFail(OperateResCodeEnum.查询不到需要的数据);
        }

        /// <summary>
        /// 用户离开房间
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="gameCode"></param>
        /// <returns>是否已经全部离开</returns>
        public bool LeavingRoom(string userCode, string gameCode)
        {
            bool all = false;
            using (FriendshipFirstContext context = new FriendshipFirstContext())
            {
                var record = context.ff_gamerecord.Where(c => c.UserCode == userCode && c.GameCode == gameCode).OrderByDescending(c => c.AddTime).FirstOrDefault();
                record.IsActivity = false;                    
                context.SaveChanges();
                all = context.ff_gamerecord.Any(c => c.RoundCode == record.RoundCode && c.IsActivity == true) == false;
            }
            return all;
        }

        public List<CGameUser> GetUsers(string gameCode, FriendshipFirstContext context = null)
        {
            if (context == null)
            {
                using (context = new FriendshipFirstContext())
                {
                    var data = from r in context.ff_gamerecord
                               join g in context.ff_game on r.RoundCode equals g.CurrentRoundCode
                               join u in context.ff_user on r.UserCode equals u.UserCode

                               select new CGameUser()
                               {
                                   UserName = u.UserName,
                                   NickName = u.NickName,
                                   HeadImg = u.HeadImg,
                                   OpenID = u.OpenID,
                                   UserCode = u.UserCode,
                                   Balance = r.Balance,
                                   BetMoney = r.BetMoney,
                                   IsBanker = r.IsBanker,
                                   PlayerStatus = r.PlayerStatus,
                                   RoundCode = r.RoundCode,
                                   WinMoney = r.WinMoney,
                                   GameCode = g.GameCode,
                                   AddTime = g.AddTime,
                                   NextRoundCode = g.NextRoundCode,
                                   IsActivity = r.IsActivity,
                                   GameStatus = g.GameStatus,
                                   RoomIndex = r.RoomIndex,
                                   GameStyle = g.GameStyle
                               };
                    return data.Where(c => c.GameCode == gameCode).ToList();
                }
            }
            else
            {
                var data = from r in context.ff_gamerecord
                           join g in context.ff_game on r.RoundCode equals g.CurrentRoundCode
                           join u in context.ff_user on r.UserCode equals u.UserCode

                           select new CGameUser()
                           {
                               UserName = u.UserName,
                               NickName = u.NickName,
                               HeadImg = u.HeadImg,
                               OpenID = u.OpenID,
                               UserCode = u.UserCode,
                               Balance = r.Balance,
                               BetMoney = r.BetMoney,
                               IsBanker = r.IsBanker,
                               PlayerStatus = r.PlayerStatus,
                               RoundCode = r.RoundCode,
                               WinMoney = r.WinMoney,
                               GameCode = g.GameCode,
                               AddTime = g.AddTime,
                               NextRoundCode = g.NextRoundCode,
                               IsActivity = r.IsActivity,
                               GameStatus = g.GameStatus,
                               RoomIndex = r.RoomIndex,
                               GameStyle = g.GameStyle
                           };
                return data.Where(c => c.GameCode == gameCode).ToList();
            }
        }

        public CGameUser GetUser(string userCode, string gameCode, FriendshipFirstContext context = null)
        {
            if (context == null)
            {
                using (context = new FriendshipFirstContext())
                {
                    var data = from r in context.ff_gamerecord
                               join g in context.ff_game on r.RoundCode equals g.CurrentRoundCode
                               join u in context.ff_user on r.UserCode equals u.UserCode

                               select new CGameUser()
                               {
                                   UserName = u.UserName,
                                   NickName = u.NickName,
                                   HeadImg = u.HeadImg,
                                   OpenID = u.OpenID,
                                   UserCode = u.UserCode,
                                   Balance = r.Balance,
                                   BetMoney = r.BetMoney,
                                   IsBanker = r.IsBanker,
                                   PlayerStatus = r.PlayerStatus,
                                   RoundCode = r.RoundCode,
                                   WinMoney = r.WinMoney,
                                   GameCode = g.GameCode,
                                   AddTime = r.AddTime,
                                   NextRoundCode = g.NextRoundCode,
                                   IsActivity = r.IsActivity,
                                   GameStatus = g.GameStatus,
                                   RoomIndex = r.RoomIndex,
                                   GameStyle = g.GameStyle
                               };

                    return data.Where(c => c.UserCode == userCode && c.GameCode == gameCode).OrderByDescending(c => c.AddTime).FirstOrDefault();
                }
            }
            else
            {
                var data = from r in context.ff_gamerecord
                           join g in context.ff_game on r.RoundCode equals g.CurrentRoundCode
                           join u in context.ff_user on r.UserCode equals u.UserCode

                           select new CGameUser()
                           {
                               UserName = u.UserName,
                               NickName = u.NickName,
                               HeadImg = u.HeadImg,
                               OpenID = u.OpenID,
                               UserCode = u.UserCode,
                               Balance = r.Balance,
                               BetMoney = r.BetMoney,
                               IsBanker = r.IsBanker,
                               PlayerStatus = r.PlayerStatus,
                               RoundCode = r.RoundCode,
                               WinMoney = r.WinMoney,
                               GameCode = g.GameCode,
                               AddTime = r.AddTime,
                               NextRoundCode = g.NextRoundCode,
                               IsActivity = r.IsActivity,
                               GameStatus = g.GameStatus,
                               RoomIndex = r.RoomIndex,
                               GameStyle = g.GameStyle
                           };

                return data.Where(c => c.UserCode == userCode && c.GameCode == gameCode).OrderByDescending(c => c.AddTime).FirstOrDefault();
            }
        }
    }
}
