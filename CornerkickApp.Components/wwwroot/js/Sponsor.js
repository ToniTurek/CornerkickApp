function getNegoDialog(parent, bMain, tblSponsors, iSponsorIx, fMood, iOffer1, iOffer2, iOffer3, _callback_fct=null, tt=null) {
  // Remove existing negotiation dialogs
  var dgCt = document.getElementsByClassName("ui-dialog");
  for (var i = dgCt.length - 1; i >= 0; i--) {
    if (dgCt[i] && dgCt[i].parentElement && dgCt[i].contains(document.getElementById("dlgNego"))) {
      dgCt[i].parentElement.removeChild(dgCt[i]);
    }
  }

  if (fMood < 0) {
    alert("Der Sponsor hat die Verhandlungen abgebrochen.");
  } else {
    var div0 = document.createElement("div");
    div0.id = "dlgNego";
    div0.title = "Vertragsverhandlung";

    // Declare inputs
    var txt1Offer = document.createElement("txt");
    var txt2Offer = document.createElement("txt");
    var txt3Offer = document.createElement("txt");
    var ipt1Req = document.createElement("input");
    var ipt2Req = document.createElement("input");
    var ipt3Req = document.createElement("input");

    /////////////////////////////////////////////////////////////////
    // Create table
    /////////////////////////////////////////////////////////////////
    var tblNego = document.createElement("table");
    tblNego.style.width = "100%";
    tblNego.style.tableLayout = "fixed";
    tblNego.className = "table table-bordered";
    tblNego.style.marginBottom = "10px";

    /////////////////////////////////////////////////////////////////
    // Table head
    /////////////////////////////////////////////////////////////////
    var tHead = tblNego.createTHead();
    var rowHead = tHead.insertRow();
    rowHead.style.backgroundColor = "gray";
    rowHead.style.color = "white";

    var cellHd0 = rowHead.insertCell();
    cellHd0.style.textAlign = "center";
    cellHd0.style.fontWeight = 'bold';

    var cellHd1 = rowHead.insertCell();
    cellHd1.innerHTML = "Geboten";
    cellHd1.style.textAlign = "center";
    cellHd1.style.fontWeight = 'bold';

    var cellHd2 = rowHead.insertCell();
    cellHd2.innerHTML = "Ihre Forderung";
    cellHd2.style.textAlign = "center";
    cellHd2.style.fontWeight = 'bold';

    /////////////////////////////////////////////////////////////////
    // Table body
    /////////////////////////////////////////////////////////////////
    var tBdyNego = document.createElement('tbody');

    // Row 1
    var row1 = tBdyNego.insertRow();

    var cell1 = row1.insertCell();
    cell1.style.paddingRight = "4px";
    cell1.style.fontWeight = "bold";
    cell1.style.textAlign = "right";
    if (bMain) {
      cell1.innerText = "Prämie/Jahr";
    } else {
      cell1.innerText = "Prämie/Heimspiel";
    }

    var cell1Offer = row1.insertCell();
    cell1Offer.style.paddingRight = "6px";
    cell1Offer.style.textAlign = "right";
    txt1Offer.id = "txt1Offer";
    txt1Offer.innerText = iOffer1.toLocaleString() + " €";
    cell1Offer.appendChild(txt1Offer);

    var cell1Req = row1.insertCell();
    cell1Req.style.textAlign = "center";
    ipt1Req.className = "form-control";
    ipt1Req.style.width = "100%";
    ipt1Req.style.textAlign = "right";
    ipt1Req.type = "tel";
    ipt1Req.min = "0";
    ipt1Req.step = "1000";
    ipt1Req.value = (Math.floor(iOffer1 / 1000) * 1300).toFixed(0);
    ipt1Req.autocomplete = "off";
    cell1Req.appendChild(ipt1Req);

    if (bMain) {
      // Row 2
      var row2 = tBdyNego.insertRow();

      var cell2 = row2.insertCell();
      cell2.style.paddingRight = "4px";
      cell2.style.fontWeight = "bold";
      cell2.style.textAlign = "right";
      cell2.innerText = "Siegprämie";

      var cell2Offer = row2.insertCell();
      cell2Offer.style.paddingRight = "6px";
      cell2Offer.style.textAlign = "right";
      txt2Offer.id = "txt2Offer";
      txt2Offer.innerText = iOffer2.toLocaleString() + " €";
      cell2Offer.appendChild(txt2Offer);

      var cell2Req = row2.insertCell();
      cell2Req.style.textAlign = "center";
      ipt2Req.className = "form-control";
      ipt2Req.style.width = "100%";
      ipt2Req.style.textAlign = "right";
      ipt2Req.type = "tel";
      ipt2Req.min = "0";
      ipt2Req.step = "1000";
      ipt2Req.value = (Math.floor(iOffer2 / 1000) * 1300).toFixed(0);
      ipt2Req.autocomplete = "off";
      cell2Req.appendChild(ipt2Req);

      // Row 3
      var row3 = tBdyNego.insertRow();

      var cell3 = row3.insertCell();
      cell3.style.paddingRight = "4px";
      cell3.style.fontWeight = "bold";
      cell3.style.textAlign = "right";
      cell3.innerText = "Meisterprämie";

      var cell3Offer = row3.insertCell();
      cell3Offer.style.paddingRight = "6px";
      cell3Offer.style.textAlign = "right";
      txt3Offer.id = "txt3Offer";
      txt3Offer.innerText = iOffer3.toLocaleString() + " €";
      cell3Offer.appendChild(txt3Offer);

      var cell3Req = row3.insertCell();
      cell3Req.style.textAlign = "center";
      ipt3Req.className = "form-control";
      ipt3Req.style.width = "100%";
      ipt3Req.style.textAlign = "right";
      ipt3Req.type = "tel";
      ipt3Req.min = "0";
      ipt3Req.step = "1000";
      ipt3Req.value = (Math.floor(iOffer3 / 1000) * 1300).toFixed(0);
      ipt3Req.autocomplete = "off";
      cell3Req.appendChild(ipt3Req);
    }

    tblNego.appendChild(tBdyNego);
    div0.appendChild(tblNego);

    /////////////////////////////////////////////////////////////////
    // Mood / Negotiate button
    /////////////////////////////////////////////////////////////////
    var div6 = document.createElement("div");
    div6.style.position = "relative";
    div6.style.width = "96%";
    div6.style.left = "2%";
    div6.style.marginTop = "40px";
    div6.align = "center";

    // Mood
    var objMood = getMoodInfoContainer();

    div6.appendChild(objMood.div);

    // Negotiation button
    var bnNegotiate = document.createElement("button");
    bnNegotiate.type = "submit";
    bnNegotiate.id = "bnNegotiate";
    bnNegotiate.className = "btn btn-primary";
    bnNegotiate.style.width = "100%";
    bnNegotiate.innerText = "verhandeln";
    bnNegotiate.onclick = function () {
      negoSponsor(bMain, tblSponsors, iSponsorIx, this, txt1Offer, txt2Offer, txt3Offer, ipt1Req, ipt2Req, ipt3Req, objMood.txtContractMood, objMood.divThumbContainer, $(div0), tt);
    };

    bnNegotiate.style.marginTop = "30px";
    bnNegotiate.style.zIndex = "99";
    div6.appendChild(bnNegotiate);
    div0.appendChild(div6);

    parent.appendChild(div0);

    // Initialize autoNumeric
    var anOptions = {
      allowDecimalPadding: false,
      decimalCharacter: ",",
      digitGroupSeparator: "."
    };
    new AutoNumeric(ipt1Req, anOptions);
    new AutoNumeric(ipt2Req, anOptions);
    new AutoNumeric(ipt3Req, anOptions);

    setMoodText(fMood, objMood.txtContractMood, objMood.divThumbContainer);

    $(div0).dialog({
      autoOpen: true,
      autoResize: true,
      resizable: true,
      modal: true,
      width: 500,
      open: function (event, ui) {
        if (_callback_fct) {
          _callback_fct();
        }
      },
      buttons: [
        {
          text: "zurück",
          id: "bnNegoDlgBack",
          icon: "ui-icon-closethick",
          class: "btnDialog btn btn-outline-secondary",
          click: function () {
            $(this).dialog('destroy').remove();
          }

          // Uncommenting the following line would hide the text,
          // resulting in the label being used as a tooltip
          //showText: false
        }
      ]
    });
  }
}

function negoSponsor(bMain, tblSponsors, iSponsorIx, bnExe, txt1Offer, txt2Offer, txt3Offer, iptReq1, iptReq2, iptReq3, txtMood, thumb, dlgNego, tt=null) {
  bnExe.disabled = true;

  return $.ajax({
    url: '/Member/NegotiateSponsor',
    dataType: "JSON",
    contentType: 'application/json; charset=utf-8',
    data: {
      bMain: bMain,
      iSponsorIx: iSponsorIx,
      iOffer1: getIntFromString(txt1Offer.innerText),
      iOffer2: getIntFromString(txt2Offer.innerText),
      iOffer3: getIntFromString(txt3Offer.innerText),
      iReq1: getIntFromString(iptReq1.value),
      iReq2: getIntFromString(iptReq2.value),
      iReq3: getIntFromString(iptReq3.value)
    },
    success: function (retObj) {
      interpolateMoodText(
        retObj.mood_start,
        retObj.mood,
        txtMood,
        thumb,
        function () {
          if (txt1Offer) {
            txt1Offer.innerText = retObj.offer1.toLocaleString() + " €";
          }
          if (txt2Offer) {
            txt2Offer.innerText = retObj.offer2.toLocaleString() + " €";
          }
          if (txt3Offer) {
            txt3Offer.innerText = retObj.offer3.toLocaleString() + " €";
          }

          if (tblSponsors) {
            tblSponsors.ajax.reload();
          }

          bnExe.disabled = false;

          // Increment tutorial
          if (tt && tt.bShow) {
            if (tt.iLevel === (iTtMainLevelSponsors * 10) + 1) {
              drawTutorial(
                document.body,
                tt.iLevel + 1,
                tt,
                [
                  new Tt_highlight((iTtMainLevelSponsors * 10) + 2, document.getElementById("bnNegoDlgBack"), true),
                  new Tt_highlight((iTtMainLevelSponsors * 10) + 2, document.getElementById("bnTake"), true)
                ]
                /*,
                    [
                      new Tt_highlight((iTtMainLevelSponsors * 10) + 1, document.getElementById("bnNegotiate")),
                      new Tt_highlight((iTtMainLevelSponsors * 10) + 1, document.getElementById("aMenuClubStaff"))
                    ]*/
              );
            }
          }
        },
        function () {
          if (!bMain) {
            $.when(getLtSponsorBoardIds()).done(function (ltSponsorTmp) {
              loadImageBoards(ltSponsorTmp);
            });
          }

          getDialog(
            document.body,
            "Der Sponsor möchte nicht mehr mit Ihnen verhandeln!",
            "Sponsorenverhandlung",
            null,
            function () {
              if (tblSponsors) {
                tblSponsors.ajax.reload();
              }

              if (bMain) {
                loadImageMain(0);

                document.getElementById("bnTake").disabled = true;
                document.getElementById("bnMainNego").disabled = true;
                document.getElementById("bnMainCancel").disabled = true;
              }

              dlgNego.dialog("close");
            }
          );
        }
      );
    },
    error: function (jqXHR, msg, obj) {
      getDialog(document.body, msg, "ERROR");
    }
  });
}
