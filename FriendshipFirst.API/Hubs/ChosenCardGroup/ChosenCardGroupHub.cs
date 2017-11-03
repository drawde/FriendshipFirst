using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using FriendshipFirst.Common;
using FriendshipFirst.Common.Util;
using Newtonsoft.Json;
using FriendshipFirst.Model;
using FriendshipFirst.BLL;
using FriendshipFirst.APIMonitor;
using Newtonsoft.Json.Linq;
using FriendshipFirst.Common.Enum;
using FriendshipFirst.Common.JsonModel;
using FriendshipFirst.Model.CustomModels;

namespace FriendshipFirst.API.Hubs.ChosenCardGroup
{
    public class ChosenCardGroupHub : Hub, IChosenCardGroupHub
    {
        public override Task OnConnected()
        {
            return base.OnConnected();
        }
        /// <summary>
        /// 用户进入游戏房间
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="userName"></param>
        /// <param name="tableID"></param>        
        [SignalRMethod]
        public string Online(string param)
        {
            JObject jobj = JObject.Parse(param);
            string userCode = jobj["UserCode"].TryParseString();
            string userName = jobj["NickName"].TryParseString();
            string password = jobj["Password"].TryParseString();
            string tableCode = jobj["TableCode"].Value<string>();

            var textRes = GameTableBll.Instance.ZhanZuoEr(tableCode, userCode, password);
            if (textRes.code != (int)OperateResCodeEnum.成功)
            {
                return JsonConvert.SerializeObject(textRes);
            }

            HS_GameTable table = GameTableBll.Instance.GetTable(tableCode);
            if (table != null)
            {
                // 查询用户。
                //var user = GameRecordBll.GetUser(userCode, table.TableCode);
                //var record = ((APISingleModelResult<FF_GameRecord>)textRes).data;

                Groups.Add(Context.ConnectionId, table.TableCode);
                //UserContextProxy.SetUser(user);

                var record = GameRecordBll.Instance.GetUser(userCode, tableCode);
                SendOnlineNotice(record, table.TableCode, "用户：" + userName + "进入房间");
            }
            else
            {
                return JsonStringResult.VerifyFail();
            }
            return JsonConvert.SerializeObject(textRes);
        }

        /// <summary>
        /// 重写Hub连接断开的事件
        /// </summary>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

        [SignalRMethod]
        public string IAmReady(string param)
        {
            JObject jobj = JObject.Parse(param);
            string userCode = jobj["UserCode"].TryParseString();
            string tableCode = jobj["TableCode"].Value<string>();
            decimal betMoney = jobj["BetMoney"].TryParseDecimal(2);

            var res = GameBll.Instance.Bet(userCode, betMoney, tableCode);
            var user = GameRecordBll.Instance.GetUser(userCode, tableCode);

            //提示客户端                
            SendReadyStatusNotice(user.GameCode, "用户：" + user.NickName + "已下注" + betMoney + "之巨！", userCode, user);
            if (res.code == (int)OperateResCodeEnum.成功)
            {
                Clients.Group(user.GameCode, new string[0]).allReady("所有人都已下注！");
            }
            return JsonStringResult.SuccessResult(user);
        }

        [SignalRMethod]
        public string IAmCancelReady(string param)
        {
            JObject jobj = JObject.Parse(param);
            string userCode = jobj["UserCode"].TryParseString();
            string tableCode = jobj["TableCode"].Value<string>();

            var res = GameBll.Instance.CancelBet(userCode, tableCode);
            var user = GameRecordBll.Instance.GetUser(userCode, tableCode);
            //提示客户端                
            //SendBordcast(user.GameCode, "用户：" + user.NickName + "已取消下注！", userCode);
            SendReadyStatusNotice(user.GameCode, "用户：" + user.NickName + "已取消下注！", userCode, user);
            return JsonStringResult.SuccessResult();
        }

        [SignalRMethod]
        public string GameRestart(string param)
        {
            JObject jobj = JObject.Parse(param);
            string userCode = jobj["UserCode"].TryParseString();
            string tableCode = jobj["TableCode"].Value<string>();

            var res = GameBll.Instance.GameRestart(userCode, tableCode);

            //提示客户端                
            //SendBordcast(user.GameCode, "用户：" + user.NickName + "已取消下注！", userCode);
            if (res.code == (int)OperateResCodeEnum.成功)
            {
                GameRestartNotice(res);

            }
            return JsonConvert.SerializeObject(res);
        }
        private void GameRestartNotice(APIResultBase res)
        {
            List<CGameUser> lstUser = ((APISingleModelResult<List<CGameUser>>)res).data;
            Clients.Group(lstUser[0].GameCode, new string[0]).gameRestartNotice("游戏已经重新开始", JsonConvert.SerializeObject(res));
        }
        /// <summary>
        /// 客户端离开房间
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="tableID"></param>
        [SignalRMethod]
        public void LeaveRoom(string param)
        {
            JObject jobj = JObject.Parse(param);
            string userCode = jobj["UserCode"].TryParseString();
            string tableCode = jobj["TableCode"].Value<string>();

            //查找房间是否存在
            var users = GameRecordBll.Instance.GetUsers(tableCode);
            //存在则进入删除
            if (users != null)
            {
                //查找要删除的用户
                var user = users.FirstOrDefault(a => a.UserCode == userCode);
                //移除此用户
                bool isAllLeaved = GameRecordBll.Instance.LeavingRoom(userCode, tableCode);
                if (isAllLeaved)
                {
                    //提示客户端                
                    SendBordcast(tableCode, "用户：" + user.NickName + "已经退出房间", user.UserCode);
                    SendOfflineNotice(userCode, user.NickName, tableCode);
                }
            }
        }

        /// <summary>
        /// 请求换庄
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [SignalRMethod]
        public string ApplySwitchBanker(string param)
        {
            JObject jobj = JObject.Parse(param);
            string userCode = jobj["UserCode"].TryParseString();
            string targetUserCode = jobj["TargetUserCode"].TryParseString();
            string tableCode = jobj["TableCode"].Value<string>();

            CGameUser user = GameRecordBll.Instance.GetUser(userCode, tableCode);
            CGameUser targetUser = GameRecordBll.Instance.GetUser(targetUserCode, tableCode);
            SendApplySwitchBankerNotice(user, targetUser);
            return JsonStringResult.SuccessResult();
        }

        private void SendApplySwitchBankerNotice(CGameUser user, CGameUser targetUser)
        {
            Clients.Group(user.GameCode, new string[0]).receiveApplySwitchBanker(JsonConvert.SerializeObject(user), JsonConvert.SerializeObject(targetUser));
        }

        /// <summary>
        /// 换庄
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [SignalRMethod]
        public string SwitchBanker(string param)
        {
            JObject jobj = JObject.Parse(param);
            string userCode = jobj["UserCode"].TryParseString();
            string targetUserCode = jobj["TargetUserCode"].TryParseString();
            string tableCode = jobj["TableCode"].Value<string>();

            var res = GameBll.Instance.SwitchBanker(tableCode, userCode, targetUserCode);
            if (res.code == (int)OperateResCodeEnum.成功)
            {
                List<CGameUser> lstUser = ((APISingleModelResult<List<CGameUser>>)res).data;
                SendSwitchBankerNotice(lstUser.First(c => c.UserCode == userCode), lstUser.First(c => c.UserCode == targetUserCode));
            }
            return JsonConvert.SerializeObject(res);
        }

        private void SendSwitchBankerNotice(CGameUser user, CGameUser targetUser)
        {
            Clients.Group(user.GameCode, new string[0]).receiveSwitchBanker(JsonConvert.SerializeObject(user), JsonConvert.SerializeObject(targetUser));
        }
        private void SendOfflineNotice(string senderUserCode, string nickName, string roomName)
        {
            Clients.Group(roomName, new string[0]).receiveOfflineNotice(senderUserCode, nickName);
        }

        [SignalRMethod]
        public void Bordcast(string param)
        {

        }

        public void SendOnlineNotice(CGameUser user, string roomName, string chatContent)
        {
            Clients.Group(roomName, new string[0]).receiveOnlineNotice(chatContent, JsonConvert.SerializeObject(user));
        }

        private void SendBordcast(string roomName, string message, string senderUserCode)
        {
            Clients.Group(roomName, new string[0]).receiveBordcast(message, senderUserCode);
        }

        private void SendReadyStatusNotice(string roomName, string message, string senderUserCode, CGameUser user)
        {
            Clients.Group(roomName, new string[0]).receiveReadyNotice(message, senderUserCode, JsonConvert.SerializeObject(user));
        }

        [SignalRMethod]
        public string GetRoomUsers(string param)
        {
            JObject jobj = JObject.Parse(param);
            string gameCode = jobj["TableCode"].TryParseString();
            var room = GameRecordBll.Instance.GetUsers(gameCode);
            return JsonStringResult.SuccessResult(room);
        }

        /// <summary>
        /// 客户端发送消息
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="userName"></param>
        /// <param name="chatContent"></param>
        public void SendChat(string userCode, string chatContent)
        {
            var game = GameBll.Instance.GetGameByUser(userCode);
            Clients.Group(game.GameCode, new string[0]).addNewMessageToPage(game.NickName, chatContent);
            //AddNewMessageToPage(game.GameCode, game.NickName, chatContent);
        }

        private void AddNewMessageToPage(string gameCode, string nickName, string chatContent)
        {
            Clients.Group(gameCode, new string[0]).addNewMessageToPage(nickName, chatContent);
        }

        /// <summary>
        /// 结算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [SignalRMethod]
        public string Settlement(string param)
        {
            JObject jobj = JObject.Parse(param);
            string userCode = jobj["UserCode"].TryParseString();
            string targetUserCode = jobj["TargetUserCode"].TryParseString();
            string tableCode = jobj["TableCode"].Value<string>();
            decimal money = jobj["Money"].TryParseDecimal(2);

            var result = GameBll.Instance.Settlement(userCode, targetUserCode, tableCode, money);
            if (result.code == (int)OperateResCodeEnum.成功)
            {
                CGameUser user = GameRecordBll.Instance.GetUser(targetUserCode, tableCode);
                if (user.GameStyle == (int)GameStyleEnum.庄家模式)
                {
                    Clients.Group(user.GameCode, new string[0]).settlementNotice(targetUserCode, JsonConvert.SerializeObject(user), money);                    
                }
                else if(user.GameStatus == (int)GameStatusEnum.已开始)
                {
                    Clients.Group(user.GameCode, new string[0]).allReady("游戏已经重新开始！");
                }
            }
            return JsonConvert.SerializeObject(result);
        }
    }
}