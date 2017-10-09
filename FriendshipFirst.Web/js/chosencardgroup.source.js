var roomHub;
var roomcount = 0;

var headLst = ["PJfU3sS", "Z6pEpgb", "SxL07ZN", "L8qXk2G", "y4abanN", "MGFGe8j", "4VGMDAG", "Y5UZVxU", "Q3H6hB7", "bJvJjLe", "viJw155", "c58H2hR", "6shK2yE", "bEhujtX", "UItSDmY", "WTk2vYK"];
var positions = [0, 1, 2, 3, 4, 5, 6, 8, 9, 10, 11, 13, 14];

var users = new Array();
var isBanker = false;
var bankCode = "";
function ClientConnected(result) {
    //GetCardGroups();    
    $("#storm h2").html(getNickName());
    //console.log(result);
    $(".money").html(result.data.Balance);
    $("#storm").attr("userCode", getUserCode());
    users.push(result.data);
    $("#storm img").attr("src", "http://i.imgur.com/" + headLst[GetRandomNum(0, headLst.length - 1)] + ".png");
    var param = "{\"UserCode\":\"" + getUserCode() + "\",\"TableCode\":\"" + getUrlParam("tableCode") + "\"}";
    var isAllReady = true;
    isBanker = result.data.IsBanker;
    
    roomHub.server.getRoomUsers(appendParam(param, getSign())).done(function (res) {
        res = JSON.parse(res);
        
        for (var i = 0; i < res.data.length; i++) {            
            if (res.data[i].UserCode != getUserCode()) {
                if (res.data[i].IsBanker) {
                    bankCode = res.data[i].UserCode;
                }
                initUserControl(res.data[i]);
                users.push(res.data[i]);
                if (isAllReady) {
                    isAllReady = res.data[i].PlayerStatus == PlayerStatusEnum.已下注;
                }
            }
        }
        if (isAllReady) {
            $("#nightcrawler").addClass("shaking");
            setTimeout(function () {
                $("#nightcrawler").removeClass("shaking");
            }, 5000);
        }

        if ((result.data.GameStatus == GameStatusEnum.已结算 || result.data.GameStatus == GameStatusEnum.初始化) && result.data.PlayerStatus != PlayerStatusEnum.已下注) {
            initSwitchBanker();
        }
        if (isBanker && result.data.GameStatus == GameStatusEnum.已结算) {
            $("#nightcrawler a").click(GameRestart);
        }
        else if (result.data.PlayerStatus == PlayerStatusEnum.未准备 && result.data.GameStatus == GameStatusEnum.初始化) {
            //console.log("showBet");
            $("#nightcrawler a").unbind();
            $("#nightcrawler a").click(function () {
                showBet();
            });
        }
        else if (result.data.PlayerStatus == PlayerStatusEnum.已下注 &&
            (result.data.GameStatus == GameStatusEnum.结算中 || result.data.GameStatus == GameStatusEnum.已开始)) {
            bindSettlementEvent(result.data.IsBanker);
        }
        else if (isBanker == false && result.data.PlayerStatus == PlayerStatusEnum.已下注 && result.data.GameStatus == GameStatusEnum.初始化) {
            $("#nightcrawler a").unbind();
            $("#nightcrawler a").click(function () {
                IAmCancelReady();
            });
        }
    });
    
}

function initSwitchBanker() {
    $(".switchBanker").show();
    $(".switchBanker").click(function () {
        showInfoMessage("请选择你要交换的玩家");
        $("li[usercode]").each(function () {
            if (getUserCode() != $(this).attr("usercode")) {
                $(this).unbind();
                $(this).click(function () {
                    ApplySwitchBanker($(this).attr("usercode"));
                });
            }
        });
    });
}

//绑定结算事件
function bindSettlementEvent(isBanker) {    
    if (isBanker) {
        $("li[usercode]").each(function () {            
            if (getUserCode() != $(this).attr("usercode")) {
                $(this).unbind();                
                $(this).click(function () {
                    Settlement($(this).attr("usercode"));
                });
            }
        });
    }
    else {
        //console.log("bindSettlementEvent");
        $("#nightcrawler a").unbind();
        $("#nightcrawler a").click(function () {            
            Settlement(bankCode);
        });
    }
}


//下注
function showBet() {
    showInput("输入下注金额", "tel", function (ipt) {
        if (isNaN(ipt) == false) {
            //console.log("showBet");
            IAmReady(ipt);
        }
    });
}

//初始化用户控件
function initUserControl(user) {
    //user = JSON.parse(user);
    $($(".characters li").get(positions[user.RoomIndex])).attr("userCode", user.UserCode);
    $($(".characters li h2").get(positions[user.RoomIndex])).html(user.NickName);
    $($(".characters li img").get(positions[user.RoomIndex])).attr("src", "http://i.imgur.com/" + headLst[GetRandomNum(0, headLst.length - 1)] + ".png");
    $($(".characters li").get(positions[user.RoomIndex])).attr("data-teams", "original");
    if (user.PlayerStatus == PlayerStatusEnum.已下注) {
        $(".characters li[userCode='" + user.UserCode + "'] div").show();
        $(".characters li[userCode='" + user.UserCode + "'] p").html(user.BetMoney);
    }
    else {
        $(".characters li[userCode='" + user.UserCode + "'] div").hide();
    }
    if (user.IsBanker) {
        initBanker(user.UserCode);
    }
    else if (user.PlayerStatus == PlayerStatusEnum.已下注) {
        userIsReady(user.UserCode);
    }
}

//刷新用户控件
function refreshUserControl(user) {
    //user = JSON.parse(user);
    $(".characters li[userCode='" + user.UserCode + "'] h2").html(user.NickName);
    if (user.PlayerStatus == PlayerStatusEnum.已下注) {
        $(".characters li[userCode='" + user.UserCode + "'] div").show();
        $(".characters li[userCode='" + user.UserCode + "'] p").html(user.BetMoney);
    }
    else {
        $(".characters li[userCode='" + user.UserCode + "'] div").hide();
    }
    if (user.IsBanker) {
        initBanker(user.UserCode);
    }
    else if (user.PlayerStatus == PlayerStatusEnum.已下注) {
        userIsReady(user.UserCode);
    }
    else {
        userUnReady(user.UserCode);
    }
}

//移除用户控件
function removeUserControl(userCode) {    
    $(".characters li[userCode='" + userCode + "']").html("<h2></h2><img src=\"/images/defaulthead.png\" />");
    $(".characters li[userCode='" + userCode + "']").attr("data-teams", "");
    $(".characters li[userCode='" + userCode + "']").attr("userCode", "");
}

//下注
function IAmReady(betMoney) {
    var param = "{\"TableCode\":\"" + getUrlParam("tableCode") + "\",\"UserCode\":\"" + getUserCode() + "\",\"BetMoney\":\"" + betMoney + "\"}";
    roomHub.server.iAmReady(appendParam(param, getSign())).done(function (res) {
        //console.log(res);
        res = JSON.parse(res);
        if (res.code == 100) {
            $(".switchBanker").hide();
            if (res.data.GameStatus == GameStatusEnum.初始化) {
                //console.log("IAmCancelReady1");
                $("#nightcrawler a").unbind();
                $("#nightcrawler a").click(function () {
                    //console.log("IAmCancelReady2");
                    IAmCancelReady();
                });
            }
        }
        else {
            showErrorMessage(res.msg);
        }
        
    });
}

//重新下注
function IAmCancelReady() {
    var param = "{\"TableCode\":\"" + getUrlParam("tableCode") + "\",\"UserCode\":\"" + getUserCode() + "\"}";
    roomHub.server.iAmCancelReady(appendParam(param, getSign())).done(function (res) {
        res = JSON.parse(res);
        if (res.code == 100) {
            $("#nightcrawler a").unbind();
            $("#nightcrawler a").click(function () {
                showBet();
            });
            initSwitchBanker();
        }
        else {
            showErrorMessage(res.msg);
        }        
    });
}

//结算
function Settlement(targetUserCode) {    
    showInput("输入赢得的金额", "tel", function (ipt) {
        if (isNaN(ipt) == false) {
            var param = "{\"TableCode\":\"" + getUrlParam("tableCode") + "\",\"UserCode\":\"" + getUserCode() + "\",\"TargetUserCode\":\"" + targetUserCode + "\",\"Money\":\"" + ipt + "\"}";
            roomHub.server.settlement(appendParam(param, getSign())).done(function (res) {
                res = JSON.parse(res);
                //console.log(res);
                if (res.code == 100) {
                    showSuccessMessage("结算完毕");
                    $("#nightcrawler a").unbind();
                    $(".money").html(res.data.Balance);
                    if (isBanker && res.data.GameStatus == GameStatusEnum.已结算) {
                        $("#nightcrawler a").click(GameRestart);
                    }
                }
                else {
                    showErrorMessage(res.msg);
                }
            });
        }
    });
}

function GameRestart() {
    var param = "{\"TableCode\":\"" + getUrlParam("tableCode") + "\",\"UserCode\":\"" + getUserCode() + "\"}";
    roomHub.server.gameRestart(appendParam(param, getSign())).done(function (res) {
        res = JSON.parse(res);
        //console.log(res);
        if (res.code != 100) {
            showErrorMessage(res.msg);
        }
    });
}

//请求换庄
function ApplySwitchBanker(targetUserCode) {
    var param = "{\"TableCode\":\"" + getUrlParam("tableCode") + "\",\"UserCode\":\"" + getUserCode() + "\",\"TargetUserCode\":\"" + targetUserCode + "\"}";
    roomHub.server.applySwitchBanker(appendParam(param, getSign())).done(function (res) {
        res = JSON.parse(res);
        if (res.code == 100) {
            showSuccessMessage("请等待对方的回应");
        }
        else{
            showErrorMessage(res.msg);
        }
    });
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
    roomHub.client.receiveOnlineNotice = function (message, user) {
        showInfoMessage(message);
        user = JSON.parse(user);
        if (user.UserCode != getUserCode() && $(".characters li[userCode='" + user.UserCode + "']").length < 1) {            
            initUserControl(user);
        }
        else if ($(".characters li[userCode='" + user.UserCode + "']").length > 0) {            
            refreshUserControl(user);
        }
    };

    roomHub.client.receiveReadyNotice = function (message, senderUserCode, user) {        
        if (senderUserCode != getUserCode()) {
            showInfoMessage(message);
            refreshUserControl(JSON.parse(user));
        }
    };

    roomHub.client.receiveOfflineNotice = function (senderUserCode, name) {
        if (senderUserCode != getUserCode()) {
            removeUserControl(senderUserCode);            
        }
    };

    roomHub.client.receiveApplySwitchBanker = function (user, targetUser) {
        user = JSON.parse(user);
        targetUser = JSON.parse(targetUser);
        if (targetUser.UserCode == getUserCode()) {
            showConfirmMessage(user.NickName + "请求和你交换庄家", function () {
                var param = "{\"TableCode\":\"" + getUrlParam("tableCode") + "\",\"UserCode\":\"" + getUserCode() + "\",\"TargetUserCode\":\"" + user.UserCode + "\"}";
                roomHub.server.switchBanker(appendParam(param, getSign())).done(function (res) {
                    res = JSON.parse(res);
                    if (res.code != 100) {
                        showErrorMessage(res.msg);
                    }
                });
            });
        }
    };

    roomHub.client.receiveSwitchBanker = function (user, targetUser) {
        user = JSON.parse(user);
        targetUser = JSON.parse(targetUser);
        refreshUserControl(user);
        refreshUserControl(targetUser);
        if (user.UserCode == getUserCode()) {
            isBanker = user.IsBanker;
            if (user.IsBanker) {
                $("#nightcrawler a").unbind();
                showSuccessMessage("交换成功,你现在是庄家了!");                
            }
            else {
                showSuccessMessage("交换成功!");       
                $("#nightcrawler a").unbind();
                $("#nightcrawler a").click(showBet);
            }
        }
        else if (targetUser.UserCode == getUserCode()) {
            isBanker = targetUser.IsBanker;
            if (targetUser.IsBanker) {
                $("#nightcrawler a").unbind();
                showSuccessMessage("交换成功,你现在是庄家了!");
            }
            else {
                showSuccessMessage("交换成功!");
                $("#nightcrawler a").unbind();
                $("#nightcrawler a").click(showBet);
            }
        }
        //else {
        //    if (user.IsBanker) {
        //        refreshUserControl(user);
        //    }
        //    else {
        //        refreshUserControl(targetUser);
        //    }
        //}
    };

    roomHub.client.allReady = function (message) {
        //console.log("allReady");
        showSuccessMessage(message);
        $(".switchBanker").hide();
        $("#nightcrawler a").unbind();
        $("#nightcrawler").addClass("shaking");
        setTimeout(function () {
            $("#nightcrawler").removeClass("shaking");
        }, 5000);
        bindSettlementEvent(isBanker);
    };

    roomHub.client.gameRestartNotice = function (message, res) {
        showSuccessMessage(message);
        //console.log(res);
        res = JSON.parse(res);
        for (var i = 0; i < res.data.length; i++) {
            if (res.data[i].UserCode != getUserCode()) {
                refreshUserControl(res.data[i]);
                $("#nightcrawler a").unbind();
                if (isBanker == false) {                    
                    $("#nightcrawler a").click(function () {
                        showBet();
                    });
                }
            }
        }
    };

    roomHub.client.settlementNotice = function (targetUserCode, user, money) {
        if (targetUserCode == getUserCode()) {
            user = JSON.parse(user);
            showInfoMessage(user.NickName + "从你这赢走了" + money + "之巨!");
            $(".money").html(user.Balance);
            //console.log(targetUserCode);
            //console.log(user);
            if (isBanker && user.GameStatus == GameStatusEnum.已结算) {
                //console.log("settlementNotice");
                $("#nightcrawler a").unbind();
                $("#nightcrawler a").click(GameRestart);
                initSwitchBanker();
            }
        }
    };
}
