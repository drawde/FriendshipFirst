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

namespace FriendshipFirst.BLL
{
    public class GameBll : BaseBLL<FF_Game>
    {
        private IRepository<FF_Game> _repository = new Repository<FF_Game>();
        private GameBll()
        {
        }
        public static GameBll Instance = new GameBll();

        public APIResultBase GetGame(string tableCode, GameStatusEnum status = GameStatusEnum.无)
        {
            var where = LDMFilter.True<FF_Game>();
            if (status != GameStatusEnum.无)
            {
                where = where.And(c => c.GameStatus == (int)status);
            }
            where = where.And(c => c.TableCode == tableCode);
            
            var lst = _repository.GetList(where, "AddTime", false).Result;
            if (lst != null && lst.TotalItemsCount > 0)
            {
                return JsonModelResult.PackageSuccess(lst.Items.FirstOrDefault());
            }
            return JsonModelResult.PackageFail(OperateResCodeEnum.查询不到需要的数据);
        }

        /// <summary>
        /// 用户下注
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="betMoney"></param>
        /// <param name="tableID"></param>
        /// <returns>所有人是否已下注</returns>
        public APIResultBase Bet(string userCode, decimal betMoney, int tableID)
        {
            using (FriendshipFirstContext context = new FriendshipFirstContext())
            {
                var gameTable = context.hs_gametable.FirstOrDefault(c => c.ID == tableID);
                var game = context.ff_game.Where(c => c.TableCode == gameTable.TableCode && c.GameStatus == (int)GameStatusEnum.初始化).OrderByDescending(c => c.AddTime).FirstOrDefault();
                var record = context.ff_gamerecord.FirstOrDefault(c => c.RoundCode == game.RoundCode && c.UserCode == userCode);
                record.BetMoney = betMoney;
                record.PlayerStatus = (int)PlayerStatusEnum.已下注;
                context.SaveChanges();

                //所有人是否已下注
                var isNotAllBeted = context.ff_gamerecord.Any(c => c.RoundCode == game.RoundCode && c.PlayerStatus != (int)PlayerStatusEnum.已下注);
                if (isNotAllBeted == false)
                {
                    game.GameStatus = (int)GameStatusEnum.结算中;
                    context.SaveChanges();
                    return JsonModelResult.PackageSuccess();
                }
            }
            return JsonModelResult.PackageFail(OperateResCodeEnum.无);
        }
    }
}
