function setPlayerStandard(cbOffence) {
  if (!e) var e = window.event;
  e.cancelBubble = true;
  if (e.stopPropagation) e.stopPropagation();

  var iPlayerIx = cbOffence.getAttribute('data-iPlayerIx');

  $.ajax({
    url: '/Member/TeamSetOffenceFlag',
    dataType: "JSON",
    data: { iPlayerIx: iPlayerIx, bSet: cbOffence.checked }
  });
}

window.showGameIni = (parent, _gameInfo, bDialog, fctExeOnOk, _ltPlayer) => {
  const gameInfo = JSON.parse(_gameInfo);
  const ltPlayer = JSON.parse(_ltPlayer);

  bStopGameIni = false;

  parent.innerHTML = '';

  var div1 = document.createElement("div");
  div1.style.position = "relative";
  div1.style.width = "100%";
  div1.style.maxWidth = "1000px";
  div1.style.height = "auto";

  /*
  var divCupEmblem = document.createElement("div");
  divCupEmblem.style.position = "absolute";
  divCupEmblem.style.top = "0px";
  divCupEmblem.style.left = "0px";
  divCupEmblem.style.textAlign = "left";
  divCupEmblem.style.width = "15%";
  divCupEmblem.innerHTML = gameInfo.sCupEmblem;
  div1.appendChild(divCupEmblem);

  var divGameInfo = document.createElement("div");
  divGameInfo.style.textAlign = "center";
  divGameInfo.style.width = "100%";
  var txtGameDate = document.createElement("p");
  txtGameDate.style.margin = "0px";
  txtGameDate.innerText = gameInfo.sGameDate;
  txtGameDate.style.fontSize = "80%";
  divGameInfo.appendChild(txtGameDate);
  var txtGameInfo = document.createElement("p");
  txtGameInfo.style.fontSize = "140%";
  txtGameInfo.style.margin = "0px";
  txtGameInfo.innerText = gameInfo.sCupName + " - " + (gameInfo.iMatchday + 1).toString() + ". Spieltag";
  divGameInfo.appendChild(txtGameInfo);
  var txtStadium = document.createElement("p");
  txtStadium.style.margin = "0px";
  txtStadium.innerText = gameInfo.sStadium;
  txtStadium.style.fontSize = "80%";
  divGameInfo.appendChild(txtStadium);
  div1.appendChild(divGameInfo);

  // Club emblems row
  var divClubEmblems = document.createElement("div");
  divClubEmblems.className = "flex-container";
  divClubEmblems.style.display = "flex";
  divClubEmblems.style.flexDirection = "row";
  divClubEmblems.style.width = "100%";

  var divEmblemH = document.createElement("div");
  divEmblemH.style.width = "50%";
  divEmblemH.style.textAlign = "center";
  var divClubEmblemH = document.createElement("div");
  divClubEmblemH.style.display = "inline-block";
  divClubEmblemH.style.width = "20%";
  divClubEmblemH.innerHTML = gameInfo.sClubEmblemH;
  divEmblemH.appendChild(divClubEmblemH);
  divClubEmblems.appendChild(divEmblemH);

  var divEmblemA = document.createElement("div");
  divEmblemA.style.width = "50%";
  divEmblemA.style.textAlign = "center";
  var divClubEmblemA = document.createElement("div");
  divClubEmblemA.style.display = "inline-block";
  divClubEmblemA.style.width = "20%";
  divClubEmblemA.innerHTML = gameInfo.sClubEmblemA;
  divEmblemA.appendChild(divClubEmblemA);
  divClubEmblems.appendChild(divEmblemA);
  div1.appendChild(divClubEmblems);

  // Club names row
  var divTeams = document.createElement("div");
  divTeams.className = "flex-container";
  divTeams.style.display = "flex";
  divTeams.style.flexDirection = "row";
  divTeams.style.width = "100%";

  var divH = document.createElement("div");
  divH.style.width = "50%";
  divH.style.textAlign = "center";
  var txtClubNameH = document.createElement("p");
  txtClubNameH.innerText = gameInfo.sClubNameH;
  txtClubNameH.style.fontSize = "130%";
  txtClubNameH.style.fontFamily = "Tahoma";
  txtClubNameH.style.margin = "0px";
  divH.appendChild(txtClubNameH);
  var txtClubPlaceH = document.createElement("p");
  txtClubPlaceH.innerText = gameInfo.sClubPlaceH;
  txtClubPlaceH.style.fontSize = "70%";
  txtClubPlaceH.style.fontFamily = "Tahoma";
  divH.appendChild(txtClubPlaceH);
  divTeams.appendChild(divH);

  var divA = document.createElement("div");
  divA.style.width = "50%";
  divA.style.textAlign = "center";
  var txtClubNameA = document.createElement("p");
  txtClubNameA.innerText = gameInfo.sClubNameA;
  txtClubNameA.style.fontSize = "130%";
  txtClubNameA.style.fontFamily = "Tahoma";
  txtClubNameA.style.margin = "0px";
  divA.appendChild(txtClubNameA);
  var txtClubPlaceA = document.createElement("p");
  txtClubPlaceA.innerText = gameInfo.sClubPlaceA;
  txtClubPlaceA.style.fontSize = "70%";
  txtClubPlaceA.style.fontFamily = "Tahoma";
  divA.appendChild(txtClubPlaceA);
  divTeams.appendChild(divA);
  div1.appendChild(divTeams);
  */

  // Play cup anthem
  var audioAnthem;
  if (gameInfo.sCupAnthem) {
    audioAnthem = new Audio(sContentDir + "/Sounds/cup_anthems/" + gameInfo.sCupAnthem);
    if (audioAnthem) {
      audioAnthem.volume = 0.7;
      audioAnthem.play();
    }
  }

  // Draw player
  if (ltPlayer) {
    var divPitch = document.createElement("div");
    divPitch.style.position = "relative";
    divPitch.style.width = "100%";
    divPitch.style.borderWidth = "3px";
    divPitch.style.borderColor = "lightgray";
    divPitch.style.borderStyle = "outset";
    var imgPitch = document.createElement("img");
    imgPitch.style.position = "relative";
    imgPitch.style.width = "100%";
    imgPitch.src = sContentDir + "/Images/stadium/field.png"
    divPitch.appendChild(imgPitch);
    var divPitchPlayerContainer = document.createElement("div");
    divPitchPlayerContainer.style.position = "absolute";
    divPitchPlayerContainer.style.left = "5%";
    divPitchPlayerContainer.style.width = "90%";
    divPitchPlayerContainer.style.height = "90%";
    divPitchPlayerContainer.style.top = "3%";
    divPitch.appendChild(divPitchPlayerContainer);
    div1.appendChild(divPitch);
  }

  if (bDialog) {
    /*
    var buttons = [];
    if (fctExeOnOk) {
      buttons = [
        {
          text: "Anstoß!",
          icon: "ui-icon-check",
          id: "bnStartGame",
          click: fctExeOnOk,
          class: "btn btn-outline-secondary",
          style: "width: 100%"
        }
      ];
    }

    if (ltPlayer) {
      imgPitch.addEventListener("load", function () {
        $(div0).dialog({
          autoOpen: true,
          width: 'auto',
          buttons: buttons,
          open: function () {
            $("#bnStartGame").button().prepend('<span><img src="/Content/Icons/whistle.png" width=24 style="margin-right:6px"></span>');
          },
          close: function (event, ui) {
            if (audioAnthem) {
              audioAnthem.pause();
              if (audioAnthem.parentNode) { audioAnthem.parentNode.removeChild(audioAnthem); }
            }
          }
        });
      });
    } else {
      $(div0).dialog({
        autoOpen: true,
        width: 'auto',
        buttons: buttons,
        open: function () {
          $("#bnStartGame").button().prepend('<span><img src="/Content/Icons/whistle.png" width=24 style="margin-right:6px"></span>');
        },
        close: function (event, ui) {
          if (audioAnthem) {
            audioAnthem.pause();
            if (audioAnthem.parentNode) { audioAnthem.parentNode.removeChild(audioAnthem); }
          }
        }
      });
    }
    */
  }

  if (ltPlayer) {
    /*
    $.ajax({
      url: '/Member/GetGamePreviewPlayer',
      type: "GET",
      dataType: "JSON",
      contentType: "application/json; charset=utf-8",
      success: function (retObj) {
      */
    const ltClubEmblems = [gameInfo.sClubEmblemH, gameInfo.sClubEmblemA];
    getPlayerBox(divPitchPlayerContainer, ltPlayer, 0, 0, ltClubEmblems);
/*      }
    });*/
  }

  /*
  // Draw referee
  var divReferee = document.createElement("div");
  divReferee.style.position = "relative";
  divReferee.style.marginTop = "10px";
  divReferee.style.width = "fit-content";
  //divReferee.style.right = "0px";
  divReferee.style.border = "1px solid gray";
  divReferee.style.borderRadius = "4px";
  divReferee.style.padding = "4px";
  divReferee.title = "Schiedsrichter";
  var imgWhistle = document.createElement("img");
  imgWhistle.style.position = "relative";
  imgWhistle.style.top = "-6px";
  imgWhistle.style.margin = "0px";
  imgWhistle.width = 20;
  imgWhistle.src = sContentDir + "/Icons/whistle.png"
  divReferee.appendChild(imgWhistle);
  if (gameInfo.referee && gameInfo.referee.fQuality > 0 && gameInfo.referee.fStrict > 0) {
    var tblReferee = document.createElement("table");
    var row1 = tblReferee.insertRow();
    var cell11 = row1.insertCell();
    cell11.style.textAlign = "right";
    cell11.innerText = "Qualität:";
    var cell12 = row1.insertCell();
    cell12.style.textAlign = "right";
    cell12.innerText = (gameInfo.referee.fQuality * 100).toFixed(1) + '%';
    var row2 = tblReferee.insertRow();
    var cell21 = row2.insertCell();
    cell21.style.textAlign = "right";
    cell21.innerText = "Härte:";
    var cell22 = row2.insertCell();
    cell22.style.textAlign = "right";
    cell22.innerText = (gameInfo.referee.fStrict * 100).toFixed(1) + '%';
    divReferee.appendChild(tblReferee);
  } else {
    var txtRefereeUnknown = document.createElement("p");
    txtRefereeUnknown.innerText = "Noch nicht zugewiesen";
    txtRefereeUnknown.style.margin = "0px";
    divReferee.appendChild(txtRefereeUnknown);
  }
  div1.appendChild(divReferee);
  */

  parent.appendChild(div1);
}

var bStopGameIni = false;
window.stopGameIni = () => {
  bStopGameIni = true;
}

function getPlayerBox(divContainer, ltPlayer, i, j, ltClubEmblems) {
  if (bStopGameIni) {
    return;
  }

  if (j - 1 >= ltPlayer[i].length) {
    j = 0;
    i = i + 1;
    if (i < ltPlayer.length) {
      divContainer.innerHTML = '';

      // Clear club emblem
      var k, elements = divContainer.parentNode.getElementsByClassName("club_emblem");
      for (k = elements.length; k--;) {
        elements[k].parentNode.removeChild(elements[k]);
      }
    }
  }
  if (i >= ltPlayer.length) {
    return;
  }

  if (j == 0) {
    var divClubEmblem = document.createElement("div");
    divClubEmblem.innerHTML = ltClubEmblems[i];
    divClubEmblem.className = "club_emblem";
    divClubEmblem.style.position = "absolute";
    divClubEmblem.style.top = "0px";
    divClubEmblem.style.left = "0px";
    divClubEmblem.style.width = "10%";
    divClubEmblem.style.opacity = 0;
    divContainer.parentNode.appendChild(divClubEmblem);

    fadeIn(divClubEmblem, 50);
  } else {
    var pl2 = ltPlayer[i][j - 1];
    var divPlayerBox = getBoxFormationDOM(j - 1, pl2.ptPos, pl2.sName, pl2.iNb.toString(), null, 0, false, -1, 0, divContainer.offsetWidth, 0.5, null, null, pl2.sNat, false, pl2.sPortrait, false);
    divPlayerBox.style.opacity = 0;
    divPlayerBox.style.zIndex = j + 100;
    divContainer.appendChild(divPlayerBox);

    fadeIn(divPlayerBox, 50);
  }

  var interval = 1000;
  if (j == ltPlayer[i].length) {
    interval = 3000;
  }

  j = j + 1;

  setTimeout(getPlayerBox, interval, divContainer, ltPlayer, i, j, ltClubEmblems);
}

window.getBoxFormationDOM = (i, ptPos, sName, sNo, sStrength, iCard, bOpponentTeam, iSelectedPlayer, iPos, parent_width, fScale, sTeamname, sAge, sNat, bCaptain, sPortrait, bMobile, _callback_fct) => {
  if (!sName) {
    console.log("getBoxFormationDOM: sName == null", i);
    return null;
  }
  if (!ptPos) {
    console.log("getBoxFormationDOM: ptPos == null", sName, i);
    return null;
  }

  if (!iPos) {
    iPos = 0;
  }

  if (!fScale) {
    fScale = 1.0;
  }

  const fImageWidthToHeightRatio = 1012 / (2 * 735);
  const fHeightTot = 122 * fScale;
  //const fBoxWidthFrac = 0.26;
  const fBoxWidthFrac = 0.16;
  const fBoxWidthPx = parent_width * fBoxWidthFrac;
  //const fBoxHeightPx = 0.2 * fBoxWidthPx;
  const fBoxHeightPx = 0.3 * fBoxWidthPx;
  const fBoxHeightPer = fBoxHeightPx / (parent_width / fImageWidthToHeightRatio);
  const iTextSize = fBoxHeightPx * 0.5;
  const iTextSizeMin = 6;

  var iTop = 100 - ((100 * ptPos.x) / fHeightTot);
  var iLeft = (100 * (ptPos.y + 25)) / 50;
  if (bOpponentTeam) {
    iTop = 103 - iTop;
    iLeft = 100 - iLeft;
  }

  iTop = iTop - ((fBoxHeightPer * 100) / 2.0);
  iLeft = iLeft - (100 * (fBoxWidthFrac / 2));

  var sNameSplit = sName.split(' ');
  var sSurname = sName;
  if (sNameSplit.length > 1) {
    sSurname = sNameSplit[sNameSplit.length - 1];
    sName = sNameSplit[0][0] + ". " + sSurname;
  }
  if (sSurname.length > 8) { sName = sSurname; }
  if (sName.length > 11) { sName = sName.substring(0, 11); }
  const sPos = ["", "TW", "IV", "LV", "RV", "DM", "LM", "RM", "OM", "LA", "RA", "ST", "LIB", "OLV", "ORV", "ZM", "", "", "", "", "", "HS"];

  var color = "white";
  if (bOpponentTeam) {
    color = "lightgray";
  }
  var color2 = "black";
  if (iCard > 1) {
    color = "rgba(255, 30, 0, .3)";
    color2 = "rgba(0, 0, 0, .5)";
    /*
    } else if (iCard == 1) {
      color = "yellow";
    */
  /*
  } else if (iSelectedPlayer >= 0 && i !== iSelectedPlayer && !bOpponentTeam) {
    color = "rgba(255, 255, 255, .5)";
    color2 = "rgba(0, 0, 0, .5)";
  */
  }

  const bDetails = (iPos > 0 || sStrength || sAge) && (!bMobile || i === iSelectedPlayer);

  var divPlayerBox = document.getElementById("divPlayerBox_" + i.toString());
  if (divPlayerBox) {
    //console.log("player box: " + i.toString() + " old");
    divPlayerBox.setAttribute('data-new', false);
  } else {
    //console.log("player box: " + i.toString() + " new", fBoxWidthPx, parent_width);
    divPlayerBox = document.createElement("div");
    divPlayerBox.id = "divPlayerBox_" + i.toString();
    divPlayerBox.className = "playerBox";
    if (bOpponentTeam) {
      divPlayerBox.className += " playerOpp";
    }
    divPlayerBox.draggable = false;
    divPlayerBox.style.touchAction = "none";
    divPlayerBox.style.position = "absolute";
    divPlayerBox.style.width = fBoxWidthPx.toFixed(0) + "px";
    //divPlayerBox.style.minWidth = "100px";
    if (bDetails) {
      divPlayerBox.style.height = fBoxHeightPx.toFixed(0) + "px";
    } else {
      divPlayerBox.style.height = (fBoxHeightPx * 0.65).toFixed(0) + "px";
    }
    divPlayerBox.style.minHeight = "16px";
    divPlayerBox.style.cursor = "pointer";
    divPlayerBox.style.webkitBoxShadow = "0px 0px 4px 4px rgba(0, 0, 0, .3)";
    divPlayerBox.style.boxShadow = "0px 0px 4px 4px rgba(0, 0, 0, .3)";
    divPlayerBox.style.border = "2px solid black";
    divPlayerBox.style.textAlign = "center";
    divPlayerBox.style.verticalAlign = "middle";
    divPlayerBox.style.userSelect = "none";
    divPlayerBox.setAttribute('data-id', i);
    divPlayerBox.setAttribute('data-new', true);

    /*
    if (_callback_fct) {
      //divPlayerBox.mousedown = _callback_fct;
      //divPlayerBox.addEventListener("mousedown", _callback_fct);
      //divPlayerBox.onmousedown = _callback_fct;
      divPlayerBox.onclick = _callback_fct;
      divPlayerBox.ontouchstart = _callback_fct;
    }
    */

    /*
    if (bOpponentTeam && fctOnClickOpp) {
      divPlayerBox.addEventListener("click", function () { fctOnClickOpp(i); });
      divPlayerBox.addEventListener("ontouchstart", function () { fctOnClickOpp(i); });
    } else if (iSelectedPlayer >= 0) {
      if (fctOnClick) {
        divPlayerBox.addEventListener("click", function () { fctOnClick(i); });
        divPlayerBox.addEventListener("ontouchstart", function () { fctOnClick(i); });
      }
      if (i === iSelectedPlayer) {
        divPlayerBox.style.zIndex = "98";
      }
    }
    */

    if (sPortrait) {
      var sImgName = "imgPortrait";
      if (bOpponentTeam) {
        sImgName += "Opp";
      }

      var divPlayerImg = document.createElement("div");
      divPlayerImg.setAttribute("name", sImgName);
      divPlayerImg.draggable = false;
      divPlayerImg.style.touchAction = "none";
      divPlayerImg.style.position = "absolute";
      divPlayerImg.style.bottom = "100%";
      //divPlayerImg.style.left = "2px";
      divPlayerImg.style.left = "35%";
      //divPlayerImg.style.width = "25%";
      divPlayerImg.style.width = "55%";
      //divPlayerImg.style.border = "2px solid black";

      /*
      var imgPlPortrait = document.createElement("img");
      imgPlPortrait.src = sPortrait;
      imgPlPortrait.style.width = "100%";
      imgPlPortrait.style.objectFit = "contain";
      divPlayerImg.appendChild(imgPlPortrait);
      */
      divPlayerImg.innerHTML = sPortrait;
      if (iCard > 1 || (iSelectedPlayer >= 0 && i !== iSelectedPlayer)) { divPlayerImg.className = "transparent"; }
      divPlayerBox.appendChild(divPlayerImg);
      var divPlayerImgCover = document.createElement("div");
      divPlayerImgCover.draggable = false;
      divPlayerImgCover.style.touchAction = "none";
      divPlayerImgCover.style.position = "absolute";
      divPlayerImgCover.style.width = "100%";
      divPlayerImgCover.style.height = "100%";
      divPlayerImgCover.style.top = "0";
      divPlayerImgCover.style.left = "0";
      //divPlayerImgCover.style.border = "1px solid red";
      divPlayerImg.appendChild(divPlayerImgCover);

      if (iCard > 0) {
        var imgPlayerCard = document.createElement("img");
        imgPlayerCard.style.position = "absolute";
        imgPlayerCard.style.left = "80%";
        imgPlayerCard.style.bottom = "100%";
        imgPlayerCard.width = Math.max(Math.round(fBoxWidthPx * 0.12), 12);
        if      (iCard == 1) { imgPlayerCard.src = sContentDir + "/Icons/yCard.png"; }
        else if (iCard == 2) { imgPlayerCard.src = sContentDir + "/Icons/yrCard.png"; }
        else if (iCard == 3) { imgPlayerCard.src = sContentDir + "/Icons/rCard.png"; }
        divPlayerBox.appendChild(imgPlayerCard);
      }
    }

    // Number
    var divPlayerNb = document.createElement("div");
    divPlayerNb.style.position = "absolute";
    divPlayerNb.style.width = "30%";
    divPlayerNb.style.height = fBoxHeightPx.toFixed(0) + "px";
    //divPlayerNb.style.top = "0px";
    divPlayerNb.style.bottom = "100%";
    divPlayerNb.style.left = "5%";
    divPlayerNb.style.display = "flex";
    divPlayerNb.style.justifyContent = "center";
    divPlayerNb.style.alignItems = "center";
    divPlayerNb.style.backgroundColor = color2;
    divPlayerNb.style.color = "white";
    divPlayerNb.style.userSelect = "none";
    divPlayerNb.style.fontSize = Math.max(iTextSize * 1.6, iTextSizeMin).toFixed(0) + "px";
    divPlayerNb.innerText = sNo;
    divPlayerBox.appendChild(divPlayerNb);

    if (bDetails) {
      const fTextSizeDetails = Math.round(iTextSize * 0.6);
      var sBottomGap = "1px";
      if (fTextSizeDetails - 0.001 < iTextSizeMin) {
        sBottomGap = "0px";
      }

      const fIconWidth = Math.max(fBoxWidthPx * 0.15, 6);
      if (sNat) {
        var divPlayerNat = document.createElement("div");
        divPlayerNat.innerHTML = window.getNatIcon(sNat, "position: absolute; width: " + fIconWidth.toFixed(0) + "px; top: 1px; right: 1px");
        divPlayerBox.appendChild(divPlayerNat);
      }
      if (bCaptain) {
        var divPlayerCap = document.createElement("div");
        divPlayerCap.innerHTML = '<img src="' + sContentDir + '/Icons/captain.png" title="Kapitän" style="position: absolute; width: ' + fIconWidth.toFixed(0) + 'px; top: 2px; left: 2px"/>';
        divPlayerBox.appendChild(divPlayerCap);
      }
      var divPlayerPos = document.createElement("div");
      divPlayerPos.className = "txtPosName";
      divPlayerPos.style.position = "absolute";
      divPlayerPos.style.width = "25%";
      divPlayerPos.style.height = "35%";
      divPlayerPos.style.minHeight = "8px";
      divPlayerPos.style.bottom = sBottomGap;
      divPlayerPos.style.left = "0px";
      divPlayerPos.style.display = "flex";
      divPlayerPos.style.justifyContent = "center";
      divPlayerPos.style.alignItems = "center";
      divPlayerPos.style.color = "black";
      //divPlayerPos.style.backgroundColor = color;
      divPlayerPos.style.wordBreak = "break-word";
      divPlayerPos.style.userSelect = "none";
      divPlayerPos.style.fontSize = Math.max(fTextSizeDetails, iTextSizeMin).toFixed(0) + "px";
      divPlayerPos.innerText = sPos[iPos];
      divPlayerBox.appendChild(divPlayerPos);
      var divPlayerStrength = document.createElement("div");
      divPlayerStrength.style.position = "absolute";
      divPlayerStrength.style.width = "25%";
      divPlayerStrength.style.height = "35%";
      divPlayerStrength.style.minHeight = "8px";
      divPlayerStrength.style.bottom = sBottomGap;
      divPlayerStrength.style.left = "25%";
      divPlayerStrength.style.display = "flex";
      divPlayerStrength.style.justifyContent = "center";
      divPlayerStrength.style.alignItems = "center";
      divPlayerStrength.style.color = "black";
      //divPlayerStrength.style.backgroundColor = color;
      divPlayerStrength.style.wordBreak = "break-word";
      divPlayerStrength.style.fontSize = Math.max(fTextSizeDetails, iTextSizeMin).toFixed(0) + "px";
      divPlayerStrength.innerText = sStrength;
      divPlayerBox.appendChild(divPlayerStrength);
      var divPlayerAge = document.createElement("div");
      divPlayerAge.style.position = "absolute";
      divPlayerAge.style.width = "50%";
      divPlayerAge.style.height = "35%";
      divPlayerAge.style.minHeight = "8px";
      divPlayerAge.style.bottom = sBottomGap;
      divPlayerAge.style.left = "50%";
      divPlayerAge.style.display = "flex";
      divPlayerAge.style.justifyContent = "center";
      divPlayerAge.style.alignItems = "center";
      divPlayerAge.style.color = "black";
      //divPlayerAge.style.backgroundColor = color;
      divPlayerAge.style.wordBreak = "break-word";
      divPlayerAge.style.userSelect = "none";
      divPlayerAge.style.fontSize = Math.max(fTextSizeDetails, iTextSizeMin).toFixed(0) + "px";
      if (sAge) {
        divPlayerAge.innerText = sAge;
      } else {
        divPlayerAge.innerText = ptPos.y.toString() + '/' + ptPos.x.toString();
        divPlayerAge.className = "txtPosition";
      }
      divPlayerBox.appendChild(divPlayerAge);
    }

    // Name
    var divPlayerName = document.createElement("div");
    divPlayerName.style.position = "absolute";
    divPlayerName.style.width = "100%";
    if (bDetails) {
      divPlayerName.style.height = "65%";
    } else {
      divPlayerName.style.height = "100%";
    }
    divPlayerName.style.top = "0px";
    divPlayerName.style.left = "0px";
    divPlayerName.style.display = "flex";
    divPlayerName.style.justifyContent = "center";
    divPlayerName.style.alignItems = "center";
    divPlayerName.style.color = "black";
    divPlayerName.style.overflow = "clip";
    divPlayerName.style.whiteSpace = "nowrap";
    divPlayerName.style.wordBreak = "break-word";
    divPlayerName.style.userSelect = "none";
    divPlayerName.style.fontSize = Math.max(iTextSize, iTextSizeMin).toFixed(0) + "px";
    divPlayerName.innerText = sName;
    divPlayerBox.appendChild(divPlayerName);

    if (sTeamname) {
      var divPlayerTeam = document.createElement("div");
      divPlayerTeam.style.position = "absolute";
      divPlayerTeam.style.width = "100%";
      divPlayerTeam.style.height = "35%";
      divPlayerTeam.style.minHeight = "8px";
      divPlayerTeam.style.bottom = "-35%";
      divPlayerTeam.style.left = "0px";
      divPlayerTeam.style.display = "flex";
      divPlayerTeam.style.justifyContent = "center";
      divPlayerTeam.style.alignItems = "center";
      divPlayerTeam.style.color = "white";
      divPlayerTeam.style.whiteSpace = "nowrap"
      divPlayerTeam.style.wordBreak = "break-word";
      divPlayerTeam.style.userSelect = "none";
      divPlayerTeam.style.fontSize = Math.max(iTextSize * 0.6, iTextSizeMin).toFixed(0) + "px";
      divPlayerTeam.innerText = sTeamname;
      divPlayerBox.appendChild(divPlayerTeam);
    }
  }

  divPlayerBox.style.top = iTop.toString() + "%";
  divPlayerBox.style.left = iLeft.toString() + "%";
  divPlayerBox.style.backgroundColor = color;
  if (iCard > 1) {
    divPlayerBox.style.zIndex = "0";
  }
  else {
    if (bOpponentTeam) {
      divPlayerBox.style.zIndex = "6";
    } else {
      divPlayerBox.style.zIndex = "7";
    }
  }
  if (i === iSelectedPlayer) {
    divPlayerBox.style.zIndex = "8";
  }

  return divPlayerBox;
}
