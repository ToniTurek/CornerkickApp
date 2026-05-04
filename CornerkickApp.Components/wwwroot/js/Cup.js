function changeLandCup(bUpdate) {
  var iLand = $('#ddlLandCup').val();
  var iSeason = $('#ddlSeasonCup').val();

  $.ajax({
    url: '/Member/CupGetDdlMatchdays',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSeason, iLand: iLand },
    success: function (ltMd) {
      $('#ddlMatchdayCup').empty();
      $.each(ltMd, function (i, p) {
        ltS = p.split(';');
        $('#ddlMatchdayCup').append($('<option></option>').val(ltS[0]).html(ltS[1]));
      });

      $.ajax({
        url: '/Member/CupGetMatchday',
        type: "GET",
        dataType: "JSON",
        data: { iSaison: iSeason, iLand: iLand },
        success: function (iMd) {
          document.getElementById("ddlMatchdayCup").value = iMd;
          setCup2(bUpdate);
        }
      });
    }
  });

  setCupEmblem(document.getElementById("imgCupEmblem"), 2, iLand, -1);
}

function setCup2(bUpdate) {
  var iSeason = $('#ddlSeasonCup').val();
  var ddlLand = document.getElementById("ddlLandCup");
  var iMd = $('#ddlMatchdayCup').val();

  $.ajax({
    url: '/Member/setCup',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSeason, iLand: ddlLand.value, iMatchday: iMd },
    success: function (sTable) {
      actionDrawTeams(sTable);
    }
  });

  // Scorer table
  if (dtCupScorer) {
    dtCupScorer.ajax.reload();
  } else {
    dtCupScorer = setTableScorer(document.getElementById("divCupScorer"), 2, ddlLand, null);
  }

  if (bUpdate) {
    setTimeout(function () { setCup2(bUpdate) }, 5000);
  }
}

function actionDrawTeams(sTable) {
  var divDrawCupTeams = $("#tableDivCupTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTable);
  divDrawCupTeams.html(result).show();
}
