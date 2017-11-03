using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FriendshipFirst.DAL;
using FriendshipFirst.DAL.Impl;
using FriendshipFirst.Model;
using FriendshipFirst.Common.JsonModel;
using FriendshipFirst.Common;
using FriendshipFirst.Common.Enum;
using System.Dynamic;
using FriendshipFirst.Model.CustomModels;
using FriendshipFirst.Common.Util;

namespace FriendshipFirst.BLL
{
    public class GameBll : BaseBLL<FF_Game>
    {
        private IRepository<FF_Game> _repository = new Repository<FF_Game>();
        private GameBll()
        {
        }
        public static GameBll Instance = new GameBll();

        public FF_Game GetGame(string tableCode, GameStatusEnum status = GameStatusEnum.无)
        {
            using (FriendshipFirstContext context = new FriendshipFirstContext())
            {
                if (status != GameStatusEnum.无)
                {
                    return context.ff_game.FirstOrDefault(c => c.GameStatus == (int)status && c.GameCode == tableCode);
                }
                else
                {
                    return context.ff_game.FirstOrDefault(c => c.GameCode == tableCode);
                }
            }
            //var where = LDMFilter.True<FF_Game>();
            //if (status != GameStatusEnum.无)
            //{
            //    where = where.And(c => c.GameStatus == (int)status);
            //}
            //where = where.And(c => c.GameCode == tableCode);

            //var lst = _repository.GetList(where, "AddTime", false).Result;
            //if (lst != null && lst.TotalItemsCount > 0)
            //{
            //    return JsonModelResult.PackageSuccess(lst.Items.FirstOrDefault());
            //}
            //return JsonModelResult.PackageFail(OperateResCodeEnum.查询不到需要的数据);
        }

        public CGameUser GetGameByUser(string userCode)
        {
            using (FriendshipFirstContext context = new FriendshipFirstContext())
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
                               RoomIndex = r.RoomIndex
                           };

                return data.Where(c => c.UserCode == userCode).OrderByDescending(c => c.AddTime).FirstOrDefault();
            }
        }

        /// <summary>
        /// 用户下注
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="betMoney"></param>
        /// <param name="tableID"></param>
        /// <returns>所有人是否已下注</returns>
        public APIResultBase Bet(string userCode, decimal betMoney, string tableCode)
        {
            using (FriendshipFirstContext context = new FriendshipFirstContext())
            {
                var gameTable = context.hs_gametable.FirstOrDefault(c => c.TableCode == tableCode);
                if (gameTable.TableStatus != (int)TableStatusEnum.正常)
                {
                    return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
                }
                var game = context.ff_game.Where(c => c.GameCode == gameTable.TableCode && c.GameStatus == (int)GameStatusEnum.初始化).OrderByDescending(c => c.AddTime).FirstOrDefault();
                var record = context.ff_gamerecord.FirstOrDefault(c => c.RoundCode == game.CurrentRoundCode && c.UserCode == userCode);
                record.BetMoney = betMoney;
                record.PlayerStatus = (int)PlayerStatusEnum.已下注;
                context.SaveChanges();


                var isNotAllBeted = context.ff_gamerecord.Any(c => c.RoundCode == game.CurrentRoundCode && c.PlayerStatus != (int)PlayerStatusEnum.已下注);
                if (isNotAllBeted == false)
                {
                    //所有人已下注
                    game.GameStatus = (int)GameStatusEnum.已开始;
                    context.SaveChanges();
                    return JsonModelResult.PackageSuccess(record);
                }
            }
            return JsonModelResult.PackageFail(OperateResCodeEnum.无);
        }

        /// <summary>
        /// 重新下注
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="tableCode"></param>
        /// <returns></returns>
        public APIResultBase CancelBet(string userCode, string tableCode)
        {
            using (FriendshipFirstContext context = new FriendshipFirstContext())
            {
                var gameTable = context.hs_gametable.FirstOrDefault(c => c.TableCode == tableCode);
                if (gameTable.TableStatus != (int)TableStatusEnum.正常)
                {
                    return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
                }
                var game = context.ff_game.Where(c => c.GameCode == tableCode).OrderByDescending(c => c.AddTime).FirstOrDefault();
                if (game.GameStatus != (int)GameStatusEnum.初始化)
                {
                    return JsonModelResult.PackageFail(OperateResCodeEnum.当前游戏无法重新下注);
                }
                var record = context.ff_gamerecord.FirstOrDefault(c => c.RoundCode == game.CurrentRoundCode && c.UserCode == userCode);
                record.BetMoney = 0;
                record.PlayerStatus = (int)PlayerStatusEnum.未准备;
                context.SaveChanges();
            }
            return JsonModelResult.PackageSuccess();
        }

        public APIResultBase GameRestart(string userCode, string tableCode)
        {
            List<CGameUser> lstRec = null;
            using (FriendshipFirstContext context = new FriendshipFirstContext())
            {
                var gameTable = context.hs_gametable.FirstOrDefault(c => c.TableCode == tableCode);
                if (gameTable.TableStatus != (int)TableStatusEnum.正常)
                {
                    return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
                }
                var game = context.ff_game.Where(c => c.GameCode == tableCode).OrderByDescending(c => c.AddTime).FirstOrDefault();
                if (game.GameStatus != (int)GameStatusEnum.已结算)
                {
                    return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
                }
                game.GameStatus = (int)GameStatusEnum.初始化;

                lstRec = GameRecordBll.Instance.GetUsers(game.GameCode, context);
                context.SaveChanges();
            }
            return JsonModelResult.PackageSuccess(lstRec);
        }

        /// <summary>
        /// 结算
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="targetUserCode"></param>
        /// <param name="gameCode"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public APIResultBase Settlement(string userCode, string targetUserCode, string gameCode, decimal money)
        {
            FF_Game game = null;
            CGameUser gameUser = null;
            using (FriendshipFirstContext context = new FriendshipFirstContext())
            {
                game = context.ff_game.Where(c => c.GameCode == gameCode).OrderByDescending(c => c.AddTime).FirstOrDefault();
                if (game.GameStatus != (int)GameStatusEnum.已开始 && game.GameStatus != (int)GameStatusEnum.结算中)
                {
                    return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
                }
                game.GameStatus = (int)GameStatusEnum.结算中;
                var lstRec = context.ff_gamerecord.Where(c => c.RoundCode == game.CurrentRoundCode);
                var record = lstRec.FirstOrDefault(c => c.RoundCode == game.CurrentRoundCode && c.UserCode == userCode);
                var targetRecord = lstRec.FirstOrDefault(c => c.RoundCode == game.CurrentRoundCode && c.UserCode == targetUserCode);
                record.BetMoney = 0;
                record.Balance += money;
                record.WinMoney += money;

                bool isAllSettlemented = false;
                if (game.GameStyle == (int)GameStyleEnum.庄家模式)
                {
                    
                    if (record.IsBanker == false)
                    {
                        record.PlayerStatus = (int)PlayerStatusEnum.已结算;
                    }
                    else if (lstRec.Count(c => c.IsBanker == false && c.PlayerStatus != (int)PlayerStatusEnum.已结算) <= 1)
                    {
                        record.PlayerStatus = (int)PlayerStatusEnum.已结算;
                    }

                    targetRecord.Balance -= money;
                    targetRecord.WinMoney -= money;
                    if (targetRecord.IsBanker == false)
                    {
                        targetRecord.PlayerStatus = (int)PlayerStatusEnum.已结算;
                    }
                    else if (lstRec.Count(c => c.IsBanker == false && c.PlayerStatus != (int)PlayerStatusEnum.已结算) <= 1)
                    {
                        targetRecord.PlayerStatus = (int)PlayerStatusEnum.已结算;
                    }
                    isAllSettlemented = (record.PlayerStatus == (int)PlayerStatusEnum.已结算 && targetRecord.PlayerStatus == (int)PlayerStatusEnum.已结算);
                }
                else
                {
                    record.PlayerStatus = (int)PlayerStatusEnum.已结算;

                    targetRecord.Balance -= money;
                    targetRecord.WinMoney -= money;
                    targetRecord.PlayerStatus = (int)PlayerStatusEnum.已结算;

                    isAllSettlemented = !lstRec.Any(c => c.PlayerStatus != (int)PlayerStatusEnum.已结算 && c.UserCode != userCode && c.UserCode != targetUserCode);
                }

                if (isAllSettlemented)
                {
                    DateTime now = DateTime.Now;
                    var lst = lstRec.ToList();
                    game.CurrentRoundCode = game.NextRoundCode;
                    game.NextRoundCode = SignUtil.CreateSign(game.BankerCode + RandomUtil.CreateRandomStr(8) + now.Ticks);

                    if (game.GameStyle == (int)GameStyleEnum.庄家模式)
                    {
                        game.GameStatus = (int)GameStatusEnum.已结算;
                        AddUserToNextBankerRound(lst, game, now, context);
                    }
                    else
                    {
                        game.GameStatus = (int)GameStatusEnum.已开始;
                        AddUserToNextFreeModelRound(lst, game, now, context);
                    }                    
                }
                context.SaveChanges();
                gameUser = GameRecordBll.Instance.GetUser(userCode, game.GameCode, context);

                //gameUser = data.Where(c => c.UserCode == userCode && c.GameCode == gameCode).OrderByDescending(c => c.AddTime).FirstOrDefault();
            }
            return JsonModelResult.PackageSuccess(gameUser);
        }

        private void AddUserToNextBankerRound(List<FF_GameRecord> lst, FF_Game game, DateTime now, FriendshipFirstContext context)
        {
            foreach (var r in lst)
            {
                FF_GameRecord model = new FF_GameRecord
                {
                    AddTime = now,
                    Balance = r.Balance,
                    BetMoney = 0,
                    GameCode = game.GameCode,
                    IsActivity = true,
                    IsBanker = r.IsBanker,
                    PlayerStatus = r.IsBanker ? (int)PlayerStatusEnum.已下注 : (int)PlayerStatusEnum.未准备,
                    RoundCode = game.CurrentRoundCode,
                    UserCode = r.UserCode,
                    WinMoney = 0,
                    RoomIndex = r.RoomIndex
                };
                context.ff_gamerecord.Add(model);
            }
        }
        private void AddUserToNextFreeModelRound(List<FF_GameRecord> lst, FF_Game game, DateTime now, FriendshipFirstContext context)
        {
            foreach (var r in lst)
            {
                FF_GameRecord model = new FF_GameRecord
                {
                    AddTime = now,
                    Balance = r.Balance,
                    BetMoney = 0,
                    GameCode = game.GameCode,
                    IsActivity = true,
                    IsBanker = r.IsBanker,
                    PlayerStatus = (int)PlayerStatusEnum.已下注,
                    RoundCode = game.CurrentRoundCode,
                    UserCode = r.UserCode,
                    WinMoney = 0,
                    RoomIndex = r.RoomIndex
                };
                context.ff_gamerecord.Add(model);
            }
        }

        /// <summary>
        /// 换庄
        /// </summary>
        /// <param name="gameCode"></param>
        /// <param name="userCode"></param>
        /// <param name="targetUserCode"></param>
        /// <returns></returns>
        public APIResultBase SwitchBanker(string gameCode, string userCode, string targetUserCode)
        {
            List<CGameUser> lstUsers = null;
            using (FriendshipFirstContext context = new FriendshipFirstContext())
            {
                var game = context.ff_game.Where(c => c.GameCode == gameCode).FirstOrDefault();
                if (game.GameStatus != (int)GameStatusEnum.已结算 && game.GameStatus != (int)GameStatusEnum.初始化)
                {
                    return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
                }

                var user = context.ff_gamerecord.FirstOrDefault(c => c.UserCode == userCode && c.RoundCode == game.CurrentRoundCode);
                var targetUser = context.ff_gamerecord.FirstOrDefault(c => c.UserCode == targetUserCode && c.RoundCode == game.CurrentRoundCode);
                if (user.IsBanker == false && targetUser.IsBanker == false)
                {
                    return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
                }
                if (user.IsBanker)
                {
                    user.IsBanker = false;
                    user.PlayerStatus = (int)PlayerStatusEnum.未准备;

                    targetUser.IsBanker = true;
                    targetUser.PlayerStatus = (int)PlayerStatusEnum.已下注;
                    targetUser.BetMoney = 0;
                }
                else
                {
                    user.IsBanker = true;
                    user.PlayerStatus = (int)PlayerStatusEnum.已下注;
                    user.BetMoney = 0;

                    targetUser.IsBanker = false;
                    targetUser.PlayerStatus = (int)PlayerStatusEnum.未准备;
                    targetUser.BetMoney = 0;
                }

                context.SaveChanges();

                lstUsers = GameRecordBll.Instance.GetUsers(game.GameCode, context);

                //lstUsers = data.Where(c => c.UserCode == userCode && c.GameCode == gameCode).OrderByDescending(c => c.AddTime).ToList();
            }
            return JsonModelResult.PackageSuccess(lstUsers);
        }
    }
}
