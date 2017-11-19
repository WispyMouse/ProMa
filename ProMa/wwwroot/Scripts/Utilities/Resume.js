/// <reference path="../References.js">

if (typeof Resume === "undefined") {
	var Resume = {
		init: function () {
			$("#Resume").html(
				"<p>This tab is here because I, <i>Wispy Mouse</i>, aka <i>Kenneth Gould</i>, have not been employed yet.</p>" +
				"<p><i class='weaktext'>(or I have, but I haven't updated this tab to be removed yet)</i></p>" +
				"<p>My email address is <a href='mailto:kenneth.gould@outlook.com'>Kenneth.Gould@outlook.com</a>. My phone number is 757-275-3679. An updated version of my resume can be found at <a href='/Resume.pdf' target='_blank'>this link</a>.</p>" +
				"<p>The codebase for this site can be found at <a href='https://www.assembla.com/spaces/wispymouse/subversion/source'>this public assembla repository</a>.</p>"
			);
		}
	}

	// SubscribeToTabCreation("Resume", "Resume", Resume.init);
}