/// <reference path="References.js">

$(document).ready(function () {
	GetLoggedInUserInfo().done(function () {
		ShowWorkshop();
	}).fail(function () {
		$(".loginPending").html("<s>" + $(".loginPending").html() + "</s>");

		if ($("#Username").val().length === 0) {
			$("#Username").val(GetCookie(USERNAMECOOKIE, ""));
		}
	});
});

var activeHeartBeat = null;

function ShowWorkshop() {
	document.title = DEFAULTPROMANAME;

	$("#UnloggedArea").remove();

	$("#siteContainer").append(
		"<div id='TopBar'>&nbsp;</div>" +
		"<div id='WorkshopHolder'>" +
			"<div id='NoteGutter'></div>" +
			"<div id='WorkshopRight'>" +
				"<ul><li><a href='#NoteManagement'>Note Management</a></li><li><a href='#WorkshopUtilities'>Workshop Utilities</a></li></ul>" +
				"<div id='NoteManagement' class='ui-corner-all'>" +
					"<div class='formSet' id='PostNote'></div>" +
				"</div>" +
				"<div id='WorkshopUtilities' class='ui-corner-all'>" +
					"<ul></ul>" + // Add to this area with the SubscribeToTabCreation function, which is later added with PopTabCreation
				"</div>" +
			"</div>" +
			"<div id='NoteGutter' class='middle'></div>" +
			"<div id='NoteLandingZone'></div>" +
			"<div id='NoteGutter'></div>" +
		"</div>" +
		"<div id='BottomBar'></div>" // Add to this area with the AddSortOption function
		);

	AppendNoteCreationArea($("#PostNote"), PostedNotes.PostNewNote);

	AddSortOption("Inactive", "Show Inactive", "When unchecked, ProMa does not show inactive notes. When checked, it does. An inactive note is one that has had its &quot;eye&quot; clicked, which psuedo-deletes the note.", "show", false,
		function (dom) { return $("#filterInactive").find(".show").find("input")[0].checked || $.parseJSON($(dom).attr("data-active")) === true; });

	AddSortOption("Inactive", "Only", "ONLY show inactive notes.", "only", false,
		function (dom) { return !$("#filterInactive").find(".only").find("input")[0].checked || ($("#filterInactive").find(".only").find("input")[0].checked && $.parseJSON($(dom).attr("data-active")) === false); });

	AddSortOption("Complete", "Show Complete", "When unchecked, ProMa does not show notes that are completed. When checked, it does.", "show", false,
		function (dom) { return $("#filterComplete").find(".show").find("input")[0].checked || $.parseJSON($(dom).attr("data-completed")) === false; });

	AddSortOption("Complete", "Sort", "Sort by completed notes, which puts completed notes at the top in the order that they were completed.", "sort", false, null);
	AddSortOption("PinHighlighted", "Pin Highlighted", "When checked, highlighted notes are pinned to the top. Otherwise, they&apos;re in the normal sorting order. They still stand out.", "sort", true, null);
	AddSortOption("Many", "Don't Limit", "When unchecked, ProMa only shows the first 100 applicable notes (50 in mobile). This is not saved in your cookies as a preference.", "show", false, null, true);

	AddSortOption("NoteType", "Filter", "Only show notes of this type.", "typefilter", -1,
		function (dom) {
			return $("#filterNoteType").find(".typefilter").find("select").val() === "-1" || $("#filterNoteType").find(".typefilter").find("select").val() === $(dom).attr("data-notetypeid")
		});

	AdjustShowSortOptions();
	HeartBeat();

	$("#WorkshopRight").tabs(
		{
			activate: function (event, ui) {
				if (ui.newPanel.selector === "#WorkshopUtilities") {
					$HTML.addClass("UtilitiesOpen");
				} else {
					$HTML.removeClass("UtilitiesOpen");
				}
			}
		});

	$("#WorkshopUtilities").tabs(
		{
			activate: function (event, ui) {
				SetCookie(LASTWORKSHOPUTILITIESTAB, ui.newPanel.selector);
			}
		});

	PopTabCreation();

	var desiredTabSelector = GetCookie(LASTWORKSHOPUTILITIESTAB, "");
	var desiredTabIndex = 0;
	if (desiredTabSelector.length !== 0) {
		desiredTabIndex = Math.max($(desiredTabSelector).index("#WorkshopUtilities > div"), 0);
	}

	$("#WorkshopUtilities").tabs("option", "active", desiredTabIndex);
	NoteTypeManagement.GetNoteTypes()
		.done(function () {
		LongPoll();
	});
}

function HeartBeat() {
	// Don't keep multiple heartbeats around
	console.log("Heart beat start")
	if (activeHeartBeat !== null) {
		console.log("Heart beat ended due to new heart beat starting");
		activeHeartBeat.reject("multiple");
	}

	activeHeartBeat = AjaxCallWithWait("/Services/Data/HeartBeat", null).done(function (msg) {
		if (msg === true) {
			AdminConsole.AddHeartBeat();
			setTimeout(HeartBeat, 100000);
		} else {
			AdminConsole.AddBrokenHeartBeat();
			AutoRelog();
		}

	}).fail(function (msg) {
		console.log("Heartbeat rejected, probably");
		if (msg === "multiple") {
			return;
		}

		if (msg.readyState === undefined || msg.readyState >= 4) {
			AdminConsole.AddBrokenHeartBeat();
		}
	});
}

function AutoRelog() {
	var previousTheme = Themes.CurrentTheme;
	Themes.ChangeTheme("Offline");

	$("html").append("<div id='AutoRelog'>" +
		"<p>You have been logged out. Either the server has gone down, or your connection to it has been impaired.</p>" +
		"<p>We're attempting to log you back in now.</p>" +
		"</div>");

	var $relogDiv = $("#AutoRelog");
	$relogDiv.dialog({
		modal: true,
		position: { my: "center", at: "center", of: window },
		resizable: false,
		closeOnEscape: false,
		open: function (event, ui) {
			$("#AutoRelog").closest(".ui-dialog").find(".ui-dialog-titlebar-close").remove(); // don't show close button
		}
	});

	AutoRelogLoop(previousTheme);
}

function AutoRelogLoop(previousTheme) {
	LoginWithInformation(LoggedInUser.userName, LoggedInUser.PassBackPassword, true).fail(function () {
		setTimeout(function () {
			Themes.ChangeTheme("Offline");
			AutoRelogLoop(previousTheme);
		}, 1000);
	}).done(function () {
		$("#AutoRelog").closest(".ui-dialog").remove();
		Themes.ChangeTheme(previousTheme); // reset theme back to previous value
		HeartBeat(); // restart heartbeat
	});
}

function UploadImage(dom, highParent) {
	var formData = new FormData();

	if ($(dom).closest(highParent).find("input[type=file]")[0].files.length >= 1) {
		formData.append("image", $(dom).closest(highParent).find("input[type=file]")[0].files[0]);

		var $textArea = $(dom).closest(highParent).find(".noteTextInput");

		AjaxCallWithWait("/Services/Data/UploadImage", formData, $(dom), true, false, true, true).done(function (msg) {
			$textArea.html($textArea.html() + ($textArea.html().length !== 0 ? "\r\n" : "") + "[[image:" + msg + "]]\r\n");
			$(dom).parent().find("input[type=file]").val("");
		}).fail(function (msg) {
			AddFadingWarning($(dom).closest(highParent).find("input[type=file]"), "Upload failed. The image may have been too big? 10MB is the maximum size.", true);
		});
	} else {
		AddFadingWarning($(dom).closest(highParent).find("input[type=file]"), "No image attached.", true);
	}

}

function DoesPostMeetFiltersByDOM(dom) {
	var passed = true;

	filterCheckFunctions.forEach(function (value) {
		if (!passed || value.func(dom) === false)
			passed = false;
	});

	return passed;
}

var tabCreationFunctions = [];

function SubscribeToTabCreation(id, tabName, functionToRun, functionForInclude) {
	if (functionForInclude === undefined)
		functionForInclude = function () { return true; }

	tabCreationFunctions.push({ id: id, tabName: tabName, functionToRun: functionToRun, functionForInclude: functionForInclude });
}

function PopTabCreation() {
	for (var ii = tabCreationFunctions.length - 1; ii >= 0; ii--) {
		if (!tabCreationFunctions[ii].functionForInclude())
			tabCreationFunctions[ii] = null;
	}

	tabCreationFunctions.forEach(function (value) {
		if (value !== null) {
			$("#WorkshopUtilities").children("ul").append("<li><a href='#" + value.id + "'>" + value.tabName + "</a></li>");
			$("#WorkshopUtilities").append("<div id='" + value.id + "'></div>");
		}
	});

	$("#WorkshopUtilities").tabs("refresh");

	tabCreationFunctions.forEach(function (value) {
		if (value !== null) {
			value.functionToRun();
		}
	});

	tabCreationFunctions = [];
}

var filterCheckFunctions = [];

function AddSortOption(id, name, title, type, defaultValue, domPassFunc, doNotSave) {
	if (doNotSave === undefined)
		doNotSave = false;

	// if a sort option with that id already exists, add this as a child to it
	// children have to have their parent checked as well to apply (only the first checkbox of the type is the parent, the rest are siblings of eachother)

	if ($("#filter" + id).length === 0) {
		$("#BottomBar").append("<span id='filter" + id + "' class='optionList' data-id='" + id + "'></span>");
	}

	// there's only one type filter at the moment, the "only show notes of this type"

	if (type === "typefilter") {
		$("#filter" + id).append(
			"<span title='" + title + "' data-defaultvalue='" + defaultValue + "' data-type='" + type + "' class='" + type + "' data-donotsave=" + (doNotSave ? true : false) + ">" + name + "&nbsp;<select onchange='PostedNotes.GetAllNotes(true); AdjustShowSortOptions();'/></select></span>"
		);
	} else {
		$("#filter" + id).append(
			"<span title='" + title + "' data-defaultvalue='" + defaultValue + "' data-type='" + type + "' class='" + type + "' data-donotsave=" + (doNotSave ? true : false) + "><input type='checkbox' onchange='PostedNotes.GetAllNotes(true); AdjustShowSortOptions();'/>" + name + "</span>"
		);
	}

	var domPassFuncInner = domPassFunc;

	if (domPassFuncInner === null) {
		domPassFuncInner = function (dom) { return true; };
	}

	filterCheckFunctions.push({
		func: domPassFuncInner
	});
}

function AdjustShowSortOptions() {
	$.each($(".optionList"), function (index, value) {
		$.each($(value).find("span"), function (innerIndex, innerValue) {
			if ($(innerValue).attr("data-type") !== undefined && $.parseJSON($(innerValue).attr("data-donotsave")) === false) {
				var cookieValue = GetCookie("SORTOPTION" + $(value).attr("data-id") + $(innerValue).attr("data-type"), $(innerValue).attr("data-defaultvalue"));

				if ($(innerValue).find("input[type=checkbox]").length > 0) {
					$(innerValue).find("input[type=checkbox]")[0].checked = $.parseJSON(cookieValue);
				} else if ($(innerValue).attr("data-type") === "typefilter") {
					$(innerValue).find("select").val($.parseJSON(cookieValue));
				}
			}
		});

		// if the first option of a group isn't checked, then hide all the children elements
		$(value).children().first().show();
		$(value).children().first().nextAll().hide();

		if ($(value).children().first().find("input:checked").length === 1) {
			$(value).children().first().nextAll().show();
		} else {
			$(value).children().first().nextAll().hide();
		}
	});
}

function SetCookiesFromSortOptions() {
	$.each($(".optionList"), function (index, value) {
		$.each($(value).find("span"), function (innerIndex, innerValue) {
			if ($(innerValue).attr("data-type") !== undefined) {
				if ($.parseJSON($(innerValue).attr("data-donotsave")) === false) {
					if ($(value).find("span").index(innerValue) === 0 || $(value).children().first().find("input:checked").length === 1) {
						var cookieValue = false;

						// is this a checkbox?
						if ($(innerValue).find("input[type=checkbox]").length > 0) {
							cookieValue = $(innerValue).find("input[type=checkbox]")[0].checked;
						} else if ($(innerValue).attr("data-type") === "typefilter") {
							cookieValue = $(innerValue).find("select").val();
						}

						SetCookie("SORTOPTION" + $(value).attr("data-id") + $(innerValue).attr("data-type"), cookieValue);
					} else {
						SetCookie("SORTOPTION" + $(value).attr("data-id") + $(innerValue).attr("data-type"), $(innerValue).attr("data-defaultvalue"));
					}
				}
			}
		});
	});
}

function GetSortOptionString() {
	var returnThis = "";

	$.each($(".optionList"), function (index, value) {
		if ($(value).children().first().find("input:checked").length === 1 || $(value).children().first().attr("data-type") === "typefilter") {
			$.each($(value).find("span"), function (innerIndex, innerValue) {
				if ($(innerValue).attr("data-type") !== undefined) {
					// is this a checkbox?
					if ($(innerValue).find("input[type=checkbox]").length > 0) {
						if ($(innerValue).find("input[type=checkbox]")[0].checked) {
							returnThis += ("[" + $(innerValue).attr("data-type") + $(value).attr("data-id") + "]").toLowerCase();
						}
					} else if ($(innerValue).attr("data-type") === "typefilter" && $(innerValue).find("select").val() !== -1) {
						returnThis += ("[" + $(value).attr("data-id") + $(innerValue).find("select").val() + "end" + $(value).attr("data-id") + "]").toLowerCase();
					}
				}
			});
		}
	});

	return returnThis;
}

function FormatNoteTextIntoHTML(text) {
	var newHTML = text;

	// change 'pretenders' into &lt; and &gt;
	// that is, if someone knew that <br/> was breakline, change that to &gt;br/&lt; ahead of time
	var validHtmlElementRegexString = "((div)|(br)|(b)|(a)|(img)|(i)|(/)|(figcaption)|(figure))";
	newHTML = newHTML.replace(new RegExp("<(?=" + validHtmlElementRegexString + ")", "g"), "&lt;").replace(new RegExp("(" + validHtmlElementRegexString + ")>", "g"), "$1&gt;");

	// new lines
	newHTML = newHTML.replace(/\\r\\n/g, "<br/>");

	// change **text** into bold
	newHTML = newHTML.replace(/\*\*(.*?)\*\*/g, "<b>$1</b>");

	// change ##text## into italics/weaktext
	newHTML = newHTML.replace(/\#\#(.*?)\#\#/g, "<i class='weaktext'>$1</i>");

	// change [[pixel:text]] into images, with extra limits; they should be as close to pixel perfect as they can be
	newHTML = newHTML.replace(/\[\[pixel:(http[^\]]*)\]\]/g, "<div class='pixelImage imageHolder' onclick='PopoutImage(this);' title='View image in popup'><figure><div><img src='$1' class='postedNotePicture'/></div></figure></div>");
	newHTML = newHTML.replace(/\[\[pixel:([^\]]*)\]\]/g, "<div class='pixelImage imageHolder' onclick='PopoutImage(this);' title='View image in popup'><figure><div><img src='" + window.location.origin + "/Images/UploadedImages/$1' class='postedNotePicture'/></div></figure></div>");

	// change [[image:text]] into images
	// it might be local or might be on the web; because local images are all guid, we can safely assume they'll never start with http
	// we separate these conceptually because locally hosted images may, in the future, move around or have special features (like who uploaded it, etc)
	newHTML = newHTML.replace(/\[\[image:(http[^\]]*)\]\]/g, "<div class='imageHolder' onclick='PopoutImage(this);' title='View image in popup'><figure><div><img src='$1' class='postedNotePicture'/></div></figure></div>");
	newHTML = newHTML.replace(/\[\[image:([^\]]*)\]\]/g, "<div class='imageHolder' onclick='PopoutImage(this);' title='View image in popup'><figure><div><img src='" + window.location.origin + "/Images/UploadedImages/$1' class='postedNotePicture'/></div></figure></div>");

	// change [[text]] into links
	// unlike images, we know that you're never going to link to a relative location
	// so if the link is lacking an http: front, add // to imply
	newHTML = newHTML.replace(/\[\[(?!https?:\/\/)([^\]]*?)\]\]/g, "<a href='//$1' target='_blank'>$1</a>");
	newHTML = newHTML.replace(/\[\[([^\]]*?)\]\]/g, "<a href='$1' target='_blank'>$1</a>");

	// change {{text}} into captions - we want to insert the caption into the figure generated by preceeding images, if possible
	newHTML = newHTML.replace(/<\/figure>.*?<\/div>(?:\s|&nbsp;|<br\/?>)*\{\{(.*?)\}\}/g, "<figcaption>$1</figcaption></figure></div>");

	// otherwise, set up independent captions
	newHTML = newHTML.replace(/\{\{(.*?)\}\}?/g, "<figcaption>$1</figcaption>");

	// change any other < > appropriately
	newHTML = newHTML.replace(new RegExp("<(?!" + validHtmlElementRegexString + ")", "g"), "&lt;").replace(new RegExp("(^" + validHtmlElementRegexString + ")>", "g"), "$1&gt;");

	return newHTML;
}

function FormatHTMLIntoNoteText(html) {
	var newText = html;

	// because this is a content editable, we don't need to change the <br/> in the html into \r\n, like you'd expect
	// it should just stay as a br

	// change figcaption tags into {{text}}, step one; remove any fig captions embedded into the image
	newText = newText.replace(/<figcaption[^>]*>([^<]*?)<\/figcaption>\s*?<\/figure>\s*?<\/div>/g, "</figure></div><br/>{{$1}}");

	// change imgs into [[pixel:text]]
	// they may be locally hosted, start with those then externally
	newText = newText.replace(new RegExp("<div class=['\"]pixelImage imageHolder['\"][^>]*><figure><div><img src=['\"].*?\/UploadedImages\/([^'\"]*)['\"][^>]*></div></figure></div>", "g"), "[[pixel:$1]]");
	newText = newText.replace(new RegExp("<div class=['\"]pixelImage imageHolder['\"][^>]*><figure><div><img src=['\"]([^'\"]*)['\"][^>]*></div></figure></div>", "g"), "[[pixel:$1]]");

	// change imgs into [[image:text]]
	// they may be locally hosted, start with those then externally
	newText = newText.replace(new RegExp("<div class=['\"]imageHolder['\"][^>]*><figure><div><img src=['\"].*?\/UploadedImages\/([^'\"]*)['\"][^>]*></div></figure></div>", "g"), "[[image:$1]]");
	newText = newText.replace(new RegExp("<div class=['\"]imageHolder['\"][^>]*><figure><div><img src=['\"]([^'\"]*)['\"][^>]*></div></figure></div>", "g"), "[[image:$1]]");

	// change bold tags into **text**
	newText = newText.replace(/<b>(.*?)<\/b>/g, "**$1**");
	// change italics tags into ##text##
	newText = newText.replace(/<i[^>]*?>(.*?)<\/i>/g, "\#\#$1\#\#");

	// change links into [[text]]
	// this is working under the assumption the link is the same as the text; e.g. <a href='http://www.google.com' target='_blank'>http://www.google.com</a>
	newText = newText.replace(new RegExp("<a[^>]*>([^<]*)</a>", "g"), "[[$1]]");

	// now just do a general replacement back for any independent captions
	newText = newText.replace(/<figcaption[^>]*>(.*?)<\/figcaption>/g, "{{$1}}");

	return newText;
}

var choreCacheVersion = -1;
var friendshipCacheVersion = -1;

function LongPoll() {
	var longPollConnection = new signalR.HubConnectionBuilder()
		.withUrl("/longpollHub", signalR.HttpTransportType.LongPolling)
		.build();

	longPollConnection.on("LongPollPop", function (newChoreCacheVersion, newFriendshipCacheVersion) {
		if (choreCacheVersion !== newChoreCacheVersion) {
			ChoreList.GetChoreItems();
			choreCacheVersion = newChoreCacheVersion;
		}

		if (friendshipCacheVersion !== newFriendshipCacheVersion) {
			UserConsole.UpdateFriends();
			friendshipCacheVersion = newFriendshipCacheVersion;
		}

		friendshipCacheVersion = newFriendshipCacheVersion;
		longPollConnection.invoke("LongPoll", LoggedInUser.userId, choreCacheVersion, friendshipCacheVersion);
	});

	longPollConnection.start().then(function () { longPollConnection.invoke("LongPoll", LoggedInUser.userId, choreCacheVersion, friendshipCacheVersion); });
}

// Helper function for LongPoll, because it returns a list of key value pairs
function ValueOfKeyInKeyValuePairArray(array, key) {
	var index = -1;

	$.each(array, function (ii, value) {
		if (value.Key === key)
			index = ii;
	});

	if (index === -1) {
		return undefined;
	} else {
		return array[index].Value;
	}
}

// this note area could be appended as a post new note area, or as an edit area
// the noteAreaButtonFunc needs to point to what function to perform, and has to accept the $dom parameter
function AppendNoteCreationArea($dom, noteAreaButtonFunc) {
	var oldHtml = $dom.html();
	var oldHeight = $dom.height();

	var highParent = $dom.closest(".postedNote").length === 0 ? "#NoteManagement" : ".postedNote";

	$dom.html("");
	var oldHtmlValue = FormatHTMLIntoNoteText(oldHtml);

	$dom.append(
		"<div class='formSet noteAreaHolder'>" +
			"<div class='formRow'><div class='noteTextInput' contenteditable='true' style='height: " + oldHeight.toString() + "px'>" + oldHtmlValue + "</div></div>" +
			"<div class='formRow'><button class='noteAreaButton formItem' type='button'>Post</button>&nbsp;as&nbsp;type&nbsp;<select class='noteTypeSelect formItem'></select></div>" +
			"<div class='formRow uploadImage'><input type='file' accept='image/*'/><button type='button' onclick='UploadImage(this, \"" + highParent + "\")'>Upload Image</button></div>" +
		"</div>"
		);

	var $noteArea = $dom.find(".noteTextInput");

	$noteArea.on("dragover dragenter", function () { PostedNotes.NoteImageDragOver($noteArea); }).on("dragleave dragend drop", function () { PostedNotes.NoteImageDragLeave($noteArea); }).on("drop", function (e) { e.preventDefault(); PostedNotes.NoteImageDragDrop($noteArea, highParent, e); e.stopPropagation(); });

	$dom.find("button.noteAreaButton").click(function () { noteAreaButtonFunc($dom); });

	if ($dom.closest(".postedNote").length !== 0) {
		var thisNote = $($dom).PostedNote();

		$dom.find("select.noteTypeSelect").html($("#PostNote select.noteTypeSelect").html());
		$dom.find("select.noteTypeSelect").val(thisNote.NoteTypeId);
	}
}

function ShowStyleGuide() {
	if ($("#StyleGuide").length === 0) {
		$(".helpArea").append("<div id='StyleGuide'><ul>" +
			"<li><b>Bold</b>:**Bold**</li>" +
			"<li><i class='weaktext '>Weak Italics</i>:##Weak Italics##</li>" +
			"<li>Link: [[url]]</li>" +
			"<li>Image link: [[image:url]]</li>" +
			"<li>Pixel image link [[pixel:url]] will try to be 'pixel perfect'</li>" +
			"<li>{{captions}} look nice when put right after images.</li>" +
			"</ul></div>").children("div").dialog({
				close: function () {
					$("#StyleGuide").closest(".ui-dialog").remove();
					$("#StyleGuide").remove();
				}
			});
	} else {
		$("#StyleGuide").dialog("close").remove();
	}
}