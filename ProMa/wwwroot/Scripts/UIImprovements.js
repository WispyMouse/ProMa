/// <reference path="References.js">

$(document).ready(function () {

	// The distinction between the two below options is that textareas have a configuration option that changes how we handle the return key
	// the standard for multi line text inputs is to always use textareas on this site

	$(document.body).on("keypress", ".noteTextInput", function (event) {
		if (event.which == jQuery.ui.keyCode.ENTER) {
			// if the user has enter as new line, and the shift key is held, submit
			// if the user has enter as submit, and the shift key is not held, submit
			if ((LoggedInUser.enterAsNewlinePref && event.shiftKey) || (!LoggedInUser.enterAsNewlinePref && !event.shiftKey)) {
				var $formSubmit = RelevantFormSubmit(this);

				if ($formSubmit != null) {
					event.preventDefault();

					// if the user is on a mobile device, focus the button (more importantly, deselect the input we're on so that the keyboard is dismissed)
					if (IsMobileDevice()) {
						$(this).blur();
						$formSubmit.focus();
					}

					$formSubmit.click();
					return false;
				}
			}
		}
	});

	$(document.body).on("keypress", ".formSet input", function (event) {
		if (event.which == jQuery.ui.keyCode.ENTER) {
			var $formSubmit = RelevantFormSubmit(this);

			if ($formSubmit != null) {
				event.preventDefault();

				// if the user is on a mobile device, focus the button (more importantly, deselect the input we're on so that the keyboard is dismissed)
				if (IsMobileDevice) {
					$(this).blur();
					$formSubmit.focus();
				}

				$formSubmit.click();

				return false;
			}
		}
	});

	$(document.body).on("focus", ".noteTextInput, .formSet input", function (event) {
		// if hitting enter would have special logic as per the above handlers, show what button will be pressed
		var $formSubmit = RelevantFormSubmit(this);

		if ($formSubmit != null) {
			$formSubmit.addClass("enterFocus");
		}
	});

	$(document.body).on("blur", ".noteTextInput, .formSet input", function (event) {
		// make sure to undo the above logic (in the focus handler)
		var $formSubmit = RelevantFormSubmit(this);

		if ($formSubmit != null) {
			$formSubmit.removeClass("enterFocus");
		}
	});

	$(document.body).on("click", ".formRow", function (event) {
		// if there is only one input in this row, it has a label, and it has the formItem class, let's intelligently act on it
		if ($(this).find("input.formItem").length == 1 && $(this).find(".label").length == 1) {
			var $formItem = $(this).find("input.formItem");

			if ($formItem.length == 1) {
				if (!$(event.target).is($formItem)) {
					$formItem.focus();
					$formItem.click();

					if ($formItem.attr("type") == "text" || $formItem.attr("type") == "password") {
						$formItem[0].setSelectionRange($formItem.val().length, $formItem.val().length);
					}

					event.preventDefault();
				}
			}
		}
	});

	$(document.body).on("paste", ".noteTextInput", function (event) {
		event.preventDefault();

		if (event.clipboardData || event.originalEvent) {
			content = (event.originalEvent || event).clipboardData.getData('text/plain');

			document.execCommand('insertText', false, content);
		}
		else if (window.clipboardData) {
			content = window.clipboardData.getData('Text');

			document.selection.createRange().pasteHTML(content);
		}
		else {
			alert("Your browser doesn't support pasting from clipboard in to custom content. Or, we couldn't figure out how to do it. Sorry for the inconvenience.");
		}
	});

	$(document.body).on("click", "#BottomBar .optionList span", function (event) {
		// similar treatment above, but focusing on the bottom bar label elements
		if ($(this).find("input").length == 1) {
			var $formItem = $(this).find("input");

			if ($formItem.length == 1) {
				if (!$(event.target).is($formItem)) {
					$formItem.focus();
					$formItem.click();
					event.preventDefault();
				}
			}
		}
	});
});

var WindowIsNavigating = false, InProgressAJAXCalls = [];

$(window).bind('beforeunload', function () {
	WindowIsNavigating = true;

	for (var ii = InProgressAJAXCalls.length - 1; ii >= 0; ii--) {
		InProgressAJAXCalls[ii].abort();
	}
});

function RelevantFormSubmit(dom) {
	var $formParent = $(dom).closest(".formRow");

	if ($formParent.find("button").length == 0) {
		$formParent = $(dom).closest(".formSet");
	}

	if ($formParent.length === 0) {
		return null;
	}

	if ($formParent.find("button").first().length === 0) {
		return null;
	}

	return $formParent.find("button").first();
}

// Taken from this answer by Michael Zaporozhets
// http://stackoverflow.com/a/11381730
function IsMobileDevice() {
	var check = false;
	(function (a) { if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true })(navigator.userAgent || navigator.vendor || window.opera);
	return check;
}

/// Animates a wait icon on a form element. Puts wait icon after element
function AjaxCallWithWait(url, data, $formElement, replaceElementWithWait, bigWait, showFinishIcon, alterHeaders, requestType) {
	var $waitImage = null;

	if (bigWait === undefined) {
		bigWait = false;
	}

	if (requestType === undefined) {
		requestType = "POST";
	}

	if (replaceElementWithWait === undefined) {
		replaceElementWithWait = false;
	}
	
	if (showFinishIcon === undefined) {
		showFinishIcon = true;
	}

	if (alterHeaders === undefined) {
		alterHeaders = true;
	}

	if ($formElement != null && $formElement !== undefined && $formElement.length !== 0) {
		var randomId = "loadingImage" + Math.floor(Math.random() * 10000).toString();

		if (bigWait) {
			if (replaceElementWithWait) {
				$formElement.after("<div class='waitImageHolder bigWait' id='" + randomId + "'><span class='blackOutCover'></span><span><div class='promaiconbig promaiconbig-wait waitImage' title='Loading...' alt='Loading...'/></div></span></div>");
			} else {
				$formElement.append("<span class='waitImageHolder bigWait' id='" + randomId + "'><span class='blackOutCover'></span><span><div class='promaiconbig promaiconbig-wait waitImage' title='Loading...' alt='Loading...'/></div></span></span>");
			}
		} else {
			$formElement.after("<span id='" + randomId + "' class='waitImageHolder'><span><span class='promaicon promaicon-wait' title='Loading...'><span></span></span></span></span>");
		}

		$waitImage = $("#" + randomId);

		if ($formElement[0].tagName === "BUTTON") {
			$($formElement).attr("disabled", "disabled");
		}

		if (replaceElementWithWait) {
			$waitImage.children().css("position", "absolute");
			$waitImage.children().offset($formElement.offset());

			if (!bigWait) {
				var heightTarget = Math.floor($formElement[0].getBoundingClientRect().height);
				var widthTarget = Math.ceil($formElement[0].getBoundingClientRect().width);

				if (heightTarget == 0)
					heightTarget = 16;
				if (widthTarget == 0)
					widthTarget = 24;

				$waitImage.children().css("height", heightTarget);
				$waitImage.children().css("width", widthTarget);
			} else {
				$waitImage.css("position", "absolute");

				// There might not be anything to set the height to; we'll have a fallback in that case
				var outerHeight = $formElement.outerHeight(true);
				var outerWidth = $formElement.outerWidth(true);

				if (outerHeight != 0 && outerWidth != 0) {
					if (outerHeight <= 64)
						outerHeight = 64;

					if (outerWidth <= 64)
						outerWidth = 64;

					$waitImage.css("height", outerHeight);
					$waitImage.css("width", outerWidth);
				}
				else {
					$waitImage.css("height", "64px");
					$waitImage.css("width", "64px");
				}
			}

			$formElement.css("visibility", "hidden");
		}
		else {
			if (bigWait) {
				$waitImage.css("height", "100%");
				$waitImage.css("width", "100%");
			}
		}
	}

	// If there is one piece of information in the data object, then we're going to send this request as a form.
	// Otherwise, stringify the object and place it in the body as a json type data
	var transferedData = null, formFlag = false, firstInformation = "";
	
	if (data == null) {
		transferedData = "";
	} else {
		if (Object.keys(data).length == 1) {
			firstInformation = data[Object.keys(data)[0]].toString();
		}

		if (firstInformation !== "")
		{
			formFlag = true;
			transferedData = "=" + firstInformation;
		} else {
			if (alterHeaders) {
				transferedData = JSON.stringify(data);
			} else if (!alterHeaders) {
				transferedData = data;
			}
		}
	}

	var def = $.Deferred();

	jQuery.support.cors = true;

	var thisAjaxCall = $.ajax({
		url: url,
		type: requestType,
		data: transferedData,
		contentType: !formFlag ? "application/json; charset=utf-8" : "application/x-www-form-urlencoded",
		processData: alterHeaders,
		success: function (msg) {
			if ($waitImage != null) {
				if ($formElement[0].tagName === "BUTTON") {
					$($formElement).removeAttr("disabled");
				}

				if (replaceElementWithWait) {
					$formElement.css("visibility", "");
				} else if (!bigWait && showFinishIcon) {
					$waitImage.after("<span class='waitImageHolder'><span><span class='promaicon promaicon-exclaim' title='Finished!'></span></span></span>").next().fadeOut(STANDARDFADEOUTSPEED);
				}

				$waitImage.remove();
			}

			def.resolve(msg);
		},
		error: function (msg) {
			// On chrome, Ajax calls will fail before onbeforeunload is fired
			// delay responding to a failure so that onbeforeunload can fire first
			setTimeout(function () {
				if (WindowIsNavigating) {
					return;
				}

				if ($waitImage != null) {
					if ($formElement[0].tagName === "BUTTON") {
						$($formElement).removeAttr("disabled");
					}

					if (replaceElementWithWait) {
						$formElement.css("visibility", "initial");
					} else if (!bigWait && showFinishIcon) {
						$waitImage.after("<span class='waitImageHolder'><span><span class='promaicon promaicon-sadface' title='Failure...'></span></span></span>").next().fadeOut(STANDARDFADEOUTSPEED);
					}

					$waitImage.remove();
				}

				def.reject(msg);
			}, 5);
		},
		complete: function (msg) {
			InProgressAJAXCalls.splice(InProgressAJAXCalls.indexOf(thisAjaxCall), 1);
		}
	});

	InProgressAJAXCalls.push(thisAjaxCall);

	return def;
}

function AddFadingWarning($toElement, text, topInsteadOfRight) {
	if (topInsteadOfRight == undefined)
		topInsteadOfRight = false;

	var left = $toElement.offset().left, top = $toElement.offset().top;

	if (topInsteadOfRight) {
		top -= $toElement.outerHeight(false);
	} else {
		left += $toElement.outerWidth(false);
		top -= 2;
	}

	$("body").append("<div style='left: " + left.toString() + "px; top: " + top.toString() + "px;' class='fadingWarning'>" + text + "</div>");
	var $fadingWarning = $("body").children(".fadingWarning").last();
	$fadingWarning.fadeOut(STANDARDFADEOUTSPEED, "easeInCubic", function () { $fadingWarning.remove(); });
}

function RoundPixelsForBrowser(toRound) {
	return Math.ceil(toRound); // todo: rounding! I think this may come in handy later
}

// Lifted mostly from this answer by Jared Farrish from Stack Overflow
// http://stackoverflow.com/a/11047740
(function ($) {
	var $window = $(window),
	    $html = $('html');

	function resize() {
		if ($window.width() < 800) {
			$html.addClass('mobile');
		} else {
			$html.removeClass('mobile');
		}

		if ($window.height() < 500) {
			$html.addClass('short');
		} else {
			$html.removeClass('short');
		}
	}

	$window
	    .resize(resize)
	    .trigger('resize');
})(jQuery);

function PopoutImage(dom) {
	$HTML.append("<div></div>");

	var $thisDialog = $HTML.find("div").last();

	var maxHeight = Math.floor($HTML.height() * .9), maxWidth = Math.floor($HTML.width() * .9);

	if ($(dom).hasClass("pixelImage"))
	{
		maxHeight = "none";
		maxWidth = "none";
	}

	$thisDialog.append($(dom).html());
	$thisDialog.dialog({
		dialogClass: "ImageDialog",
		width: "auto",
		maxHeight: maxHeight,
		maxWidth: maxWidth,
		resizable: false,
		close: function (event, ui) {
			$thisDialog.closest(".ui-dialog").remove();
			$thisDialog.remove();
		}
	});

	$thisDialog.find("figure").height(Math.floor($thisDialog.closest(".ui-dialog").height() - $thisDialog.closest(".ui-dialog").find(".ui-dialog-titlebar").outerHeight(true)));

	var figCaptionHeight = 0;

	if ($thisDialog.find("figcaption").length != 0) {
		figCaptionHeight = $thisDialog.find("figcaption").outerHeight(true);
	}

	$thisDialog.find("img").css("maxHeight", Math.floor($thisDialog.find("figure").height() - figCaptionHeight).toString() + "px");
}