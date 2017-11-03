using FriendshipFirst.DAL;
using FriendshipFirst.DAL.Impl;
using FriendshipFirst.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FriendshipFirst.Common.Util;
using FriendshipFirst.Common.JsonModel;
using FriendshipFirst.Common.Enum;
using FriendshipFirst.Model.CustomModels;

namespace FriendshipFirst.BLL
{
    public class GameTableBll : BaseBLL<HS_GameTable>
    {
        private IRepository<HS_GameTable> _repository = new Repository<HS_GameTable>();
        private GameTableBll()
        {
        }
        public static GameTableBll Instance = new GameTableBll();

        public APIResultBase AddOrUpdate(HS_GameTable gameTable, GameStyleEnum GameStyle)
        {
            if (gameTable.CreateUserCode.IsNullOrEmpty() || gameTable.TableName.IsNullOrEmpty())
            {
                return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
            }
            if (_repository.Get(c => (c.CreateUserCode == gameTable.CreateUserCode || c.PlayerUserCode == gameTable.CreateUserCode) && gameTable.ID < 1 && c.TableStatus == (int)TableStatusEnum.正常).Result.TotalItemsCount > 0)
            {
                return JsonModelResult.PackageFail(OperateResCodeEnum.同时只能创建或占用一个游戏房间);
            }
            if (gameTable.ID > 0)
            {
                //gameTable.AddTime = _repository.GetByKey(gameTable.ID).Result.AddTime;
                _repository.Update(gameTable);
                return JsonModelResult.PackageSuccess(gameTable.ID.ToString());
            }
            else
            {
                DateTime now = DateTime.Now;
                gameTable.PlayerUserCode = "";
                gameTable.TableCode = SignUtil.CreateSign(UsersBll.Instance.GetUserByAdmin(gameTable.CreateUserCode).SecretCode + RandomUtil.CreateRandomStr(8) + now.Ticks);
                gameTable.AddTime = DateTime.Now;
                using (FriendshipFirstContext context = new FriendshipFirstContext())
                {
                    gameTable.BankerCode = gameTable.CreateUserCode;
                    context.hs_gametable.Add(gameTable);
                    FF_Game game = new FF_Game();
                    game.AddTime = now;
                    game.BankerCode = gameTable.CreateUserCode;
                    game.GameStatus = GameStyle == GameStyleEnum.庄家模式 ? (int)GameStatusEnum.初始化 : (int)GameStatusEnum.已开始;
                    game.CurrentRoundCode = SignUtil.CreateSign(gameTable.CreateUserCode + RandomUtil.CreateRandomStr(8) + game.AddTime.Ticks);
                    game.NextRoundCode = SignUtil.CreateSign(gameTable.CreateUserCode + RandomUtil.CreateRandomStr(8) + game.AddTime.AddMinutes(5).Ticks);
                    game.GameCode = gameTable.TableCode;
                    game.GameStyle = (int)GameStyle;
                    context.ff_game.Add(game);

                    FF_GameRecord record = new FF_GameRecord
                    {
                        AddTime = now,
                        BetMoney = 0,
                        IsBanker = true,
                        PlayerStatus = (int)PlayerStatusEnum.已下注,
                        RoundCode = game.CurrentRoundCode,
                        UserCode = game.BankerCode,
                        WinMoney = 0,
                        Balance = 0,
                        GameCode = game.GameCode,
                        IsActivity = false,
                        RoomIndex = 0
                    };
                    context.ff_gamerecord.Add(record);

                    context.SaveChanges();

                }
                return JsonModelResult.PackageSuccess<HS_GameTable>(gameTable);
            }
            
        }

        public HS_GameTable GetTable(string tableCode)
        {
            return _repository.Get(c => c.TableCode == tableCode).Result.Items.FirstOrDefault();
        }

        /// <summary>
        /// 占座儿
        /// </summary>
        /// <param name="gameTableID"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public APIResultBase ZhanZuoEr(string tableCode, string userCode, string password)
        {
            if (tableCode.IsNullOrEmpty() || userCode.IsNullOrEmpty())
            {
                return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
            }
            var lstTables = _repository.GetList(c => c.TableCode == tableCode && c.Password == password).Result;

            if (lstTables.TotalItemsCount < 1)
            {
                return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
            }
            var gameTable = lstTables.Items.First();
            //if (gameTable.TableStatus != (int)TableStatusEnum.正常)
            //{
            //    return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
            //}


            if (_repository.Get(c => (c.PlayerUserCode == userCode || c.CreateUserCode == userCode) && c.TableCode != tableCode && c.TableStatus == (int)TableStatusEnum.正常).Result.TotalItemsCount > 0)
            {
                return JsonModelResult.PackageFail(OperateResCodeEnum.同时只能创建或占用一个游戏房间);
            }
            CGameUser recordRes = null;
            using (FriendshipFirstContext context = new FriendshipFirstContext())
            {
                var game = GameBll.Instance.GetGame(gameTable.TableCode);

                if (game != null)
                {
                    var data = GameRecordBll.Instance.GetUsers(game.GameCode,context);
                    
                    recordRes = data.FirstOrDefault(c => c.RoundCode == game.CurrentRoundCode && c.UserCode == userCode);
                    
                    if (game.GameStatus == (int)GameStatusEnum.结算中)
                    {
                        //if (recordRes != null)
                        //{
                        //    return JsonModelResult.PackageFail(OperateResCodeEnum.游戏已经开始);
                        //}
                        if (recordRes == null)
                        {
                            return JsonModelResult.PackageFail(OperateResCodeEnum.游戏已经开始);
                        }
                        if (recordRes.PlayerStatus != (int)PlayerStatusEnum.已下注)
                        {
                            return JsonModelResult.PackageFail(OperateResCodeEnum.游戏已经开始);
                        }
                    }

                    if (recordRes == null)
                    {
                        context.ff_gamerecord.Add(new FF_GameRecord
                        {
                            AddTime = DateTime.Now,
                            BetMoney = 0,
                            IsBanker = false,
                            PlayerStatus = game.GameStyle == (int)GameStyleEnum.自由模式 ? (int)PlayerStatusEnum.已下注 : (int)PlayerStatusEnum.未准备,
                            RoundCode = game.CurrentRoundCode,
                            UserCode = userCode,
                            WinMoney = 0,
                            Balance = 0,
                            GameCode = game.GameCode,
                            IsActivity = true,
                            RoomIndex = data.Count(c => c.RoundCode == game.CurrentRoundCode)
                        });
                    }
                    else
                    {
                        if (!recordRes.IsActivity)
                        {
                            recordRes.IsActivity = true;
                        }
                    }
                    context.SaveChanges();
                    if (recordRes == null)
                    {
                        recordRes = context.ff_gamerecord.Join(context.ff_user, g => g.UserCode, u => u.UserCode, (g, u) => new CGameUser
                        {
                            UserName = u.UserName,
                            NickName = u.NickName,
                            HeadImg = u.HeadImg,
                            OpenID = u.OpenID,
                            UserCode = u.UserCode,
                            Balance = g.Balance,
                            BetMoney = g.BetMoney,
                            IsBanker = g.IsBanker,
                            PlayerStatus = g.PlayerStatus,
                            RoundCode = g.RoundCode,
                            WinMoney = g.WinMoney,
                            GameCode = g.GameCode,
                            AddTime = g.AddTime,
                            RoomIndex = g.RoomIndex
                        }).Where(c => c.UserCode == userCode && c.GameCode == game.GameCode).OrderByDescending(c => c.AddTime).FirstOrDefault();
                    }
                }
                //var game = GameBll.Instance.GetGame(gameTable.TableCode);
                //FF_GameRecord record = null;
                //if (game != null)
                //{
                //    var recordRes = GameRecordBll.Instance.GetRecord(game.CurrentRoundCode, userCode);

                //    if (game.GameStatus == (int)GameStatusEnum.结算中)
                //    {
                //        if (recordRes.code == (int)OperateResCodeEnum.查询不到需要的数据)
                //        {
                //            return JsonModelResult.PackageFail(OperateResCodeEnum.游戏已经开始);
                //        }
                //        var tempRec = ((APISingleModelResult<FF_GameRecord>)recordRes).data;
                //        if (tempRec.PlayerStatus != (int)PlayerStatusEnum.已下注)
                //        {                        
                //            return JsonModelResult.PackageFail(OperateResCodeEnum.游戏已经开始);
                //        }
                //    }
                //    if (recordRes.code == (int)OperateResCodeEnum.查询不到需要的数据)
                //    {
                //        record = new FF_GameRecord
                //        {
                //            AddTime = DateTime.Now,
                //            BetMoney = 0,
                //            IsBanker = false,
                //            PlayerStatus = (int)PlayerStatusEnum.未准备,
                //            RoundCode = game.CurrentRoundCode,
                //            UserCode = userCode,
                //            WinMoney = 0,
                //            Balance = 0,
                //            GameCode = game.GameCode,
                //            IsActivity = true
                //        };
                //        GameRecordBll.Instance.Insert(record);
                //    }
                //    else
                //    {
                //        record = ((APISingleModelResult<FF_GameRecord>)recordRes).data;
                //        if (!record.IsActivity)
                //        {
                //            record.IsActivity = true;
                //            GameRecordBll.Instance.Update(record);
                //        }                    
                //    }
                //}
                return JsonModelResult.PackageSuccess(recordRes);
            }
        }
        
    }
}
