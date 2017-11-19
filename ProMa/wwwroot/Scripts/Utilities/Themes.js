/// <reference path="../References.js">

if (typeof Themes === "undefined") {
	var Themes = {
		init: function () {
			$("#Themes").html(
				"<div class='formSet'>" +
					"<div class='formRow'><span class='label'>Set Style:</span><button type='button' class='formItem' onclick='Themes.ChangeTheme(\"CoolTouch\");'>Cool Touch</button><button type='button' class='formItem' onclick='Themes.ChangeTheme(\"OffWork\");'>Off Work</button></div>" +
					"<div class='formRow'><span class='label'>&nbsp;</span><button type='button' class='formItem' onclick='Themes.ChangeTheme(\"Offline\");'>Offline</button><button type='button' class='formItem' onclick='Themes.ChangeTheme(\"MorningEmbers\");'>Morning Embers</button></div>" +
					"<div class='formRow'><span class='label'>&nbsp;</span><button type='button' class='formItem' onclick='Themes.ChangeTheme(\"Lafayette\");'>Lafayette</button></div>" +
				"</div>" +
				"<hr/>" +
				"<p>The below integrates with the Egg Timer tab. Ex: After four checkmarks, switch style to Cool Touch.</p>" +
				"<div class='formSet'>" +
					"<div class='formRow changeCheckmarkThemeRow'><span class='label'>Default theme:</span><select class='formItem' data-checkmarks='0' onchange='Themes.ChangeCheckmarkTheme(this);'></select></div>" +
					"<div class='formRow changeCheckmarkThemeRow'><span class='label'>After one:</span><select class='formItem' data-checkmarks='1' onchange='Themes.ChangeCheckmarkTheme(this);'></select></div>" +
					"<div class='formRow changeCheckmarkThemeRow'><span class='label'>After four:</span><select class='formItem' data-checkmarks='4' onchange='Themes.ChangeCheckmarkTheme(this);'></select></div>" +
					"<div class='formRow changeCheckmarkThemeRow'><span class='label'>After eight:</span><select class='formItem' data-checkmarks='8' onchange='Themes.ChangeCheckmarkTheme(this);'></select></div>" +
					"<div class='formRow changeCheckmarkThemeRow'><span class='label'>After twelve:</span><select class='formItem' data-checkmarks='12' onchange='Themes.ChangeCheckmarkTheme(this);'></select></div>" +
				"</div>"
			);

			$(".changeCheckmarkThemeRow").each(function (index, value) {
				$(value).find("select").html(
					"<option value='CoolTouch'>Cool Touch</option>" +
					"<option value='OffWork'>Off Work</option>" +
					"<option value='Offline'>Offline</option>" +
					"<option value='MorningEmbers'>Morning Embers</option>" +
					"<option value='Lafayette'>Lafayette</option>"
					);

				$(value).find("select").val(GetCookie(Themes.ChangeCheckmarkThemeCookieName + $(value).find("select").attr("data-checkmarks"), "CoolTouch"));
				SetCookie(Themes.ChangeCheckmarkThemeCookieName + $(value).find("select").attr("data-checkmarks"), $(value).find("select").val());
			});
		},
		ChangeCheckmarkTheme: function (dom) {
			var checkmarkCount = $(dom).attr("data-checkmarks");

			SetCookie(Themes.ChangeCheckmarkThemeCookieName + checkmarkCount, $(dom).val());
			Themes.SetThemeOnCheckmarkCount();
		},
		ChangeTheme: function (themename) {
			Themes.CleanParticles();

			if (themename != Themes.CurrentTheme) {
				Themes.CurrentTheme = themename;

				if (themename === "Offline") {
					$("#ThemeSelector").attr("href", null);
					$("#NewThemeSelector").attr("href", null);
					return;
				}

				$("#NewThemeSelector").attr("href", "/Styles/Themes/" + themename + ".css");

				$.ajax({
					url: "/Styles/Themes/" + themename + ".css",
					cache: true,
					success: function () {
						$("#ThemeSelector").attr("href", "/Styles/Themes/" + themename + ".css");

						Themes.CleanParticles();
						if (themename === "MorningEmbers") {
							Themes.MorningEmbersParticleLoop();
						}

						$("#NewThemeSelector").attr("href", null);
					}
				});
			}
		},
		SetThemeOnCheckmarkCount: function () {
			var fallingCheckmarkCount = EggTimer.currentCheckMarks;

			while (fallingCheckmarkCount >= 0) {
				var newTheme = GetCookie(Themes.ChangeCheckmarkThemeCookieName + fallingCheckmarkCount, "-1");

				if (newTheme !== "-1") {
					Themes.ChangeTheme(newTheme);
					break;
				} else {
					fallingCheckmarkCount--;
				}
			}
		},

		CleanParticles: function () {
			$(".particle").remove();
			if (Themes.ParticleTimer !== null) {
				clearTimeout(Themes.ParticleTimer);
			}
		},
		MorningEmbersParticleLoop: function () {
			var particlesToMake = Math.floor(Math.random() * 3) + 3;

			for (particlesToMake; particlesToMake > 0; particlesToMake--) {
				Themes.ParticleParent().append("<div class='particle'><img src='/Styles/Themes/MorningEmbersParticle.png'/></div>");

				var $thisParticle = $(Themes.ParticleParent().find(".particle").last()[0]);

				var xPos = Math.random() * ($HTML.width() - 40) + 20; // try to avoid having particles on the far left or far right
				var yPos = $HTML.height() + $HTML.height() * (Math.random() * .3);
				$thisParticle.offset({ top: yPos, left: xPos });

				$thisParticle.css("opacity", .25);
				var initialSize = Math.floor(Math.random() * 10) + 20;
				$thisParticle.css("width", initialSize.toString() + "px");
				$thisParticle.css("height", initialSize.toString() + "px");

				var targetSize = "0px";

				$thisParticle.animate({
					top: 0,
					width: targetSize,
				}, { duration: 14000, easing: "linear" }).promise().done(function () { $thisParticle.remove() });
			}

			Themes.ParticleTimer = setTimeout(Themes.MorningEmbersParticleLoop, Math.floor(Math.random() * 1000) + 1000);
		},
		ParticleParent: function () {
			if ($("#ParticleParent").length > 0) {
				return $("#ParticleParent");
			}

			$HTML.append("<span style='position: absolute; left: 0; top:0;' id='ParticleParent'>&nbsp;</span>");
			return $("#ParticleParent");
		},

		ParticleTimer: null,
		ChangeCheckmarkThemeCookieName: "ChangeCheckmarkTheme",
		CurrentTheme: "CoolTouch"
	}

	SubscribeToTabCreation("Themes", "Themes", Themes.init);
}