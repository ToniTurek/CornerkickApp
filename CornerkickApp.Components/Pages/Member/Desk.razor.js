export function init(bWebApp, Model) {
  const mdl = JSON.parse(Model);

  var spanDeskPlaceCup = document.getElementById("spanDeskPlaceCup");

  initialize2();

  function initialize2() {
    setPlaceCupInfo(spanDeskPlaceCup, mdl.sNatCupRound, mdl.sNatCupEliminated, window.innerWidth);
  } // initialize
}

var tblNews;
var bMarkReadGlobal;

export function setNewspaper(ltNewsIn) {
  const ltNews = JSON.parse(ltNewsIn);

  const bMobile = window.innerWidth < 600;

  var divNewspaper = document.getElementById("divNewspaper");
  if (!divNewspaper) { return; }

  var sFrontPageFontSize = "13px";
  var sHeaderFontSize = "22px";
  var sTextFontSize = "14px";
  if (bMobile) {
    sFrontPageFontSize = "10px";
    sHeaderFontSize = "14px";
    sTextFontSize = "10px";
  }

  divNewspaper.style.display = "none";

  // Create front page
  if (ltNews.length > 0) {
    var divFp = document.createElement('div');
    divFp.className = "divNewspaperPage";
    divFp.id = "divNewspaperPage_0";
    divFp.style.position = "absolute";
    divFp.style.right = "0px";
    divFp.style.width = "100%";
    divFp.style.height = "100%";
    divFp.style.backgroundColor = "white";
    divFp.style.border = "1px solid black";
    divFp.style.cursor = "pointer";
    divFp.style.textAlign = "center";
    divFp.style.fontSize = sFrontPageFontSize;
    divFp.style.fontFamily = "Times New Roman";
    divFp.style.clear = "both";
    if (typeof sContentDir !== 'undefined') {
      divFp.style.backgroundImage = 'url("' + sContentDir + '/Images/ck.png")';
    }
    divFp.style.backgroundRepeat = 'no-repeat';
    //divFp.style.backgroundPosition = "bottom";
    //divFp.style.backgroundSize = '100%';
    if (bMobile) {
      divFp.innerHTML = '<b data-ipage="0"><br/>CK ANZEIGER</b>';
    } else {
      divFp.innerHTML = '<b data-ipage="0"><br/>CORNERKICK ANZEIGER</b>';
    }
    divFp.addEventListener("click", switchNewspaperPage);
    divFp.setAttribute('data-ipage', 0);

    var divFp2 = document.createElement('div');
    divFp2.style.position = "absolute";
    divFp2.style.top = "0px";
    divFp2.style.left = "0px";
    divFp2.style.width = "100%";
    divFp2.style.textAlign = "right";
    divFp2.style.fontSize = sFrontPageFontSize;
    divFp2.setAttribute('data-ipage', 0);
    divFp2.innerHTML = ltNews[0].sDate.split(' ')[0];

    divFp.appendChild(divFp2);
    divNewspaper.appendChild(divFp);

    divNewspaper.style.display = "block";
  }

  for (var iN = 0; iN < ltNews.length; iN++) {
    var news = ltNews[iN];

    var sTextSplit = news.sText.split('#');
    if (sTextSplit.length < 2) {
      continue;
    }

    var sHeader = sTextSplit[0];
    var sText = sTextSplit[1];

    var div0 = document.createElement('div');
    div0.className = "divNewspaperPage";
    div0.id = "divNewspaperPage_" + (iN + 1).toString();
    div0.style.position = "absolute";
    div0.style.right = "0px";
    div0.style.width = "50%";
    div0.style.height = "100%";
    div0.style.backgroundColor = "white";
    div0.style.border = "1px solid black";
    div0.style.cursor = "pointer";
    div0.style.fontFamily = "Times New Roman";
    div0.addEventListener("click", switchNewspaperPage);
    div0.setAttribute('data-ipage', iN + 1);
    div0.style.display = "none";

    var div1 = document.createElement('div');
    div1.style.position = "relative";
    div1.style.top = "20px";
    div1.style.left = "4%";
    div1.style.width = "92%";
    div1.style.textAlign = "center";
    //div1.style.border = "1px solid green";
    div1.setAttribute('data-ipage', iN + 1);
    div1.style.fontSize = sHeaderFontSize;
    div1.style.lineHeight = "1.2";
    div1.innerHTML = '<b data-ipage="' + (iN + 1).toString() + '">' + sHeader + '</b>';
    div0.appendChild(div1);

    var div2 = document.createElement('div');
    div2.style.position = "relative";
    div2.style.left = "4%";
    div2.style.marginTop = "12%";
    div2.style.width = "92%";
    //div2.style.border = "1px solid red";
    div2.setAttribute('data-ipage', iN + 1);

    var div21 = document.createElement('div');
    div21.style.position = "relative";
    div21.style.width = "30%";
    div21.style.float = "right";
    div21.style.clear = "both";
    if (news.iId >= 0) {
      div21.innerHTML = news.sImg;
    }
    div21.setAttribute('data-ipage', iN + 1);
    div2.appendChild(div21);

    var div22 = document.createElement('div');
    div22.style.position = "relative";
    div22.style.textAlign = "left";
    div22.style.fontSize = sTextFontSize;
    //div22.style.lineHeight = "1.2";
    div22.innerHTML = sText;
    div22.setAttribute('data-ipage', iN + 1);
    div2.appendChild(div22);
    div0.appendChild(div2);

    var divDt = document.createElement('div');
    divDt.style.position = "absolute";
    divDt.style.left = "4px";
    divDt.style.top = "2px";
    divDt.style.fontSize = "12px";
    divDt.innerText = news.sDate;
    div0.appendChild(divDt);

    var divNb = document.createElement('div');
    divNb.style.position = "absolute";
    divNb.style.right = "4px";
    divNb.style.bottom = "2px";
    divNb.style.fontSize = "12px";
    divNb.innerText = (iN + 2).toString() + "/" + (ltNews.length + 1).toString();
    div0.appendChild(divNb);

    divNewspaper.appendChild(div0);
  }
}

export function hideNewspaper() {
  var divNewspaper = document.getElementById("divNewspaper");

  //if (divNewspaper) divNewspaper.classList = "";
  if (divNewspaper) switchNewspaperPage();
}

export function getClubEmblem(iId) {
  var sIcon = '<img src="' + sContentDir + '/Uploads/emblems/';

  sIcon += iId.toString() + '.png" title="emblem" style="width: 100%"/>';

  return sIcon;
}

var iNewspaperPage = 0;
function switchNewspaperPage(e) {
  var divNewspaper = document.getElementById("divNewspaper");

  // Get current page number
  var iPageNo = 1;
  if (e) {
    if (e.stopPropagation) e.stopPropagation();
    iPageNo = parseInt(e.target.getAttribute('data-ipage'));
  }
  var pages = document.getElementsByClassName('divNewspaperPage');

  if (iPageNo === pages.length - 1 && iPageNo % 2 === 0) {
    return;
  }

  divNewspaper.classList = "open";

  // First, hide all pages
  for (var i = 0; i < pages.length; ++i) {
    pages[i].style.display = "none";
  }

  if (iPageNo % 2 === 0) {
    var pagePost1 = document.getElementById('divNewspaperPage_' + (iPageNo + 1).toString());
    var pagePost2 = document.getElementById('divNewspaperPage_' + (iPageNo + 2).toString());
    if (pagePost1 != null) {
      if (iPageNo + 1 === pages.length - 1) { // If last page --> show on right site
        pagePost1.style.right = "0px";
      } else {
        pagePost1.style.left = "0px";
      }
      pagePost1.style.display = "block";
    }

    if (pagePost2 != null) {
      pagePost2.style.right = "0px";
      pagePost2.style.display = "block";
    }
  } else {
    var pagePre1 = document.getElementById('divNewspaperPage_' + (iPageNo - 1).toString());
    var pagePre2 = document.getElementById('divNewspaperPage_' + (iPageNo - 2).toString());
    if (pagePre1 != null) {
      pagePre1.style.right = "0px";
      pagePre1.style.display = "block";
      if (iPageNo - 1 === 0) {
        divNewspaper.classList = "";
      }
    }

    if (pagePre2 != null) {
      pagePre2.style.left = "0px";
      pagePre2.style.display = "block";
    }
  }
}

function showBalanceToday(parent) {
  $.ajax({
    url: '/Member/DeskGetBalanceToday',
    dataType: "JSON",
    type: 'GET',
    success: function (ltBalanceToday) {
      if (!ltBalanceToday) {
        return;
      }

      if (ltBalanceToday.length === 0) {
        return;
      }

      var div0 = document.createElement("div");
      div0.id = "dlgBalanceToday";
      div0.title = "Heutige Transaktionen";

      var div1 = document.createElement("div");
      div1.style.position = "relative";
      div1.style.width = "100%";

      var tbl1 = document.createElement("table");
      tbl1.style.width = "100%";
      tbl1.className = "table table-bordered";
      tbl1.cellPadding = 4;

      let rowHeader = tbl1.createTHead().insertRow(0);

      let cellHd;
      cellHd = rowHeader.insertCell();
      cellHd.style.textAlign = "center";
      cellHd.style.fontWeight = "bold";
      cellHd.innerHTML = "Betreff";

      cellHd = rowHeader.insertCell();
      cellHd.style.textAlign = "center";
      cellHd.style.fontWeight = "bold";
      cellHd.innerHTML = "Umsatz";

      var tblbdy1 = tbl1.createTBody();

      var iBalanceTodayTotal = 0;
      var i = 0;
      for (i = 0; i < ltBalanceToday.length; i++) {
        var blc = ltBalanceToday[i];

        var tr1 = tblbdy1.insertRow();

        var td1 = tr1.insertCell();
        td1.align = "right";
        td1.innerHTML = blc.sSubject;

        var td2 = tr1.insertCell();
        td2.align = "right";
        td2.innerHTML = blc.iValue.toLocaleString();
        if (blc.iValue < 0) {
          td2.style.color = "red";
        } else {
          td2.style.color = "green";
        }

        iBalanceTodayTotal += blc.iValue;
      }

      var tblFoot = tbl1.createTFoot();
      var trTotal = tblFoot.insertRow();

      var tdTotal1 = trTotal.insertCell();
      tdTotal1.align = "right";
      tdTotal1.innerHTML = "Gesamt";
      tdTotal1.style.fontWeight = "bold";

      var tdTotal2 = trTotal.insertCell();
      tdTotal2.align = "right";
      tdTotal2.innerHTML = iBalanceTodayTotal.toLocaleString();
      if (iBalanceTodayTotal < 0) {
        tdTotal2.style.color = "red";
      } else {
        tdTotal2.style.color = "green";
      }
      tdTotal2.style.fontWeight = "bold";

      div1.appendChild(tbl1);
      div0.appendChild(div1);
      parent.appendChild(div0);

      $(div0).dialog({
        autoOpen: true,
        width: 'auto',
        modal: false,
        buttons: [
          {
            text: "schließen und nicht mehr anzeigen",
            class: "btnDialog btn btn-outline-secondary",
            id: "bnOkAndHide",
            tabIndex: -1,
            click: function () {
              $.ajax({
                url: '/Member/DeskSetBalanceTodayDialog',
                dataType: "JSON",
                type: 'POST',
                data: { bOn: false },
              });

              $(this).dialog('destroy').remove();
            }
            /*
            },
            {
              text: "schließen",
              class: "btnDialog btn btn-outline-primary",
              id: "bnOk",
              click: function () {
                $(this).dialog('destroy').remove();
              }
            */
          }
        ]
      });
    }
  });
}

function setPlaceCupInfo(spanDeskPlaceCup, sPlaceCup, sPlaceCupEliminated, window_width) {
  if (!spanDeskPlaceCup) { return; }

  if (sPlaceCupEliminated) {
    var sPlaceCupEliminatedTmp = sPlaceCupEliminated;
    if (window_width < 600) {
      sPlaceCupEliminatedTmp = sPlaceCupEliminated.substring(0, 4) + ".";
    }

    spanDeskPlaceCup.innerText = sPlaceCupEliminatedTmp + " (" + sPlaceCup + ")";
  } else {
    spanDeskPlaceCup.innerText = sPlaceCup;
  }
}
