import { Render3D } from "/js/render/render.js"

var gLocArray = []; // Array of gameLoc struct
var bFinished = true;
var bStopPlay = false;
var bAdminStop = false;
var playerGlobal = [];
var imgBall;
var divBallTarget;
var iGameSpeedUsed;
var render3D = null;
var bAnimation = false;
var iColorH = 0;
var iColorA = 0;
var sColorJerseyH;
var sColorJerseyA;
var ltStadiumBlockSizes = [];
var divSpeach = null;

// iState
//   -4: admin
//   -3: initial call
//   -2: game finished
//   -1: running game
//   >= 0: specific state
// bExecuteGame
//   false(default): game data from game running in background are fetched
//   true:           game is executed at each step
var bDrawGame_executed = false;
var iDebugCounter = 0;
function drawGame(iState, iGameSpeed, bExecuteGame) {
  if (bDrawGame_executed || bAnimation) {
    return;
  }
  bDrawGame_executed = true;

  //console.log(iDebugCounter + ", state: " + iState + ", game speed: " + iGameSpeed + ", exec.: " + bExecuteGame);
  iDebugCounter += 1;

  var divDrawGameContainer = document.getElementById("divDrawGameContainer");
  var iPositionsValue = $('#ddlPositions').val();
  var bOnlyMainComments = document.getElementById("ddlComments").value > 0;

  var bAverage = iPositionsValue > 0;
  var sAjaxTextStatus = "";

  if (!iGameSpeed) {
    iGameSpeed = 300;
  }

  if (!iGameSpeedUsed) {
    iGameSpeedUsed = iGameSpeed;
  }
  var iGameSpeedUsed2 = iGameSpeedUsed;

  $.ajax({
    cache: false,
    url: "/ViewGame/ViewGameLocations",
    type: "GET",
    dataType: "JSON",
    data: { iState: iState, bAverage: bAverage, bExecuteGame: bExecuteGame, bOnlyMainComments: bOnlyMainComments },
    contentType: "application/json; charset=utf-8",
    success: function (gLoc2) {
      if (gLoc2) {
        // Set finished flag
        bFinished = gLoc2.bFinished;

        // Get used game speed
        if (bFinished) { iGameSpeedUsed = 0; }
        else { iGameSpeedUsed = gLoc2.iGameSpeedUsed; }
        iGameSpeedUsed2 = iGameSpeedUsed;

        if (iState >= 0 || gLoc2.bFinished || iState === -3 || iState === -4) { // If specific state or game is finished or initial call --> draw only once
          if (iState === -3) { // If initial call --> set global bFinished flag and recall function if game not finished
            playerGlobal = drawPlayer(gLoc2.ltPlayer);

            imgBall = drawBall();
            divBallTarget = drawBallTarget();
            //ptBallLast = gLoc2.gBall.ptPos;

            setShowHidePitch(document.getElementById("ddlShowPitch").value);
          }

          if (iState >= 0 || iState === -3) {
            $("#tblComments tr").remove();
          }

          ltStadiumBlockSizes = gLoc2.ltStadiumBlockSizes;

          gLocArray = [];
          drawGame2(gLoc2, iState, iGameSpeedUsed, bExecuteGame);
          //plotStatistics(iState);
        } else if (gLoc2.fBreak > 0) { // If half-time --> set speech dialog
          divSpeach = drawHalftimeSpeechDialog(gLoc2.fBreak, gLoc2.ltSpeachOptions, divDrawGameContainer);
        } else { // If running game ...
          if (divSpeach) {
            $(divSpeach).dialog('destroy').remove();
            divSpeach = null;
          }

          if (bExecuteGame) {
            drawGame2(gLoc2, iState, iGameSpeedUsed, bExecuteGame); // ... draw game directly

            // If shoot, set game-speed to default value
            if (gLoc2.iShootHA > 0) {
              iGameSpeedUsed2 = 300;
            }
          } else {
            gLocArray.push(gLoc2); // ... add latest element of locations to the array
          }
        }

        //drawHalftimeSpeechDialog();
      }

      bDrawGame_executed = false;

      // Recall function if game not finished
      if (iState === -3 && !bFinished && iGameSpeedUsed > 0) {
        setTimeout(drawGame, iGameSpeedUsed2, -1, iGameSpeedUsed, bExecuteGame);
      }

      // If array big enough, show results
      if (gLocArray.length > 0) {
        if (gLocArray.length > 2 && iState === -1) {
          // Get smallest state indicator (oldest state)
          let iStateMin = gLocArray[0].iState;
          for (var i = 1; i < gLocArray.length; ++i) {
            iStateMin = Math.min(iStateMin, gLocArray[i].iState);
          }

          // Loop over state array
          for (var i = 0; i < gLocArray.length; ++i) {
            var gLoc = gLocArray[i];

            // If oldest state --> draw it and remove it from list
            if (gLoc.iState === iStateMin) {
              bFinished = gLoc.bFinished;

              drawGame2(gLoc, iState, iGameSpeedUsed, bExecuteGame);

              // Remove that element from the array
              gLocArray.splice(i, 1);

              break;
            }
          }
        }
      }

      // If running game and not finished --> recall function (loop)
      if (iState === -1 && !bFinished && !bAnimation && sAjaxTextStatus !== "error" && !bStopPlay && !bAdminStop) {
        bDrawGame_executed = false;
        setTimeout(drawGame, iGameSpeedUsed2, -1, iGameSpeedUsed, bExecuteGame);
      }
    },
    error: function () {
      //alert("ERROR");
      plotStatistics(iState);
      bDrawGame_executed = false;

      return false;
    },
    complete: function (jqXHR, textStatus) {
      sAjaxTextStatus = textStatus;
    }
  });
}

var iB = 0;
function drawGame2(gLoc, iState, iGameSpeed, bExecuteGame) {
  // Initiate rendering
  var iShowPitch = document.getElementById("ddlShowPitch").value;
  let divRenderContainer = document.getElementById("divRenderContainer");
  let txtRenderLeft = document.getElementById("txtRenderLeft");
  let txtRenderShootResult = document.getElementById("txtRenderShootResult");
  var iPositionsValue = parseInt(document.getElementById("ddlPositions").value);
  var iAnimationsOption = document.getElementById("ddlAnimations").value;

  let lbGoalsH = document.getElementById("lbGoalsH");
  let lbGoalsA = document.getElementById("lbGoalsA");

  if (iShowPitch > 0) {
    updatePlayer(playerGlobal, gLoc.ltPlayer, iPositionsValue === 0 && iShowPitch === 1, iGameSpeed > 0);
    updateBallPos(imgBall, divBallTarget, gLoc.gBall, iGameSpeed);

    if (iShowPitch == 1) {
      // Show player portrait
      printPlayerActivePortrait(gLoc.sPlActiveName, gLoc.sPlActivePortraitImg, gLoc.iPlActiveHA);

      // Draw player chances
      if (document.getElementById("bShowChances").checked) {
        drawPlayerChances(gLoc.fPlAction, gLoc.fPlActionRnd);
      }

      // Draw player pass targets
      if (document.getElementById("bShowPassTargets").checked) {
        drawPassTargets(gLoc.gBall.fPosX, gLoc.gBall.fPosY, gLoc.ltPassTargets);
      }

      // Draw player target positions
      if (document.getElementById("bShowTargetPos").checked) {
        drawPlayerTargetPositions(gLoc.ltPlayer);
      }
    }
  }

  // Print comment
  printComments(gLoc.ltComments);

  if (iState < 0 && gLoc.bFinished) {
    iState = -2;
  }

  // Color goal indicator at shoot
  lbGoalsH.style.color = "black";
  lbGoalsA.style.color = "black";
  lbGoalsH.style.fontSize = "2.75em";
  lbGoalsA.style.fontSize = "2.75em";
  lbGoalsH.style.top = "0px";
  lbGoalsA.style.top = "0px";
  lbGoalsH.style.right = "3px";
  lbGoalsA.style.left = "3px";
  lbGoalsH.style.fontWeight = 'normal';
  lbGoalsA.style.fontWeight = 'normal';
  if (gLoc.iShootHA === 1) {
    lbGoalsH.style.color = "red";
    lbGoalsH.style.fontSize = "3em";
    lbGoalsH.style.top = "-6px";
    lbGoalsH.style.right = "0px";
    lbGoalsH.style.fontWeight = 'bold';
  } else if (gLoc.iShootHA === 2) {
    lbGoalsA.style.color = "red";
    lbGoalsA.style.fontSize = "3em";
    lbGoalsA.style.top = "-6px";
    lbGoalsA.style.left = "0px";
    lbGoalsA.style.fontWeight = 'bold';
  }

  // Play sounds
  playSound(gLoc.iEvent);

  // Update statistic
  var bUpdate = gLoc.bUpdateStatistic;
  if (bUpdate && iState === -1) {
    bUpdate = document.getElementById("cbUpdateStatistic").checked;
  }
  if (bUpdate || iState === -2 || iState === -3 || iState > 0) {
    plotStatistics(iState);
  }

  // Animate game
  if (iAnimationsOption > 0 && !render3D) {
    bAnimation = false;

    // Create renderer
    render3D = new Render3D(
      divRenderContainer,
      txtRenderLeft, null, txtRenderShootResult,
      ltStadiumBlockSizes
    );

    // Add player to renderer
    render3D.TeamH.ltPlayer = [];
    render3D.TeamH.ltPlayer.push(null);
    for (var iP = 1; iP < 11; iP++) {
      //console.log("ViewGame.js | render3D.TeamH.addPlayer(), " + iP);
      render3D.TeamH.addPlayer();
    }
    render3D.TeamA.ltPlayer = [];
    render3D.TeamA.ltPlayer.push(null);
    for (var iP = 1; iP < 11; iP++) {
      render3D.TeamA.addPlayer();
    }
  }

  if (!bAnimation) {
    if (iAnimationsOption == 2) {
      render3D.setCamera(3);
    } else {
      divRenderContainer.style.display = 'none';
    }
  }

  if (iAnimationsOption > 1 || (gLoc.iShootRes < 8 && iAnimationsOption == 1)) {
    let divRenderContainer = document.getElementById("divRenderContainer");

    divRenderContainer.style.display = "block";
    divRenderContainer.style.zIndex = 101;

    // Set player positions
    //console.log("render3D.TeamH.ltPlayer.length: " + render3D.TeamH.ltPlayer.length);
    for (let i = 0; i < render3D.TeamH.ltPlayer.length; i++) {
      if (render3D.TeamH.ltPlayer[i] != null) {
        render3D.TeamH.ltPlayer[i].ptPos = { x: render3D.convertXToRenderCoord(gLoc.ltPlayer[i].ptPos.x), y: -gLoc.ltPlayer[i].ptPos.y };
        render3D.TeamH.ltPlayer[i].ptPosLast = { x: render3D.convertXToRenderCoord(gLoc.ltPlayer[i].ptPosLast.x), y: -gLoc.ltPlayer[i].ptPosLast.y };
        //console.log("gLoc.ltPlayer[" + i + "].ptPos: " + gLoc.ltPlayer[i].iNo + ", " + gLoc.ltPlayer[i].ptPos.x + ", " + gLoc.ltPlayer[i].ptPos.y);
        //render3D.TeamH.ltPlayer[i].ptPos     = { x: gLoc.iShootHA == 0 ? gLoc.ltPlayer[i].ptPos    .x : transformPosition(gLoc.ltPlayer[i].ptPos    ).x, y: gLoc.ltPlayer[i].ptPos    .y };
        //render3D.TeamH.ltPlayer[i].ptPosLast = { x: gLoc.iShootHA == 0 ? gLoc.ltPlayer[i].ptPosLast.x : transformPosition(gLoc.ltPlayer[i].ptPosLast).x, y: gLoc.ltPlayer[i].ptPosLast.y };
        //render3D.TeamH.ltPlayer[i].updatePos({ x: gLoc.iShootHA == 0 ? gLoc.ltPlayer[i].ptPos.x : transformPosition(gLoc.ltPlayer[i].ptPos).x, y: gLoc.ltPlayer[i].ptPos.y });
      }
    }
    //console.log("render3D.TeamA.ltPlayer.length: " + render3D.TeamA.ltPlayer.length);
    for (let i = 0; i < render3D.TeamA.ltPlayer.length; i++) {
      if (render3D.TeamA.ltPlayer[i] != null) {
        render3D.TeamA.ltPlayer[i].ptPos = { x: render3D.convertXToRenderCoord(gLoc.ltPlayer[i + 11].ptPos.x), y: -gLoc.ltPlayer[i + 11].ptPos.y };
        render3D.TeamA.ltPlayer[i].ptPosLast = { x: render3D.convertXToRenderCoord(gLoc.ltPlayer[i + 11].ptPosLast.x), y: -gLoc.ltPlayer[i + 11].ptPosLast.y };
        //console.log("gLoc.ltPlayer[" + (i + 11).toString() + "].ptPos: " + gLoc.ltPlayer[i + 11].iNo + ", " + gLoc.ltPlayer[i + 11].ptPos.x + ", " + gLoc.ltPlayer[i + 11].ptPos.y);
        //render3D.TeamA.ltPlayer[i].ptPos     = { x: gLoc.iShootHA == 0 ? gLoc.ltPlayer[i + 11].ptPos    .x : transformPosition(gLoc.ltPlayer[i + 11].ptPos    ).x, y: gLoc.ltPlayer[i + 11].ptPos    .y };
        //render3D.TeamA.ltPlayer[i].ptPosLast = { x: gLoc.iShootHA == 0 ? gLoc.ltPlayer[i + 11].ptPosLast.x : transformPosition(gLoc.ltPlayer[i + 11].ptPosLast).x, y: gLoc.ltPlayer[i + 11].ptPosLast.y };
        //render3D.TeamA.ltPlayer[i].updatePos({ x: gLoc.iShootHA == 0 ? gLoc.ltPlayer[i + 11].ptPos.x : transformPosition(gLoc.ltPlayer[i + 11].ptPos).x, y: gLoc.ltPlayer[i + 11].ptPos.y });
      }
    }

    let ptPosBallm1 = { x: gLoc.gBall.ptPosLast.x, y: -gLoc.gBall.ptPosLast.y, z: 0 };
    let ptPosBall0 = { x: gLoc.gBall.ptPos.x, y: -gLoc.gBall.ptPos.y, z: 0 };
    let ballTarget = { x: gLoc.gBall.ptPosTarget.x, y: -gLoc.gBall.ptPosTarget.y, z: -1 };
    /*
    if (gLoc.iShootHA == 1) {
      ptPosBallm1.x = transformPosition(ptPosBallm1).x;
      ptPosBall0 .x = transformPosition(ptPosBall0) .x;
      ballTarget .x = transformPosition(ballTarget) .x;
    }
    */

    // Animate shoot
    let iShootRes = -1;
    let fctContinue = null;
    let iSideY = 1;
    if (gLoc.iShootRes < 8 && iAnimationsOption == 1) {
      bAnimation = true;
      iShootRes = gLoc.iShootRes;

      // Reset camera
      render3D.setCamera(1, ptPosBall0);

      //ptPosBallm1.x = render3D.convertXToRenderCoord(ptPosBallm1.x);
      //ptPosBall0.x = render3D.convertXToRenderCoord(ptPosBall0.x);
      ballTarget.x = render3D.convertXToRenderCoord(ballTarget.x);

      if (gLoc.iShootRes > 0 && gLoc.iShootRes < 7) {
        //if (transformPosition(gLoc.gBall.ptPosTarget).y < 0) { iSideY = -1; }
        if (gLoc.gBall.ptPosTarget.y < 0) { iSideY = -1; }
        ballTarget = render3D.getBallTarget(gLoc.iShootRes, gLoc.fShootRnd, ballTarget.x > 0 ? +1 : -1, iSideY);
      } else if (gLoc.iShootRes == 0 && render3D.checkOnGoal(ballTarget, true)) {
        ballTarget.z = 3.45;
      }

      fctContinue = function () {
        bAnimation = false;
        divRenderContainer.style.display = "none";
        drawGame(-1, iGameSpeedUsed, bExecuteGame);
      };

      //console.log("Render shoot. Pos ball 0: " + ptPosBall0.x + "/" + ptPosBall0.y + " (" + gLoc.gBall.ptPos.x + "/" + gLoc.gBall.ptPos.y + "), ballTarget x/y: " + ballTarget.y.toFixed(5) + " / " + ballTarget.y.toFixed(5) + ", iHA: " + gLoc.iShootHA + ", iShootRes: " + gLoc.iShootRes);
    }

    render3D.animate(
      { x: ptPosBall0.x, y: ptPosBall0.y, z: gLoc.gBall.fPosZ },
      { x: ptPosBallm1.x, y: ptPosBallm1.y, z: 0 },
      iShootRes,
      gLoc.fShootRnd,
      iSideY,
      ballTarget,
      false,
      fctContinue
    );
  }
}

function transformPosition(ptPos) {
  const ptPosTransf = { x: 122 - ptPos.x, y: -ptPos.y, z: ptPos.z };
  return ptPosTransf;
}

function drawBall() {
  var divDrawGame = document.getElementById("divDrawGame");

  // Remove ball if exist
  var ltDivToRm = divDrawGame.getElementsByClassName("divBall");
  while (ltDivToRm.length > 0) {
    ltDivToRm[0].parentNode.removeChild(ltDivToRm[0]);
  }

  var divBallTmp = document.createElement("div");
  divBallTmp.className = "divBall";
  divBallTmp.id = "divBall";
  divBallTmp.style.position = "absolute";
  divBallTmp.style.top  = "49.0625%";
  divBallTmp.style.left = "49.35%";
  divBallTmp.style.width = "1.25%";
  divBallTmp.style.height = "1.875%";
  divBallTmp.style.minWidth = "10px";
  divBallTmp.style.minHeight = "10px";
  divBallTmp.style.zIndex = "99";
  var imgBallTmp = document.createElement("img");
  imgBallTmp.id = "imgBall";
  imgBallTmp.src = "/Content/Icons/ball_white.png";
  imgBallTmp.alt = "Ball";
  imgBallTmp.style.position = "absolute";
  imgBallTmp.style.top  = "0px";
  imgBallTmp.style.left = "0px";
  imgBallTmp.style.width  = "100%";
  imgBallTmp.style.height = "100%";
  divBallTmp.appendChild(imgBallTmp);

  divDrawGame.appendChild(divBallTmp);

  return divBallTmp;
}

function drawBallTarget(id, clBorder) {
  var divDrawGame = document.getElementById("divDrawGame");

  // Ball target
  var divBallTargetTmp = document.createElement("div");
  if (id) {
    divBallTargetTmp.id = id;
  }
  divBallTargetTmp.style.position = "absolute";
  divBallTargetTmp.style.top  = "49.1%";
  divBallTargetTmp.style.left = "49.4%";
  divBallTargetTmp.style.width = "1.2%";
  divBallTargetTmp.style.height = "1.8%";
  divBallTargetTmp.style.webkitBorderRadius = "50%";
  divBallTargetTmp.style.borderRadius = "50%";
  divBallTargetTmp.style.zIndex = "98";
  divBallTargetTmp.style.display = "none";

  // Set color
  let sStyle = "2px solid ";
  if (clBorder) {
    sStyle += clBorder;
  } else {
    sStyle += "rgb(0,230,230)";
  }
  divBallTargetTmp.style.border = sStyle;

  divDrawGame.appendChild(divBallTargetTmp);

  return divBallTargetTmp;
}

//var ptBallLast;
var ptBallTargetGlobal;
function updateBallPos(imgBallTmp, divBallTargetTmp, gBall, iGameSpeed) {
  if (timerInterpolateBall == null) { ptBallTargetGlobal = null; }

  if (gBall.nPassSteps > 0) {
    //alert(gBall.nPassSteps + ", " + gBall.iPassStep);
    ptBallTargetGlobal = gBall.ptPosTarget;

    // Interpolate ball
    if (iGameSpeed > 0) {
      // Start interpolation at first pass step only
      if (gBall.nPassSteps > 0 && (gBall.nPassSteps - gBall.iPassStep === 0 || timerInterpolateBall == null)) {
        const nInterpSteps = 10;

        //console.log("Pass: Start pass: pt0=" + gBall.ptPosLast.x.toString() + "/" + gBall.ptPosLast.y.toString() + " pt1=" + gBall.ptPosTarget.x.toString() + "/" + gBall.ptPosTarget.y.toString() + ", step=" + (gBall.nPassSteps - gBall.iPassStep).toString() + "/" + gBall.nPassSteps.toString() + ", " + (timerInterpolateBall == null).toString());
        interpolateBall(imgBallTmp, gBall.ptPosLast, gBall.ptPosTarget, !gBall.bLowPass, nInterpSteps * gBall.nPassSteps, iGameSpeed * gBall.nPassSteps);
      } else if (timerInterpolateBall == null || gBall.nPassSteps == 0) {
        var ptBall = [getPosPixX(gBall.fPosX), getPosPixY(gBall.fPosY)];
        //console.log("Pass: Interrupt!" + ptBall[0].toString() + "/" + ptBall[1].toString());
        setBallPos(imgBallTmp, 0, ptBall, null, false, 0, 0);
      //} else {
      //  console.log("Pass: Interpolating... step: " + (gBall.nPassSteps - gBall.iPassStep).toString() + "/" + gBall.nPassSteps.toString());
      }
    } else { // No interpolation
      var pt0 = getPosPix(gBall.ptPosLast);
      var pt1 = null;
      if (gBall.ptPosTarget) { pt1 = getPosPix(gBall.ptPosTarget); }
      //console.log("Pass: No interp.: " + pt0 + ", " + pt1);
      setBallPos(imgBallTmp, gBall.nPassSteps - gBall.iPassStep, pt0, pt1, !gBall.bLowPass, gBall.nPassSteps, 0);
    }

    /*
    if (gBall.nPassSteps - gBall.iPassStep === 0) {
      ptBallLast = gBall.ptPosLast;
    }

    if (!ptBallLast) {
      ptBallLast = gBall.ptPosLast;
    }

    interpolateBall(imgBallTmp, ptBallLast, gBall.Pos3D, gBall, gBall.iPassType === 2, 10, iGameSpeed);

    ptBallLast.x = gBall.Pos3D.x;
    ptBallLast.y = gBall.Pos3D.y;
     */

    updateBallTargetPos(divBallTargetTmp, gBall.ptPosTarget.x, gBall.ptPosTarget.y);
  } else {
    var ptBall = getPosPix(gBall.ptPos);
    //var ptBall = [getPosPixX(gBall.fPosX), getPosPixY(gBall.fPosY)];
    //console.log("Set ball: " + gBall.ptPos.x.toString() + "/" + gBall.ptPos.y.toString() + "  " + ptBall[0].toString() + "/" + ptBall[1].toString());
    setBallPos(imgBallTmp, 0, ptBall, null, false, 0, 0);
    /*
    var fSizeX = 1.25;
    var fSizeY = fSizeX * 1.5;

    var ptBall = [getPosPixX(gBall.fPosX), getPosPixY(gBall.fPosY)];
    if (!ptBall) {
      ptBall = [50 - fSizeX, 50 - fSizeY];
    }

    imgBallTmp.style.left = ptBall[0].toString() + '%';
    imgBallTmp.style.top  = ptBall[1].toString() + '%';
    imgBallTmp.style.width  = fSizeX.toString() + '%';
    imgBallTmp.style.height = fSizeY.toString() + '%';
    */

    divBallTargetTmp.style.display = "none";
  }
}

function updateBallTargetPos(divBallTargetTmp, x, y) {
  divBallTargetTmp.style.display = "block";

  let sXbt = ((100 * (x / 122.0)) - 0.60).toString();
  let sYbt = ((100 * ((y + 25) / 50.0)) - 0.90).toString();
  divBallTargetTmp.style.left = sXbt + '%';
  divBallTargetTmp.style.top  = sYbt + '%';
}

var timerInterpolateBall = null;
function interpolateBall(imgBallTmp, pt0Ck, pt1Ck, bHighPass, nInterpSteps, iGameSpeed) {
  var pt0 = getPosPix(pt0Ck);
  var pt1 = getPosPix(pt1Ck);

  if (timerInterpolateBall !== null) {
    clearTimeout(timerInterpolateBall);
    timerInterpolateBall = null;
  }

  setBallPos(imgBallTmp, 0, pt0, pt1, bHighPass, nInterpSteps, iGameSpeed);
}

function setBallPos(imgBallTmp, iB, pt0, pt1, bHighPass, nInterpSteps, iGameSpeed) {
  //console.log("setBallPos: " + pt0 + ", " + pt1);
  timerInterpolateBall = null;

  if (ptBallTargetGlobal) { pt1 = getPosPix(ptBallTargetGlobal); }

  // Position
  var fX = pt0[0];
  var fY = pt0[1];
  if (pt1 && nInterpSteps > 0) {
    fX = (pt0[0] * ((nInterpSteps - iB) / nInterpSteps)) + (pt1[0] * (iB / nInterpSteps));
    fY = (pt0[1] * ((nInterpSteps - iB) / nInterpSteps)) + (pt1[1] * (iB / nInterpSteps));
  }
  imgBallTmp.style.left = fX.toString() + '%';
  imgBallTmp.style.top  = fY.toString() + '%';
  //console.log("Ball: x=" + fX.toFixed(4) + ", y=" + fY.toFixed(4));

  // Size
  var fSizeX = 1.25;
  if (bHighPass && nInterpSteps > 0) {
    fSizeX = 1.25 + (1.0 - Math.pow((2.0 * (iB / nInterpSteps)) - 1.0, 2));
  }
  const fSizeY = fSizeX * 1.5;
  imgBallTmp.style.width  = fSizeX.toString() + '%';
  imgBallTmp.style.height = fSizeY.toString() + '%';

  if (nInterpSteps > 0 && iB < nInterpSteps && iGameSpeed > 0) {
    const fSlowDownFactor = 1.6;

    timerInterpolateBall = setTimeout(function () { setBallPos(imgBallTmp, iB + 2, pt0, pt1, bHighPass, nInterpSteps, iGameSpeed); }, (iGameSpeed * fSlowDownFactor) / nInterpSteps);
  }
}

function getPosPix(ptPos) {
  if (!ptPos) {
    return null;
  }

  return [getPosPixX(ptPos.x), getPosPixY(ptPos.y)];
}

function getPosPixX(fPosX) {
  if (!fPosX) {
    fPosX = 0;
  }

  return (100 * (fPosX / 122.0)) - 0.6250;
}

function getPosPixY(fPosY) {
  if (!fPosY) {
    fPosY = 0;
  }

  return (100 * ((fPosY + 25) / 50.0)) - 0.9375;
}

function drawPlayer(ltPlayer) {
  var divDrawGame = document.getElementById("divDrawGame");

  // Clear pitch
  var ltDivToRm = divDrawGame.getElementsByClassName("divPlayer");
  while (ltDivToRm.length > 0) {
    ltDivToRm[0].parentNode.removeChild(ltDivToRm[0]);
  }

  if (ltPlayer.length < 1) return;

  var fLookAtSize = 0.3;

  var player = [];
  for (var iP = 0; iP < 11; iP++) {
    if (!ltPlayer[iP +  0]) {
      continue;
    }

    // Player Home
    var divPlH = document.createElement("div");
    divPlH.className = "divPlayer divPlayerH";
    divPlH.id = "divPlayerH_" + iP.toString();
    divPlH.style.position = "absolute";
    divPlH.style.width = "2%";
    divPlH.style.height = "3%";
    divPlH.style.minWidth = "10px";
    divPlH.style.minHeight = "10px";
    divPlH.style.top = (30 + (iP * 4)).toString() + "%";
    divPlH.style.left = "40%";
    divPlH.style.border = "2px solid";
    divPlH.style.webkitBorderRadius = "50%";
    divPlH.style.borderRadius = "50%";
    divPlH.style.zIndex = "21";
    divPlH.style.display = "flex";
    divPlH.style.justifyContent = "center";
    divPlH.style.alignItems = "center";
    divPlH.innerText = ltPlayer[iP + 0].iNo.toString();
    divPlH.title = ltPlayer[iP + 0].iNo.toString() + " - " + ltPlayer[iP + 0].sName;

    // Draw look-at circle
    var divPlLookAtH = document.createElement("div");
    divPlLookAtH.style.position = "absolute";
    divPlLookAtH.style.width  = (fLookAtSize * 100).toString() + '%';
    divPlLookAtH.style.height = (fLookAtSize * 100).toString() + '%';
    divPlLookAtH.style.top = ((0.5 - (fLookAtSize / 2)) * 100).toString() + '%';
    divPlLookAtH.style.right = (-(fLookAtSize / 2) * 100).toString() + '%';
    divPlLookAtH.style.backgroundColor = 'black';
    divPlLookAtH.style.webkitBorderRadius = "50%";
    divPlLookAtH.style.borderRadius = "50%";
    divPlLookAtH.style.zIndex = "24";
    divPlH.appendChild(divPlLookAtH);
    divDrawGame.appendChild(divPlH);

    player.push(divPlH);
  }

  for (var iP = 0; iP < 11; iP++) {
    if (!ltPlayer[iP + 11]) {
      continue;
    }

    // Player Away
    var divPlA = document.createElement("div");
    divPlA.className = "divPlayer divPlayerA";
    divPlA.id = "divPlayerA_" + iP.toString();
    divPlA.style.position = "absolute";
    divPlA.style.width = "2%";
    divPlA.style.height = "3%";
    divPlA.style.minWidth = "10px";
    divPlA.style.minHeight = "10px";
    divPlA.style.top = (30 + (iP * 4)).toString() + "%";
    divPlA.style.left = "60%";
    divPlA.style.border = "2px solid";
    divPlA.style.webkitBorderRadius = "50%";
    divPlA.style.borderRadius = "50%";
    divPlA.style.zIndex = "21";
    divPlA.style.display = "flex";
    divPlA.style.justifyContent = "center";
    divPlA.style.alignItems = "center";
    divPlA.innerText = ltPlayer[iP + 11].iNo.toString();
    divPlA.title = ltPlayer[iP + 11].iNo.toString() + " - " + ltPlayer[iP + 11].sName;

    // Draw look-at circle
    var divPlLookAtA = document.createElement("div");
    divPlLookAtA.style.position = "absolute";
    divPlLookAtA.style.width = (fLookAtSize * 100).toString() + '%';
    divPlLookAtA.style.height = (fLookAtSize * 100).toString() + '%';
    divPlLookAtA.style.top = ((0.5 - (fLookAtSize / 2)) * 100).toString() + '%';
    divPlLookAtA.style.left = (-(fLookAtSize / 2) * 100).toString() + '%';
    divPlLookAtA.style.backgroundColor = 'black';
    divPlLookAtA.style.webkitBorderRadius = "50%";
    divPlLookAtA.style.borderRadius = "50%";
    divPlLookAtA.style.zIndex = "24";
    divPlA.appendChild(divPlLookAtA);
    divDrawGame.appendChild(divPlA);

    player.push(divPlA);
  }

  return player;
}

function updatePlayer(player, ltPlayer, bShowLookAt, bLive) {
  if (ltPlayer.length < 1) return;

  var iPositionsValue = parseInt(document.getElementById("ddlPositions").value);

  const fLookAtSize = 0.3;

  // For each player
  for (var iP = 0; iP < 22; iP++) {
    if (ltPlayer.length <= iP) {
      break;
    }

    var pl = ltPlayer[iP];

    if (!pl) {
      continue;
    }

    if (pl.iCard > 1 && bLive) { // if red card
      player[iP].style.display = "none";
      continue;
    }

    if (iPositionsValue < 0) {
      player[iP].style.display = "none";
      continue;
    }

    var fXh = pl.ptPos.x / 122.0;
    var fYh = pl.ptPos.y /  50.0;

    var sXh = ((100 *  fXh       ) - 1.0).toString();
    var sYh = ((100 * (fYh + 0.5)) - 1.5).toString();

    let divLookAt = player[iP].children[0];
    if (bShowLookAt) {
      divLookAt.style.display = "block";
      var fLftH = ((1.0 - Math.cos(pl.iLookAt * 60 * Math.PI / 180.0)) / 2);
      divLookAt.style.left = ((fLftH - (fLookAtSize / 2)) * 100).toString() + '%';
      var fTopH = ((1.0 - Math.sin(pl.iLookAt * 60 * Math.PI / 180.0)) / 2);
      divLookAt.style.top  = ((fTopH - (fLookAtSize / 2)) * 100).toString() + '%';
    } else {
      divLookAt.style.display = "none";
    }

    player[iP].style.left = sXh + '%';
    player[iP].style.top  = sYh + '%';

    player[iP].style.display = "flex";
  }
}

function changeJerseyColors(iHA, divJersey) {
  if (iHA === 0) {
    if (iColorH === 1) {
      divJersey.style.backgroundColor = sColorJerseyH[0];
      divJersey.style.borderColor = sColorJerseyH[1];
      divJersey.style.color = sColorJerseyH[2];
    } else {
      divJersey.style.backgroundColor = sColorJerseyH[3];
      divJersey.style.borderColor = sColorJerseyH[4];
      divJersey.style.color = sColorJerseyH[5];
    }

    // Set team home color in renderer
    if (render3D) {
      render3D.TeamH.applyColorToPlayer(divJersey.style.backgroundColor, divJersey.style.borderColor, divJersey.style.color);
    }

    iColorH = 1 - iColorH;
  } else if (iHA === 1) {
    if (iColorA === 0) {
      divJersey.style.backgroundColor = sColorJerseyA[0];
      divJersey.style.borderColor = sColorJerseyA[1];
      divJersey.style.color = sColorJerseyA[2];
    } else {
      divJersey.style.backgroundColor = sColorJerseyA[3];
      divJersey.style.borderColor = sColorJerseyA[4];
      divJersey.style.color = sColorJerseyA[5];
    }

    // Set team away color in renderer
    if (render3D) {
      render3D.TeamA.applyColorToPlayer(divJersey.style.backgroundColor, divJersey.style.borderColor, divJersey.style.color);
    }

    iColorA = 1 - iColorA;
  }

  changePlayerJerseyColor(iHA, divJersey.style.backgroundColor, divJersey.style.borderColor, divJersey.style.color);
}

function changeJerseyView(bSmall) {
  for (var iP = 0; iP < 22; iP++) {
    if (!playerGlobal[iP]) {
      continue;
    }

    let divPlayerNb = playerGlobal[iP].getElementsByClassName("divPlayerNb");

    if (bSmall) {
      playerGlobal[iP].style.border = "1px solid";
      if (divPlayerNb && divPlayerNb.length > 0) {
        divPlayerNb[0].style.display = "none";
      }
    } else {
      playerGlobal[iP].style.border = "2px solid";
      if (divPlayerNb && divPlayerNb.length > 0) {
        divPlayerNb[0].style.display = "table-cell";
      }
    }
  }
}

function changePlayerJerseyColor(iHA, cl1, cl2, cl3) {
  if (iHA === 0) {
    for (var iP = 0; iP < 11; iP++) {
      if (!playerGlobal[iP]) {
        continue;
      }

      playerGlobal[iP].style.backgroundColor = cl1;
      if (iP === 0) {
        playerGlobal[iP].style.borderColor = "rgb(57,255,20)";
      } else {
        playerGlobal[iP].style.borderColor = cl2;
      }
      playerGlobal[iP].style.color = cl3;
    }
  } else if (iHA === 1) {
    for (var iP = 11; iP < 22; iP++) {
      if (!playerGlobal[iP]) {
        continue;
      }

      playerGlobal[iP].style.backgroundColor = cl1;
      if (iP === 11) {
        playerGlobal[iP].style.borderColor = "rgb(243,243,21)";
      } else {
        playerGlobal[iP].style.borderColor = cl2;
      }
      playerGlobal[iP].style.color = cl3;
    }
  }
}

function setShowHidePitch(iShowPitch) {
  var divDrawGameContainer = document.getElementById("divDrawGameContainer");
  var divPlActiveDetails = document.getElementById("divPlActiveDetails");
  var txtReferee = document.getElementById("txtReferee");
  var divCbUpdate = document.getElementById("divCbUpdate");
  var cbUpdateStatistic = document.getElementById("cbUpdateStatistic");
  var divLinks = document.getElementById("divLinks");
  var divLinkTactic = document.getElementById("divLinkTactic");
  var divLinkOptions = document.getElementById("divLinkTactic");
  var divAdminChanceShoot = document.getElementById("divAdminChanceShoot");

  if (iShowPitch == 1) {
    divDrawGameContainer.style.display = "block";
    divDrawGameContainer.style.position = "relative";
    divDrawGameContainer.style.width = "100%";
    divDrawGameContainer.style.height = "100%";
    divDrawGameContainer.style.top = "0px";
    divDrawGameContainer.style.removeProperty('right');
    divDrawGameContainer.style.left = "0px";
    divDrawGameContainer.style.zIndex = "0";
    if (divLinks) {
      divLinks.style.left = "5%";
      divLinks.style.width = "90%";
    }
    if (divLinkTactic) {
      divLinkTactic.style.position = "absolute";
      divLinkTactic.style.top = "calc(20px * var(--ckRelSize))";
    }
    if (divLinkOptions) {
      divLinkOptions.style.top = "calc(20px * var(--ckRelSize))";
      divLinkOptions.style.right = "0px";
    }
    divPlActiveDetails.style.display = "block";
    txtReferee.style.display = "block";
    if (divCbUpdate) {
      divCbUpdate.style.display = "block";
    }
    if (divAdminChanceShoot) {
      divAdminChanceShoot.style.display = "block";
    }
    changeJerseyView(false);
  } else {
    if (iShowPitch == 2) {
      divDrawGameContainer.style.display = "block";
      divDrawGameContainer.style.position = "absolute";
      divDrawGameContainer.style.width = "194px";
      divDrawGameContainer.style.height = "128px";
      //divDrawGameContainer.style.top = document.getElementById("divComments").offsetTop + "px";
      divDrawGameContainer.style.top = "32px";
      divDrawGameContainer.style.removeProperty('left');
      divDrawGameContainer.style.right = "0px";
      divDrawGameContainer.style.zIndex = "99";
    } else {
      divDrawGameContainer.style.display = "none";
    }
    if (divLinks) {
      divLinks.style.left = "0px";
      divLinks.style.width = "100%";
    }
    if (divLinkTactic) {
      divLinkTactic.style.position = "relative";
      divLinkTactic.style.top = "0px";
    }
    if (divLinkOptions) {
      divLinkOptions.style.top = "0px";
      divLinkOptions.style.right = "calc(56px * var(--ckRelSize))";
    }
    divPlActiveDetails.style.display = "none";
    txtReferee.style.display = "none";
    if (divCbUpdate) {
      divCbUpdate.style.display = "none";
    }
    if (divAdminChanceShoot) {
      divAdminChanceShoot.style.display = "none";
    }
    changeJerseyView(true);
  }

  if (iShowPitch != 1) {
    document.getElementById("ddlHeatmap").value = -1;
    document.getElementById("ddlShoots").value = -1;
    document.getElementById("ddlDuels").value = -1;
    document.getElementById("ddlPasses").value = -1;
    document.getElementById("ddlPositions").value = 0;
    if (cbUpdateStatistic) {
      cbUpdateStatistic.checked = true;
    }
  }

  document.getElementById("ddlHeatmap").disabled = iShowPitch != 1;
  document.getElementById("ddlShoots").disabled = iShowPitch != 1;
  document.getElementById("ddlDuels").disabled = iShowPitch != 1;
  document.getElementById("ddlPasses").disabled = iShowPitch != 1;
  document.getElementById("ddlPositions").disabled = iShowPitch != 1;
}

function playSound(iType) {
  if (iType < 1) {
    return;
  }

  var cbSound = document.getElementById("cbSound");
  if (cbSound) {
    if (!cbSound.checked) {
      return;
    }
  }

  //alert("sa");
  var sAudioDir = "/content/Sounds/";
  var sAudioFile = "";
  if (iType === 1 || iType === 2) {
    sAudioFile = sAudioDir + "homegoal.mp3";
  } else if (iType === 3) {
    sAudioFile = sAudioDir + "bar.wav";
  } else if (iType === 4) {
    sAudioFile = sAudioDir + "missed_goal.wav";
  } else if (iType === 5) {
    sAudioFile = sAudioDir + "whistle.wav";
  } else if (iType === 6) {
    sAudioFile = sAudioDir + "whistle.wav";
  }

  //alert("sb");
  var audioGame = new Audio(sAudioFile);
  if (audioGame) {
    audioGame.volume = 0.5;
    if (iType === 2) {
      audioGame.volume = 0.25;
    }
    audioGame.play();
  }
  //alert("sz");
}

function printComments(ltComments) {
  // Comment box
  var tblComments = document.getElementById('tblComments');

  var sLastComment = [];
  var colLast = tblComments.getElementsByTagName("tbody")[0].getElementsByTagName('td');
  if (colLast.length > 2) {
    sLastComment[0] = colLast[0].innerHTML;
    sLastComment[1] = colLast[1].innerHTML;
    sLastComment[2] = colLast[2].innerHTML; // State i
  }

  for (var iC = 0; iC < ltComments.length; ++iC) {
    if (sLastComment[0] === ltComments[iC][0] && sLastComment[1] === ltComments[iC][1]) {
      continue;
    }

    // Check state i
    if (parseInt(ltComments[iC][3]) <= parseInt(sLastComment[2])) {
      continue;
    }

    var rowComments = tblComments.insertRow(0);
    var cellComments0 = rowComments.insertCell(-1);
    var cellComments1 = rowComments.insertCell(-1);
    var cellComments2 = rowComments.insertCell(-1);
    cellComments0.innerHTML = ltComments[iC][0];
    cellComments1.innerHTML = ltComments[iC][1];
    if (ltComments[iC][2]) {
      cellComments0.style.fontWeight = ltComments[iC][2];
      cellComments1.style.fontWeight = ltComments[iC][2];
    }
    if (ltComments[iC].length > 4) {
      if (ltComments[iC][4]) {
        cellComments1.style.fontStyle = ltComments[iC][4];
      }
    }
    cellComments2.innerText = ltComments[iC][3]; // State i
    cellComments2.style.display = "none";
  }
}

function printPlayerActivePortrait(sPlActiveName, sPlActivePortraitImg, iPlActiveHA) {
  let divPlActiveDetails = document.getElementById("divPlActiveDetails");
  let imgPlActivePortrait = document.getElementById("imgPlActivePortrait");
  let divPlActiveName = document.getElementById("divPlActiveName");
  let txtPlActiveName = document.getElementById("txtPlActiveName");

  if (sPlActivePortraitImg) {
    txtPlActiveName.innerText = sPlActiveName;
    imgPlActivePortrait.src = sPlActivePortraitImg;
    divPlActiveDetails.style.display = "block";
  } else {
    divPlActiveDetails.style.display = "none";
    imgPlActivePortrait.src = "";
    txtPlActiveName.innerText = "";
  }
}

var bPlotStatistics_executed = false;

function drawPlayerChances(fPlAction, fPlActionRnd) {
  if (!fPlAction) {
    return;
  }

  // Check if sum of chances > 0
  var fChanceTotal = 0;
  for (var iC = 0; iC < fPlAction.length; iC++) {
    fChanceTotal += fPlAction[iC];
  }

  // Only draw chances if sum > 0
  if (fChanceTotal > 0) {
    document.getElementById("divPlActionChart").style.display = "block";

    var chartPlAction = new CanvasJS.Chart("divPlActionChart", {
      backgroundColor: "transparent",
      animationEnabled: false,
      theme: "theme2",//theme1
      dataPointWidth: 30,
      toolTip: {
        shared: true,
        borderColor: "black",
        contentFormatter: function (e) {
          var content = "<table>";

          // For each chance type
          for (var i = 0; i < e.entries.length; i++) {
            content += "<tr><td style=\"text-align:right\"><strong>" + e.entries[i].dataSeries.name + ":</strong></td><td style=\"text-align:right\">" + (e.entries[i].dataPoint.y * 100).toFixed(1) + "%</td>";
          }

          if (fPlActionRnd >= 0.0) {
            content += "<tr><td style=\"text-align:right\"><strong>Entscheidung:</strong></td><td style=\"text-align:right\">" + (fPlActionRnd * 100).toFixed(1) + "%</td>";
          }

          content += "</table>";

          return content;
        }
      },
      axisX: {
        title: "",
        tickLength: 0,
        margin: 0,
        lineThickness: 0,
        valueFormatString: " " //comment this to show numeric values
      },
      axisY: {
        interval: 100,
        title: "",
        tickLength: 0,
        lineThickness: 0,
        margin: 0,
        valueFormatString: " ", //comment this to show numeric values
        stripLines: [{
          value: fPlActionRnd * 100.0,
          color: "black",
          showOnTop: true,
          thickness: 2
        }]
      },
      data: [
        {
          // Change type to "bar", "column", "splineArea", "area", "spline", "pie",etc.
          type: "stackedBar100",
          color: "red",
          name: "Schuss",
          dataPoints: [
            { y: fPlAction[0] }
          ]
        },
        {
          // Change type to "bar", "column", "splineArea", "area", "spline", "pie",etc.
          type: "stackedBar100",
          color: "blue",
          name: "Pass",
          dataPoints: [
            { y: fPlAction[1] }
          ]
        },
        {
          // Change type to "bar", "column", "splineArea", "area", "spline", "pie",etc.
          type: "stackedBar100",
          color: "yellow",
          name: "Dribbling",
          dataPoints: [
            { y: fPlAction[2] }
          ]
        },
        {
          // Change type to "bar", "column", "splineArea", "area", "spline", "pie",etc.
          type: "stackedBar100",
          color: "grey",
          name: "Warten",
          dataPoints: [
            { y: fPlAction[3] }
          ]
        }
      ]
    });

    chartPlAction.render();
  } else {
    document.getElementById("divPlActionChart").style.display = "none";
  }
}

function drawPassTargets(x0, y0, ltPassTargets) {
  var divDrawGame = document.getElementById("divDrawGame");

  // Clear current pass targets
  const passTargets = divDrawGame.getElementsByClassName("passTargets");
  if (passTargets) {
    while (passTargets.length > 0) {
      passTargets[0].parentNode.removeChild(passTargets[0]);
    }
  }

  // Add pass targets
  if (ltPassTargets) {
    for (var i = 0; i < ltPassTargets.length; i++) {
      let passTar = ltPassTargets[i];

      let sColor = "yellow";
      if (passTar.bPlayerChoice) {
        sColor = "red";
      }

      // Draw pass target
      let divBallTargetPossible = drawBallTarget("", sColor);
      divBallTargetPossible.className = "passTargets";
      updateBallTargetPos(divBallTargetPossible, passTar.x, passTar.y);
      divBallTargetPossible.style.zIndex = "97";

      // Draw numbers to pass target circle
      let divBallTargetPossibleNb = document.createElement("div");
      //pBallTargetPossibleNb.style.top  = iPix[2] + "px";
      //pBallTargetPossibleNb.style.left = iPix[3] + "px";
      divBallTargetPossibleNb.style.position = "absolute";
      divBallTargetPossibleNb.style.top = "-4px";
      divBallTargetPossibleNb.style.left = divBallTargetPossible.offsetWidth.toString() + "px";
      divBallTargetPossibleNb.style.color = sColor;
      divBallTargetPossibleNb.style.lineHeight = "1";
      divBallTargetPossibleNb.style.fontSize = "12px";
      divBallTargetPossibleNb.innerText = (passTar.z * 100).toFixed(1) + "%";
      divBallTargetPossible.appendChild(divBallTargetPossibleNb);

      // Draw line to pass target
      let iPix = convertPosToPix(x0, y0, passTar.x, passTar.y, divDrawGame, true);
      let divBallTargetPossibleLine = drawLine(iPix[0], iPix[1], iPix[2], iPix[3], sColor, "", 0.5, 96, "solid");
      if (divBallTargetPossibleLine) {
        divBallTargetPossibleLine.className = "passTargets";
        divDrawGame.appendChild(divBallTargetPossibleLine);
      }
    }
  }
}

function drawPlayerTargetPositions(ltPlayer) {
  var divDrawGame = document.getElementById("divDrawGame");

  // Clear current target positions
  const targetPos = divDrawGame.getElementsByClassName("targetPos");
  if (targetPos) {
    while (targetPos.length > 0) {
      targetPos[0].parentNode.removeChild(targetPos[0]);
    }
  }

  // Add pass targets
  for (var i = 0; i < ltPlayer.length; i++) {
    let plTargetPos = ltPlayer[i];

    if (!plTargetPos) {
      continue;
    }

    let curPos = plTargetPos.ptPos;       // Current position
    let tarPos = plTargetPos.ptPosTarget; // Target position

    let sColor = sColorJerseyH[0];
    if (i > 11) {
      sColor = sColorJerseyA[0];
    }

    // Draw pass target
    let divTargetPos = drawBallTarget("", sColor);
    divTargetPos.className = "targetPos";
    updateBallTargetPos(divTargetPos, tarPos.x, tarPos.y);
    divTargetPos.style.zIndex = 1;

    // Draw line to pass target
    let iPix = convertPosToPix(curPos.x, curPos.y, tarPos.x, tarPos.y, divDrawGame, true);
    let divTargetPosLine = drawLine(iPix[0], iPix[1], iPix[2], iPix[3], sColor, "", 0.5, 0, "dashed");
    if (divTargetPosLine) {
      divTargetPosLine.className = "targetPos";
      divDrawGame.appendChild(divTargetPosLine);
    }
  }
}

function drawHalftimeSpeechDialog(fBreak, ltSpeachOptions, divParent) {
  var div0 = document.getElementById("divDialogHalftime");
  if (div0 == null) {
    const option_width = 200;

    div0 = document.createElement("div");
    div0.id = "divDialogHalftime";
    div0.title = "Halbzeitansprache";
    div0.style.width = "100%";

    var div1 = document.createElement("div");
    div1.style.position = "relative";
    div1.style.width = "96%";
    //div1.style.height = "90%";
    //div1.style.height = "400px";
    div1.style.top = "2%";
    div1.style.left = "2%";
    //div1.style.border = "1px solid red";

    var div2 = document.createElement("div");
    div2.style.position = "relative";
    div2.style.width = "100%";
    div2.style.display = "flex";
    var angle_start = -90;
    if (ltSpeachOptions) {
      for (let i = 0; i < ltSpeachOptions.length; i++) {
        const so = ltSpeachOptions[i];
        var div3 = document.createElement("div");
        div3.style.position = "relative";
        div3.style.width = option_width.toString() + "px";
        div3.style.marginRight = "10px";
        //div3.style.float = "left";
        div3.style.flexShrink = "0";
        var bn = document.createElement("button");
        bn.className = "bnSpeach";
        bn.style.width = "100%";
        //bn.style.minWidth = "100px";
        //bn.style.marginRight = "10px";
        bn.style.marginBottom = "10px";
        bn.innerHTML = so.name;
        //bn.innerHTML += "<br> " + (so.chance_moral_boost * 100) + "% für Moral +" + (so.moral_boost * 100).toString() + '%';
        //bn.innerHTML += "<br> " + (so.chance_moral_drop * 100) + "% für Moral " + (so.moral_drop * 100).toString() + '%';
        div3.appendChild(bn);

        var div4 = document.createElement("div");
        div4.style.position = "relative";
        div4.style.width = "100%";
        div4.style.height = option_width.toString() + "px";
        var dataSpeach = [{ y: so.chance_moral_boost * 100, label: "+" + (so.moral_boost * 100).toFixed(1) + '%' }, { y: so.chance_moral_drop * 100, label: (so.moral_drop * 100).toFixed(1) + '%' }, { y: (1 - so.chance_moral_boost - so.chance_moral_drop) * 100, label: "±0" }];
        let chartSpeach = new CanvasJS.Chart(div4, {
          animationEnabled: false,
          culture: "de",
          width: option_width,
          height: option_width,
          //theme: "light2",
          /*
          title: {
            text: "Budget"
          },
          legend: {
            maxWidth: 350,
            itemWidth: 120
          },
          */
          data: [{
            type: "pie",
            startAngle: angle_start,
            toolTipContent: "Moral: {label} (#percent %)",
            indexLabelPlacement: "inside",
            indexLabelFontColor: "white",
            indexLabelFontSize: 16,
            //yValueFormatString: "#,###,###. €",
            indexLabel: "{label}",
            //indexLabel: "{label} {y}",
            dataPoints: dataSpeach
          }]
        });
        chartSpeach.render();

        let divLine = document.createElement("div");
        divLine.style.position = "absolute";
        divLine.style.left = ((option_width / 2) - 1).toString() + "px";
        divLine.style.top = "0px";
        divLine.style.width = "0px";
        divLine.style.height = "20px";
        divLine.style.border = "2px solid black";
        div4.appendChild(divLine);
        div3.appendChild(div4);

        div2.appendChild(div3);
        bn.addEventListener(
          "click",
          function () {
            var bnsSpeach = document.getElementsByClassName("bnSpeach");
            for (var i = 0; i < bnsSpeach.length; i++) bnsSpeach[i].disabled = true;

            const rnd = Math.random();
            //console.log(rnd + ", " + ((360 + angle_start) + (360 * rnd)).toString());

            updateSpeachChart(chartSpeach, (360 + angle_start) + (360 * rnd));

            var moral_change = 0;
            if (rnd < so.chance_moral_drop) { moral_change = so.moral_drop; }
            else if (rnd > 1 - so.chance_moral_boost) { moral_change = so.moral_boost; }
            set_moral_speach(moral_change);
          }
        );
      }
    }
    div1.appendChild(div2);

    var divPb = document.createElement("div");
    divPb.style.position = "relative";
    divPb.style.top = "0px";
    divPb.style.left = "0px";
    divPb.style.width = "100%";
    divPb.style.marginTop = "10px";
    divPb.style.backgroundColor = "white";
    divPb.style.webkitBorderRadius = "3px";
    var divPb1 = document.createElement("div");
    divPb1.className = "myPb";
    divPb.appendChild(divPb1);
    div1.appendChild(divPb);

    div0.appendChild(div1);

    divParent.appendChild(div0);

    $(div0).dialog({
      modal: false,
      autoOpen: true,
      autoResize: true,
      resizable: false,
      width: Math.min(divParent.offsetWidth, (ltSpeachOptions.length * (option_width + 10)) / 0.94),
      //maxWidth: 600,
      height: 'auto',
      open: function (event, ui) {
        //updateSpeachChart(lt_chartSpeach[0], angle);
      }
    });
  }

  var divPb2 = div0.getElementsByClassName("myPb")[0];
  divPb2.style.width = (fBreak * 100).toString() + '%';
  //divPb2.innerHTML = (fBreak * 100).toFixed(0) + '%';

  //console.log(fBreak);
  /*
  if (fBreak < 0) {
    $(div0).dialog('destroy').remove();
  }
  */

  return div0;
}

function updateSpeachChart(chart, angle_stop) {
  chart.options.data[0].startAngle = chart.options.data[0].startAngle + 5;
  chart.render();
  //console.log(chart.options.data[0].startAngle + ", " + angle_stop);

  if (chart.options.data[0].startAngle < angle_stop) {
    setTimeout(
      updateSpeachChart,
      10,
      chart,
      angle_stop
    );
  }
}

function set_moral_speach(moral_change) {
  $.ajax({
    cache: false,
    url: "/ViewGame/SetMoralSpeach",
    //type: "POST",
    //dataType: "JSON",
    data: { fMoralChange: moral_change },
    //contentType: "application/json; charset=utf-8",
    success: function (ret) {
    }
  });
}

async function play(iState) {
  if (bStopPlay) {
    return;
  }

  iState = iState + 1;

  drawGame(iState);

  setTimeout(function () { play(iState); }, 250);
}

async function stop() {
  bStopPlay = true;
}

function playAdmin() {
  if (bAdminStop === true) return;

  var iAdminGameSpeed = document.getElementById("inputAdminGameSpeed").value;

  if (iAdminGameSpeed >= 0) {
    $.ajax({
      url: '/ViewGame/AdminNext',
      type: "GET",
      dataType: "JSON",
      data: { iNextAction: -1 },
      success: function () {
        $.when(drawGame(-4, 0, false)).done(function () {
          setTimeout(function () { playAdmin() }, iAdminGameSpeed);
        });
      }
    });
  }
}

function getPlayerChancesAdmin() {
  $.ajax({
    url: '/ViewGame/AdminGetPlayerChances',
    type: "GET",
    dataType: "JSON",
    success: function (ret) {
      if (ret) {
        drawPlayerChances(ret.fPlAction, -1);
        drawPassTargets(ret.plPos.x, ret.plPos.y, ret.ltPassTargets);
      }
    }
  });
}

function openDialogChances(bChecked) {
  if (bChecked) {
    document.getElementById("divPlActionChart").style.display = "block";
  } else {
    document.getElementById("divPlActionChart").style.display = "none";
  }

  document.getElementById("bShowChances").checked = bChecked;
}

export { drawGame, changeJerseyColors, changeJerseyView, setShowHidePitch, play, stop };
