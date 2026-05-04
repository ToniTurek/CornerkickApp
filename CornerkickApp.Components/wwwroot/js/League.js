function iniDivision(bUpdate) {
  var iLand = $('#ddlLand').val();

  $.ajax({
    url: '/Member/getDdlDivisions',
    type: "GET",
    dataType: "JSON",
    data: { iLand: iLand },
    success: function (ltDiv) {
      $('#ddlDivision').empty();
      $.each(ltDiv, function (i, p) {
        $('#ddlDivision').append($('<option></option>').val(p[1]).html(p[0]));
      });

      iniLeague(bUpdate);
    }
  });
}

function iniLeague(bUpdate) {
  var iSeason = $('#ddlSeason').val();
  var iLand = $('#ddlLand').val();
  var iDivision = $('#ddlDivision').val();

  var divTableLeague = document.getElementById("divTableLeague");
  if (divTableLeague) {
    divTableLeague.classList.add("disabled");
  }

  $.ajax({
    url: '/Member/getDdlMatchdays',
    type: "GET",
    dataType: "JSON",
    data: { iSeason: iSeason, iLand: iLand, iDivision: iDivision },
    success: function (ltMd) {
      $('#ddlMatchday').empty();
      $.each(ltMd, function (i, p) {
        $('#ddlMatchday').append($('<option></option>').val(p).html(p));
      });

      $.ajax({
        url: '/Member/LeagueGetMatchday',
        type: "GET",
        dataType: "JSON",
        data: { iSeason: iSeason, iLand: iLand, iDivision: iDivision },
        success: function (ret) {
          // Set current matchday
          if (ret.iMd) {
            document.getElementById("ddlMatchday").value = ret.iMd;
          }

          // Draw team of matchday
          drawMatchdayTeam(iLand, iDivision);

          // Draw club place history (if user league)
          var divChartPlaceHistMain = document.getElementById("divChartPlaceHistMain");
          var fct_draw_place_hist = null;
          if (ret.bLeagueUser && ret.iMd > 1) {
            divChartPlaceHistMain.style.display = "block";
            fct_draw_place_hist = function () {
              plotLeaguePlaceGraph(iSeason, (ltMd.length / 2) + 1);
            }
          } else {
            divChartPlaceHistMain.style.display = "none";
          }

          // Draw table
          setLeague2(bUpdate, fct_draw_place_hist);
        }
      });
    }
  });

  setCupEmblem(document.getElementById("imgLeagueEmblem"), 1, iLand, iDivision);
}

function setLeague2(bUpdate, _callback) {
  var ddlSeason = document.getElementById("ddlSeason");
  var ddlLand = document.getElementById("ddlLand");
  var ddlDivision = document.getElementById("ddlDivision");
  var ddlMatchday = document.getElementById("ddlMatchday");
  var rbTableH = document.getElementById("rbTableH");
  var rbTableA = document.getElementById("rbTableA");
  var divTableLeague = document.getElementById("divTableLeague");

  // Teams
  setTeams(
    ddlSeason.value,
    ddlLand.value,
    ddlDivision.value,
    ddlMatchday.value,
    _callback
  );

  // Table
  if (oTableLeague) {
    oTableLeague.ajax.reload(
      function () {
        if (divTableLeague.classList.contains('disabled')) {
          divTableLeague.classList.remove('disabled');
        }
      }
    );
  } else {
    oTableLeague = getTableDatatable(divTableLeague, 1, ddlSeason, ddlLand, ddlDivision, ddlMatchday, null, rbTableH, rbTableA);
  }

  // Scorer table
  if (oTableScorer) {
    oTableScorer.ajax.reload();
  } else {
    oTableScorer = setTableScorer(document.getElementById("divLeagueScorer"), 1, ddlLand, ddlDivision);
  }

  // Keeper table
  if (oTableKeeper) {
    oTableKeeper.ajax.reload();
  } else {
    oTableKeeper = getTableKeeper(document.getElementById("divLeagueKeeper"), 1, ddlLand, ddlDivision);
  }

  if (bUpdate) {
    setTimeout(function () { setLeague2(bUpdate, null) }, 5000);
  }
}

function setTeams(iSeason, iLand, iDivision, iMatchday, _callback) {
  $.ajax({
    url: '/Member/setLeagueTeams',
    type: "GET",
    dataType: "JSON",
    data: { iSeason: iSeason, iLand: iLand, iDivision: iDivision, iMatchday: iMatchday },
    success: function (sTeams) {
      actionDrawTeams(sTeams);

      if (_callback) { _callback(); }
    }
  });
}

function plotLeaguePlaceGraph(iSeason, fLigaMax) {
  fLigaMax = fLigaMax + 0.1;

  $.ajax({
    type: 'post',
    url: '/Member/GetLeaguePlaceHistory',
    dataType: "json",
    data: { iSeason: iSeason },
    success: function (ltPlace) {
      var divChartPlaceHistMain = document.getElementById("divChartPlaceHistMain");

      if (ltPlace.length > 0) {
        divChartPlaceHistMain.style.display = "block";

        var chart = new CanvasJS.Chart("chartContainerPlaceHist", {
          animationEnabled: true,
          theme: "theme2",//theme1
          axisX: {
            labelFontSize: 16,
            lineThickness: 0,
            gridThickness: 1,
            interval: 1,
            minimum: 0.9
          },
          axisY: {
            labelFontSize: 16,
            interval: 1,
            minimum: 0.9,
            maximum: fLigaMax,
            reversed: true
          },
          /*legend: {
            horizontalAlign: "center", // left, center ,right
            verticalAlign: "bottom",  // top, center, bottom
            dockInsidePlotArea: true
          },*/
          data: [
            {
              // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
              type: "line",
              showInLegend: false,
              legendText: "Tabellenplatz",
              dataPoints: ltPlace
            }
          ]
        });

        chart.render();
      } else {
        divChartPlaceHistMain.style.display = "none";
      }
    }
  });
}

function getTableDatatable(parent, iGameType, ddlSeason, ddlLand, ddlDivision, ddlMatchday, ddlGroup, rbTableH, rbTableA, iColor1, iColor2, iColor3, iColor4) {
  parent.innerHTML = '';

  var tbl = document.createElement("table");
  tbl.style.position = "relative";
  tbl.cellPadding = 0;
  tbl.border = 0;
  tbl.className = "display responsive nowrap compact";

  var thead = document.createElement("thead");
  var tr = document.createElement("tr");

  var th0 = document.createElement("th");
  th0.innerText = "id";
  tr.appendChild(th0);
  var th1 = document.createElement("th");
  th1.innerText = "";
  tr.appendChild(th1);
  var th2 = document.createElement("th");
  th2.innerText = "";
  tr.appendChild(th2);
  var th3 = document.createElement("th");
  th3.innerText = "";
  tr.appendChild(th3);
  var th4 = document.createElement("th");
  th4.innerText = "Verein";
  tr.appendChild(th4);
  var th5 = document.createElement("th");
  th5.innerText = "Sp";
  tr.appendChild(th5);
  var th6 = document.createElement("th");
  th6.innerText = "g";
  tr.appendChild(th6);
  var th7 = document.createElement("th");
  th7.innerText = "u";
  tr.appendChild(th7);
  var th8 = document.createElement("th");
  th8.innerText = "v";
  tr.appendChild(th8);
  var th9 = document.createElement("th");
  th9.innerText = "Tore";
  tr.appendChild(th9);
  var th10 = document.createElement("th");
  th10.innerText = "+/-";
  tr.appendChild(th10);
  var th11 = document.createElement("th");
  th11.innerText = "Pkt.";
  tr.appendChild(th11);
  var th12 = document.createElement("th");
  th12.innerText = "bgcl";
  tr.appendChild(th12);
  var th13 = document.createElement("th");
  th13.innerText = "bold";
  tr.appendChild(th13);

  thead.appendChild(tr);
  tbl.appendChild(thead);
  tbl.appendChild(document.createElement("tbody"));
  parent.appendChild(tbl);

  return $(tbl).DataTable({
    "ajax": {
      "url": '/Member/getTableDatatable',
      "type": 'GET',
      "dataType": "JSON",
      "data": function (d) {
        var iLand = -1;
        if (ddlLand) { iLand = ddlLand.value; }

        var iDivision = -1;
        if (ddlDivision) { iDivision = ddlDivision.value; }

        var iGroup = -1;
        if (ddlGroup) { iGroup = ddlGroup.value; }

        var bH = false;
        if (rbTableH) { bH = rbTableH.checked; }

        var bA = false;
        if (rbTableA) { bA = rbTableA.checked; }

        d.iSeason = ddlSeason.value;
        d.iType = iGameType;
        d.iLand = iLand;
        d.iDivision = iDivision;
        d.iMatchday = ddlMatchday.value;
        d.iGroup = iGroup;
        d.bH = bH;
        d.bA = bA;
        d.iColor1 = iColor1;
        d.iColor2 = iColor2;
        d.iColor3 = iColor3;
        d.iColor4 = iColor4;
      },
      "cache": false,
      "contentType": "application/json; charset=utf-8"
    },
    "columns": [
      { "data": "iId" },
      { "data": "iPl" },
      {
        "data": "iPlLast",
        "render": function (iPlLast, type, row) {
          if (iPlLast === 0) {
            return "-";
          }
          return iPlLast;
        }
      },
      { "data": "sEmblem" },
      { "data": "sClubName" },
      { "data": "iGames" },
      { "data": "iWin" },
      { "data": "iDraw" },
      { "data": "iDefeat" },
      { "data": "sGoals" },
      { "data": "iGoalsDiff" },
      { "data": "iPoints" },
      { "data": "sBgColor" },
      { "data": "bBold" }
    ],
    "paging": false,
    "info": false,
    "searching": false,
    "order": [[1, "asc"]],
    "language": {
      "emptyTable": "keine Vereine"
    },
    "columnDefs": [
      {
        "targets": [0, 12, 13],
        "visible": false,
        "orderable": false,
        "searchable": false
      },
      {
        "targets": [2, 9],
        "orderable": false,
        "className": "dt-center"
      },
      {
        "targets": [3, 11],
        "orderable": false,
        "className": "dt-right"
      },
      {
        "targets": [1],
        "className": "dt-right",
      },
      {
        "targets": [5, 6, 7, 8, 10],
        "className": "dt-right",
        "orderSequence": ["desc", "asc"]
      }
    ],
    "fnRowCallback": function (nRow, aData, iDisplayIndex) {
      $('td', nRow).eq(0).css("background-color", aData.sBgColor);

      if (aData.bBold) {
        $('td', nRow).css("font-weight", "bold");
      }

      if (aData.iPlLast > 0) {
        if (aData.iPlLast < aData.iPl) {
          $('td', nRow).eq(1).css("color", "red");
        } else if (aData.iPlLast > aData.iPl) {
          $('td', nRow).eq(1).css("color", "green");
        }
      }
    },
    "initComplete": function (settings, json) {
      if (parent.classList.contains('disabled')) {
        parent.classList.remove('disabled');
      }
    }
  });
}

function actionDrawTeams(sTeams) {
  var divDrawTeams = $("#tableDivTeams");
  divDrawTeams.html('');
  result = drawTeams(sTeams);
  divDrawTeams.html(result).show();
}

function drawTeams(sTeams) {
  var sBox = '';

  if (!sTeams) {
    return sBox;
  }

  if (sTeams.includes('<td>')) {
    sBox += '<h4>Begegnungen</h4>';
    sBox += '<table id="tableTeams" border="0" cellpadding="2" style="width: 100%">';
    sBox += '<tr>';
    sBox += '  <th colspan="1">Anstoß</th>';
    sBox += '  <th style="text-align:right">Heim</th>';
    sBox += '  <th style="text-align:center">&nbsp;</th>';
    sBox += '  <th style="text-align:left">Auswärts</th>';
    sBox += '  <th style="text-align:center">Erg.</th>';
    sBox += sTeams;
    sBox += '</tr>';

    sBox += '</table>';
  } else {
    sBox += '<h4>Teilnehmer</h4>';
    sBox += sTeams;
  }

  return sBox;
}

// DEPRECATED
function drawTable(sTable) {
  var sBox = '';

  if (!sTable) {
    return sBox;
  }

  sBox += "<table id=\"tableLeague\" style=\"width: 100%\" class=\"display responsive nowrap compact\">";
  sBox += "<tr>";
  sBox += "<th colspan=\"2\">Pl.</th>";
  sBox += "<th style=\"text-align:center; width: 3%\">&nbsp;</th>";
  sBox += "<th>Verein</th>";
  sBox += "<th style=\"text-align:right; width: 3%\">&nbsp;</th>";
  sBox += "<th style=\"text-align:right\">Sp.</th>";
  sBox += "<th style=\"text-align:center; width: 3%\">&nbsp;</th>";
  sBox += "<th style=\"text-align:right\">g.</th>";
  sBox += "<th style=\"text-align:right\">u.</th>";
  sBox += "<th style=\"text-align:right\">v.</th>";
  sBox += "<th style=\"text-align:center; width: 3%\">&nbsp;</th>";
  sBox += "<th style=\"text-align:center\">Tore</th>";
  sBox += "<th style=\"text-align:right\">Diff.</th>";
  sBox += "<th style=\"text-align:center; width: 3%\">&nbsp;</th>";
  sBox += "<th style=\"text-align:right\"> Pkte.</th>";
  sBox += "</tr>";
  sBox += sTable;
  sBox += "</table>";

  return sBox;
}

function drawMatchdayTeam(iLand, iDivision) {
  $.ajax({
    cache: false,
    url: "/Member/GetMatchdayTeam",
    type: "GET",
    data: { iCupId: 1, iLand: iLand, iDiv: iDivision },
    success: function (teamData) {
      if (teamData) { // check if data is defined
        var divMatchdayTeamContainer = document.getElementById("divMatchdayTeamContainer");
        if (teamData.ltPlayer2.length > 0) {
          divMatchdayTeamContainer.style.display = "block";

          //var bMobile = $(window).width() < 960;
          $.each(teamData.ltPlayer2, function (i, pl2) {
            if (pl2) {
              divMatchdayTeamContainer.appendChild(getBoxFormationDOM(i, teamData.ptPos[i], pl2.sName, (i + 1).toString(), pl2.sSkillAve, 0, false, -1, pl2.iPos, divMatchdayTeamContainer.offsetWidth, 0.5, pl2.sTeamname, pl2.sAge, pl2.sNat, false, pl2.sPortrait));
            }

            return i !== 11;
          });

          //var txtStatAverage = $("#txtStatAverage");
          //txtStatAverage.html("Durchschnittsstärke (-alter): " + teamData.fTeamAveStrength.toFixed(2) + " (" + teamData.fTeamAveAge.toFixed(1) + ")");
        } else {
          divMatchdayTeamContainer.style.display = "none";
        }
      } else {
        console.log("data hasn't worked!");
      }
    }
  });
}

function changeMatchday(iPrePost, ddlMatchday, exeFunction) {
  if (!ddlMatchday) {
    return;
  }

  var iMdNew = 0;
  if (iPrePost < 0 && parseInt(ddlMatchday.value) > 0) {
    iMdNew = parseInt(ddlMatchday.value) - 1;
  }

  if (iPrePost > 0) {
    iMdNew = parseInt(ddlMatchday.value) + 1;
  } else {
    iMdNew = parseInt(ddlMatchday.value) - 1;
  }

  if (ddlMatchday.innerHTML.indexOf('value="' + iMdNew.toString() + '"') < 0) {
    return;
  }

  ddlMatchday.display = "none";
  ddlMatchday.value = iMdNew.toString();
  ddlMatchday.display = "block";

  exeFunction();
}

function setMatchdayCupInt(iCupId, ddlSeason, ddlMatchday, ddlGroup, divTableContainer, dtCup, lbGroups, divDrawCupTeams, divScorerContainer, bTeamsOnly = false) {
  var iMd = ddlMatchday.value;

  $.ajax({
    url: '/Member/setCupInt',
    type: "GET",
    dataType: "JSON",
    data: { iCupId: iCupId, iSaison: ddlSeason.value, iMatchday: iMd - 1, iGroup: ddlGroup.value },
    success: function (sTeams) {
      divDrawCupTeams.innerHTML = drawTeams(sTeams);
    }
  });

  if (bTeamsOnly) { return null; }

  // Show/hide group ddl
  if (iMd > 0 && iMd < 7) {
    lbGroups.style.display = "inline";

    // Table
    if (dtCup) {
      dtCup.ajax.reload();
    } else {
      dtCup = getTableDatatable(divTableContainer, iCupId, ddlSeason, null, null, ddlMatchday, ddlGroup, null, null, 2, 0, 0, 0);
    }
  } else {
    lbGroups.style.display = "none";
  }
  divTableContainer.style.display = lbGroups.style.display;

  // Scorer table
  var dtCupScorer = setTableScorer(divScorerContainer, iCupId, null, null);

  return dtCup;
}

function setCupEmblem(img, iCupId, iCupId2, iCupId3) {
  if (img) {
    $.ajax({
      url: '/Member/GetCupEmblemAjax',
      type: "GET",
      dataType: "JSON",
      data: { iCupId: iCupId, iCupId2: iCupId2, iCupId3: iCupId3, iImgWidth: img.parentNode.offsetWidth },
      success: function (sCupEmblem) {
        img.src = sCupEmblem;
      }
    });
  }
}

function getSelectClubs(sLeagueId, sctClubs, bExcludeUserClubs, _callbackFnc) {
  if (sctClubs) {
    // Clear options from select
    var i, L = sctClubs.options.length - 1;
    for (i = L; i >= 0; i--) {
      sctClubs.remove(i);
    }
  } else {
    sctClubs = document.createElement("select");
  }

  if (sLeagueId) {
    $.ajax({
      url: '/Member/GetClubsSelectOptions',
      type: "GET",
      dataType: "JSON",
      data: { sLeagueId: sLeagueId, bExcludeUserClubs: bExcludeUserClubs },
      success: function (ltClubs) {
        // Clear options
        $.each(ltClubs, function (i, p) {
          var optClub = document.createElement("option");
          optClub.value = p[0];
          optClub.text = p[1];
          sctClubs.appendChild(optClub);
        });

        sctClubs.disabled = ltClubs.length < 2;

        if (_callbackFnc) {
          _callbackFnc();
        }
      }
    });
  } else {
    sctClubs.disabled = true;

    if (_callbackFnc) {
      _callbackFnc();
    }

    return sctClubs;
  }
}
