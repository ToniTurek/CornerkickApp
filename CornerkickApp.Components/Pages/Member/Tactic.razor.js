export function init(bGame, iBrowserWidth, _tt) {
  const tt = JSON.parse(_tt);

  setView(iBrowserWidth);

  // Show tutorial
  if (tt && tt.bShow) {
    if (parseInt(tt.iLevel / 10) === iTtMainLevelCalendar) { tt.iLevel = iTtMainLevelTactic * 10; }

    if (parseInt(tt.iLevel / 10) === iTtMainLevelTactic) {
      drawTutorial(
        document.getElementById("divBody"),
        tt.iLevel,
        tt,
        [
          new Tt_highlight((iTtMainLevelTactic * 10) + 0, document.getElementById("divTcOrientation")),
          new Tt_highlight((iTtMainLevelTactic * 10) + 0, document.getElementById("divTcPower")),
          new Tt_highlight((iTtMainLevelTactic * 10) + 1, document.getElementById("divTcPower")),
          new Tt_highlight((iTtMainLevelTactic * 10) + 2, document.getElementById("divTcSubs")),
          new Tt_highlight((iTtMainLevelTactic * 10) + 3, document.getElementById("navbarDropdownClub")),
          new Tt_highlight((iTtMainLevelTactic * 10) + 3, document.getElementById("aMenuClubSponsor"))
        ]
      );
    }
  }
}

function setView(iBrowserWidth) {
  var ddlSystem = document.getElementById("ddlSystem");
  var divTcOrientation = document.getElementById("divTcOrientation");
  var divTcGapOffence = document.getElementById("divTcGapOffence");
  var divTcShootDist = document.getElementById("divTcShootDist");
  var divTcPassLeft = document.getElementById("divTcPassLeft");
  var divTcPassRight = document.getElementById("divTcPassRight");
  var divTcPassMid = document.getElementById("divTcPassMid");
  var divTcPower = document.getElementById("divTcPower");
  var divTcDuel = document.getElementById("divTcDuel");
  var divTcPassLength = document.getElementById("divTcPassLength");
  var divTcPassRisk = document.getElementById("divTcPassRisk");
  var divTcPassFreq = document.getElementById("divTcPassFreq");
  var divTcOffsite = document.getElementById("divTcOffsite");
  var divTcSubs = document.getElementById("divTcSubs");
  var imgTacticBoard = document.getElementById("imgTacticBoard");

  var divLabels = document.getElementsByClassName("labels");

  if (iBrowserWidth < 600) {
    if (ddlSystem) {
      ddlSystem.classList.remove("form-control");
      ddlSystem.style.width = "100%";
    }

    divTcOrientation.style.top = "40px";
    divTcGapOffence.style.top = "40px";
    divTcPassLeft.style.top = "40px";
    divTcPassRight.style.top = "40px";
    divTcPassMid.style.top = "40px";
    divTcShootDist.style.top = "40px";
    divTcShootDist.style.left = "64%";
    divTcShootDist.style.transform = "rotate(0deg)";

    divTcPower.style.top = "360px";
    divTcDuel.style.top = "402px";
    divTcPassLength.style.top = "444px";
    divTcPassRisk.style.top = "486px";
    divTcPassFreq.style.top = "528px";
    divTcOffsite.style.top = "570px";

    //divTcPower.style.left = "10px";
    //divTcDuel.style.left = "10px";
    divTcPassLength.style.left = "5%";
    divTcPassRisk.style.left = "5%";
    divTcPassFreq.style.left = "5%";

    divTcPower.style.width = "90%";
    divTcDuel.style.width = "90%";
    divTcPassLength.style.width = "90%";
    divTcPassRisk.style.width = "90%";
    divTcPassFreq.style.width = "90%";

    divTcSubs.style.top = "630px";
    divTcSubs.style.left = "10px";

    // Show labels
    for (var i = 0; i < divLabels.length; i++) {
      divLabels[i].style.display = "table";
    }

    imgTacticBoard.src = sContentDir + "/Images/tactic_clean.png";
  } else {
    if (ddlSystem) {
      ddlSystem.classList.add("form-control");
      ddlSystem.style.width = "120px";
    }

    divTcOrientation.style.top = "11%";
    divTcGapOffence.style.top = "20%";
    divTcShootDist.style.top = "2%";
    divTcShootDist.style.left = "58%";
    divTcShootDist.style.transform = "rotate(-23deg)";
    divTcPassLeft.style.top = "13%";
    divTcPassRight.style.top = "13%";
    divTcPassMid.style.top = "13%";

    divTcPower.style.top = "53%";
    divTcDuel.style.top = "60%";
    divTcPassLength.style.top = "53%";
    divTcPassRisk.style.top = "60%";
    divTcPassFreq.style.top = "67%";

    //divTcPower.style.left = "5%";
    //divTcDuel.style.left = "5%";
    divTcPassLength.style.left = "55%";
    divTcPassRisk.style.left = "55%";
    divTcPassFreq.style.left = "55%";

    divTcPower.style.width = "40%";
    divTcDuel.style.width = "40%";
    divTcPassLength.style.width = "40%";
    divTcPassRisk.style.width = "40%";
    divTcPassFreq.style.width = "40%";

    divTcOffsite.style.top = "67%";

    divTcSubs.style.top = "75%";
    divTcSubs.style.left = "20%";

    // Hide labels
    for (var i = 0; i < divLabels.length; i++) {
      divLabels[i].style.display = "none";
    }

    imgTacticBoard.src = sContentDir + "/Images/tactic.png";
  }

  if (ddlSystem) {
    ddlSystem.style.minWidth = ddlSystem.style.width;
  }
}
