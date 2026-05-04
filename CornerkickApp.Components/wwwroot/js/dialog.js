/*
 Required jquery files:
  <link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
  <script type="text/javascript" src="~/lib/jquery/dist/jquery.min.js"></script>
  <script type="text/javascript" src="~/lib/jqueryui/jquery-ui.min.js"></script>
 */

function getDialog(parent, sText, sTitle, fctToExecute = null, fctToExecuteCancel = null) {
  var div0 = document.createElement("div");
  div0.id = "divDialogYN";
  div0.title = sTitle;

  // Contract length
  var div1 = document.createElement("div");
  div1.style.position = "relative";
  div1.style.width = "92%";
  //div1.style.minWidth = "300px";
  div1.style.height = "auto";
  div1.innerHTML = sText;
  div0.appendChild(div1);

  parent.appendChild(div0);

  if (fctToExecute) {
    $(div0).dialog({
      autoOpen: true,
      width: 'auto',
      minWidth: 306,
      buttons: [
        {
          text: "Bestätigen",
          icon: "ui-icon-check",
          //class: "foo bar baz",
          class: "btnDialog btn btn-outline-primary",
          id: "bnOk",
          click: function () {
            fctToExecute();

            $(this).dialog('destroy').remove();
          }
        },
        {
          text: "Abbrechen",
          icon: "ui-icon-closethick",
          class: "btnDialog btn btn-outline-secondary",
          tabIndex: -1,
          click: function () {
            if (fctToExecuteCancel) {
              fctToExecuteCancel();
            }

            $(this).dialog('destroy').remove();
          }

          // Uncommenting the following line would hide the text,
          // resulting in the label being used as a tooltip
          //showText: false
        }
      ]
    });
  } else {
    $(div0).dialog({
      autoOpen: true,
      width: 'auto',
      minWidth: 306,
      buttons: [
        {
          text: "OK",
          icon: "ui-icon-closethick",
          class: "btnDialog btn btn-outline-primary",
          click: function () {
            if (fctToExecuteCancel) {
              fctToExecuteCancel();
            }

            $(this).dialog('destroy').remove();
          }
        }
      ]
    });
  }
}

function setMoodText(fMood, txtMood, thumb) {
  const fMoodAdj = Math.max((fMood - 0.1) / 0.9, 0);
  const color_mood = getColor0_1(fMoodAdj);

  if (txtMood) {
    txtMood.innerText = (fMoodAdj * 100).toFixed(0) + "%";
    txtMood.style.color = color_mood;
  }

  if (thumb) {
    const angle = (1 - fMoodAdj) * 180;
    thumb.style.transform = "rotate(" + angle + "deg)";
    if (thumb.getElementsByTagName("img").length > 0) {
      thumb.getElementsByTagName("img")[0].style.backgroundColor = color_mood;
    } else {
      thumb.style.backgroundColor = color_mood;
    }
  }
}

function getColor0_1(f) {
  return ["rgb(", Math.min(2 * (1 - f) * 255, 255), ",", Math.min(2 * f * 255, 255), ",0)"].join("");
}

function interpolateMoodText(fMood, fMoodFinal, txtMood, thumb, fctOk, fctCancel) {
  setMoodText(fMood, txtMood, thumb);

  fMood = fMood - 0.01;

  if (fMood < 0) {
    if (fctCancel) { fctCancel(); }
    return;
  }

  if (fMood > fMoodFinal) {
    setTimeout(function () { interpolateMoodText(fMood, fMoodFinal, txtMood, thumb, fctOk, fctCancel); }, 50);
  } else {
    if (fctOk) { fctOk(); }
  }
}

function getMoodInfoContainer() {
  const div0 = document.createElement("div");
  div0.style.position = "relative";
  div0.innerHTML += "<b>Verhandlungsbereitschaft: </b>";
  div0.style.zIndex = -1;
  var txtContractMood = document.createElement("text");
  txtContractMood.id = "txtContractMood";
  txtContractMood.innerText = "100%";
  div0.appendChild(txtContractMood);

  const thumbWidth = 60;
  var divThumbContainer = document.createElement("div");
  divThumbContainer.style.position = "absolute";
  divThumbContainer.style.width = (thumbWidth + 4).toString() + "px";
  divThumbContainer.style.height = (thumbWidth + 4).toString() + "px";
  divThumbContainer.style.top = "-30px";
  divThumbContainer.style.right = "0px";
  var divThumbBorder = document.createElement("div");
  divThumbBorder.style.position = "absolute";
  divThumbBorder.style.width = (thumbWidth + 4).toString() + "px";
  divThumbBorder.style.height = (thumbWidth + 4).toString() + "px";
  divThumbBorder.style.top = "0px";
  divThumbBorder.style.left = "0px";
  divThumbBorder.style.border = "4px solid white";
  divThumbBorder.style.zIndex = -2;
  var divThumb = document.createElement("div");
  divThumb.style.position = "absolute";
  divThumb.style.top = "2px";
  divThumb.style.left = "2px";
  divThumb.style.width = thumbWidth + "px";
  divThumb.style.height = thumbWidth + "px";
  divThumb.style.zIndex = -3;
  //divThumbContainer.style.border = "4px solid white";
  var thumb = document.createElement("img");
  thumb.src = "/Content/Icons/thumb.png";
  thumb.width = thumbWidth;
  divThumb.appendChild(thumb);
  /*
  var divThumbContainer = document.createElement("div");
  divThumbContainer.style.width = thumbWidth + "px";
  divThumbContainer.style.height = thumbWidth + "px";
  var thumb = document.createElementNS("/Content/Icons/thumbs_up.svg", "svg");
  thumb.setAttributeNS(null, "viewBox", "0 0 " + thumbWidth + " " + thumbWidth);
  thumb.setAttributeNS(null, "width", thumbWidth);
  thumb.setAttributeNS(null, "height", thumbWidth);
  //thumb.style.display = "block";
  divThumbContainer.appendChild(thumb);
  */
  divThumbContainer.appendChild(divThumb);
  divThumbContainer.appendChild(divThumbBorder);
  div0.appendChild(divThumbContainer);

  return { div: div0, txtContractMood: txtContractMood, divThumbContainer: divThumbContainer };
}
