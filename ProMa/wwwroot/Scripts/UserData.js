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

	AjaxCallWithWait("/Services/Data/LogInProMaUser", data, $(".loginWaiter"), true, false, false, true, "POST").done(function (msg) {
		SetUserInformation(msg);
		def.resolve();
	}).fail(function (msg) {
		def.reject();
	});

	return def.promise();
}

function SetUserInformation(ProMaUserEntityObject) {
	console.log(ProMaUserEntityObject);
	SetCookie(USERNAMECOOKIE, ProMaUserEntityObject.UserName);

	LoggedInUser = {
		userId: ProMaUserEntityObject.UserId,
		isAdmin: ProMaUserEntityObject.IsAdmin,
		isDemo: ProMaUserEntityObject.IsDemo,
		enterAsNewlinePref: ProMaUserEntityObject.EnterIsNewLinePref,
		emailAddress: ProMaUserEntityObject.EmailAddress === null ? "" : ProMaUserEntityObject.EmailAddress,
		userName: ProMaUserEntityObject.UserName,
		PassBackPassword: ProMaUserEntityObject.PassBackPassword
	};
}

function GetLoggedInUserInfo() {
	var def = $.Deferred();

	AjaxCallWithWait("/Services/Data/GetLoggedInUser", null, $(".loginPending"), true, false, false, false, "GET")
	.done(function (msg) {
		SetUserInformation(msg);

		def.resolve();
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

		AjaxCallWithWait("/Services/Data/RegisterProMaUser", data, $("#Register"), true, false, false, false, "POST")
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