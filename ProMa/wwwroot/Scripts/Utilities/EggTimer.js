/// <reference path="../References.js">

if (typeof EggTimer === "undefined") {
	var EggTimer = {
		init: function () {
			EggTimer.currentEggTimerSeconds = GetCookie(EggTimer.currentEggTimerSecondsCookieName, 0);
			EggTimer.maxEggTimerSeconds = Math.max(EggTimer.currentEggTimerSeconds, EggTimer.defaultMaxEggTimer);
			EggTimer.timerRunning = GetCookie(EggTimer.timerIsRunningCookieName, false);

			if (isNaN(EggTimer.maxEggTimerSeconds)) {
				EggTimer.maxEggTimerSeconds = 20 * 60;
			}
			if (isNaN(EggTimer.currentEggTimerSeconds)) {
				EggTimer.currentEggTimerSeconds = 0;
			}

			var $EggTimerLanding = $("#EggTimer");

			$EggTimerLanding.append("<div>" +
			"<div class='half'>" +
				"<span id='TimerPercents'><span class='timerPercent'>0%</span><span class='timerPercent'>25%</span><span class='timerPercent'>50%</span><span class='timerPercent'>75%</span><span class='timerPercent'>100%</span><span id='TimePointer'><img src='/Images/Utilities/TimerPointer.png' alt='&gt;' title='&gt;' /></span><span id='TimeCounter'>00:00</span></span>" +
				"<div id='EggTimerCountUp' style='visibility:hidden;'>Last update: <span></span></div>" +
				"<div class='formSet'>" +
					"<div class='formRow'><button onclick='EggTimer.EggTimerAddMinute(); return false;' type='button'>Add minute</button><button onclick='EggTimer.PlusFiveMinutes(); return false;' style='max-width: 2rem;' type='button'>+5</button></div>" +
					"<div class='formRow'><button class='eggTimerStartStop' onclick='EggTimer.EggTimerStartStopToggle(); return false;' type='button'>Start</button></div>" +
					"<div class='formRow'><button onclick='EggTimer.EggTimerReset(); return false;' type='button'>Reset</button></div>" +
					"<div class='formRow'><button onclick='EggTimer.AddCheckmark(true, true); return false;' type='button'>Add Checkmarks</button></div><span id='ReminderToAddCheckmarkHolder' style='display: none;'>Remember<br/>to add a checkmark!</span>" +
					"<div class='formRow'><button onclick='EggTimer.RemoveCheckmarks(); return false;' type='button'>Clear Checkmarks</button></div>" +
				"</div>" +
				"<div id='EggTimerCheckmarkHolderBox'><div id='EggTimerCheckmarkHolder'></div><div id='EggTimerCheckmarkCount' class='weaktext'></div></div>" +
			"</div>" +
			"<div class='half' id='PomodoroText'>" +
			"From <a href='https://en.wikipedia.org/wiki/Pomodoro_Technique'>wikipedia</a>:<ol><li>Decide on task.</li><li>Set timer to 20-25 minutes</li><li>Work on task. Write down inspirations and distractions, but focus on task.</li><li>After timer rings, put a checkmark.</li><li>If you have fewer than four checkmarks, take a short break.</li><li>Else, take a longer break, reset checkmarks, go back to zero.</li></ol><p><i class='weaktext'>Tally marks intentionally cross out at 4 instead of 5.</i></div></div>");

			if (EggTimer.timerRunning) {
				var timeTimerStarted = GetCookie(EggTimer.timerStartTimeCookieName, null);

				if (timeTimerStarted !== null && timeTimerStarted !== "null") {
					var nowTime = new Date();
					var previousDate = new Date(timeTimerStarted);
					var secondsElapsed = Math.floor((nowTime - previousDate) / 1000);
					EggTimer.currentEggTimerSeconds -= secondsElapsed;
					EggTimer.currentEggTimerSeconds = Math.max(0, EggTimer.currentEggTimerSeconds);
					EggTimer.maxEggTimerSeconds = Math.max(EggTimer.currentEggTimerSeconds, EggTimer.defaultMaxEggTimer);
					SetCookie(EggTimer.timerStartTimeCookieName, nowTime);
					EggTimer.timeEggTimerLastUpdated = nowTime;
				}

				if (EggTimer.currentEggTimerSeconds > 0) {
					EggTimer.StartTheTimer();
				}
			}

			EggTimer.AdjustPointer();

			var checkMarksToAdd = GetCookie(EggTimer.checkmarkCookieName, 0);

			for (var ii = 0; ii < checkMarksToAdd; ii++) {
				EggTimer.AddCheckmark(true, false);
			}

			Themes.SetThemeOnCheckmarkCount();
		},
		EggTimerAddMinute: function () {
			if (isNaN(EggTimer.maxEggTimerSeconds)) {
				EggTimer.maxEggTimerSeconds = 20 * 60;
			}
			if (isNaN(EggTimer.currentEggTimerSeconds)) {
				EggTimer.currentEggTimerSeconds = 0;
			}

			EggTimer.currentEggTimerSeconds = EggTimer.currentEggTimerSeconds + 60;
			EggTimer.maxEggTimerSeconds = Math.max(EggTimer.currentEggTimerSeconds, EggTimer.defaultMaxEggTimer);
			SetCookie(EggTimer.currentEggTimerSecondsCookieName, EggTimer.currentEggTimerSeconds);
			SetCookie(EggTimer.timerStartTimeCookieName, new Date());

			if (EggTimer.timerNeedsStopping) {
				EggTimer.StartTheTimer();
			}

			EggTimer.AdjustPointer();
		},
		PlusFiveMinutes: function () {
			for (var ii = 0; ii < 5; ii++) {
				EggTimer.EggTimerAddMinute();
			}
		},
		EggTimerReset: function () {
			EggTimer.currentEggTimerSeconds = 0;
			EggTimer.maxEggTimerSeconds = EggTimer.defaultMaxEggTimer;
			SetCookie(EggTimer.currentEggTimerSecondsCookieName, EggTimer.currentEggTimerSeconds);
			SetCookie(EggTimer.timerIsRunningCookieName, false);
			SetCookie(EggTimer.timerStartTimeCookieName, null);

			$(".eggTimerStartStop").text("Start");
			EggTimer.timerRunning = false;
			$("#EggTimerCountUp").css("visibility", "hidden");

			EggTimer.AdjustPointer();

			if (EggTimer.timerFunction !== null) {
				clearTimeout(EggTimer.timerFunction);
			}

			if (EggTimer.timerNeedsStopping) {
				EggTimer.EggTimerStartStopToggle();
			}
		},
		EggTimerStartStopToggle: function () {
			var $startStop = $(".eggTimerStartStop");

			if (EggTimer.timerNeedsStopping) {
				EggTimer.timerNeedsStopping = false;
				document.title = DEFAULTPROMANAME;
				$startStop.text("Start");
				SetCookie(EggTimer.timerIsRunningCookieName, false);
				SetCookie(EggTimer.timerStartTimeCookieName, null);
				SetCookie(EggTimer.currentEggTimerSecondsCookieName, EggTimer.currentEggTimerSeconds);
				return false;
			}

			if (EggTimer.timerRunning) {
				EggTimer.StopTheTimer();
			} else {
				EggTimer.StartTheTimer();
			}
		},
		StartTheTimer: function () {
			var $startStop = $(".eggTimerStartStop");
			if (EggTimer.currentEggTimerSeconds !== 0) {
				SetCookie(EggTimer.currentEggTimerSecondsCookieName, EggTimer.currentEggTimerSeconds);
				SetCookie(EggTimer.timerIsRunningCookieName, true);
				SetCookie(EggTimer.timerStartTimeCookieName, new Date());
				EggTimer.timeEggTimerLastUpdated = new Date();
				$startStop.text("Stop");
				EggTimer.timerRunning = true;

				if (EggTimer.timerFunction !== null) {
					clearTimeout(EggTimer.timerFunction);
				}

				EggTimer.timerNeedsStopping = false;

				EggTimer.EggTimerLoop();
			} else {
				AddFadingWarning($(".eggTimerStartStop"), "No time left on timer");
			}
		},
		StopTheTimer: function () {
			var $startStop = $(".eggTimerStartStop");
			SetCookie(EggTimer.currentEggTimerSecondsCookieName, EggTimer.currentEggTimerSeconds);
			SetCookie(EggTimer.timerIsRunningCookieName, false);
			SetCookie(EggTimer.timerStartTimeCookieName, null);

			$startStop.text("Start");
			EggTimer.timerRunning = false;
		},
		EggTimerLoop: function () {
			if (EggTimer.timerRunning) {
				// shouldn't be null at this point, but protect against the possibility
				var nowTime = new Date();
				if (EggTimer.timeEggTimerLastUpdated !== null) {
					var secondsElapsed = Math.floor((nowTime - EggTimer.timeEggTimerLastUpdated) / 1000);

					secondsElapsed = Math.floor(secondsElapsed);
					if (secondsElapsed >= 1) {
						EggTimer.currentEggTimerSeconds -= secondsElapsed;
						EggTimer.timeEggTimerLastUpdated = new Date(EggTimer.timeEggTimerLastUpdated.getTime() + 1000 * secondsElapsed);
					}
				} else {
					// assume one second has passed
					EggTimer.currentEggTimerSeconds -= 1;
					EggTimer.timeEggTimerLastUpdated = new Date();
				}

				$("#EggTimerCountUp").css("visibility", "hidden");

				if (EggTimer.currentEggTimerSeconds <= 0) {
					EggTimer.TimerRanOut();
				}
			}
			else {
				if (EggTimer.timeEggTimerLastUpdated !== null) {
					var nowTime = new Date();
					var secondsElapsed = Math.floor((nowTime - EggTimer.timeEggTimerLastUpdated) / 1000);
					var minutes = Math.floor(secondsElapsed / 60);
					var seconds = secondsElapsed % 60;

					$("#EggTimerCountUp").find("span").text(minutes.toString() + ":" + (seconds < 10 ? "0" : "") + seconds.toString());
					$("#EggTimerCountUp").css("visibility", "visible");
				} else {
					$("#EggTimerCountUp").css("visibility", "hidden");
				}
			}

			EggTimer.AdjustPointer();

			if (EggTimer.timerFunction !== null) {
				clearTimeout(EggTimer.timerFunction);
			}

			EggTimer.timerFunction = setTimeout(function () { EggTimer.EggTimerLoop(); }, 1000);
		},
		AddCheckmark: function (updateCookies, updateThemes) {
			var $holder = $("#EggTimerCheckmarkHolder");
			var newHtml = $holder.html();

			EggTimer.currentCheckMarks++;

			if (EggTimer.currentCheckMarks % 4 == 0) {
				newHtml = newHtml.replace(/\|\|\|$/, "<span class='crossedTallies'>|||<span class='tallyCrosser'>&nbsp;</span></span>");
			} else {
				newHtml += "|";
			}

			$holder.html(newHtml);

			$("#EggTimerCheckmarkCount").text(EggTimer.currentCheckMarks.toString());

			if (updateCookies) {
				SetCookie(EggTimer.checkmarkCookieName, EggTimer.currentCheckMarks);
			}

			$("#ReminderToAddCheckmarkHolder").hide();

			if (updateThemes) {
				Themes.SetThemeOnCheckmarkCount();
			}
		},
		RemoveCheckmarks: function () {
			$("#EggTimerCheckmarkHolder").text("");
			EggTimer.currentCheckMarks = 0;
			SetCookie(EggTimer.checkmarkCookieName, 0);
			$("#ReminderToAddCheckmarkHolder").hide();

			Themes.SetThemeOnCheckmarkCount();
			$("#EggTimerCheckmarkCount").text(EggTimer.currentCheckMarks.toString());
		},
		FlashTitleToggle: function () {
			if (document.title === DEFAULTPROMANAME && EggTimer.timerNeedsStopping) {
				document.title = "* * * TIMER COMPLETED * * *";
			} else {
				document.title = DEFAULTPROMANAME;
			}

			if (EggTimer.timerNeedsStopping) {
				setTimeout(function () { EggTimer.FlashTitleToggle(); }, 1000);
			}
		},
		AdjustPointer: function () {
			var minutes = Math.floor(EggTimer.currentEggTimerSeconds / 60);
			var seconds = EggTimer.currentEggTimerSeconds % 60;

			$("#TimeCounter").text(minutes.toString() + ":" + (seconds < 10 ? "0" : "") + seconds.toString());

			var degreeCount = 180 + (EggTimer.currentEggTimerSeconds / EggTimer.maxEggTimerSeconds) * 180;
			$("#TimePointer").css("-ms-transform", "rotate(" + degreeCount.toString() + "deg)");
			$("#TimePointer").css("-webkit-transform", "rotate(" + degreeCount.toString() + "deg)");
			$("#TimePointer").css("transform", "rotate(" + degreeCount.toString() + "deg)");
		},
		TimerRanOut: function () {
			EggTimer.currentEggTimerSeconds = 0;

			EggTimer.timerRunning = false;

			$(".eggTimerStartStop").text("* Stop Timer Alert *");

			SetCookie(EggTimer.timerIsRunningCookieName, false);
			SetCookie(EggTimer.timerStartTimeCookieName, null);
			SetCookie(EggTimer.currentEggTimerSecondsCookieName, 0);
			EggTimer.timerNeedsStopping = true;
			EggTimer.FlashTitleToggle();

			$("#ReminderToAddCheckmarkHolder").show();

			alert("Timer ran out!");
		},

		currentEggTimerSeconds: 0,
		defaultMaxEggTimer: 20 * 60,
		maxEggTimerSeconds: -1, // set below to defaultMaxEggTimer
		timerRunning: false,
		timerNeedsStopping: false,
		timeEggTimerLastUpdated: null,
		currentEggTimerSecondsCookieName: "EggTimer.currentEggTimerSeconds",
		timerIsRunningCookieName: "EggTimer.EGGTIMERISRUNNING",
		timerStartTimeCookieName: "EggTimer.EGGTIMERSTARTTIME",
		timerFunction: null,
		checkmarkCookieName: "EggTimer.EGGTIMERCHECKMARKS",
		currentCheckMarks: 0
	}

	EggTimer.maxEggTimerSeconds = EggTimer.defaultMaxEggTimer;
	SubscribeToTabCreation("EggTimer", "Egg Timer", EggTimer.init);
}