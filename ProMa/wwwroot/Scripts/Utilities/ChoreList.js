/// <reference path="../References.js">

if (typeof ChoreList === "undefined") {
	var ChoreList = {
		init: function () {
			var $choreListLanding = $("#ChoreList");

			$choreListLanding.append("<div id='ChoreItems'></div>" +
				"<div class='formSet'>" +
					"<div class='formRow'><input type='text' id='ChoreDate' value='' class='formItem'/><button onclick='ChoreList.ChangeDay(-1); return false;' type='button'>&lt;</button><button onclick='ChoreList.ChangeDay(0); return false;'>today</button><button onclick='ChoreList.ChangeDay(1); return false;'>&gt;</button></div>" +
					"<div class='formRow addItem'><input type='text' class='formItem'/><button onclick='ChoreList.AddChoreItem(); return false;'>Add New Chore</button></div>" +
				"</div>" +
				"<hr/>" +
				"<p>This chore utility helps organize daily tasks that you might have. Ex, take out the trash.</p>" +
				"<p>It remembers every day's checked off status. This may be useful for looking back at when or if you did something. These chores are personally sortable, just try dragging them.</p>" +
				"<p>You can also share chores with your friends. For example, you might share 'give a treat to Fido', so that you don't both overfeed him. Or to show your friend how often YOU do the dishes.</p>");

			$("#ChoreDate").datepicker({}).on("change", function (e) { ChoreList.GetChoreItems(); });
			$("#ChoreDate").datepicker('setDate', TODAY);

			// function called by initial long poll: ChoreList.GetChoreItems();
		},
		AddChoreItem: function () {
			var newItemText = $("#ChoreList").find(".addItem").find("input").val();

			if (newItemText.length > 0) {
				if (!VerifyBasicCleanliness(newItemText)) {
					AddFadingWarning($("#ChoreList").find(".addItem").find("button"), "Invalid chore name");
				} else {
					var data = { newItemName: newItemText };

					$("#ChoreList").find(".addItem").find("input").val("");

					AjaxCallWithWait("/Services/Chores/AddNewChore", data, $("#ChoreList").find(".addItem").find("button"), true)
					.done(function (msg) {
						// This will refresh thanks to the long poll
					});
				}
			} else {
				AddFadingWarning($("#ChoreList").find(".addItem").find("button"), "No chore name");
			}
		},
		GetChoreItems: function () {
			var def = $.Deferred();

			var selectedDate = ChoreList.GetSelectedDate();

			ChoreList.choreListLoadingItems = true;

			if (DateToDay(selectedDate) === TODAYDATENUMBER) {
				// clear each old timeout
				for (var ii = 0; ii < ChoreList.choreAlerts.length; ii++) {
					clearTimeout(ChoreList.choreAlerts[ii]);
				}
				ChoreList.choreAlerts = [];
			}

			var nowTime = new Date();

			var data = { year: selectedDate.getFullYear(), month: selectedDate.getMonth() + 1, day: selectedDate.getDate() };
			AjaxCallWithWait("/Services/Chores/GetChoreItems", data, $("#ChoreItems"), true, true)
			.done(function (msg) {
				$("#ChoreItems").html("");
				$.each(msg, function (index, value) {
					var completionString = "";

					if (value.UserId !== null) {
						var completionTime = ParseDateFromJSONReturn(value.PostedTime);
						var completionTimeString = FormatDateString(completionTime, TimeModes.JUSTTIMEMODE);

						completionString = "<i class='weaktext fadedBackground' title='" + FormatDateString(completionTime, TimeModes.JUSTDATEMODE, DateModes.FULLDATE) + "'>" + value.CompletedUser.UserName + ", " + completionTimeString + "</i>";
					} else if (value.LastDoneUser !== null) {
						var dateMode = DateModes.CASUAL;
						if (DateToDay(selectedDate) !== TODAYDATENUMBER || selectedDate.getFullYear() !== TODAY.getFullYear())
							dateMode = DateModes.REMOVEYEARIFSAME;

						var completionTime = ParseDateFromJSONReturn(value.LastDoneTime);
						var completionTimeString = FormatDateString(completionTime, TimeModes.JUSTDATEMODE, dateMode);
						completionString = "<i class='weaktext fadedBackground' title='" + FormatDateString(completionTime, TimeModes.JUSTDATEMODE, DateModes.FULLDATE) + "'>" + value.LastDoneUser.UserName + ", " + completionTimeString + "</i>";
					}

					var alertHour = value.SharedChore.Membership.AlertHour;
					var alertMinute = value.SharedChore.Membership.AlertMinute;
					var alertTime = "";
					var alertBellString = "";

					if (alertHour != null) {
						alertTime = AMPMString(alertHour, alertMinute);
						alertBellString = "<span class='noteAction'><span class='promaicon promaicon-bell promaicon-clear' title='Alert set for " + alertTime + "'></span></span>";
					}

					$("#ChoreItems").append(
						"<span data-choreid='" + value.SharedChoreId + "' data-alertHour='" + alertHour + "' data-alertMinute='" + alertMinute + "' class='choreListItem'>" +
							"<span class='promaicon promaicon-handle'></span>" +
							"<span class='checkboxHolder'>" +
								"<input type='checkbox' onchange='ChoreList.ChangeChoreItemCompletion(this);' " + (value.Completed ? "checked" : "") + "/>" +
							"</span>" +
							"<span class='choreName' title='" + value.SharedChore.ChoreName + "'>" + value.SharedChore.ChoreName + "</span>" +
							"<span class='noteActionList'>" +
								"<span class='noteAction'>" + completionString + "</span>" +
								alertBellString +
								"<span class='noteAction'><a href='javascript:void(0);' onclick='ChoreList.ManageChore(this, false);'><span class='promaicon promaicon-page' title='Manage this chore.'></span></a></span>" +
							"</span>" +
						"</span>"
						);

					if (DateToDay(selectedDate) === TODAYDATENUMBER) {
						// prime to send an alert if the chore wasn't done today
						if (alertHour != null && alertMinute != null && value.UserId === null) {
							var targetTime = new Date(nowTime);
							targetTime.setHours(alertHour, alertMinute, 0, 0);
							var timeDiff = targetTime - nowTime;

							if (timeDiff > 0) {
								ChoreList.choreAlerts.push(setTimeout(function () { ChoreList.ChoreAlert(value.SharedChore.ChoreName, value.SharedChoreId, alertTime); }, timeDiff));
							} else {
								// we're past due for completing this chore, and it wasn't marked off
								ChoreList.ChoreAlert(value.SharedChore.ChoreName, value.SharedChoreId, alertTime);
							}
						}
					}
				});
				ChoreList.choreListLoadingItems = false;

				$("#ChoreItems").sortable({
					handle: ".promaicon.promaicon-handle",
					stop: function (event, ui) {
						var data = { pairings: [] };

						$.each($("#ChoreItems > span"), function (index, value) {
							data.pairings.push({ Key: parseInt($(value).attr("data-choreid")), Value: index });
						});

						AjaxCallWithWait("/Services/Chores/RememberSorting", data, $("#ChoreItems"), true, true).done(function () {
							ChoreList.GetChoreItems();
						});
					}
				});

				def.resolve();
			});

			return def;
		},
		RemoveChoreMembership: function (choreId) {
			var data = { choreId: choreId };

			if (confirm("Are you sure you want to remove yourself from this chore?")) {
				AjaxCallWithWait("/Services/Chores/RemoveChoreMembership", data, $("#ChoreItems"), true, true)
				.done(function (msg) {
					// This will refresh thanks to the long poll
				});
			}
		},
		ChangeChoreItemCompletion: function (button) {
			var selectedDate = ChoreList.GetSelectedDate();

			var choreId = $(button).closest(".choreListItem").attr("data-choreid");

			var data = { choreId: $(button).closest(".choreListItem").attr("data-choreid"), completed: $(button).closest(".choreListItem").find("input")[0].checked, year: selectedDate.getFullYear(), month: selectedDate.getMonth() + 1, day: selectedDate.getDate() };

			var $existingAlertForId = $(".ChoreAlert[data-choreid=" + choreId.toString() + "]");
			if ($existingAlertForId.length !== 0) {
				$existingAlertForId.closest(".ui-dialog").remove();
				$existingAlertForId.remove();
			}

			AjaxCallWithWait("/Services/Chores/ChangeChoreItemCompletion", data, $(button).parent(), true, false)
			.done(function (msg) {
				// This will refresh thanks to the long poll
			});
		},
		ManageChore: function (button, forceOpen) {
			if ($(button).closest(".choreListItem").find(".manageChore").length !== 0) {
				$(button).closest(".choreListItem").find(".manageChore").remove();

				if (!forceOpen) {
					return false;
				}
			}

			var choreId = $(button).closest(".choreListItem").attr("data-choreid");
			var data = { choreId: choreId };

			AjaxCallWithWait("/Services/Chores/GetUsersNotAssignedToChore", data, $(button), true, false)
			.done(function (msg) {
				var userSelectorHtml = "<option value='-1'>(none)</option>";

				$.each(msg, function (index, value) {
					userSelectorHtml += "<option value='" + value.UserId + "'>" + value.UserName + "</option>";
				});

				$(button).closest(".choreListItem").append("<div class='manageChore'>" +
						"<div class='formSet'>" +
							"<div class='formRow'>" +
								"<select class='userIdSelector'>" + userSelectorHtml + "</select><button type='button' title='Share this chore with the selected user to the left.' onclick='ChoreList.AddChoreMembership(this)'>Share</button>" +
								"<button onclick='ChoreList.RemoveChoreMembership(" + choreId + ")' title='Remove YOURSELF from this chore. All other people will keep this chore, if anyone has it.' type='button'>Remove Chore</button>" +
							"</div>" +
						"</div>" +
						"<div class='formSet'>" +
							"<div class='formRow'>" +
								"<input type='number' min='0' max='23' title='Military-time style hours, where 0 is midnight and 12 is noon.' class='hourInput'/><input type='number' min='0' max='59' title='Minutes.' class='minuteInput'/><button type='button' onclick='ChoreList.SaveAlert(this)' title='Save the alert. Alerts are daily popups reminding you of this chore.''>Save Alert</button><button type='button' onclick='ChoreList.ClearAlert(this)' title='Clears the inputs to the left. This does not save the alert being cleared, so make sure to hit save after this!'>Clear</button>" +
							"</div>" +
						"</div>" +
					"</div>");

				if ($(button).closest(".choreListItem").attr("data-alerthour") !== "null") {
					$(button).closest(".choreListItem").find(".hourInput").val($(button).closest(".choreListItem").attr("data-alerthour"));
					$(button).closest(".choreListItem").find(".minuteInput").val($(button).closest(".choreListItem").attr("data-alertminute"));
				}
			});
		},
		AddChoreMembership: function (button) {
			var choreId = $(button).closest(".choreListItem").attr("data-choreid");
			var userId = $(button).closest(".choreListItem").find(".userIdSelector").val();

			if (userId !== "-1") {
				var data = { choreId: choreId, userId: userId };
				AjaxCallWithWait("/Services/Chores/AssignUserToChore", data, $(button), true, false)
				.done(function (msg) {
					ChoreList.ManageChore($(button).closest(".choreListItem").find(".noteAction").find("a")[0], true);
				});
			}
		},
		GetSelectedDate: function () {
			var selectedDate = TODAY;

			var datePickerValue = $("#ChoreDate").val();

			// date validation taken from this stack overflow answer by Gaurav Kalyan, changed somewhat
			// http://stackoverflow.com/a/27529924
			var parsedValue = Date.parse(datePickerValue);
			if (isNaN(parsedValue) && datePickerValue !== "") {
				$("#ChoreDate").datepicker('setDate', TODAY); // reset date to today
			} else {
				selectedDate = new Date(datePickerValue);
			}

			// planned obsolete by this time; otherwise we get a lot of out of range exceptions
			// it's a bit funny, to me, that I wrote down 3000, then crossed that out and wrote 4000
			// pretty optimistic, right?
			if (selectedDate.getFullYear() > 4000) {
				$("#ChoreDate").datepicker('setDate', TODAY);
				selectedDate = TODAY;
			}

			return selectedDate;
		},
		ChangeDay: function (direction) {
			if (!ChoreList.choreListLoadingItems) {
				// 0 direction sets day to today, and direction isn't sanitized
				// we're counting on the user to provide legitimate input.
				var selectedDate = ChoreList.GetSelectedDate();

				if (direction === 0) {
					selectedDate = TODAY;
				} else {
					selectedDate.setDate(selectedDate.getDate() + direction);
				}

				$("#ChoreDate").datepicker('setDate', selectedDate);

				ChoreList.GetChoreItems();
			}
		},
		ChoreAlert: function (choreText, choreId, timeString) {
			var $existingAlertForId = $(".ChoreAlert[data-choreid=" + choreId.toString() + "]");

			if ($existingAlertForId.length === 0) {
				$("html").append("<div class='ChoreAlert' data-choreid='" + choreId.toString() + "'>" +
				"<p>This is the alert you set for <strong>" + choreText + "</strong>,</p><p>which is going off at <strong>" + timeString + "</strong>!</p>" +
				"</div>");

				var $alertDiv = $(".ChoreAlert").last();

				$alertDiv.dialog({
					modal: false,
					position: { my: "center", at: "center", of: window },
					resizable: false,
					close: function (event, ui) {
						$alertDiv.closest(".ui-dialog").remove();
						$alertDiv.remove();
					}
				});
			}
		},
		ClearAlert: function (button) {
			$(button).parent().find("input").val("");
		},
		SaveAlert: function (button) {
			var alertHour = $(button).closest(".choreListItem").find(".hourInput").val();
			var alertMinute = $(button).closest(".choreListItem").find(".minuteInput").val();

			if (alertHour === "" || alertMinute === "") {
				alertHour = -1;
				alertMinute = -1;
			}

			var data = { choreId: $(button).closest(".choreListItem").attr("data-choreid"), alertHour: alertHour, alertMinute: alertMinute };

			AjaxCallWithWait("/Services/Chores/SaveChoreAlert", data, $(button), true, false).done(function () {
				ChoreList.GetChoreItems();
			});
		},

		choreListLoadingItems: false,
		choreAlerts: []
	}

	SubscribeToTabCreation("ChoreList", "Chore List", ChoreList.init);
}