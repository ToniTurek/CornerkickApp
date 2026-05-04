//import "/_content/CornerkickApp.Components/js/Player.js";
var CornerkickApp = CornerkickApp || {};

var iFormation = 0;
var iSelectedPlayer = 0;
var bAllowMove = false;
var bPreventOnClick = false;

window.init = (_teamData, _tt) => {
  const tt = JSON.parse(_tt);
  const iMinFieldSystemContainerHeight = 500;

  // declarations
  var ddlSystem = document.getElementById("ddlSystem");
  var ddlFormation = document.getElementById("ddlFormation");

  let iSystem = 0;
  if (ddlSystem) {
    iSystem = ddlSystem.value;
  }

  let bnSaveFormation = document.getElementById("bnSaveFormation");
  if (bnSaveFormation) { bnSaveFormation.disabled = true; }
  let bnDeleteFormation = document.getElementById("bnDeleteFormation");

  const divBody = document.getElementById("divBody");
  if (window.innerWidth < 800) {
    divBody.style.left = "0px";
    divBody.style.width = "100%";
  }
  const divFieldContainer = document.getElementById("divFieldContainer");
  if (divFieldContainer === null) return;
  const divFieldSystemContainer = document.getElementById("divFieldSystemContainer");
  if (divFieldSystemContainer === null) return;
  divFieldSystemContainer.style.top = "0px";

  const iGapHeight = 120;
  var fFieldContHeight = Math.max(window.innerHeight - divBody.offsetTop - iGapHeight, iMinFieldSystemContainerHeight);
  var fFieldContWidth = ((1012 / (2 * 735)) * (fFieldContHeight - divFieldContainer.offsetTop)) + 2;
  if (divBody.offsetWidth < fFieldContWidth) {
    fFieldContWidth = divBody.offsetWidth;
    fFieldContHeight = ((fFieldContWidth - 2) * ((2 * 735) / 1012)) + divFieldContainer.offsetTop;
  } else if (fFieldContHeight < 800) {
    fFieldContHeight = Math.min(fFieldContHeight * 1.25, 800);
    fFieldContWidth = ((1012 / (2 * 735)) * (fFieldContHeight - divFieldContainer.offsetTop)) + 2;
  }
  divFieldSystemContainer.style.height = fFieldContHeight.toFixed(0) + "px";
  divFieldSystemContainer.style.width = fFieldContWidth.toFixed(0) + "px";

  divFieldContainer.style.height = (fFieldContHeight - divFieldContainer.offsetTop).toFixed(0) + "px";
  const divTable = document.getElementById("tablediv");
  const iDivTableLeft = divFieldSystemContainer.offsetLeft + fFieldContWidth + 40;
  if (divBody.offsetWidth - iDivTableLeft > 300) {
    divTable.style.left = iDivTableLeft.toString() + "px";
    divTable.style.width = "auto";
  } else {
    divTable.style.left = "0px";
    divTable.style.top = (divFieldSystemContainer.offsetTop + fFieldContHeight + 40).toString() + "px";
    divTable.style.width = "100%";
  }

  // Set main body height
  const iBdyHeight = Math.max(divFieldSystemContainer.offsetTop + divFieldSystemContainer.offsetHeight, divTable.offsetTop + divTable.offsetHeight);
  document.getElementsByTagName("main")[0].style.height = iBdyHeight.toString() + "px";
} // initialize


let divDrawFormationWidth = 0;
window.drawAufstellung = (_teamData, iSelectedPlayer, iTactic, bForceNew, bMobile, DotNetRef) => {
  //console.log("drawAufstellung", iSelectedPlayer);
  const teamData = JSON.parse(_teamData);

  var divDrawFormation = document.getElementById("divDrawFormation");
  let tblTeamStrengthPos = document.getElementById("tblTeamStrengthPos");

  /*
  var fWindowWidth = $(window).width();
  var bMobile = fWindowWidth < 960;
  */

  if (!divDrawFormation) { return; }

  if (teamData) {  // check if data is defined
    var i = 0;

    // Store div width for partial reload
    if (divDrawFormation.offsetWidth > 0) {
      divDrawFormationWidth = divDrawFormation.offsetWidth;
    }

    if (bForceNew) {
      divDrawFormation.innerHTML = '';
    }

    /*
    ['click', 'mouseup'].forEach(
      function (e) {
        window.addEventListener(
          e,
          function (ee) {
            clearSelectedPlayer(ee, divDrawFormation, DotNetRef);
          },
          false
        );
      }
    );
    */
    divDrawFormation.onmouseup = function (e) { clearSelectedPlayer(e, divDrawFormation, DotNetRef); };
    //divDrawFormation.onclick = function (e) { clearSelectedPlayer(e, divDrawFormation, DotNetRef); };

    var ltDivBoxPl = Array(teamData.ltPlayer2.length); // List for player
    var ltLineManMarking = []; // List for man-marking lines
    var plSelected = null;

    for (var iPl = 0; iPl < teamData.ltPlayer2.length; iPl++) {
      const player = teamData.ltPlayer2[iPl];

      if (player.iId === iSelectedPlayer) {
        plSelected = player;
      }

      /*
      if (teamData.ltPlayerOpp2 !== null) {
        if (player.iIxManMarking >= 0 && player.iIxManMarking < teamData.ltPlayerOpp2.length) {
          var iPosMM = convertPosToPix(teamData.ltPlayer2[iPl].ptPos.y, 122 - teamDatateamData.ltPlayer2[iPl].ptPos.x, -teamData.ltPlayer2Opp[player.iIxManMarking].ptPos.y, teamData.ltPlayer2Opp[player.iIxManMarking].ptPos.x, document.getElementById("drawFormation"), false);
          ltLineManMarking.push(drawLine(iPosMM[0], iPosMM[1], iPosMM[2], iPosMM[3], "orange", "", 2, 1));
        }
      }
      */

      var sNo = player.iNb.toString();
      if (teamData.bNation) {
        sNo = (iPl + 1).toString();
      }

      const jPl = iPl;
      const divBoxPl = window.getBoxFormationDOM(
        player.iId, player.ptPos, player.sName, sNo, player.sSkillAve, player.iCard, false, iSelectedPlayer, player.iPos, divDrawFormationWidth, 1.0, null, null, player.sNat, jPl === teamData.iCaptainIx, player.sPortrait, bMobile,
        function () {
          //window.selectPlayer(player.iId, player.ptPos, DotNetRef);
        }
      );
      if (divBoxPl) {
        ltDivBoxPl[i] = divBoxPl;

        var bNew = divBoxPl.getAttribute('data-new');
        if (bNew === 'true') {
          dragElement(divBoxPl, divDrawFormation, player.iId, DotNetRef);
        }
      }

      i = i + 1;
      if (i > 11) { break; }
    }

    if (teamData.bOppTeam) {
      if (teamData.ltPlayerOpp2 && teamData.iKibitzer > 0) {
        // opponent player
        var j = 0;
        //$.each(teamData.ltPlayerOpp2, function (iPl, playerOpp) {
        for (var iPl = 0; iPl < teamData.ltPlayerOpp2.length; iPl++) {
          const playerOpp = teamData.ltPlayerOpp2[iPl];
          var sPlayerOppName = "";
          var sPlayerOppAveSkill = "";
          var sPlayerOppPos = "";
          if (teamData.iKibitzer > 1) {
            sPlayerOppName = playerOpp.sName;

            if (teamData.iKibitzer > 2) {
              sPlayerOppAveSkill = playerOpp.sSkillAve;
              sPlayerOppPos = playerOpp.iPos;
            }
          } else {
            sPlayerOppName = "?";
          }

          var sOppNo = playerOpp.iNb.toString();
          var sNatOpp = null;
          if (teamData.bNation) {
            sOppNo = (iPl + 1).toString();
            sNatOpp = playerOpp.sNat;
          }

          const jPl = iPl;
          const divBoxPl = window.getBoxFormationDOM(
            playerOpp.iId, playerOpp.ptPos, sPlayerOppName, sOppNo, sPlayerOppAveSkill, playerOpp.iCard, true, iSelectedPlayer, sPlayerOppPos, divDrawFormationWidth, 1.0, null, null, sNatOpp, false, playerOpp.sPortrait, bMobile,
            function () {
              //window.setManMarking(jPl, DotNetRef);
            }
          );
          if (divBoxPl) {
            ltDivBoxPl[i] = divBoxPl;
            //divDrawFormation.appendChild(divBoxPl);

            divBoxPl.onmouseup = function (e) { setManMarking(jPl, DotNetRef); };
            divBoxPl.ontouchend = function (e) { setManMarking(jPl, DotNetRef); };
          }

          i = i + 1;
          j = j + 1;
          if (j > 11) { break; }
        }

        if (teamData.iKibitzer > 2) {
          var divTeamOppAve = document.getElementById("divTeamOppAve");
          const sTeamOppAve = '<p style="position: absolute; margin: 4px; background-color: rgb(31, 158, 69); color: white; font-size: 100%">' + teamData.sTeamOppAveSkill + ' (' + teamData.sTeamOppAveAge + ')</p>';
          divTeamOppAve.innerHTML = sTeamOppAve;
        }
      }
    } else {
      var divNoOppInfo = document.createElement("div");
      divNoOppInfo.innerHTML = '<div style="position: absolute; width: 90%; height: 50%; left: 5%; text-align: center"><p style="position: absolute; top: 40%; width: 100%; color: white; font-size: 150%">Kein Gegner</p></div>';
      divDrawFormation.appendChild(divNoOppInfo);
    }

    divDrawFormation.innerHTML = '';
    for (var iPl = 0; iPl < ltDivBoxPl.length; iPl++) {
      if (ltDivBoxPl[iPl] !== null && ltDivBoxPl[iPl] !== undefined) divDrawFormation.appendChild(ltDivBoxPl[iPl]);
    }

    /*
    if (iSelectedPlayer >= 0 && plSelected !== null && plSelected !== undefined) {
      window.selectPlayer(ltDivBoxPl[iSelectedPlayer], iSelectedPlayer, plSelected.ptPos, DotNetRef)
    }
    */

    // hide orientation slider on start
    //divTacticOrientation.style.display = 'none';
    //divTacticIndOrientation.style.display = 'none';

    // Team averages
    tblTeamStrengthPos.innerHTML = '';
    let rowTeamStrengthPos0 = tblTeamStrengthPos.insertRow(-1);
    rowTeamStrengthPos0.insertCell(0).innerText = " A: " + (teamData.fTeamStrengthPos[0] > 0 ? teamData.fTeamStrengthPos[0].toFixed(1) : '?') + " ";
    let rowTeamStrengthPos1 = tblTeamStrengthPos.insertRow(-1);
    rowTeamStrengthPos1.insertCell(0).innerText = " M: " + (teamData.fTeamStrengthPos[1] > 0 ? teamData.fTeamStrengthPos[1].toFixed(1) : '?') + " ";
    let rowTeamStrengthPos2 = tblTeamStrengthPos.insertRow(-1);
    rowTeamStrengthPos2.insertCell(0).innerText = " S: " + (teamData.fTeamStrengthPos[2] > 0 ? teamData.fTeamStrengthPos[2].toFixed(1) : '?') + " ";

    // NEW approach with javascript DOM elements
    // Draw man-marking lines
    if (ltLineManMarking) {
      for (var iMM = 0; iMM < ltLineManMarking.length; ++iMM) {
        divDrawFormation.appendChild(ltLineManMarking[iMM]);
      }
    }

    // Draw team emblems
    if (teamData.sEmblem) {
      var divEmblem = document.createElement("div");
      divEmblem.style.position = "absolute";
      divEmblem.style.left = "0px";
      divEmblem.style.bottom = "0px";
      divEmblem.style.width = "10%";
      divEmblem.innerHTML = teamData.sEmblem;
      divDrawFormation.appendChild(divEmblem);
    }

    if (teamData.sEmblemOpp) {
      var divEmblemOpp = document.createElement("div");
      divEmblemOpp.style.position = "absolute";
      divEmblemOpp.style.left = "0px";
      divEmblemOpp.style.top = "0px";
      divEmblemOpp.style.width = "10%";
      divEmblemOpp.innerHTML = teamData.sEmblemOpp;
      divDrawFormation.appendChild(divEmblemOpp);
    }
  } else {
    //getDialog(document.body, "data hasn't worked!", "Fehler");
    console.log("ERROR: data emptry!");
  }
}

window.movePlayer = (e, elmnt, iSelPlIx, iMoveDirection, DotNetRef) => {
  //console.log("movePlayer", DotNetRef);
  if (!e) var e = window.event;
  e.cancelBubble = true;
  if (e.stopPropagation) e.stopPropagation();

  iFormation = 0;
  document.getElementById('ddlFormation').value = iFormation;

  let bnSaveFormation = document.getElementById("bnSaveFormation");
  if (bnSaveFormation) {
    bnSaveFormation.disabled = false;
  }

  if (DotNetRef) {
    DotNetRef.invokeMethodAsync("movePlayerJs", iSelPlIx, iMoveDirection).then(iXY => {
      setPosName(DotNetRef, elmnt, iSelPlIx, iXY);
    });
    //DotNetRef.invokeMethodAsync("movePlayerJs", iSelPlIx, iMoveDirection);
  }

  //window.drawAufstellung(_teamData, iFormation, iSelectedPlayer, document.getElementById("ddlSystem").value, true, DotNetRef);
  //console.log(DotNetRef);
  //DotNet.invokeMethodAsync('CornerkickApp.Components', 'movePlayerJs', iSelectedPlayer - 1, iMoveDirection);
  //@movePlayer(iMoveDirection);
  //movePlayerAjax(iMoveDirection);
}

window.movePlayerDrag = (elmnt, iSelPlId, iPixX, iPixY, DotNetRef) => {
  var iXY = getXYfromElement(elmnt, iPixX, iPixY);
  //console.log("movePlayerDrag", iSelPlId, iPixX, iPixY, iXY);
  if (!iXY) {
    return;
  }

  //var iPlId = elmnt.getAttribute('data-id');

  if (DotNetRef) {
    DotNetRef.invokeMethodAsync("movePlayerToJs", iSelPlId, iXY[0], iXY[1]);
  }
}

window.selectPlayer = (elmnt, iSelPlayer, ptPos, DotNetRef) => {
  //console.log("selectPlayer", iSelPlayer, bPreventOnClick, ptPos);
  if (bPreventOnClick) return;

  if (!e) var e = window.event;
  e.cancelBubble = true;
  if (e.stopPropagation) e.stopPropagation();

  /*
  let ddlSystem = document.getElementById("ddlSystem")
  let iSystem = 0;
  if (ddlSystem) {
    iSystem = ddlSystem.value;
  }
  */

  if (DotNetRef) {
    DotNetRef.invokeMethodAsync("selectPlayerJs", iSelPlayer);
  }

  iSelectedPlayer = iSelPlayer;

  SetViewOfPlayers(iSelPlayer);

  let divParent = document.getElementById("divDrawFormation");
  if (divParent) {
    if (iSelPlayer < 0) {
      // Remove move arrows
      var arrows = divParent.getElementsByClassName("arrow_move_player");
      while (arrows[0]) {
        arrows[0].parentNode.removeChild(arrows[0]);
      }
    } else if (elmnt) {
      // Add move arrows
      getBoxMovePlayerDom(elmnt, ptPos, divParent, iSelPlayer, DotNetRef);
    }
  }
}

function setManMarking(iSelPlayerOppIx, DotNetRef) {
  //console.log("setManMarking", iSelPlayerOppIx, bPreventOnClick);
  if (iSelPlayerOppIx < 0) {
    return;
  }

  if (!e) var e = window.event;
  e.cancelBubble = true;
  if (e.stopPropagation) e.stopPropagation();

  if (DotNetRef) {
    DotNetRef.invokeMethodAsync("setManMarkingJs", iSelPlayerOppIx);
    clearSelectedPlayer(e, null, DotNetRef);
  }
}

function getXYfromElement(elmnt, iPixX, iPixY) {
  if (!elmnt) {
    return null;
  }

  var elmntPar = elmnt.parentElement;
  if (!elmntPar) {
    return null;
  }

  var iXY = [];

  var iBoxH = elmnt.offsetHeight;
  var iBoxW = elmnt.offsetWidth;

  var fX = ((elmntPar.offsetHeight - iPixX) - (iBoxH / 2)) / (elmntPar.offsetHeight / 2);
  var fY = (iPixY + (iBoxW / 2) - (elmntPar.offsetWidth / 2)) / elmntPar.offsetWidth;
  iXY.push(Math.round(fX * 61));
  iXY.push(Math.round(fY * 50));

  return iXY;
}

window.getBoxMovePlayerDom = (elmnt, ptPos, parent, iSelPlIx, DotNetRef) => {
  if (!ptPos) { return; }

  // Total box width: 18%
  const iBoxWidth = 10; // [%]
  const iBoxGapH = 4; // Horizontal gap between box and arrows [%]
  var iTop = (100 - ((100 * ptPos.x) / 122)) - 1.75;
  var iLeft = (100 * (ptPos.y + 25)) / 50;
  var iRight = (100 * (50 - (ptPos.y + 25))) / 50;

  var arrows = parent.getElementsByClassName("arrow_move_player");
  /*
  while (arrows[0]) {
    arrows[0].parentNode.removeChild(arrows[0]);
  }​
  */

  var ltDivArrow = [null, null, null, null];
  if (arrows != null && arrows.length > 0) {
    for (let i = 0; i < ltDivArrow.length; i++) {
      if (arrows.length > i) ltDivArrow[i] = arrows[i];
    }
  }

  const ltImgArrow = ["up", "right", "down", "left"];

  for (let i = 0; i < ltDivArrow.length; i++) {
    if (!ltDivArrow[i]) {
      ltDivArrow[i] = document.createElement("div");
      ltDivArrow[i].style.position = "absolute";
      ltDivArrow[i].className = "arrow_move_player";
      ltDivArrow[i].style.zIndex = 8;

      if (i === 0 || i === 2) {
        ltDivArrow[i].style.width = "10%";
        ltDivArrow[i].style.minWidth = "60px";
      } else if (i === 1 || i === 3) {
        ltDivArrow[i].style.width = "4%";
        ltDivArrow[i].style.minWidth = "24px";
      } 

      var imgArrow = document.createElement("img");
      imgArrow.id = "img_arrow_" + ltImgArrow[i];
      imgArrow.style.position = "relative";
      imgArrow.style.width = "100%";
      imgArrow.style.cursor = "pointer";
      imgArrow.src = sContentDir + "/Images/arrow_" + ltImgArrow[i] + ".png";
      //console.log("imgArrow.onclick", i);
      imgArrow.onmouseup = function (e) { movePlayer(e, elmnt, iSelPlIx, i + 1, DotNetRef); };
      //imgArrow.addEventListener("click", function (e) { movePlayer(e, elmnt, iSelPlIx, i + 1, DotNetRef); });
      //imgArrow.addEventListener("click", function () { movePlayer(elmnt, iSelPlIx, i + 1, DotNetRef); }, true);
      //imgArrow.addEventListener("click", function (e) { movePlayer(e, elmnt, iSelPlIx, i + 1, DotNetRef); }, false);
      //imgArrow.addEventListener("touchstart", function (e) { movePlayer(e, elmnt, iSelPlIx, i + 1, DotNetRef); }, true);

      ltDivArrow[i].appendChild(imgArrow);
      parent.appendChild(ltDivArrow[i]);
    }
  }

  // Container for up arrow
  ltDivArrow[0].style.top = (iTop - 8) + '%';
  ltDivArrow[0].style.left = (iLeft + 0 - 5) + '%';

  // Container for right arrow
  ltDivArrow[1].style.top = (iTop - 1.5) + '%';
  ltDivArrow[1].style.left = (iLeft + (iBoxWidth / 2) + iBoxGapH) + '%';

  // Container for down arrow
  ltDivArrow[2].style.top = (iTop + 4) + '%';
  ltDivArrow[2].style.left = (iLeft + 0 - 5) + '%';

  // Container for left arrow
  ltDivArrow[3].style.top = (iTop - 1.5) + '%';
  ltDivArrow[3].style.right = (iRight + (iBoxWidth / 2) + iBoxGapH) + '%';
}

var bDragging = false;
function dragElement(elmnt, parent, jPlId, DotNetRef) {
  var pos1 = -100, pos2 = -100, pos3 = 0, pos4 = 0;

  var iPixX = 0;
  var iPixY = 0;

  elmnt.onmousedown = function (e) { dragMouseDown(e, false); };
  elmnt.ontouchstart = touchStart;
  elmnt.ontouchend = touchEnd;

  //console.log("dragElement");

  function dragMouseDown(e, bTouch) {
    //console.log("dragMouseDown", bTouch);

    e = e || window.event;
    e.preventDefault();
    if (e.stopPropagation) e.stopPropagation();

    SetViewOfPlayers(jPlId);

    // get the mouse cursor position at startup:
    if (bTouch) {
      elmnt.style.cursor = "grabbing";

      // Increase size of player box
      elmnt.style.width  = (parseInt(elmnt.style.width .replace("px", "")) * 1.25).toFixed() + "px";
      elmnt.style.height = (parseInt(elmnt.style.height.replace("px", "")) * 1.25).toFixed() + "px";
      //elmnt.style.fontSize = (parseInt(elmnt.style.fontSize.replace("px", "")) * 1.5).toFixed() + "px";

      pos3 = e.touches[0].clientX;
      pos4 = e.touches[0].clientY;
    } else {
      pos3 = e.clientX;
      pos4 = e.clientY;
    }

    elmnt.onmouseup  = function (e) { closeDragElement(e, false); };
    elmnt.ontouchend = function (e) { closeDragElement(e, true ); };

    // call a function whenever the cursor moves:
    parent.onmousemove = function (e) { elementDrag(e, false); };
    parent.ontouchmove = function (e) { elementDrag(e, true ); };

    //var iXY = getXYfromElement(elmnt, iPixX, iPixY);
    //window.selectPlayer(jPlId, iXY, DotNetRef);
  }

  function elementDrag(e, bTouch) {
    //console.log("elementDrag");
    bDragging = true;
    bPreventOnClick = true;

    e = e || window.event;
    e.preventDefault();
    if (e.stopPropagation) e.stopPropagation();

    // Clean move arrows
    var arrows_move_player = parent.getElementsByClassName("arrow_move_player");
    for (var i = arrows_move_player.length - 1; i >= 0; i--) {
      if (arrows_move_player[i] && arrows_move_player[i].parentElement) {
        arrows_move_player[i].parentElement.removeChild(arrows_move_player[i]);
      }
    }

    // calculate the new cursor position:
    if (bTouch) {
      pos1 = pos3 - e.touches[0].clientX;
      pos2 = pos4 - e.touches[0].clientY;
      pos3 = e.touches[0].clientX;
      pos4 = e.touches[0].clientY;
    } else {
      pos1 = pos3 - e.clientX;
      pos2 = pos4 - e.clientY;
      pos3 = e.clientX;
      pos4 = e.clientY;
    }

    iPixX = elmnt.offsetTop - pos2;
    iPixY = elmnt.offsetLeft - pos1;

    // set the element's new position:
    elmnt.style.top = iPixX + "px";
    elmnt.style.left = iPixY + "px";

    var iXY = getXYfromElement(elmnt, iPixX, iPixY);
    if (!iXY) {
      return;
    }

    setPosName(DotNetRef, elmnt, jPlId, iXY);
  }

  function closeDragElement(e, bTouch) {
    //console.log("closeDragElement", bDragging);

    e = e || window.event;
    e.preventDefault();
    if (e && e.stopPropagation) e.stopPropagation();

    /* stop moving when mouse button is released:*/
    parent.onmousemove = null;
    parent.ontouchmove = null;
    elmnt.ontouchend = touchEnd;

    if (!bDragging) {
      iPixX = elmnt.offsetTop;
      iPixY = elmnt.offsetLeft;
      var iXY = getXYfromElement(elmnt, iPixX, iPixY);
      window.selectPlayer(elmnt, jPlId, { x: iXY[0], y: iXY[1] }, DotNetRef);
      return;
    }
    bDragging = false;

    if (bTouch) {
      // Reset size of player box
      elmnt.style.width  = (parseInt(elmnt.style.width .replace("px", "")) / 1.25).toFixed() + "px";
      elmnt.style.height = (parseInt(elmnt.style.height.replace("px", "")) / 1.25).toFixed() + "px";
    }

    if (pos1 < -99) {
      return;
    }
    if (pos2 < -99) {
      return;
    }

    bPreventOnClick = false;
    window.movePlayerDrag(elmnt, jPlId, iPixX, iPixY, DotNetRef);

    clearSelectedPlayer(e, elmnt, DotNetRef);

    // Set formation index to new formation
    iFormation = 0;
    document.getElementById("ddlFormation").value = iFormation;

    var bnSaveFormation = document.getElementById("bnSaveFormation");
    if (bnSaveFormation) bnSaveFormation.disabled = false;
  }

  var timerTouch = 0;
  function touchStart(e) {
    //console.log("touchStart");
    timerTouch = setTimeout(function () {
      //alert("Touch start");
      dragMouseDown(e, true);
    }, 1000);
  }

  function touchEnd(e) {
    //alert("Touch end");
    clearTimeout(timerTouch);
  }
}

function setPosName(DotNetRef, elmnt, jPlId, iXY) {
  var txtPosition = elmnt.getElementsByClassName("txtPosition");
  if (txtPosition && txtPosition.length > 0) {
    txtPosition[0].innerHTML = iXY[1].toString() + "/" + iXY[0].toString();
    //console.log("setPosName", txtPosition, iXY);
  }

  if (DotNetRef) {
    DotNetRef.invokeMethodAsync("setXYInfoJs", iXY[0], iXY[1]);

    var txtPosName = elmnt.getElementsByClassName("txtPosName");
    if (txtPosName && txtPosName.length > 0) {
      DotNetRef.invokeMethodAsync("getPosNameJs", jPlId, iXY[0], iXY[1]).then(sPosName => {
        txtPosName[0].innerHTML = sPosName;
      });
    }
  }
}

function SetViewOfPlayers(iSelPlayer) {
  // Add transparent class to player portraits
  var imgPortraits = document.getElementsByName("imgPortrait");
  for (var iImg = 0; iImg < imgPortraits.length; iImg++) {
    var imgP = imgPortraits[iImg];
    const iPlId = parseInt(imgP.parentNode.getAttribute('data-id'));
    if (iSelPlayer < 0 || iPlId === iSelPlayer) {
      imgP.className = "";
    } else {
      imgP.className = "transparent";
    }
  }
  /*
  var imgPortraitsOpp = document.getElementsByName("imgPortraitOpp");
  for (var iImg = 0; iImg < imgPortraitsOpp.length; iImg++) {
    var imgP = imgPortraitsOpp[iImg];
    if (iSelPlayer < 0) {
      imgP.className = "";
    } else {
      imgP.className = "transparent";
    }
  }
  */

  // Add transparent class to player portraits
  //var divBoxes = document.getElementsByClassName("playerBox");
  var divBoxes = document.querySelectorAll('.playerBox:not(.playerOpp)');
  for (var iP = 0; iP < divBoxes.length; iP++) {
    var divP = divBoxes[iP];
    const iPlId = parseInt(divP.getAttribute('data-id'));
    if (iSelPlayer < 0 || iPlId === iSelPlayer) {
      divP.style.opacity = 1;
      divP.style.zIndex = 7;
    } else {
      divP.style.opacity = 0.5;
      divP.style.zIndex = 0;
    }
  }
}

function clearSelectedPlayer(e, elemt, DotNetRef) {
  if (!e) var e = window.event;
  e.cancelBubble = true;
  if (e.stopPropagation) e.stopPropagation();

  /*
  // Remove transparent class from player portraits
  var imgPortraits = document.getElementsByName("imgPortrait");
  for (var iImg = 0; iImg < imgPortraits.length; iImg++) {
    imgPortraits[iImg].className = "";
  }
  var imgPortraitsOpp = document.getElementsByName("imgPortraitOpp");
  for (var iImg = 0; iImg < imgPortraitsOpp.length; iImg++) {
    imgPortraitsOpp[iImg].className = "";
  }
  // Remove transparent class from player boxes
  var divBoxes = document.getElementsByClassName("playerBox");
  //var divBoxes = document.querySelectorAll('.playerBox:not(.playerOpp)');
  for (var iP = 0; iP < divBoxes.length; iP++) {
    divBoxes[iP].style.opacity = 1;
  }
  */

  SetViewOfPlayers(-1);

  // Remove move arrows
  if (elemt) {
    var arrows = elemt.getElementsByClassName("arrow_move_player");
    while (arrows[0]) {
      arrows[0].parentNode.removeChild(arrows[0]);
    }
  }

  if (DotNetRef) {
    DotNetRef.invokeMethodAsync("clearSelectedPlayerJs");
  }
}
