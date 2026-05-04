function plotTrainingDevelopment(sDiv, bAnimate, bExpected, iTrainingsCamp, bLight, date_min, iWeek) {
  var chartContainer = document.getElementById(sDiv);
  if (!chartContainer) { return; }

  /*
  var selectCamp = document.getElementById("selectCamp");
  if (bExpected) {
    selectCamp.style.visibility = "visible";
  } else {
    selectCamp.style.visibility = "hidden";
  }
  */

  Date.prototype.addDays = function (days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
  }

  var date_max = null;
  var lt_night = [];
  if (!bLight && date_min) {
    date_min.setHours(0, 0, 0, 0);
    date_max = date_min.addDays(7);

    var night_start = date_min;
    night_start.setHours(23, 0, 0, 0);
    var night_end = date_min.addDays(1);
    night_end.setHours(7, 0, 0, 0);
    for (var iN = 0; iN < 7; iN++) {
      lt_night.push({ x: night_start.addDays(iN).getTime(), y: -1 });
      lt_night.push({ x: night_start.addDays(iN).getTime(), y: 2 });
      lt_night.push({ x: night_end.addDays(iN).getTime(), y: 2 });
      lt_night.push({ x: night_end.addDays(iN).getTime(), y: -1 });
    }
  } else {
    date_min = null;
  }

  var sDateFormat = "DDD DD MMM";
  var interval = 0;
  if (bLight) {
    sDateFormat = "DD.MM.";
    interval = 0.1;
  }

  /*
  return $.ajax({
    type: 'post',
    url: '/Member/GetTeamDevelopmentData',
    dataType: "json",
    data: { bExpected: bExpected, iTrainingsCamp: iTrainingsCamp, iWeek: iWeek },
    success: function (dataKFM) {
    */
      if (dataKFM && dataKFM[0][0]) {
        chartContainer.style.display = "block";

        var chart = new CanvasJS.Chart(sDiv, {
          animationEnabled: bAnimate,
          theme: "theme2",//theme1
          toolTip: {
            shared: true,
            borderColor: "black",
            contentFormatter: function (e) {
              var content = "";

              // Date
              var dateOptions = { day: 'numeric', weekday: 'short', month: 'numeric', hour: "numeric", minute: "2-digit" };
              var d = new Date();
              d.setTime(e.entries[0].dataPoint.x);
              content += "<div style=\"width: 100%; text-align: center\"><u>" + d.toLocaleString(undefined, dateOptions) + "</u></div>";

              // CFM
              if (e.entries[0].dataPoint.z) {
                content += "<table>";
                // For each type
                for (var i = 0; i < e.entries.length; i++) {
                  content += "<tr><td style=\"text-align:right; color:" + e.entries[i].dataSeries.color + "\">" + e.entries[i].dataSeries.name + ":</td><td style=\"text-align:right\">" + (e.entries[i].dataPoint.y * 100).toFixed(1) + "%</td>";
                }
                content += "</table>";

                // Training type
                content += "<div style=\"width: 100%; text-align: center\">" + e.entries[0].dataPoint.z + "</div>";
              } else {
                content += "<div style=\"width: 100%; text-align: center; color:" + e.entries[0].dataSeries.color + "\">" + e.entries[0].dataSeries.name + "</div>";
              }

              return content;
            }
          },
          axisX: {
            gridThickness: 1,
            interval: 1,
            minimum: date_min,
            maximum: date_max,
            intervalType: "day",
            valueFormatString: sDateFormat,
            labelAngle: -20
          },
          axisY: {
            interval: interval,
            valueFormatString: "0%",
            includeZero: false
          },
          axisY2: {
            minimum: 0,
            maximum: 1,
            gridThickness: 0,
            tickLength: 0,
            lineThickness: 0,
            labelFormatter: function () {
              return " ";
            }
          },
          legend: {
            horizontalAlign: "center", // left, center ,right
            verticalAlign: "bottom",  // top, center, bottom
            dockInsidePlotArea: true
          },
          data: [
            {
              type: "area",
              axisYType: "secondary",
              color: "rgb(0, 0, 140)",
              fillOpacity: 0.2,
              markerSize: 0,
              lineThickness: 0,
              showInLegend: false,
              name: "Nacht",
              xValueType: "dateTime",
              xValueFormatString: "DD MMM HH:mm",
              dataPoints: lt_night
            },
            {
              // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
              type: "line",
              color: "red",
              showInLegend: bLight,
              legendText: "Kondition",
              name: "Kondition",
              xValueType: "dateTime",
              xValueFormatString: "DD MMM HH:mm",
              yValueFormatString: "0.0%",
              dataPoints: dataKFM[0][0]
            },
            {
              type: "line",
              color: "#00BFFF",
              showInLegend: bLight,
              legendText: "Frische",
              name: "Frische",
              xValueType: "dateTime",
              xValueFormatString: "DD MMM HH:mm",
              yValueFormatString: "0.0%",
              dataPoints: dataKFM[0][1]
            },
            {
              type: "line",
              color: "gold",
              showInLegend: bLight,
              legendText: "Moral",
              name: "Moral",
              xValueType: "dateTime",
              xValueFormatString: "DD MMM HH:mm",
              yValueFormatString: "0.0%",
              dataPoints: dataKFM[0][2]
            },
            {
              type: "line",
              color: "red",
              lineDashType: "dash",
              lineThickness: 1,
              showInLegend: false,
              name: "Kondition",
              xValueType: "dateTime",
              xValueFormatString: "DD MMM HH:mm",
              yValueFormatString: "0.0%",
              dataPoints: dataKFM[1][0]
            },
            {
              type: "line",
              color: "#00BFFF",
              lineDashType: "dash",
              lineThickness: 1,
              showInLegend: false,
              name: "Frische",
              xValueType: "dateTime",
              xValueFormatString: "DD MMM HH:mm",
              yValueFormatString: "0.0%",
              dataPoints: dataKFM[1][1]
            },
            {
              type: "line",
              color: "gold",
              lineDashType: "dash",
              lineThickness: 1,
              showInLegend: false,
              name: "Moral",
              xValueType: "dateTime",
              xValueFormatString: "DD MMM HH:mm",
              yValueFormatString: "0.0%",
              dataPoints: dataKFM[1][2]
            }
          ]
        });

        chart.render();
      } else {
        chartContainer.style.display = "none";
  }
      /*
    },
    error: function (xhr) {
      chartContainer.style.display = "none";
    }
  });
  */
}

function setTrainingRules(iRule) {
  var tblTrainingRules = document.getElementById("tblTrainingRules");

  for (let i = 0; i < tblTrainingRules.rows.length; i++) {
    let row = tblTrainingRules.rows[i];
    let iDataRow = row.getAttribute('data-row');

    if (iDataRow == iRule) {
      $.ajax({
        url: '/Member/TrainingSetTrainingRule',
        dataType: "JSON",
        data: { iRule: iDataRow, iCFM: row.getElementsByClassName("sctTrainingRulesCFM")[0].value, iSmGr: row.getElementsByClassName("sctTrainingRulesSmGr")[0].value, fValue: row.getElementsByClassName("iptTrainingRulesValue")[0].value, iType: row.getElementsByClassName("sctTrainingRulesType")[0].value },
        success: function (ret) {
          if (ret) {
            if (ret.ok) {
              enableTrainingRulesRow(iRule + 1);
            } else if (ret.message) {
              alert(ret.message);
            }
          }
        }
      });

      break;
    }
  }
}

function enableTrainingRulesRow(iRow) {
  var tblTrainingRules = document.getElementById("tblTrainingRules");

  for (let i = 0; i < tblTrainingRules.rows.length; i++) {
    let row = tblTrainingRules.rows[i];
    let iDataRow = row.getAttribute('data-row');

    if (iDataRow == iRow || iRow < 0) {
      let ctrTrainingRules = row.getElementsByClassName("ctrTrainingRules");
      for (let j in ctrTrainingRules) {
        ctrTrainingRules[j].disabled = false;
      }

      // Enable next row if it has input
      if (row.getElementsByClassName("iptTrainingRulesValue")[0].value > 0) {
        enableTrainingRulesRow(iRow + 1);
      }

      break;
    }
  }
}
