/// <reference path="References.js">

// usage: $(dom).PostedNote(), where dom can be the posted note element or any child element of it
$.fn.PostedNote = function () {
	var $dom = $(this);

	// if we're not on a postedNote, go find one
	if (!$dom.hasClass(".postedNote")) {
		$dom = $dom.closest(".postedNote");

		if ($dom.length == 0) {
			// something really strange happened, and we couldn't find a postedNote
			throw "No posted note from requested dom";
		}
	}

	this.NoteId = $dom.attr("data-noteid");
	this.NoteTypeId = $dom.attr("data-notetypeid");
	this.NoteTitle = $dom.closest(".postedNote").find(".noteTitle").text();
	return this;
}

if (typeof PostedNotes == "undefined") {
	var PostedNotes = {
		PostNewNote: function () {
			var noteText = $("#PostNote").find(".noteTextInput").html();
			var noteTypeId = $("#PostNote").find("select").val();

			SetCookie(DEFAULTNOTETYPECOOKIENAME, noteTypeId);

			if (PostedNotes.VerifyIfPostIsValid(noteText) && $("#PostNote").find("button.noteAreaButton").is(":visible")) {

				var data = { noteText: noteText, noteTypeId: noteTypeId };

				AjaxCallWithWait("/Services/PostedNotes.asmx/PostNote", data, $("#PostNote").find("button.noteAreaButton"), true)
				.done(function (msg) {
					if (msg != null) {
						PostedNotes.AppendNote(msg, true);
						$("#PostNote").find(".noteTextInput").html("");
					}
				});
			} else {
				AddFadingWarning($("#PostNote").find("button.noteAreaButton"), "No note written");
			}
		},
		VerifyIfPostIsValid: function (text) {
			if (text == null || text.trim().length === 0) {
				return false;
			}

			return true;
		},
		AppendNote: function (noteEntity, notFromGetAll) {
			var first = false;
			var $existingDom = $(".postedNote[data-noteid='" + noteEntity.Data.NoteId + "']");

			if ($existingDom.length == 0 && notFromGetAll) {
				first = true;
			}

			if ($("#NoteLandingZone").find(".postedNote").length == 0) {
				PostedNotes.previousAppendedDay = null;
				PostedNotes.lastDayWasHighlighted = false;
			}

			var dateObject = ParseDateFromJSONReturn(noteEntity.Data.PostedTime);
			var comparisonDate = dateObject;

			if ($("#filterComplete").find(".sort").find("input")[0].checked || ($("#filterPinHighlighted").find(".sort").find("input")[0].checked && noteEntity.Data.Highlighted)) {
				comparisonDate = null;

				if (noteEntity.Data.Completed) {
					comparisonDate = ParseDateFromJSONReturn(noteEntity.Data.CompletedTime);
				}
			}

			var appendDaybreak = false;

			var numberOfCheckedSorts = $(".sort").find("input[type=checkbox]:checked").length;

			if (!first && $existingDom.length == 0 && numberOfCheckedSorts <= 1) {
				if (PostedNotes.previousAppendedDay != null) {
					appendDaybreak = comparisonDate == null || ((PostedNotes.previousAppendedDay.getYear() != comparisonDate.getYear() || PostedNotes.previousAppendedDay.getMonth() != comparisonDate.getMonth() || PostedNotes.previousAppendedDay.getDate() != comparisonDate.getDate()));
				}

				if ($("#filterPinHighlighted").find(".sort").find("input")[0].checked) {
					if (noteEntity.Data.Highlighted) {
						appendDaybreak = false;
					} else {
						appendDaybreak = appendDaybreak || PostedNotes.lastDayWasHighlighted;
					}
				}
			}

			if (appendDaybreak) {
				$("#NoteLandingZone").append("<hr/>");
			}

			PostedNotes.previousAppendedDay = comparisonDate;
			PostedNotes.lastDayWasHighlighted = noteEntity.Data.Highlighted;

			var formattedTime = FormatDateString(dateObject, TimeModes.TIMEANDDATEMODE, DateModes.REMOVEYEARIFSAME);

			var timeString = "<span class='postDate' title='Posted at " + FormatDateString(dateObject, TimeModes.TIMEANDDATEMODE, DateModes.FULLDATE) + " by " + noteEntity.PostedUser.UserName + "'><span style='white-space: nowrap;'>" + formattedTime + "</span> by " + noteEntity.PostedUser.UserName + "</span>";

			var completedTimeString = "";

			if (noteEntity.Data.Completed) {
				var completedDateObject = ParseDateFromJSONReturn(noteEntity.Data.CompletedTime);
				var completedFormattedTime = FormatDateString(completedDateObject, TimeModes.TIMEANDDATEMODE, DateModes.REMOVEYEARIFSAME);
				// NOTE: Legacy notes do not have completed user information
				completedTimeString = "<span class='completedDate' title='Marked complete at " + FormatDateString(dateObject, TimeModes.TIMEANDDATEMODE, DateModes.FULLDATE) + (noteEntity.CompletedUser != null ? " by " + noteEntity.CompletedUser.UserName : "") + "'>, marked complete at <span style='white-space: nowrap;'>" + completedFormattedTime + "</span>" + (noteEntity.CompletedUser != null ? " by " + noteEntity.CompletedUser.UserName : "") + "</span>";
			}

			if (noteEntity.Data.EditedTime != null) {
				var editedDateObject = ParseDateFromJSONReturn(noteEntity.Data.EditedTime);
				var editedFormattedTime = FormatDateString(editedDateObject, TimeModes.TIMEANDDATEMODE, DateModes.REMOVEYEARIFSAME);
				completedTimeString += "<span class='editedDate' title='Last edited at " + FormatDateString(editedDateObject, TimeModes.TIMEANDDATEMODE, DateModes.FULLDATE) + (noteEntity.EditedUser != null ? " by " + noteEntity.EditedUser.UserName : "") + "'>, last edited at <span style='white-space: nowrap;'>" + editedFormattedTime + "</span>" + (noteEntity.EditedUser != null ? " by " + noteEntity.EditedUser.UserName : "") + "</span>";
			}

			var noteTitleString = "";

			if (noteEntity.Data.NoteTitle != null) {
				noteTitleString += "<div class='noteTitle'>";
				noteTitleString += noteEntity.Data.NoteTitle;
				noteTitleString += "</div>";
			}

			var toAppend =
				"<div class='postedNote" + (first ? " newlyCreated" : "") + "' data-noteid='" + noteEntity.Data.NoteId + "' data-active='" + noteEntity.Data.Active + "' data-completed='" + noteEntity.Data.Completed + "' data-highlighted='" + noteEntity.Data.Highlighted + "' data-notetypeid='" + (noteEntity.TypeOfNote != null ? noteEntity.TypeOfNote.NoteTypeId : "-1") + "'>" +
					"<span class='noteActionList'>" +
						PostedNotes.GetNoteTypeActionsFromEntity(noteEntity) +
					"</span>" +
					"<div class='stringHolders'>" +
						"<div>" +
							timeString +
							completedTimeString +
						"</div>" +
						"<div title='" + (noteEntity.TypeOfNote != null ? "Note type: " + noteEntity.TypeOfNote.NoteTypeName : "") + "'>" + (noteEntity.TypeOfNote != null ? noteEntity.TypeOfNote.NoteTypeName : "") + "</div>" +
						noteTitleString +
					"</div>" +
					"<span class='postContent'>" +
						FormatNoteTextIntoHTML(noteEntity.Data.NoteText) +
					"</span>" +
				"</div>";

			// Note does not exist yet, append it
			if ($existingDom.length <= 0) {
				var $considered;

				if (first) {
					$("#NoteLandingZone").prepend(toAppend);
					$considered = $("#NoteLandingZone").children(".postedNote").first();
				} else {
					$("#NoteLandingZone").append(toAppend);
					$considered = $("#NoteLandingZone").children(".postedNote").last();
				}

				if (!DoesPostMeetFiltersByDOM($considered)) {
					PostedNotes.RemoveNoteFromDOM($considered, false);
				}
			} else {
				$existingDom.before(toAppend);
				var $constituteNote = $existingDom.prev();
				$existingDom.remove();

				// don't remove based on filters if we're inside a popup (see PopoutNote's close function handler)
				if ($constituteNote.closest(".ui-dialog").length == 0 && !DoesPostMeetFiltersByDOM($constituteNote)) {
					PostedNotes.RemoveNoteFromDOM($constituteNote, false);
				}
			}
		},
		SetNoteActive: function (dom, value) {
			var thisNote = $(dom).PostedNote();
			var def = $.Deferred();

			var data = { noteId: thisNote.NoteId, active: value };

			AjaxCallWithWait("/Services/PostedNotes.asmx/SetNoteActive", data, $(dom), true)
			.done(function (msg) {
				PostedNotes.AppendNote(msg, true);
				def.resolve();
			}).fail(function (msg) {
				def.reject();
			});

			return def;
		},
		EditNote: function (dom) {
			var $dom = $(dom);
			var $noteTextArea = $dom.closest(".postedNote").find(".postContent");

			var text = $noteTextArea.html();

			$dom.closest(".postedNote").find(".hiddenNoteEdit").html(text);

			$dom.closest(".postedNote").find(".noteAction").toggle();
			$dom.closest(".noteActionGroup").find(".noteActionGroup").toggle().find(".noteAction").toggle();

			AppendNoteCreationArea($noteTextArea, PostedNotes.FinishedEditingNote);
		},
		FinishedEditingNote: function ($dom) {
			var $noteTextArea = $dom.closest(".postedNote").find(".noteTextInput");
			var thisNote = $($dom).PostedNote();

			var text = $noteTextArea.html();

			if (PostedNotes.VerifyIfPostIsValid(text)) {
				$dom.closest(".postedNote").find(".noteAction").toggle();
				$dom.closest(".noteActionGroup").find(".noteActionGroup").toggle().find(".noteAction").toggle();

				var noteId = thisNote.NoteId;

				var noteTypeId = parseInt($dom.closest(".postedNote").find("select.noteTypeSelect").val());
				var noteTitle = "";

				if ($dom.find(".noteAreaHolder").find(".noteTitleSetter").length !== 0) {
					noteTitle = $dom.find(".noteAreaHolder").find(".noteTitleSetter").val();
				}

				var data = { noteId: noteId, noteText: text, noteTypeId: noteTypeId, noteTitle: noteTitle };

				$noteTextArea.parent().html(text);

				AjaxCallWithWait("/Services/PostedNotes.asmx/EditNote", data, $dom, true)
				.done(function (msg) {
					PostedNotes.AppendNote(msg, true);
				});
			} else {
				AddFadingWarning($noteTextArea, "No note written", true);
			}
		},
		CancelEditingNote: function (dom) {
			var $dom = $(dom);
			var $hiddenNoteArea = $dom.closest(".postedNote").find(".hiddenNoteEdit");

			var text = $hiddenNoteArea.html();

			$dom.closest(".postedNote").find(".noteAction").toggle();
			$dom.closest(".noteActionGroup").find(".noteActionGroup").toggle().find(".noteAction").toggle();

			$dom.closest(".postedNote").find(".postContent").html(text);
		},
		ToggleHighlightNote: function (dom) {
			var $dom = $(dom);
			var thisNote = $dom.PostedNote();
			var data = { noteId: thisNote.NoteId };

			AjaxCallWithWait("/Services/PostedNotes.asmx/ToggleHighlightNote", data, $dom, true)
			.done(function (msg) {
				PostedNotes.AppendNote(msg, true);
			});
		},
		CompleteNote: function (dom, amount) {
			var $dom = $(dom);
			var thisNote = $dom.PostedNote();

			var data = { noteId: thisNote.NoteId, progressLevel: amount };

			AjaxCallWithWait("/Services/PostedNotes.asmx/SetNoteProgress", data, $dom, true)
			.done(function (msg) {
				PostedNotes.AppendNote(msg, true);
			});
		},
		GetAllNotes: function (updateCookies) {
			var def = $.Deferred();

			if (updateCookies === true) {
				SetCookiesFromSortOptions();
			}

			$("#NoteLandingZone").html("<div class='bigWaitHolder'></div>");

			var sortOption = GetSortOptionString();

			var data = { sortOption: sortOption };

			AjaxCallWithWait("/Services/PostedNotes.asmx/GetAllNotes", data, $(".bigWaitHolder"), false, true)
			.done(function (msg) {
				$("#NoteLandingZone").html("");

				$.each(msg, function (index, value) {
					PostedNotes.AppendNote(value, false);
				});

				def.resolve();
			});

			return def.promise();
		},
		GetNoteTypeActionsFromEntity: function (noteEntity) {
			var deleteOrRestore = noteEntity.Data.Active ? "<a href='javascript:void(0);' onclick='PostedNotes.SetNoteActive(this, false)'><span class='promaicon promaicon-openeye' title='Hide'></span></a>" : "<a href='javascript:void(0);' onclick='PostedNotes.SetNoteActive(this, true)'><span class='promaicon promaicon-closedeye' title='Show'></span></a>";

			var returnThis =
				(noteEntity.TypeOfNote == null || noteEntity.TypeOfNote.Membership.CanUseNotes ?
				"<span class='noteAction hideInDialog'>" +
					"<a href='javascript:void(0);' onclick='PostedNotes.PopoutNote(this)'><span class='promaicon promaicon-popout' title='Popout Note'></span></a>" +
				"</span>" +
				"<span class='noteAction'>" +
					(noteEntity.Data.Highlighted === true ? "<a href='javascript:void(0);' onclick='PostedNotes.ToggleHighlightNote(this)'><span class='promaicon promaicon-star' title='Toggle Highlight'></span></a>" : "<a href='javascript:void(0);' onclick='PostedNotes.ToggleHighlightNote(this)'><span class='promaicon promaicon-emptystar' title='Toggle Highlight'></span></a>") +
				"</span>" +
				"<span class='noteAction'>" +
					(noteEntity.Data.Completed === false ? "<a href='javascript:void(0);' onclick='PostedNotes.CompleteNote(this, true)'><span class='promaicon promaicon-check' title='Complete Note'></span></a>" : "<a href='javascript:void(0);' onclick='PostedNotes.CompleteNote(this, false)'><span class='promaicon promaicon-filledcheck' title='Un-Complete Note'></span></a>") +
				"</span>" +
				"<span class='noteActionGroup editNoteGroup'>" +
					"<span class='noteAction'>" +
						"<a href='javascript:void(0);' onclick='PostedNotes.EditNote(this)'><span class='promaicon promaicon-page' title='Edit Note'></span></a>" +
					"</span>" +
					"<span class='noteAction' style='display:none;'>" +
						"<a href='javascript:void(0);' onclick='PostedNotes.CancelEditingNote(this)'><span class='promaicon promaicon-x' title='Cancel Edit'></span></a>" +
						"<span class='hiddenNoteEdit' style='display:none;'></span>" +
					"</span>" +
				"</span>" +
				"<span class='noteAction'>" +
					deleteOrRestore +
				"</span>" +
			"</span>" : "");

			return returnThis;
		},
		RemoveNoteFromDOM: function ($noteDom, justHide) {
			// RemoveNoteFromDOM JUST removes it from the DOM, it is not deleting the note.
			if (justHide === undefined)
				justHide = false;

			$noteDom.fadeOut(QUICKFADEOUTSPEED).promise().done(function () {
				if (!justHide) {
					$noteDom.remove();
				}
			});
		},
		PopoutNote: function (dom) {
			// add a note wrapper to create a dialog before
			// if we don't do this, dialog information will be attached to the .postedNote element, which makes it harder to manage
			var $noteDom = $(dom).closest(".postedNote");
			$noteDom.before("<div></div>");
			var $wrapperNode = $noteDom.prev();
			$wrapperNode.append($noteDom);

			// we're going to try and remember where this needs to return to
			// could be blown away by changing sorting options, though.
			$wrapperNode.before("<div id='" + $noteDom.PostedNote().NoteId + "dialogreturn'></div>");

			$noteDom = $wrapperNode.find(".postedNote");
			$noteDom.removeClass("newlyCreated");

			// this spaces the note popout position randomly, based on how many notes there are
			// 0 notes already existing puts it dead center, but having 3 existing notes spaces it randomly out off center
			var countOfNotes = $(".PostedNoteDialog").length;
			var maxSpreadCount = 3;
			var intensity = Math.min(countOfNotes, maxSpreadCount);

			var maxWidthDispersion = window.screen.width * .6;
			var maxHeightDispersion = window.screen.height * .4;

			var randomX = Math.round(Math.random() * (intensity * (maxWidthDispersion / maxSpreadCount)) - (intensity * (maxWidthDispersion / maxSpreadCount / 2)));
			var randomY = Math.round(Math.random() * (intensity * (maxHeightDispersion / maxSpreadCount)) - (intensity * (maxHeightDispersion / 3 / 2)));

			$wrapperNode.dialog({
				dialogClass: "PostedNoteDialog",
				width: "auto",
				height: "auto",
				minWidth: "auto",
				minHeight: "auto",
				position: { my: "center", at: "center+" + randomX.toString() + " center+" + randomY.toString(), of: window },
				resizable: false,
				close: function (event, ui) {
					$noteDom = $wrapperNode.find(".postedNote");
					var $dialogReturn = $("#" + $noteDom.PostedNote().NoteId + "dialogreturn");

					if ($dialogReturn.length != 0) {
						$noteDom.insertAfter($dialogReturn);
						$dialogReturn.remove();
					} else {
						$("#NoteLandingZone").prepend($noteDom);
					}

					// consider if we want to remove based on filters now, which was delayed earlier
					if (!DoesPostMeetFiltersByDOM($noteDom)) {
						PostedNotes.RemoveNoteFromDOM($noteDom, false);
					}

					$wrapperNode.closest(".ui-dialog").remove();

					return false;
				}
			});
		},
		NoteImageDragOver: function (dom) {
			$(dom).addClass("imageOver");
		},
		NoteImageDragLeave: function (dom) {
			$(dom).removeClass("imageOver");
		},
		NoteImageDragDrop: function (dom, highParent, e) {
			var droppedFile = e.originalEvent.dataTransfer.files[0];

			var formData = new FormData();

			formData.append("image", droppedFile);

			var $textArea = $(dom).closest(highParent).find(".noteTextInput");

			AjaxCallWithWait("/Services/Data/UploadImage", formData, $(dom), true, false, true, false).done(function (msg) {
				var text = $textArea.html();
				var addBreaklineToBefore = text.length === 0 ?
					false :
					text.match(/<br\/?>\s*$/gi) === null;

				$textArea.html(text + (addBreaklineToBefore ? "<br/>" : "") + "[[image:" + msg.fileName + "]]" + "<br/><br/>");
			}).fail(function (msg) {
				AddFadingWarning($(dom).closest(highParent).find("input[type=file]"), "Upload failed. The image may have been too big? 10MB is the maximum size.", true);
			});

			PostedNotes.NoteImageDragLeave(dom);
		},

		previousAppendedDay: null,
		lastDayWasHighlighted: true
	}
}