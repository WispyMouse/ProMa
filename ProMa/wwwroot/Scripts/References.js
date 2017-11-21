/// <reference path="/Imports/jquery/jquery-2.2.2.js">
/// <reference path="/Imports/jquery/jquery-2.2.2.intellisense.js">
/// <reference path="/Imports/jquery/jquery-ui-1.11.4.js">
/// <reference path="/Imports/jquery/jquery.md5.js">

/// <reference path="UIImprovements.js">
/// <reference path="Workshop.js">

/// <reference path="UserData.js">
/// <reference path="PostedNotes.js">

/// <reference path="Utilities/UserConsole.js">
/// <reference path="Utilities/EggTimer.js">
/// <reference path="Utilities/ChoreList.js">
/// <reference path="Utilities/AdminConsole.js">
/// <reference path="Utilities/NoteTypeManagement.js">
/// <reference path="Utilities/FunStuff.js">
/// <reference path="Utilities/Resume.js">
/// <reference path="Utilities/Themes.js">
/// <reference path="Utilities/Calendar.js">

var SLOWFADEOUTSPEED = 4000;
var STANDARDFADEOUTSPEED = 2500;
var QUICKFADEOUTSPEED = 750;
var DEFAULTNOTETYPECOOKIENAME = "defaultnotetypeid";
var DEFAULTPROMANAME = "ProMa";
var USERNAMECOOKIE = "usernamecookie";
var LASTWORKSHOPUTILITIESTAB = "workshoputilitiestab";
var $HTML = $("html");
var $BODY = $("body");

var TODAY = new Date();
var TODAYDATENUMBER = DateToDay(TODAY);

var TimeModes = {
	JUSTTIMEMODE: 1,
	JUSTDATEMODE: 2,
	TIMEANDDATEMODE: 3,
}

var DateModes = {
	FULLDATE: 1,
	REMOVEYEARIFSAME: 2,
	CASUAL: 3
}

function FormatDateString(toFormat, timeMode, dateMode, adjustTimezone) {
	if (timeMode == undefined) {
		timeMode = TimeModes.TIMEANDDATEMODE;
	}

	if (dateMode == undefined) {
		dateMode = DateModes.FULLDATE;
	}

	var fixedTime = new Date(toFormat);

	if (adjustTimezone === false) {
		// add the timezone offset back in to the date, so that it represents UTC instead of local
		fixedTime.setTime(fixedTime.getTime() - 60 * 60 * 1000 * GetTimezoneOffsetInHours(fixedTime));
	}

	var dateString = "";

	var yearIsDifferent = fixedTime.getFullYear() !== TODAY.getFullYear();
	var dayDifference = -1;
	var dateIsBefore = false;

	if (dateMode === DateModes.CASUAL) {
		var thenDate = DateToDay(fixedTime);

		dayDifference = TODAYDATENUMBER - thenDate;

		if (yearIsDifferent) {
			if (fixedTime.getFullYear() > TODAY.getFullYear()) {
				for (var ii = TODAY.getFullYear() ; ii < fixedTime.getFullYear() ; ii++) {
					dayDifference -= IsLeapYear(ii) ? 366 : 365;
				}
			} else {
				for (var ii = fixedTime.getFullYear() ; ii < TODAY.getFullYear() ; ii++) {
					dayDifference += IsLeapYear(ii) ? 366 : 365;
				}
			}
		}

		dateIsBefore = dayDifference > 0;
		dayDifference = Math.abs(dayDifference);
	}

	if (dateMode == DateModes.FULLDATE || dateMode === DateModes.REMOVEYEARIFSAME) {
		dateString = (fixedTime.getMonth() + 1).toString() + "/" + fixedTime.getDate().toString();

		if (yearIsDifferent || dateMode === DateModes.FULLDATE) {
			dateString += "/" + fixedTime.getFullYear().toString();
		}
	}
	else {
		// default shouldn't be hit below; dayDifference should protect against it
		if (dayDifference === 0) {
			dateString = "Today";
		} else if (dayDifference === 1 && dateIsBefore) {
			dateString = "Yesterday";
		} else if (dayDifference === 1 && !dateIsBefore) {
			dateString = "Tomorrow";
		} else if (dayDifference < 8) {
			dateString = InWords(dayDifference) + " Days"

			if (dateIsBefore) {
				dateString += " Ago";
			}
		} else if (dayDifference <= 100) {
			var weekNum = Math.floor(dayDifference / 7);
			dateString = InWords(weekNum) + " Week";

			if (weekNum >= 2) {
				dateString += "s";
			}

			if (dateIsBefore) {
				dateString += " Ago";
			}
		} else if (dayDifference <= 1000) {
			// 30 is the magical month length; while not fully accurate, this won't kick in until 3 months, and that smooths over the curve
			var monthNum = Math.round(dayDifference / 30);
			dateString = InWords(monthNum) + "Months";

			if (dateIsBefore) {
				dateString += " Ago";
			}
		} else {
			dateString = "A Long Time";

			if (dateIsBefore) {
				dateString += " Ago";
			} else {
				dateString += " From Now";
			}
		}
	}

	var timeString = AMPMString(fixedTime.getHours(), fixedTime.getMinutes());

	if (timeMode === TimeModes.JUSTTIMEMODE) {
		return timeString;
	} else if (timeMode === TimeModes.JUSTDATEMODE) {
		return dateString;
	} else {
		return dateString + " " + timeString;
	}
}

function AMPMString(hour, minute) {
	return (hour % 12 + (hour % 12 == 0 ? 12 : 0)).toString() + ":" + (minute < 10 ? "0" : "") + minute.toString() + " " + (hour < 12 ? "AM" : "PM");
}

/* SetCookie and GetCookie mutated from this stack overflow answer from Vignesh Pichamani
http://stackoverflow.com/a/18652401 */
function SetCookie(key, value) {
	var expires = new Date();
	expires.setTime(expires.getTime() + (14 * 24 * 60 * 60 * 1000));
	document.cookie = key + '=' + value + ';expires=' + expires.toUTCString();
}

function GetCookie(key, defaultValue) {
	var keyValue = document.cookie.match('(^|;) ?' + key + '=([^;]*)(;|$)');

	if (keyValue) {
		var returnThis = keyValue[2];

		if (returnThis === "false") {
			return false;
		} if (returnThis === "true") {
			return true;
		}

		if (!isNaN(returnThis) && !isNaN(parseInt(returnThis))) {
			return parseInt(returnThis);
		}

		return returnThis;
	} else {
		return defaultValue;
	}
}

function ParseDateFromJSONReturn(toParse) {
	return new Date(toParse);
}

// http://javascript.about.com/library/bldst.htm
function DaylightSavingsTimeOffset() {
	var jan = new Date(TODAY.getFullYear(), 0, 1);
	var jul = new Date(TODAY.getFullYear(), 6, 1);
	return Math.max(jan.getTimezoneOffset(), jul.getTimezoneOffset()) - TODAY.getTimezoneOffset();
}

function GetTimezoneOffsetInHours(overrideDate) {
	if (overrideDate === undefined) {
		overrideDate = TODAY;
	}

	return -(overrideDate.getTimezoneOffset() / 60);
}

// DaysInFebruary and DateToDay are taken from this answer from Stacked Overflow by Ryan O'Hara
// http://stackoverflow.com/a/8619899
function DaysInFebruary(year) {
	if (IsLeapYear(year)) {
		// Leap year
		return 29;
	} else {
		// Not a leap year
		return 28;
	}
}

function IsLeapYear(year) {
	return year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0);
}

function DateToDay(date) {
	var feb = DaysInFebruary(date.getFullYear());
	var aggregateMonths = [0, // January
				     31, // February
				     31 + feb, // March
				     31 + feb + 31, // April
				     31 + feb + 31 + 30, // May
				     31 + feb + 31 + 30 + 31, // June
				     31 + feb + 31 + 30 + 31 + 30, // July
				     31 + feb + 31 + 30 + 31 + 30 + 31, // August
				     31 + feb + 31 + 30 + 31 + 30 + 31 + 31, // September
				     31 + feb + 31 + 30 + 31 + 30 + 31 + 31 + 30, // October
				     31 + feb + 31 + 30 + 31 + 30 + 31 + 31 + 30 + 31, // November
				     31 + feb + 31 + 30 + 31 + 30 + 31 + 31 + 30 + 31 + 30, // December
	];
	return aggregateMonths[date.getMonth()] + date.getDate();
}

function VerifyBasicCleanliness(toVerify) {
	return (toVerify.trim().length !== 0 && toVerify.indexOf("'") === -1 && toVerify.indexOf('"') === -1 && toVerify.indexOf("\\") === -1);
}

// Taken and mutated from this stack overflow answer by Салман
// http://stackoverflow.com/a/14767071

var ones = ['', 'One ', 'Two ', 'Three ', 'Four ', 'Five ', 'Six ', 'Seven ', 'Eight ', 'Nine ', 'Ten ', 'Eleven ', 'Twelve ', 'Thirteen ', 'Fourteen ', 'Fifteen ', 'Sixteen ', 'Seventeen ', 'Eighteen ', 'Nineteen '];
var tens = ['', '', 'Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];

function InWords(num) {
	if ((num = num.toString()).length > 9) return '{OVERFLOW}';
	n = ('000000000' + num).substr(-9).match(/^(\d{2})(\d{2})(\d{2})(\d{1})(\d{2})$/);
	if (!n) return; var str = '';
	str += (n[1] != 0) ? (ones[Number(n[1])] || tens[n[1][0]] + ' ' + ones[n[1][1]]) : '';
	str += (n[2] != 0) ? (ones[Number(n[2])] || tens[n[2][0]] + ' ' + ones[n[2][1]]) : '';
	str += (n[3] != 0) ? (ones[Number(n[3])] || tens[n[3][0]] + ' ' + ones[n[3][1]]) + 'Thousand ' : '';
	str += (n[4] != 0) ? (ones[Number(n[4])] || tens[n[4][0]] + ' ' + ones[n[4][1]]) + 'Hundred ' : '';
	str += (n[5] != 0) ? ((str != '') ? 'and ' : '') + (ones[Number(n[5])] || tens[n[5][0]] + ' ' + ones[n[5][1]]) : '';
	return str;
}