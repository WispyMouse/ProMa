/// <reference path="../References.js">

if (typeof NoteTypeManagement === "undefined") {
	var NoteTypeManagement = {
		init: function () {
			$("#NoteTypeManagement").append(
				"<div class='formSet' id='AddNoteType'>" +
					"<div class='formRow'><span class='label'>New note type name:</span><input type='text' class='formItem'/></div>" +
					"<div class='formRow'><span class='label'>&nbsp;</span><button onclick='NoteTypeManagement.AddNoteType();' type='button' class='formItem'>Add New Note Type</button></div>" +
				"</div>" +
				"<hr/>" +
				"<span id='OwnNoteActions'>" +
					"<div class='formSet'>" +
						"<div class='formRow'><span class='label'>With this note type:</span><select class='formItem' id='NoteTypeSelector' onchange='NoteTypeManagement.NoteTypeSelectorChanged();'></select></div>" +
					"</div>" +
					"<div class='formSet'>" +
						"<div class='formRow'><span class='label'>Share with:</span><select class='formItem' id='ShareNoteTypeUserTarget'></select></div>" +
						"<div class='formRow' title='If unchecked, the target user can only view notes of this type. If checked, the target can create notes of this type, edit them, etc. They still cannot delete the note type.'><span class='label'>Full privs:</span><input type='checkbox' class='formItem' id='ShareNoteTypeEdit'></div>" +
						"<div class='formRow' id='ShareNoteType'><span class='label'>&nbsp;</span><button class='formItem' onclick='NoteTypeManagement.ShareNoteType();' type='button'>Share</button></div>" +
					"</div>" +
					"<div class='formSet oneClickActions'>" +
						"<div class='formRow' id='DeleteNoteType'><span class='label'>&nbsp;</span><button onclick='NoteTypeManagement.DeleteNoteType();' type='button' class='formItem'>Delete Note Type</button></div>" +
						"<div class='formRow' id='HibernateNoteType'><span class='label'>&nbsp;</span><button type='button' class='formItem'>Hibernate Note Type</button></div>" +
					"</div>" +
					"<div id='RenameNoteForm' class='formSet'>" +
						"<div class='formRow'><span class='label'>Rename to:</span><input type='text' class='formItem'/><button class='formItem' onclick='NoteTypeManagement.RenameNoteType();' type='button'>Rename</button></div>" +
					"</div>" +
				"</span>" +
				"<hr/>" +
				"<div class='formSet'>" +
					"<div class='formRow' id='NoteTypesSharedWithYouArea'><span class='label'>Note types shared with you:</span><ul class='formItem'></ul></div>" +
				"</div>");
		},
		GetNoteTypes: function () {
			var def = $.Deferred();

			AjaxCallWithWait("/Services/NoteTypes.asmx/GetNoteTypes", null, $("#NoteTypeSelector"), true, false)
			.done(function (msg) {
				var ownNoteTypesLabel = "Own Note Types";
				var sharedNoteTypesLabel = "Note Types Shared With You";

				var activeNoteTypesLabel = "Active Note Types";
				var hibernatedNoteTypesLabel = "Hibernated Note Types";

				$("#NoteTypeSelector").html("<optgroup label='" + activeNoteTypesLabel + "'></optgroup><optgroup label='" + hibernatedNoteTypesLabel + "'></optgroup>");
				$("#PostNote").find("select").html("<option value='-1'>(none)</option><optgroup label='" + ownNoteTypesLabel + "'></optgroup><optgroup label='" + sharedNoteTypesLabel + "'></optgroup>");
				$("#BottomBar").find("span[data-type=typefilter]").find("select").html("<option value='-1'>(none)</option><optgroup label='" + ownNoteTypesLabel + "'></optgroup><optgroup label='" + sharedNoteTypesLabel + "'></optgroup>");
				$("#NoteTypesSharedWithYouArea ul").html("");
				$("#MarkovNoteTypes").html("<option value='-1'>All note types</option>");

				var defaultNoteTypePost = GetCookie(DEFAULTNOTETYPECOOKIENAME, -1);
				var defaultNoteTypePostFound = false;

				$.each(msg, function (index, value) {
					var canPostNote = true;
					var isOwner = true;
					var hibernated = value.Hibernated;

					if (!value.Membership.IsCreator) {
						isOwner = false;
						if (!value.Membership.CanUseNotes) {
							canPostNote = false;
						}
					}

					optGroupLabel = activeNoteTypesLabel;

					if (hibernated) {
						optGroupLabel = hibernatedNoteTypesLabel;
					}

					var sharedWithOthers = value.SharedWithOthers != null && value.SharedWithOthers.length > 0;

					if (isOwner) {
						$("#NoteTypeSelector").find("optgroup[label='" + optGroupLabel + "']").append("<option value='" + value.NoteTypeId + "' data-hibernated='" + value.Hibernated + "' title='" + (sharedWithOthers ? "This note type is shared with others" : "") + "'>" + value.NoteTypeName + (sharedWithOthers ? "*" : "") + "</option>");
					} else {
						$("#NoteTypesSharedWithYouArea").find("ul").append("<li><span>" + value.NoteTypeName + "</span><span class='noteActionList'><span class='noteAction'><a href='javascript:void(0);' onclick='NoteTypeManagement.RemoveFromNoteType(" + value.NoteTypeId + ", this);'><span title='Remove yourself from this note type' class='promaicon promaicon-x'></span></a></span></span>" + "</li>");
					}

					if (!hibernated) {
						optGroupLabel = ownNoteTypesLabel;

						if (!isOwner) {
							optGroupLabel = sharedNoteTypesLabel;
						}
						else {
							$("#MarkovNoteTypes").append("<option value='" + value.NoteTypeId + "'>" + value.NoteTypeName + "</option>");
						}

						if (canPostNote) {
							$("#PostNote").find("select").find("optgroup[label='" + optGroupLabel + "']").append("<option value='" + value.NoteTypeId + "' title='" + (sharedWithOthers ? "This note type is shared with others" : "") + "'>" + value.NoteTypeName + (sharedWithOthers ? "*" : "") + "</option>");
						}

						$("#BottomBar").find("span[data-type=typefilter]").find("select").find("optgroup[label='" + optGroupLabel + "']").append("<option value='" + value.NoteTypeId + "'>" + value.NoteTypeName + "</option>");

						if (value.NoteTypeId === defaultNoteTypePost) {
							defaultNoteTypePostFound = true;
						}
					}
				});

				if (!defaultNoteTypePostFound) {
					defaultNoteTypePost = -1;
				}

				$("#PostNote").find("select").val(defaultNoteTypePost);

				$.each($("#BottomBar").find(".optionList").find(".typefilter"), function (index, value) {
					var desiredValue = GetCookie("SORTOPTION" + $(value).parent().attr("data-id") + $(value).attr("data-type"), $(value).attr("data-defaultvalue"));

					if ($(value).find("option[value=" + desiredValue + "]").length == 1) {
						$(value).find("select").val(desiredValue);
					}
					else {
						$(value).find("select").val(-1);
					}
				});

				PostedNotes.GetAllNotes(false).done(function () {
					def.resolve();
				});

				NoteTypeManagement.NoteTypeSelectorChanged();
			});

			return def;
		},
		AddNoteType: function () {
			var text = $("#AddNoteType").find("input").val();

			if (!VerifyBasicCleanliness(text)) {
				AddFadingWarning($("#AddNoteType").find("input"), "Invalid note type name", true);
			} else {
				var data = { noteTypeName: text.trim() }

				$("#AddNoteType").find("input").val("");

				AjaxCallWithWait("/Services/NoteTypes.asmx/AddNoteType", data, $("#AddNoteType").find("button"), true, false)
				.done(function (msg) {
					NoteTypeManagement.GetNoteTypes();
				});
			}
		},
		DeleteNoteType: function () {
			if (confirm("Are you sure you want to delete this note type? All notes of this type will be set to inactive. There is no way to quickly restore this note type.")) {
				var data = { noteTypeId: $("#NoteTypeSelector").val() }

				AjaxCallWithWait("/Services/NoteTypes.asmx/DeleteNoteType", data, $("#DeleteNoteType").find("button"), true, false)
				.done(function (msg) {
					NoteTypeManagement.GetNoteTypes();
				});
			}
		},
		HibernateNoteType: function () {
			if (confirm("Are you sure you want to hibernate this note type? Notes of this type won't show up in your normal note stream, and you won't be able to make more notes of this type. You can restore this note type easily by clicking this button again.")) {
				var data = { noteTypeId: $("#NoteTypeSelector").val() }

				AjaxCallWithWait("/Services/NoteTypes.asmx/HibernateNoteType", data, $("#HibernateNoteType").find("button"), true, false)
				.done(function (msg) {
					NoteTypeManagement.GetNoteTypes();
				});
			}
		},
		RestoreNoteType: function () {
			var data = { noteTypeId: $("#NoteTypeSelector").val() }

			AjaxCallWithWait("/Services/NoteTypes.asmx/RestoreNoteType", data, $("#HibernateNoteType").find("button"), true, false)
			.done(function (msg) {
				NoteTypeManagement.GetNoteTypes();
			});
		},
		ShareNoteType: function () {
			var noteTypeId = $("#NoteTypeSelector").val();
			var userId = $("#ShareNoteTypeUserTarget").val();
			var canEdit = $("#ShareNoteTypeEdit")[0].checked;

			var data = { noteTypeId: noteTypeId, userId: userId, canEdit: canEdit };

			AjaxCallWithWait("/Services/NoteTypes.asmx/ShareNoteType", data, $("#ShareNoteType").find("button"), true, false).done(function () {
				NoteTypeManagement.GetNoteTypes();
			});
		},
		NoteTypeSelectorChanged: function () {
			var selectedIsHibernated = $("#NoteTypeSelector").find("option:selected").attr("data-hibernated") === "true";
			$("#HibernateNoteType button").unbind();

			if (!selectedIsHibernated) {
				$("#HibernateNoteType button").text("Hibernate Note Type");
				$("#HibernateNoteType button").click(function () { NoteTypeManagement.HibernateNoteType(); });
			} else {
				$("#HibernateNoteType button").text("Restore Note Type");
				$("#HibernateNoteType button").click(function () { NoteTypeManagement.RestoreNoteType(); });
			}
		},
		RemoveFromNoteType: function (noteTypeId, dom) {
			var data = { noteTypeId: noteTypeId };

			if (confirm("Are you sure you want to remove yourself from this note type? The owner of the note type would have to share it with you again to see that note type.")) {
				AjaxCallWithWait("/Services/NoteTypes.asmx/RemoveFromNoteType", data, $(dom), true).done(function (msg) {
					NoteTypeManagement.GetNoteTypes();
				});
			}
		},
		RenameNoteType: function () {
			var data = { noteTypeId: $("#NoteTypeSelector").val(), newName: $("#RenameNoteForm").find("input").val() };

			if (data.newName.trim().length === 0 || data.newName.indexOf("'") !== -1 || data.newName.indexOf('"') !== -1 || data.newName.indexOf("\\") !== -1) {
				AddFadingWarning($("#RenameNoteForm").find("input"), "Invalid note type name", true);
			} else {
				AjaxCallWithWait("/Services/NoteTypes.asmx/RenameNoteType", data, $("#RenameNoteForm").find("button"), true).done(function (msg) {
					NoteTypeManagement.GetNoteTypes();
				});
			}
		}
	}

	SubscribeToTabCreation("NoteTypeManagement", "Note Types", NoteTypeManagement.init);
}