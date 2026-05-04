// Type: -1 - skill, 0 - Condi, 1 - Fresh, 2 - Moral, 99 - delta
function getColorFromValue(iType, f) {
  if (iType == 0) {
    if (f > 0.9) return "green";
    if (f > 0.7) return "YellowGreen";
    if (f > 0.5) return "orange";
    return "red";
  } else if (iType == 1) {
    if (f > 0.95) return "green";
    if (f > 0.90) return "YellowGreen";
    if (f > 0.80) return "orange";
    return "red";
  } else if (iType == 2) {
    if (f > 1.1) return "green";
    if (f > 0.95) return "YellowGreen";
    if (f > 0.90) return "orange";
    return "red";
  } else if (iType == 99) {
    if (f >  0.05) return "green";
    if (f >  0.00) return "YellowGreen";
    if (f > -0.00001) return "black";
    if (f > -0.05) return "orange";
    return "red";
  } else if (iType == -1) {
    if (f > 12.5) return "#ff00ff"; // magenta
    if (f > 10.5) return "#ffc0cb"; // pink
    if (f >  8.5) return "#2cba00"; // dark-green
    if (f >  7.5) return "#00ff00"; // green
    if (f >  6.5) return "#a3ff00"; // yellow-green
    if (f >  5.5) return "#fff400"; // yellow
    if (f >  4.5) return "#ffa700"; // orange
    if (f >  3.5) return "#ff0000"; // red
    return "#c80000"; // dark-red
  }

  return "black";
}

window.getNatIcon = (sNat, sStyle) => {
  var sIcon = '<img src="' + sContentDir + '/Icons/flags/';

  if (sNat) {
    sIcon += sNat + '.png" title="' + sNat;
  } else {
    sIcon += '0.png" title="unknown';
  }

  sIcon += '" style="';

  if (sStyle) {
    sIcon += sStyle;
  } else {
    sIcon += 'width: 16px';
  }

  sIcon += '"/>';

  return sIcon;
}

function getFormIcon(sForm) {
  if (!sForm) return 'o';

  sForm = sForm.trim();

  var sIcon = '<img src="/Content/Icons/';
  if        (sForm === '---') {
    sIcon += 'form0';
  } else if (sForm ===  '-')  {
    sIcon += 'form1';
  } else if (sForm ===  'o')  {
    sIcon += 'form2';
  } else if (sForm ===  '+')  {
    sIcon += 'form3';
  } else if (sForm === '+++') {
    sIcon += 'form4';
  } else if (sForm === 'verl') {
    sIcon += 'ambulance';
  } else if (sForm === 'ang.') {
    sIcon += 'ambulance2';
  }

  sIcon += '.png" title="' + sForm + '" style="width: 16px"/>';

  return sIcon;
}

function showPassChart(fMaxPassLengthHigh, fMaxPassLengthLow, fFootR, fFootL, fTecnic) {
  const iBorderWidth = 4;
  const iCanvasWidth = 600;
  const iCanvasHeight = 600;

  var div0 = document.createElement("div");
  div0.id = "dlgPassChart";
  div0.title = "Pass-/Flankenweite";
  div0.style.backgroundColor = "lightgrey";

  var div1 = document.createElement("div");
  div1.style.width = (iCanvasWidth + iBorderWidth).toString() + "px";
  div1.style.height = (iCanvasHeight + iBorderWidth).toString() + "px";
  div1.style.backgroundColor = "white";
  div1.style.borderStyle = "ridge";
  div1.style.borderWidth = iBorderWidth.toString() + "px";
  div1.style.borderColor = "lightgray";

  const canvas = document.createElement('canvas');
  canvas.width = iCanvasWidth;
  canvas.height = iCanvasHeight;

  var divRb = document.createElement("div");
  divRb.style.position = "absolute";

  let lbRbHigh = document.createElement("label");
  lbRbHigh.innerText = "Flanke";
  lbRbHigh.style.margin = "4px";
  let rbHigh = document.createElement("input");
  rbHigh.type = "radio";
  rbHigh.name = "passType";
  rbHigh.checked = true;
  rbHigh.style.marginLeft = "4px";
  rbHigh.addEventListener("change", function () {
    if (this.checked) {
      drawPassChart(canvas, fMaxPassLengthHigh, fFootR, fFootL, fTecnic);
    }
  });
  lbRbHigh.appendChild(rbHigh);
  divRb.appendChild(lbRbHigh);

  let lbRbLow = document.createElement("label");
  lbRbLow.innerText = "Pass";
  lbRbLow.style.margin = "4px";
  var rbLow = document.createElement("input");
  rbLow.type = "radio";
  rbLow.name = "passType";
  rbLow.style.marginLeft = "4px";
  rbLow.addEventListener("change", function () {
    if (this.checked) {
      drawPassChart(canvas, fMaxPassLengthLow, fFootR, fFootL, fTecnic);
    }
  });
  lbRbLow.appendChild(rbLow);
  divRb.appendChild(lbRbLow);
  div1.appendChild(divRb);

  drawPassChart(canvas, fMaxPassLengthHigh, fFootR, fFootL, fTecnic);

  div1.appendChild(canvas);
  div0.appendChild(div1);

  document.body.appendChild(div0);

  $(div0).dialog({
    autoOpen: true,
    resize: function (event, ui) {
      div1.style.width = (div0.offsetWidth - 20) + "px";
      div1.style.height = (div0.offsetHeight - 30) + "px";
    },
    width: 'auto'
  });
}

function drawPassChart(canvas, fMaxPassLength, fFootR, fFootL, fTecnic) {
  const iCanvasWidth = canvas.width;
  const iCanvasHeight = canvas.height;
  const iMarginY = 20;

  const fRotAngle = 20 * (Math.PI / 180);

  const fMaxPassLengthPix = (iCanvasHeight * (fMaxPassLength / 122)) - iMarginY;
  //console.log(canvas.offsetWidth + ", " + canvas.height);

  const ctx = canvas.getContext('2d');
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  // Right foot
  ctx.fillStyle = 'red';
  ctx.beginPath();
  //console.log(Math.sin(-fRotAngle) + ", " + Math.sin(-fRotAngle) * fMaxPassLengthPix * fFootR);
  ctx.ellipse(
    (iCanvasWidth / 2) + (Math.sin(-fRotAngle) * fMaxPassLengthPix * fFootR),
    (iCanvasHeight - (fMaxPassLengthPix * fFootR) - (1.5 * iMarginY)) + (fMaxPassLengthPix * fFootR - (Math.cos(-fRotAngle) * fMaxPassLengthPix * fFootR)),
    (fMaxPassLengthPix + (iMarginY / 2)) * fFootR * fTecnic,
    (fMaxPassLengthPix + (iMarginY / 2)) * fFootR,
    -fRotAngle,
    0,
    Math.PI * 2
  );
  ctx.fillStyle = 'rgba(255, 180, 180)';
  ctx.fill();

  // Left foot
  ctx.fillStyle = 'red';
  ctx.beginPath();
  ctx.ellipse(
    (iCanvasWidth / 2) + (Math.sin(+fRotAngle) * fMaxPassLengthPix * fFootL),
    (iCanvasHeight - (fMaxPassLengthPix * fFootL) - (1.5 * iMarginY)) + (fMaxPassLengthPix * fFootL - (Math.cos(+fRotAngle) * fMaxPassLengthPix * fFootL)),
    (fMaxPassLengthPix + (iMarginY / 2)) * fFootL * fTecnic,
    (fMaxPassLengthPix + (iMarginY / 2)) * fFootL,
    +fRotAngle,
    0,
    Math.PI * 2
  );
  ctx.fillStyle = 'rgba(255, 180, 180)';
  ctx.fill();

  // Right foot line
  ctx.fillStyle = 'red';
  ctx.beginPath();
  //console.log(Math.sin(-fRotAngle) + ", " + Math.sin(-fRotAngle) * fMaxPassLengthPix * fFootR);
  ctx.ellipse(
    (iCanvasWidth / 2) + (Math.sin(-fRotAngle) * fMaxPassLengthPix * fFootR),
    (iCanvasHeight - (fMaxPassLengthPix * fFootR) - (1.5 * iMarginY)) + (fMaxPassLengthPix * fFootR - (Math.cos(-fRotAngle) * fMaxPassLengthPix * fFootR)),
    (fMaxPassLengthPix + (iMarginY / 2)) * fFootR * fTecnic,
    (fMaxPassLengthPix + (iMarginY / 2)) * fFootR,
    -fRotAngle,
    0,
    Math.PI * 2
  );
  ctx.lineWidth = 2;
  ctx.strokeStyle = 'red';
  ctx.stroke();

  // Left foot line
  ctx.fillStyle = 'red';
  ctx.beginPath();
  ctx.ellipse(
    (iCanvasWidth / 2) + (Math.sin(+fRotAngle) * fMaxPassLengthPix * fFootL),
    (iCanvasHeight - (fMaxPassLengthPix * fFootL) - (1.5 * iMarginY)) + (fMaxPassLengthPix * fFootL - (Math.cos(+fRotAngle) * fMaxPassLengthPix * fFootL)),
    (fMaxPassLengthPix + (iMarginY / 2)) * fFootL * fTecnic,
    (fMaxPassLengthPix + (iMarginY / 2)) * fFootL,
    +fRotAngle,
    0,
    Math.PI * 2
  );
  ctx.lineWidth = 2;
  ctx.strokeStyle = 'red';
  ctx.stroke();

  ctx.beginPath();
  ctx.ellipse(
    iCanvasWidth / 2,
    iCanvasHeight - (fMaxPassLengthPix * fFootR) - (1.5 * iMarginY),
    100,
    (fMaxPassLengthPix + (iMarginY / 2)) * fFootR,
    0,
    0,
    Math.PI * 2);
  ctx.lineWidth = 1;
  ctx.strokeStyle = 'blue';
  //ctx.stroke();

  for (var iR = 10; iR < 100; iR += 10) {
    ctx.beginPath();
    ctx.arc(iCanvasWidth / 2, iCanvasHeight - iMarginY, (iCanvasHeight * (iR / 122) * 2) - iMarginY, 0, 2 * Math.PI, false);
    ctx.lineWidth = 1;
    ctx.setLineDash([8, 12]);
    ctx.strokeStyle = 'black';
    ctx.fillStyle = 'black';
    ctx.stroke();
    ctx.font = "14px Arial";
    ctx.fillText(iR.toString() + "m", (iCanvasWidth / 2) + 4, (iCanvasHeight - iMarginY) - ((iCanvasHeight * (iR / 122) * 2) - iMarginY) - 4);
  }

  // Axis
  ctx.beginPath();
  ctx.moveTo(0, iCanvasHeight - iMarginY);
  ctx.lineTo(iCanvasWidth, iCanvasHeight - iMarginY);
  ctx.setLineDash([]);
  ctx.stroke();

  ctx.beginPath();
  ctx.moveTo(iCanvasWidth / 2, 0);
  ctx.lineTo(iCanvasWidth / 2, iCanvasHeight);
  ctx.setLineDash([]);
  ctx.stroke();

  ctx.beginPath();
  ctx.arc(iCanvasWidth / 2, iCanvasHeight - iMarginY, 6, 0, 2 * Math.PI, false);
  ctx.fillStyle = 'black';
  ctx.fill();
}

function getContractColor(f) {
  var sColor = "green";
  if (f < 0.5) { sColor = "red"; }
  else if (f < 0.75) { sColor = "orange"; }
  return sColor;
}

function setIndTraining(iPlayerId, iInd, _callback_fct) {
  $.ajax({
    url: '/Member/setPlayerIndTraining',
    dataType: "JSON",
    data: { iPlayerId: iPlayerId, iIndTr: iInd },
    success: function (ret) {
      if (_callback_fct) {
        _callback_fct();
      }
    }
  });
}

function setDopingDesc(iDpIx) {
  $.ajax({
    url: '/Member/PlayerDetailsGetDopingDesc',
    type: "GET",
    dataType: "JSON",
    data: { iDp: iDpIx },
    success: function (ret) {
      var tblDopingDesc = document.getElementById("tblDopingDesc");
      tblDopingDesc.innerHTML = '';

      var row1 = tblDopingDesc.insertRow();
      //var test = document.createElement("div");
      row1.style.whiteSpace = "nowrap";
      var cell11 = row1.insertCell();
      cell11.style.paddingRight = "4px";
      cell11.style.textAlign = "right";
      cell11.innerText = "Steigerung max. Kondition:";
      var cell12 = row1.insertCell();
      cell12.style.paddingRight = "4px";
      cell12.style.textAlign = "right";
      cell12.innerText = ret.effect_max;

      var row2 = tblDopingDesc.insertRow();
      row2.style.whiteSpace = "nowrap";
      var cell21 = row2.insertCell();
      cell21.style.paddingRight = "4px";
      cell21.style.textAlign = "right";
      cell21.innerText = "Einmaliger Frischegewinn:";
      var cell22 = row2.insertCell();
      cell22.style.paddingRight = "4px";
      cell22.style.textAlign = "right";
      cell22.innerText = ret.fresh_gain;

      var row3 = tblDopingDesc.insertRow();
      row3.style.whiteSpace = "nowrap";
      var cell31 = row3.insertCell();
      cell31.style.paddingRight = "4px";
      cell31.style.textAlign = "right";
      cell31.innerText = "Reduktionsrate / d (HWZ):";
      var cell32 = row3.insertCell();
      cell32.style.paddingRight = "4px";
      cell32.style.textAlign = "right";
      cell32.innerText = ret.reduction + " (" + ret.hwz + ")";

      var row4 = tblDopingDesc.insertRow();
      row4.style.whiteSpace = "nowrap";
      var cell41 = row4.insertCell();
      cell41.style.paddingRight = "4px";
      cell41.style.textAlign = "right";
      cell41.innerText = "Max. Detektionsrisiko:";
      var cell42 = row4.insertCell();
      cell42.style.paddingRight = "4px";
      cell42.style.textAlign = "right";
      cell42.innerText = ret.detect;

      var row5 = tblDopingDesc.insertRow();
      row5.style.whiteSpace = "nowrap";
      var cell51 = row5.insertCell();
      cell51.style.paddingRight = "4px";
      cell51.style.textAlign = "right";
      cell51.innerText = "Kosten:";
      var cell52 = row5.insertCell();
      cell52.style.paddingRight = "4px";
      cell52.style.textAlign = "right";
      cell52.innerText = ret.cost;
    }
  });
}

function getPlayerTrainingHistoryData(iPlayerId, _callback) {
  return $.ajax({
    url: "/Member/GetPlayerTrainingHistotyData",
    dataType: "JSON",
    data: { iPlayerId: iPlayerId },
    success: function (data) {
      if (_callback) {
        _callback(data);
      }
    },
    error: function (xhr) {
      debugger;
      getDialog(document.body, xhr.status + ': ' + xhr.statusText + ", " + xhr.responseText, "Fehler");
    }
  });
}

function addTrainingHistoryTable(divContainer, data, _callback) {
  var div1 = document.createElement("div");
  div1.style.position = "relative";
  div1.style.width = "100%";
  div1.style.height = "auto";

  var tbl = document.createElement("table");
  tbl.className = "table table-bordered compact";
  //tbl.cellPadding = "1";
  tbl.style.whiteSpace = "nowrap";

  var tblHd = tbl.createTHead();
  var rowHdr = tblHd.insertRow(0);
  rowHdr.style.fontWeight = 'bold';
  var cellHdr1 = rowHdr.insertCell();
  cellHdr1.style.textAlign = "center";
  cellHdr1.innerHTML = "Datum";
  var cellHdr2 = rowHdr.insertCell();
  cellHdr2.style.textAlign = "center";
  cellHdr2.innerHTML = "Training";
  var cellHdr3 = rowHdr.insertCell();
  cellHdr3.style.textAlign = "center";
  cellHdr3.colSpan = 2;
  cellHdr3.innerHTML = "Kondi. [%]";
  var cellHdr4 = rowHdr.insertCell();
  cellHdr4.style.textAlign = "center";
  cellHdr4.colSpan = 2;
  cellHdr4.innerHTML = "Frische [%]";
  var cellHdr5 = rowHdr.insertCell();
  cellHdr5.style.textAlign = "center";
  cellHdr5.colSpan = 2;
  cellHdr5.innerHTML = "Moral [%]";

  var tblBd = tbl.createTBody();
  for (var i = data[0].length - 1; i >= 0; i--) {
    var tr = tblBd.insertRow();
    tr.style.fontSize = "95%";
    //tr.style.paddingLeft = "4px";
    //tr.style.paddingRight = "4px";

    const date = new Date(data[0][i].x);

    var cell0 = tr.insertCell();
    cell0.style.display = "none";
    cell0.innerText = data[0][i].x;

    var cell1 = tr.insertCell();
    cell1.style.textAlign = "center";
    cell1.innerHTML = date.toLocaleDateString([], { month: '2-digit', day: '2-digit' }) + " " + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

    var cell2 = tr.insertCell();
    cell2.style.textAlign = "center";
    cell2.innerHTML = data[0][i].z;

    var dy0 = dy1 = dy2 = 0;
    if (i > 0) {
      dy0 = data[0][i].y - data[0][i - 1].y;
      dy1 = data[1][i].y - data[1][i - 1].y;
      dy2 = data[2][i].y - data[2][i - 1].y;
    }

    var cell31 = tr.insertCell();
    cell31.style.textAlign = "right";
    cell31.style.color = getColorFromValue(0, data[0][i].y);
    cell31.innerHTML = (data[0][i].y * 100).toFixed(2);
    var cell32 = tr.insertCell();
    cell32.style.textAlign = "right";
    cell32.style.color = getColorFromValue(99, dy0);
    cell32.innerHTML = (dy0 > 0 ? "+" : "") + (dy0 * 100).toFixed(2);

    var cell41 = tr.insertCell();
    cell41.style.textAlign = "right";
    cell41.style.color = getColorFromValue(1, data[1][i].y);
    cell41.innerHTML = (data[1][i].y * 100).toFixed(2);
    var cell42 = tr.insertCell();
    cell42.style.textAlign = "right";
    cell42.style.color = getColorFromValue(99, dy1);
    cell42.innerHTML = (dy1 > 0 ? "+" : "") + (dy1 * 100).toFixed(2);

    var cell51 = tr.insertCell();
    cell51.style.textAlign = "right";
    cell51.style.color = getColorFromValue(2, data[2][i].y);
    cell51.innerHTML = (data[2][i].y * 100).toFixed(2);
    var cell52 = tr.insertCell();
    cell52.style.textAlign = "right";
    cell52.style.color = getColorFromValue(99, dy2);
    cell52.innerHTML = (dy2 > 0 ? "+" : "") + (dy2 * 100).toFixed(2);
  }
  div1.appendChild(tbl);
  divContainer.appendChild(div1);

  if (_callback) {
    _callback();
  }

  return div1;
}

function printAveSkill(iPlayerId) {
  $.ajax({
    url: "/Member/PlayerDetailsGetAveSkill",
    dataType: "JSON",
    data: { iPlayerId: iPlayerId },
    success: function (fAveSkill) {
      var sAveSkill = fAveSkill[0] > 0 ? fAveSkill[0].toFixed(1) : "?"
      var sAveSkillIdeal = fAveSkill[1] > 0 ? fAveSkill[1].toFixed(1) : "?"
      document.getElementById("txtAveSkill").innerText = sAveSkill;
      document.getElementById("tdAveSkill").innerText = sAveSkill + " (" + sAveSkillIdeal + ")";
    },
    error: function (xhr) {
      debugger;
      getDialog(document.body, xhr.status + ': ' + xhr.statusText, "Fehler");
    }
  });
}
