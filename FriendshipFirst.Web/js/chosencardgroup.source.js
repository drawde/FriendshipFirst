var roomHub;
var roomcount = 0;

var headLst = ["PJfU3sS", "Z6pEpgb", "SxL07ZN", "L8qXk2G", "y4abanN", "MGFGe8j", "4VGMDAG", "Y5UZVxU", "Q3H6hB7", "bJvJjLe", "viJw155", "c58H2hR", "6shK2yE", "bEhujtX", "UItSDmY", "WTk2vYK"];
var positions = [2, 5, 9, 0, 4, 1, 3, 10, 14, 6, 8, 11, 13];
var userCount = 0;
var users = new Array();
function ClientConnected(res) {
    //GetCardGroups();
    $("#storm h2").html(getNickName());
    $("#storm").attr("userCode", getUserCode());
    users.push(res.data);
    $("#storm img").attr("src", "http://i.imgur.com/" + headLst[GetRandomNum(0, headLst.length - 1)] + ".png");
    var param = "{\"UserCode\":\"" + getUserCode() + "\"}";
    roomHub.server.getRoomUsers(appendParam(param, getSign())).done(function (res) {
        res = JSON.parse(res);
        console.log(res.Users);
        for (var i = 0; i < res.Users.length; i++) {            
            if (res.Users[i].UserCode != getUserCode()) {                
                initUserControl(res.Users[i].NickName, res.Users[i].UserCode, res.Users[i].PlayerStatus, res.Users[i].IsBanker);
                users.push(res.Users[i]);
            }
        }
    });
    //console.log(res.data.PlayerStatus);
    //console.log(res.data.UserCode);
    if (res.data.IsBanker && res.data.UserCode == getUserCode()) {
        $("#nightcrawler a").click(function () {
            startGame();
        });
    }
    else if (res.data.PlayerStatus != PlayerStatusEnum.已下注) {
        $("#nightcrawler a").click(function () {
            showBet();
        });
    }
}

function startGame() {
    var param = "{\"TableID\":\"" + getUrlParam("saloonid") + "\",\"UserCode\":\"" + getUserCode() + "\"}";
    roomHub.server.leaveRoom(appendParam(param, getSign()));
}

function showBet() {
    showInput("输入下注金额", "tel", function (ipt) {
        if (isNaN(ipt) == false) {
            IAmReady(ipt);
        }
    });
}

function initUserControl(name, userCode, playerStatus, isBanker) {
    $($(".characters li").get(positions[userCount])).attr("userCode", userCode);
    $($(".characters li h2").get(positions[userCount])).html(name);
    $($(".characters li img").get(positions[userCount])).attr("src", "http://i.imgur.com/" + headLst[GetRandomNum(0, headLst.length - 1)] + ".png");
    $($(".characters li").get(positions[userCount])).attr("data-teams", "original");
    userCount += 1;
    if (isBanker) {
        initBanker(userCode);
    }
    else if (playerStatus == PlayerStatusEnum.已下注) {
        userIsReady(userCode);
    }
}
function refreshUserControl(name, userCode, playerStatus, isBanker) {
    $(".characters li[userCode='" + userCode + "'] h2").html(name);
    if (isBanker) {
        initBanker(userCode);
    }
    else if (playerStatus == PlayerStatusEnum.已下注) {
        userIsReady(userCode);
    }
}
function removeUserControl(userCode) {
    userCount -= 1;
    $(".characters li[userCode='" + userCode + "']").html("<h2></h2><img src=\"/images/defaulthead.png\" />");
    $(".characters li[userCode='" + userCode + "']").attr("data-teams", "");
    $(".characters li[userCode='" + userCode + "']").attr("userCode", "");
}
function IAmReady(betMoney) {
    var param = "{\"TableID\":\"" + getUrlParam("saloonid") + "\",\"UserCode\":\"" + getUserCode() + "\",\"BetMoney\":\"" + betMoney + "\"}";
    roomHub.server.iAmReady(appendParam(param, getSign()));
}
function userIsReady(userCode) {    
    $(".characters li[userCode='" + userCode + "'] img").css("box-shadow", "0 0 20px #B6FF00");
    $(".characters li[userCode='" + userCode + "'] img").css("border", "4px solid #B6FF00");    
}
function initBanker(userCode) {
    $(".characters li[userCode='" + userCode + "'] img").css("box-shadow", "0 0 20px #ffb613");
    $(".characters li[userCode='" + userCode + "'] img").css("border", "4px solid #ffb613");
}
function userUnReady(userCode) {
    $(".characters li[userCode='" + userCode + "'] img").css("box-shadow", "0 0 20px #000");
    $(".characters li[userCode='" + userCode + "'] img").css("border", "4px solid #fff");
}
function registCustomRoomFunction() {
    roomHub.client.receiveOnlineNotice = function (name, userCode, message, playerStatus, isBanker) {
        showInfoMessage(message);
        if (userCode != getUserCode() && $(".characters li[userCode='" + userCode + "']").length < 1) {            
            initUserControl(name, userCode, playerStatus, isBanker);
        }
        else if ($(".characters li[userCode='" + userCode + "']").length > 0) {
            refreshUserControl(name, userCode, playerStatus, isBanker);
        }
    };
    roomHub.client.receiveReadyNotice = function (message, senderUserCode, playerStatus, isBanker) {
        if (senderUserCode != getUserCode()) {
            showInfoMessage(message);
            refreshUserControl(name, userCode, playerStatus, isBanker);
        }
    };
    roomHub.client.receiveOfflineNotice = function (senderUserCode, name) {
        if (senderUserCode != getUserCode()) {
            removeUserControl(senderUserCode);
            //for (var i = 0; i < users.length; i++) {
            //    if (users[i].UserCode == senderUserCode) {
            //        users = users.splice(i, 1);
            //        console.log(users);
            //    }
            //}
        }
    };
    roomHub.client.allReady = function (message) {
        console.log(message);
        showSuccessMessage(message);
    };
    
}
