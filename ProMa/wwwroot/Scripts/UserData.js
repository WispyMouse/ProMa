/// <reference path="References.js">

var LoggedInUser = {
	userId: -1,
	isAdmin: false,
	isDemo: false,
	enterAsNewlinePref: false,
	emailAddress: "",
	userName: "",
	PassBackPassword: ""
};

function DemoLogin() {
	LoginWithInformation("DemoAccount", $.md5("DemoAccount"), false).done(function (msg) {
		ShowWorkshop();
	});
}

function LoginWithInformation(userName, password, skipHash) {
	var def = $.Deferred();

	var data = { userName: userName, password: password, skipHash: skipHash };

	AjaxCallWithWait("/Services/Data/LogInProMaUser", data, $(".loginWaiter"), true).done(function (msg) {
		SetUserInformation(msg);
		def.resolve();
	}).fail(function (msg) {
		def.reject();
	});

	return def.promise();
}

function SetUserInformation(ProMaUserEntityObject) {
	SetCookie(USERNAMECOOKIE, ProMaUserEntityObject.UserName);

	LoggedInUser.userId = ProMaUserEntityObject.User.UserId;
    LoggedInUser.isAdmin = ProMaUserEntityObject.User.IsAdmin;
    LoggedInUser.isDemo = ProMaUserEntityObject.User.IsDemo;
    LoggedInUser.enterAsNewlinePref = ProMaUserEntityObject.User.EnterIsNewLinePref;
    LoggedInUser.emailAddress = ProMaUserEntityObject.User.EmailAddress === null ? "" : ProMaUserEntityObject.User.EmailAddress;
    LoggedInUser.userName = ProMaUserEntityObject.User.UserName;
    LoggedInUser.PassBackPassword = ProMaUserEntityObject.PassBackPassword;
}

function GetLoggedInUserInfo() {
	var def = $.Deferred();

	AjaxCallWithWait("/Services/Data/GetLoggedInUser", null, $(".loginPending"), true)
		.done(function (msg) {
			if (msg === null || typeof (msg) === 'undefined') {
			 // 204 no content returns an undefined
			def.reject(); // The user isn't logged in
		} else {
			SetUserInformation(msg);
			def.resolve();
		}
	}).fail(function (msg) {
		def.reject();
	});

	return def;
}

function LogInProMaUser() {
	if ($("#Login").attr("disabled") !== "disabled") {
		var userName = $("#Username").val();
		var plainPassword = $("#Password").val();

		if (userName.length !== 0 && plainPassword.length !== 0) {
			LoginWithInformation(userName, $.md5(plainPassword), false).done(function () {
				ShowWorkshop();
			}).fail(function () {
				AddFadingWarning($("#Login"), "Invalid login");
			});
		}
		else {
			AddFadingWarning($("#Login"), "Enter Username and Password above");
		}
	}
}

function RegisterProMaUser() {
	if ($("#Register").attr("disabled") !== "disabled") {
		var userName = $("#Username").val();
		var plainPassword = $("#Password").val();

		var md5Password = $.md5(plainPassword);

		if (userName.length === 0 || plainPassword.length === 0) {
			AddFadingWarning($("#Register"), "Enter Username and Password above");
			return;
		}

		if (userName.indexOf("@") !== -1 || userName.indexOf(" ") !== -1 || !VerifyBasicCleanliness(userName)) {
			AddFadingWarning($("#Register"), "Please do not use spaces, quotes, or email addresses.");
			return;
		}

		var data = { userName: userName, md5Password: md5Password, skipHash: false };

		AjaxCallWithWait("/Services/Data/RegisterProMaUser", data, $("#Register"), true)
		.done(function (msg) {
			LogInProMaUser();
		}).fail(function (msg) {
			AddFadingWarning($("#Register"), "Registration failed. Perhaps that name is taken?");
		});
	}
}

function LogOutUser() {
	AjaxCallWithWait("/Services/Data/LogOutUser", null).done(function () {
		location.reload();
	});
}