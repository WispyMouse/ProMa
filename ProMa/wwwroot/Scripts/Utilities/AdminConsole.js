/// <reference path="../References.js">

// While this can be spoofed without a terrible amount of difficulty, all of the functionality and most of the visuals are driven by secured service calls
// spoofing the ability to see the console would only give knowledge to categories of activities, like seeing posts, but not anything of substance

if (typeof AdminConsole === "undefined") {
	var AdminConsole = {
		init: function () {
			if (LoggedInUser.isAdmin) {
				$("#AdminConsole").append(
					"<div class='formSet'>" +
						"<div class='formRow'><span class='label'>&nbsp;</span><button class='formItem' id='ResetCaches' type='button' onclick='AdminConsole.ResetCaches();'>Reset Caches</button></div>" +
						"<div class='formRow'><span class='label'>&nbsp;</span><button class='formItem' type='button' onclick='AdminConsole.HideThisTab();'>Hide This Tab</button></div>" +
					"</div>" +
					"<div class='formSet'><div class='formRow'><span class='label'>Last five users:</span><ul class='formItem' id='MostRecentUsers'></ul></div></div>" +
					"<div id='HeartBeatArea'></div>"
					);

				AjaxCallWithWait("/Services/AdminConsole.asmx/GetMostRecentUsers", null, $("#MostRecentUsers").closest(".formSet"), true, true).done(function (msg) {
					$.each(msg.d, function (index, value) {
						$("#MostRecentUsers").append("<li><span>" + value.UserName + ", Email: " + (value.EmailAddress !== null ? value.EmailAddress : "<i class='weaktext'>(none)</i>") + ", Joined: " + FormatDateString(ParseDateFromJSONReturn(value.JoinTime), TimeModes.JUSTDATEMODE, DateModes.REMOVEYEARIFSAME) + "</span></li>");
					});
				});
			} else {
				$("#AdminConsole").remove();
				$("#WorkshopUtilities").find("a[href='#AdminConsole']").parent().remove();
				$("#WorkshopUtilities").tabs("refresh");
			}
		},
		ResetCaches: function () {
			AjaxCallWithWait("/Services/AdminConsole.asmx/ResetCaches", null, $("#ResetCaches"));
		},
		AddHeartBeat: function () {
			var nowTime = new Date();
			$("#HeartBeatArea").append("<span title='" + FormatDateString(nowTime, TimeModes.JUSTTIMEMODE) + " " + nowTime.getSeconds().toString() + "' class='promaicon promaicon-heart'></span>");
		},
		AddBrokenHeartBeat: function () {
			var nowTime = new Date();
			$("#HeartBeatArea").append("<span title='" + FormatDateString(nowTime, TimeModes.JUSTTIMEMODE) + " " + nowTime.getSeconds().toString() + "'  class='promaicon promaicon-friendrequest'></span>");
		},
		HideThisTab: function () {
			$("#AdminConsole").remove();
			$("#WorkshopUtilities").find("a[href='#AdminConsole']").parent().remove();
			$("#WorkshopUtilities").tabs("refresh");
		}
	}

	SubscribeToTabCreation("AdminConsole", "Admin Console", AdminConsole.init, function () { return LoggedInUser.isAdmin; });
}