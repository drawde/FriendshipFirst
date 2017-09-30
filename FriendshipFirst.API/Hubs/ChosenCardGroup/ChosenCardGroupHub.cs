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

namespace FriendshipFirst.API.Hubs.ChosenCardGroup
{    
    public class ChosenCardGroupHub : Hub, IChosenCardGroupHub
    {
        public override Task OnConnected()
        {
            if (UserContextProxy.GetRooms() == null)
            {
                UserContextProxy.Init();
            }
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
            int tableID = jobj["TableID"].Value<int>();

            var textRes = GameTableBll.Instance.ZhanZuoEr(tableID, userCode, password);
            if (textRes.code != (int)OperateResCodeEnum.成功)
            {
                return JsonConvert.SerializeObject(textRes);
            }

            HS_GameTable table = GameTableBll.Instance.GetById(tableID);
            if (table != null)
            {
                // 查询用户。
                var user = UserContextProxy.GetUsers().SingleOrDefault(u => u.UserCode == userCode);
                var record = ((APISingleModelResult<FF_GameRecord>)textRes).data; 
                if (user == null)
                {
                    user = new SignalRUser()
                    {
                        NickName = userName,
                        UserCode = userCode,
                        Balance = record.Balance,
                        BetMoney = record.BetMoney,
                        IsBanker = record.IsBanker,
                        PlayerStatus = record.PlayerStatus
                    };
                    UserContextProxy.AddUser(user);
                }
                else
                {
                    user.NickName = userName;
                    user.UserCode = userCode;
                    user.Balance = record.Balance;
                    user.BetMoney = record.BetMoney;
                    user.IsBanker = record.IsBanker;
                    user.PlayerStatus = record.PlayerStatus;
                    user.ChosenCardGroupCode = "";
                    UserContextProxy.SetUser(user);
                }
                ConversationRoom room = UserContextProxy.GetRooms().FirstOrDefault(c => c.RoomName == table.TableName + "-" + tableID);
                if (room == null)
                {
                    room = new ConversationRoom();
                    room.RoomName = table.TableName + "-" + tableID;
                    room.TableID = tableID;
                    room.Users.Add(user);
                    UserContextProxy.AddRoom(room);
                    //user.room = room;
                    
                }
                else
                {
                    if (!room.Users.Any(c => c.UserCode == userCode))
                    {
                        room.Users.Add(user);
                    }
                    UserContextProxy.SetRoom(room);
                }
                //user.room = room;
                Groups.Add(Context.ConnectionId, room.RoomName);
                //UserContextProxy.SetUser(user);
                

                SendOnlineNotice(userCode, room.RoomName, "用户：" + user.NickName + "进入房间");
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
        public void IAmReady(string param)
        {
            JObject jobj = JObject.Parse(param);
            string userCode = jobj["UserCode"].TryParseString();
            int tableID = jobj["TableID"].Value<int>();
            decimal betMoney = jobj["BetMoney"].TryParseDecimal(2);
            //查找房间是否存在
            var room = UserContextProxy.GetRooms().Find(a => a.TableID == tableID);
            //存在则进入删除
            if (room != null && room.Users.Any(c => c.UserCode == userCode))
            {
                var res = GameBll.Instance.Bet(userCode, betMoney, tableID);
                SignalRUser user = UserContextProxy.GetUsers().First(c => c.UserCode == userCode);
                user.PlayerStatus = (int)PlayerStatusEnum.已下注;
                user.BetMoney = betMoney;
                UserContextProxy.SetUser(user);
                //提示客户端                
                SendReadyNotice(room.RoomName, "用户：" + room.Users.First(c => c.UserCode == userCode).NickName + "已下注" + betMoney + "之巨！", userCode, user.PlayerStatus, user.IsBanker);
                if (res.code == (int)OperateResCodeEnum.成功)
                {                    
                    Clients.Group(room.RoomName, new string[0]).allReady("所有人都已下注！");
                }
            }
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
            int tableID = jobj["TableID"].Value<int>();
            //查找房间是否存在
            var room = UserContextProxy.GetRooms().Find(a => a.TableID == tableID);
            //存在则进入删除
            if (room != null)
            {
                //查找要删除的用户
                var user = room.Users.Where(a => a.UserCode == userCode).FirstOrDefault();
                //移除此用户
                room.Users.Remove(user);
                //如果房间人数为0,则删除房间
                if (room.Users.Count <= 0)
                {
                    UserContextProxy.RemoveRoom(room);                    
                    Groups.Remove(Context.ConnectionId, room.RoomName);
                }
                else
                {
                    UserContextProxy.SetRoom(room);
                }
                //提示客户端                
                SendBordcast(room.RoomName, "用户：" + user.NickName + "已经退出房间", user.UserCode);
                SendOfflineNotice(user.UserCode, room.RoomName);
            }
        }

        private void SendOfflineNotice(string userCode, string roomName)
        {
            var user = UserContextProxy.GetUsers().Where(a => a.UserCode == userCode).FirstOrDefault();
            if (user != null)
            {
                Clients.Group(roomName, new string[0]).receiveOfflineNotice(user.UserCode, user.NickName);
            }
        }

        [SignalRMethod]
        public void Bordcast(string param)
        {
            
        }

        public void SendOnlineNotice(string userCode,string roomName, string chatContent)
        {
            var user = UserContextProxy.GetUsers().Where(a => a.UserCode == userCode).FirstOrDefault();
            if (user != null)
            {
                Clients.Group(roomName, new string[0]).receiveOnlineNotice(user.NickName, user.UserCode, chatContent, user.PlayerStatus, user.IsBanker);
            }
        }

        private void SendBordcast(string roomName, string message,string senderUserCode)
        {
            Clients.Group(roomName, new string[0]).receiveBordcast(message, senderUserCode);
        }

        private void SendReadyNotice(string roomName, string message, string senderUserCode,int playerStatus,bool isBanker)
        {
            Clients.Group(roomName, new string[0]).receiveReadyNotice(message, senderUserCode, playerStatus, isBanker);
        }

        [SignalRMethod]
        public string GetRoomUsers(string param)
        {
            JObject jobj = JObject.Parse(param);
            string userCode = jobj["UserCode"].TryParseString();
            var room = UserContextProxy.GetRooms().First(c => c.Users.Any(x => x.UserCode == userCode));
            return JsonConvert.SerializeObject(room);
        }

        /// <summary>
        /// 客户端发送消息
        /// </summary>
        /// <param name="userCode"></param>
        /// <param name="userName"></param>
        /// <param name="chatContent"></param>
        public void SendChat(string userCode, string chatContent)
        {
            var user = UserContextProxy.GetUsers().Where(a => a.UserCode == userCode).FirstOrDefault();            
            var room = UserContextProxy.GetRooms().First(c => c.Users.Any(x => x.UserCode == userCode));
            if (user != null)
            {
                Clients.Group(room.RoomName, new string[0]).addNewMessageToPage(user.NickName, chatContent);
            }
        }
    }
}