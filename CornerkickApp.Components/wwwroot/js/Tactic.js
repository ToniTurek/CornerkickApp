function getValueString(sSliderValue) {
  var sValue = "";
  if (parseInt(sSliderValue) > 0) sValue += '+';
  else if (parseInt(sSliderValue) == 0) sValue += ' ';

  sValue += sSliderValue.toString() + "%";

  return sValue;
}

function updateSliderLabels() {
  var divSliderContainers = document.getElementsByClassName("divSliderContainer");

  for (var i = 0; i < divSliderContainers.length; i++) {
    var divSliderContainer = divSliderContainers[i];

    var iptSliders = divSliderContainer.getElementsByTagName("input");
    if (iptSliders && iptSliders.length > 0) {
      updateSliderLabel(iptSliders[0]);
    }
  }
}

function updateSliderLabel(iptSlider) {
  var lbSliders = iptSlider.parentElement.getElementsByTagName("span");
  if (lbSliders && lbSliders.length > 0) {
    lbSliders[0].innerHTML = iptSlider.value;
  }
}

function setTacticToSliders(tactic) {
  document.getElementById("sldOrientation").value = (tactic.fOrientation * 100).toFixed(0);
  document.getElementById("sldGapOffence").value = tactic.iGapOffsite.toString();
  document.getElementById("sldPower").value = (tactic.fPower * 100).toFixed(0);
  document.getElementById("sldShootDist").value = ((tactic.fShootFreq * -1) * 100).toFixed(0);
  document.getElementById("sldDuel").value = (tactic.fAggressive * 100).toFixed(0);
  document.getElementById("sldPassRisk").value = (tactic.fPassRisk * 100).toFixed(0);
  document.getElementById("sldPassLength").value = (tactic.fPassLength * 100).toFixed(0);
  document.getElementById("sldPassFreq").value = (tactic.fPassFreq * 100).toFixed(0);
  document.getElementById("sldPassLeft").value = ((tactic.fPassLeft + 1) * 100).toFixed(0);
  document.getElementById("sldPassRight").value = ((tactic.fPassRight + 1) * 100).toFixed(0);
  document.getElementById("sldPassMid").value = ((1 - (tactic.fPassLeft + tactic.fPassRight)) * 100).toFixed(0);
  document.getElementById("cbTcOffsite").checked = tactic.bOffsite;

  updateSliderLabels();
}
