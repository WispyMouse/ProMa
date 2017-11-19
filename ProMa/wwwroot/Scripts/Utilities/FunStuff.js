/// <reference path="../References.js">

if (typeof FunStuff === "undefined") {
	var FunStuff = {
		init: function () {
			$("#FunStuff").html(
				"<div class='formSet'>" +
					"<div class='formRow'><span class='label'>&nbsp;</span><button type='button' class='formItem' onclick='FunStuff.HackerButton();' title='Pops out all of your notes, as if this was a 90s hacker film. Warning: You will probably have to refresh the screen after pressing this button.'>90s Hacker Button</button></div>" +
				"</div>" +
				"<div class='formSet'>" +
					"<div class='formRow'><span class='label'>With this note type:</span><select id='MarkovNoteTypes' class='formItem'></select></div>" +
					"<div class='formRow'><span class='label'>&nbsp;</span><button id='MarkovChainButton' type='button' class='formItem' onclick='FunStuff.MarkovChain();' title='Use notes that you have written to make a Markov Chain generated note. Only uses YOUR notes, and only ones that are not inactive.'>Markov Chain</button></div>" +
				"</div>" +
				"<div id='MarkovLanding'></div>"
			);
		},
		HackerButton: function () {
			FunStuff.HackerTimer();
		},
		HackerTimer: function () {
			if ($('#NoteLandingZone .postedNote').length > 0) {
				PostedNotes.PopoutNote($("#NoteLandingZone").find(".postedNote")[0]);
				setTimeout(FunStuff.HackerTimer, 50);
			}
		},
		MarkovChain: function () {
			var data = { noteTypeId: $("#MarkovNoteTypes").val() };

			AjaxCallWithWait("/Services/FunStuff/MarkovPostedNote", data, $("#MarkovChainButton"), true, false).done(function (msg) {
				$("#MarkovLanding").html("<p>" + FormatNoteTextIntoHTML(msg) + "</p>");
			});
		}
	}

	SubscribeToTabCreation("FunStuff", "Fun Stuff", FunStuff.init);
}