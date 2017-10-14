$(function () {
    showLoader();
    var interval = setInterval(function () {
        if (!!apiTime && !!signObj) {
            getSaloons();
            clearInterval(interval);
        }
    }, 100);

    $("#createsaloonFrom").submit(function () {
        if (!!$("input[name='name']").val() == false) {
            showMessage("请输入房间名");
            return false;
        }
        showLoader();
        
        var param = "{\"Name\":\"" + $("input[name='name']").val() + "\",\"Password\":\"" + $("input[name='password']").val() + "\",\"UserCode\":\"" + getUserCode() + "\"}";
        ajaxGetData("/Saloon/CreateOrUpdate", param, signObj.rndStr, signObj.sign, signObj.sendTime, function (data) {
            showMessage(data.msg, function () {
                hideLoader();
                if (data.code == 100) {
                    window.location = "/Game/ChosenCardGroup?tableCode=" + data.data.TableCode + "&password=" + $("input[name='password']").val();
                }
            });
        });

        return false;
    });
});
var PageNo = 1;
function getSaloons() {    
    var param = "{\"PageSize\":\"10\",\"PageNo\":\"" + PageNo + "\"}";
    ajaxGetData("/Saloon/GetSaloons", param,signObj.rndStr,signObj.sign,signObj.sendTime, function (data) {
        hideLoader();
        if (data.code == 100) {
            //console.log(data.Items);
            var needPassword = false;
            for (var i = 0; i < data.data.Items.length; i++) {
                if (!!data.data.Items[i].Password) {
                    needPassword = true;
                }
                $(".text-list").append("<div class=\"item\">" +
                    "<div class=\"description\">" +
                    "<h2><a href=\"javascript:zhanZuoEr(" + needPassword + ",'" + data.data.Items[i].TableCode + "');\">" + data.data.Items[i].TableName + "</a></h2>" +
                    "<p></p>" +
                    "<div class=\"meta\">" +
                    "<ul class=\"tag-list\">" +
                    "<li><a href=\"javascript:;\" class=\"btn btn-sm\">宝宝只想做个吃瓜群众</a></li>" +
                    "</ul>" +
                    "</div>" +
                    "</div>" +
                    "<div class=\"action\">" +
                    "<a href=\"javascript:zhanZuoEr(" + needPassword + ",'" + data.data.Items[i].TableCode + "');\" class=\"btn\">进入房间</a>" +
                    "</div>" +
                    "</div>");
            }
        }
    });
}

function zhanZuoEr(needPassword,tableCode) {    
    if (needPassword) {
        showInput("请输入你的密码","text", function (ipt) {
            goRoom(ipt, tableCode);
        });
    }
    else {
        goRoom(id, '');
    }
}
function goRoom(ipt, tableCode) {
    showLoader();
    
    var param = "{\"TableCode\":\"" + tableCode + "\",\"UserCode\":\"" + getUserCode() + "\",\"Password\":\"" + ipt + "\"}";
    ajaxGetData("/Saloon/ZhanZuoEr", param,signObj.rndStr,signObj.sign,signObj.sendTime, function (data) {
        hideLoader();
        if (data.code == 100) {
            window.location = "/Game/ChosenCardGroup?tableCode=" + tableCode + "&password=" + ipt;
        }
        else {
            showErrorMessage(data.msg);
        }
    });
}