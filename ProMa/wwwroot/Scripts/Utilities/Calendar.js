/// <reference path="../References.js">

if (typeof Calendar === "undefined") {
	var Calendar = {
		init: function () {
			var calendarLanding = $("#Calendar");

			calendarLanding.html("<div id='CalendarItems'></div>" +
				"<hr/>" +
				"<div class='formSet'>" +
					"<div class='formRow'><span class='label'>Date:</span><input type='text' id='CalendarDate' value='' class='formItem'/></div>" +
					"<div class='formRow'><span class='label'>Name:</span><input type='text' id='CalendarItemName' value='' class='formItem'/></div>" +
					"<div class='formRow'><span class='label'>Yearly:</span><input type='checkbox' id='CalendarYearly' class='formItem'></div>" +
					"<div class='formRow'><span class='label'>&nbsp;</span><button onclick='Calendar.AddItem(); return false;' class='formItem'>Add Calendar Item</button></div>" +
				"</div>");

			$("#CalendarDate").datepicker({});
			$("#CalendarDate").datepicker('setDate', TODAY);

			Calendar.GetItems();
		},
		GetItems: function () {
			AjaxCallWithWait("/Services/Calendar.asmx/GetCalendarEntries", { utcOffset: GetTimezoneOffsetInHours() }, $("#CalendarItems"), true, true)
			.done(function (msg) {
				$("#CalendarItems").html("<ul></ul>");

				$.each(msg.d, function (index, value) {
					value.ForDate = ParseDateFromJSONReturn(value.ForDate);

					$("#CalendarItems ul").append(
						"<li data-calendarid='" + value.CalendarId + "'>" + value.CalendarName +
							"<span class='noteActionList'>" +
								"<span class='noteAction'><i class='weaktext fadedBackground' title='" + FormatDateString(value.ForDate, TimeModes.JUSTDATEMODE, DateModes.FULLDATE, false) + "'>" +
									FormatDateString(value.ForDate, TimeModes.JUSTDATEMODE, DateModes.CASUAL, false) +
								"</i></span>" +
								"<span class='noteAction'>" +
									"<a onclick='Calendar.DeleteDate(this)' href='javascript:void(0);'><span class='promaicon promaicon-x' title='Delete Calendar Entry'></span></a>" +
								"</span>" +
								(value.Yearly ?
								"<span class='noteAction'><span class='promaicon promaicon-refresh promaicon-clear' title='Occurs yearly'></span></span>" :
								"") +
							"</span>" +
						"</li>");
				});
			});
		},
		AddItem: function () {
			var selectedDate = Calendar.GetSelectedDate();

			var data = {
				newItemName: $("#CalendarItemName").val(),
				year: selectedDate.getFullYear(),
				month: selectedDate.getMonth() + 1,
				day: selectedDate.getDate(),
				yearly: $("#CalendarYearly")[0].checked
			};

			AjaxCallWithWait("/Services/Calendar.asmx/AddCalendar", data, $("#Calendar button"), true, true)
			.done(function (msg) {
				Calendar.GetItems();
			});
		},
		DeleteDate: function (button) {
			if (confirm("Are you certain you want to delete this calendar entry?")) {
				var data = { calendarId: $(button).closest("li").attr("data-calendarid") };

				AjaxCallWithWait("/Services/Calendar.asmx/DeleteCalendar", data, $(button).closest("li"), true, false)
				.done(function (msg) {
					Calendar.GetItems();
				});
			}
		},
		GetSelectedDate: function () {
			var selectedDate = TODAY;

			var datePickerValue = $("#CalendarDate").val();

			// date validation taken from this stack overflow answer by Gaurav Kalyan, changed somewhat
			// http://stackoverflow.com/a/27529924
			var parsedValue = Date.parse(datePickerValue);
			if (isNaN(parsedValue) && datePickerValue !== "") {
				$("#CalendarDate").datepicker('setDate', TODAY); // reset date to today
			} else {
				selectedDate = new Date(datePickerValue);
			}

			// planned obsolete by this time; otherwise we get a lot of out of range exceptions
			// it's a bit funny, to me, that I wrote down 3000, then crossed that out and wrote 4000
			// pretty optimistic, right?
			if (selectedDate.getFullYear() > 4000) {
				$("#CalendarDate").datepicker('setDate', TODAY);
				selectedDate = TODAY;
			}

			return selectedDate;
		},
	}

	SubscribeToTabCreation("Calendar", "Calendar", Calendar.init);
}