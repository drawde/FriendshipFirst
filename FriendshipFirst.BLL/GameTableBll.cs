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

namespace FriendshipFirst.BLL
{
    public class GameTableBll : BaseBLL<HS_GameTable>
    {
        private IRepository<HS_GameTable> _repository = new Repository<HS_GameTable>();
        private GameTableBll()
        {
        }
        public static GameTableBll Instance = new GameTableBll();

        public APITextResult AddOrUpdate(HS_GameTable gameTable)
        {
            if (gameTable.CreateUserCode.IsNullOrEmpty() || gameTable.TableName.IsNullOrEmpty())
            {
                return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
            }
            if (_repository.Get(c => (c.CreateUserCode == gameTable.CreateUserCode || c.PlayerUserCode == gameTable.CreateUserCode) && gameTable.ID < 1).Result.TotalItemsCount > 0)
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
                    game.GameStatus = (int)GameStatusEnum.初始化;
                    game.RoundCode = SignUtil.CreateSign(gameTable.CreateUserCode + RandomUtil.CreateRandomStr(8) + game.AddTime.Ticks);
                    game.TableCode = gameTable.TableCode;
                    context.ff_game.Add(game);

                    FF_GameRecord record = new FF_GameRecord();
                    record.AddTime = now;
                    record.BetMoney = 0;
                    record.IsBanker = true;
                    record.PlayerStatus = (int)PlayerStatusEnum.已下注;
                    record.RoundCode = game.RoundCode;
                    record.UserCode = game.BankerCode;
                    record.WinMoney = 0;
                    record.Balance = 0;
                    context.ff_gamerecord.Add(record);

                    context.SaveChanges();

                }
                return JsonModelResult.PackageSuccess(gameTable.ID.ToString());
            }
            
        }

        /// <summary>
        /// 占座儿
        /// </summary>
        /// <param name="gameTableID"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public APIResultBase ZhanZuoEr(int gameTableID, string userCode, string password)
        {
            if (gameTableID < 1 || userCode.IsNullOrEmpty())
            {
                return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
            }
            var lstTables = _repository.GetList(c => c.ID == gameTableID && c.Password == password).Result;
            
            if (lstTables.TotalItemsCount < 1)
            {
                return JsonModelResult.PackageFail(OperateResCodeEnum.参数错误);
            }
            var gameTable = lstTables.Items.First();
            if (_repository.Get(c => (c.PlayerUserCode == userCode || c.CreateUserCode == userCode) && c.ID != gameTableID).Result.TotalItemsCount > 0)
            {
                return JsonModelResult.PackageFail(OperateResCodeEnum.同时只能创建或占用一个游戏房间);
            }

            //if (!gameTable.PlayerUserCode.IsNullOrEmpty() && gameTable.PlayerUserCode != userCode && gameTable.CreateUserCode != userCode)
            //{
            //    return JsonModelResult.PackageFail(OperateResCodeEnum.这个房间已被其他玩家占用);
            //}
            //if (gameTable.PlayerUserCode.IsNullOrEmpty() && gameTable.CreateUserCode != userCode)
            //{
            //    gameTable.PlayerUserCode = userCode;
            //    return JsonModelResult.PackageSuccess(_repository.Update(gameTable).Result.ToString());
            //}

            var gameRes = GameBll.Instance.GetGame(gameTable.TableCode);
            FF_GameRecord record = null;
            if (gameRes.code == (int)OperateResCodeEnum.成功)
            {
                var game = ((APISingleModelResult<FF_Game>)gameRes).data;
                var recordRes = GameRecordBll.Instance.GetRecord(game.RoundCode, userCode);
                if (recordRes.code == (int)OperateResCodeEnum.查询不到需要的数据)
                {
                    record = new FF_GameRecord();
                    record.AddTime = DateTime.Now;
                    record.BetMoney = 0;
                    record.IsBanker = false;
                    record.PlayerStatus = (int)PlayerStatusEnum.未准备;
                    record.RoundCode = game.RoundCode;
                    record.UserCode = userCode;
                    record.WinMoney = 0;
                    record.Balance = 0;
                    GameRecordBll.Instance.Insert(record);
                }
                else
                {
                    record = ((APISingleModelResult<FF_GameRecord>)recordRes).data;
                }
            }
            return JsonModelResult.PackageSuccess(record);
        }
    }
}
