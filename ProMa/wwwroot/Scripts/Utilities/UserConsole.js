/// <reference path="../References.js">

if (typeof UserConsole === "undefined") {
	var UserConsole = {
		init: function () {
			var newHtml = "";

			newHtml +=
				"<div class='formSet'>" +
					"<div class='formRow helpArea'><span class='label'>Style guide:</span><button class='formItem' type='button' onclick='ShowStyleGuide();'>Show Style Guide</button></div>" +
				"</div>" +

				(LoggedInUser.isDemo ? "" :
				"<div class='formSet'>" +
					"<div class='formRow' id='UserEmailAddress'><span class='label'>Email address:&nbsp;<span class='help' title='Your email address is not required. Filling this out helps me contact you. Your email address is only displayed in-interface to the site admin, although it is not particularly hidden from users who search for your user name. As a reminder, no privacy policy is offered.'>( ? )</span></span><input type='text' class='formItem' value='" + LoggedInUser.emailAddress + "'/><button class='formItem' type='button' onclick='UserConsole.SaveEmailAddress();'>Update</button></div>" +
					"<div class='formRow' id='ChangeUserName'><span class='label'>Change Username:</span><input type='text' class='formItem' value='" + LoggedInUser.userName + "'/><button class='formItem' type='button' onclick='UserConsole.ChangeUsername();'>Update</button></div>" +
					"<div class='formRow' id='ChangePassword'><span class='label'>Change Password:</span><input type='password' class='formItem' value=''/><button class='formItem' type='button' onclick='UserConsole.ChangePassword();'>Update</button></div>" +
				"</div>") +

				"<div class='formSet' id='EnterNoteAreaRadio'>" +
					"<div class='formRow'><span class='label'>Enter key in note:</span><input type='radio' class='formItem' name='EnterInNewArea' onchange='UserConsole.ChangeEnterPref();' value='false' " + (LoggedInUser.enterAsNewlinePref ? "" : "checked") + "/>Submits<i class='weaktext'>+shift for new line</i></div>" +
					"<div class='formRow'><span class='label'>&nbsp;</span><input type='radio' class='formItem' name='EnterInNewArea' onchange='UserConsole.ChangeEnterPref();' value='true' " + (LoggedInUser.enterAsNewlinePref ? "checked" : "") + "/>Adds new line<i class='weaktext'>+shift for submit</i></div>" +
				"</div>" +
				"<div class='formSet'>" +
					"<div class='formRow'><span class='label'>&nbsp;</span><button class='formItem' type='button' onclick='LogOutUser();'>Log Out</button></div>" +
				"</div>" +
				"<hr/>" +
				"<div id='PendingUserConsole'></div>" +
				"<div class='formSet'>" +
					"<div class='formRow'><span class='label'>Friends:</span><ul id='existingFriends' class='formItem'></ul></div>" +
				"</div>" +
				"<div class='formSet'>" +
					"<div class='formRow'><span class='label'>Name contains:</span><input id='UserNameContains' class='formItem' type='text'/></div>" +
					"<div class='formRow'><span class='label'>&nbsp;</span><button class='formItem' type='button' onclick='UserConsole.SearchForFriends();'>Search</button></div>" +
					"<div class='formRow'><span class='label'>&nbsp;</span><div class='formItem' id='UserConsoleSearchResults'></div></div>" +
				"</div>";

			$("#UserConsole").html(newHtml);

			// function called by initial long poll: UserConsole.UpdateFriends();
		},
		SearchForFriends: function () {
			var data = { name: $("#UserNameContains").val(), excludeOwn: true };

			AjaxCallWithWait("/Services/UserConsole/UsersForFriendRequest", data, $("#ChoreItems"), true, true).done(function (msg) {
				$("#UserConsoleSearchResults").html("<ul></ul>");

				$.each(msg, function (index, value) {
					$("#UserConsoleSearchResults").find("ul").append("<li>" + value.UserName + "<span class='noteActionList'><span class='noteAction'><a href='javascript:void(0);' onclick='UserConsole.SendFriendRequest(" + value.UserId + ", this);'><span title='Send friend request' class='promaicon promaicon-friendrequest'></span></a></span></span></li>");
				});
			});
		},
		GetPendingUserConsole: function () {
			var deferred = $.Deferred();

			// if there's nothing in the pending user console yet, don't show loading icon
			var $hideDom = $("#PendingUserConsole").find("ul");

			if ($hideDom.children().length === 0) {
				$hideDom = null;
			}

			AjaxCallWithWait("/Services/UserConsole/GetFriendshipRequests", null, $hideDom, true, true).done(function (msg) {
				if (msg.length > 0) {
					$("#PendingUserConsole").html("<ul></ul>");
				}
				else {
					$("#PendingUserConsole").html("");
				}

				$.each(msg, function (index, value) {
					$("#PendingUserConsole").find("ul").append(
						"<li>" + (value.Sender.UserId !== LoggedInUser.userId ? value.Sender.UserName : value.Recipient.UserName) +
							"<span class='noteActionList'>" +
								(value.Sender.UserId !== LoggedInUser.userId ?
									"<span class='noteAction'><a href='javascript:void(0);' onclick='UserConsole.AcceptFriendRequest(" + value.Sender.UserId + ", this);'><span title='Accept friend request' class='promaicon promaicon-check'></span></a></span><span class='noteAction'><a href='javascript:void(0);' onclick='UserConsole.RejectFriendRequest(" + value.Sender.UserId + ", this);'><span title='Reject friend request' class='promaicon promaicon-x'></span></a></span>"
									:
									"<span class='noteAction'><i class='weaktext'>(not responded to)</i><a href='javascript:void(0);' onclick='UserConsole.CancelFriendRequest(" + value.Recipient.UserId + ", this);'><span title='Cancel friend request' class='promaicon promaicon-x'></span></a></span>") +
							"</span>" +
						"</li>"
						);
				});

				deferred.resolve();
			});

			return deferred.promise();
		},
		SendFriendRequest: function (userId, dom) {
			var data = { toUser: userId };

			AjaxCallWithWait("/Services/UserConsole/SendFriendRequest", data, $(dom), true).done(function (msg) {
				UserConsole.SearchForFriends();
			});
		},
		AcceptFriendRequest: function (userId, dom) {
			var data = { fromUser: userId };

			AjaxCallWithWait("/Services/UserConsole/AcceptFriendRequest", data, $(dom), true).done(function (msg) {
				// This will refresh thanks to the long poll
			});
		},
		RejectFriendRequest: function (userId, dom) {
			var data = { fromUser: userId };

			AjaxCallWithWait("/Services/UserConsole/RejectFriendRequest", data, $(dom), true).done(function (msg) {
				// This will refresh thanks to the long poll
			});
		},
		CancelFriendRequest: function (userId, dom) {
			var data = { recipient: userId };

			AjaxCallWithWait("/Services/UserConsole/CancelFriendRequest", data, $(dom), true).done(function (msg) {
				// This will refresh thanks to the long poll
			});
		},
		UpdateFriends: function () {
			AjaxCallWithWait("/Services/Data/GetFriends", null, $("#ShareNoteTypeUserTarget"), true, false).done(function (msg) {
				var targetHtml = "";

				$("#existingFriends").html("");

				$.each(msg, function (index, value) {
					targetHtml += "<option value='" + value.UserId + "'>" + value.UserName + "</option>";
					$("#existingFriends").append(
						"<li data-userid=" + value.UserId + "><span>" + value.UserName + "</span>" +
							"<span class='noteActionList'><span class='noteAction'><a href='javascript:void(0);' onclick='UserConsole.RemoveFriend(" + value.UserId + ", this);'><span title='Remove friend' class='promaicon promaicon-x'></span></a></span></span>" +
						"</li>"
						);
				});

				$("#ShareNoteTypeUserTarget").html(targetHtml);

				UserConsole.GetPendingUserConsole();
			});
		},
		ChangeEnterPref: function () {
			var data = { value: $("#EnterNoteAreaRadio").find("input[value=true]")[0].checked };

			AjaxCallWithWait("/Services/UserConsole/ChangeEnterPref", data, $("#EnterNoteAreaRadio"), true).done(function (msg) {
				LoggedInUser.enterAsNewlinePref = data.value;
			});
		},
		RemoveFriend: function (userId, dom) {
			var data = { fromUser: userId };

			if (confirm("Are you sure you want to remove this friendship?")) {
				AjaxCallWithWait("/Services/UserConsole/RemoveFriend", data, $(dom), true).done(function (msg) {
					UserConsole.GetPendingUserConsole();
				});
			}
		},
		SaveEmailAddress: function () {
			var data = { emailAddress: $("#UserEmailAddress").find("input").val() };

			AjaxCallWithWait("/Services/UserConsole/UpdateEmailAddress", data, $("#UserEmailAddress").find("button"), true).done(function (msg) {
			});
		},
		ChangeUsername: function () {
			var data = { userName: $("#ChangeUserName").find("input").val() };

			if (!VerifyBasicCleanliness(data.userName)) {
				AddFadingWarning($("#ChangeUserName"), "Invalid user name", true);
			} else {
				AjaxCallWithWait("/Services/UserConsole/ChangeUsername", data, $("#ChangeUserName").find("button"), true).done(function (msg) {
					alert("The screen will now refresh. Login with your new information.");
					LogOutUser();
				}).fail(function (msg) {
					AddFadingWarning($("#ChangeUserName input"), "Invalid user name, or it is taken", true);
				});
			}
		},
		ChangePassword: function () {
			if (!VerifyBasicCleanliness($("#ChangePassword").find("input").val())) {
				AddFadingWarning($("#ChangePassword"), "Invalid password", true);
			} else {
				var data = { md5Password: $.md5($("#ChangePassword").find("input").val()) };

				AjaxCallWithWait("/Services/UserConsole/ChangePassword", data, $("#ChangePassword").find("button"), true).done(function (msg) {
					alert("The screen will now refresh. Login with your new information.");
					LogOutUser();
				}).fail(function (msg) {
					AddFadingWarning($("#ChangePassword input"), "Invalid password", true);
				});
			}
		}
	}

	SubscribeToTabCreation("UserConsole", "User Console", UserConsole.init);
}