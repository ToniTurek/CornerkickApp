function drawLine(ax, ay, bx, by, sColor, sTitle, iWidth, izIndex, sBorderStyle) {
  //alert(ax + ", " + ay + ", " + bx + ", " + by + ", " + sColor);

  if (ax > bx) {
    bx = ax + bx;
    ax = bx - ax;
    bx = bx - ax;
    by = ay + by;
    ay = by - ay;
    by = by - ay;
  }

  var angle = -Math.atan((ay - by) / (bx - ax)) * (180.0 / Math.PI);

  var length = Math.sqrt((ax - bx) * (ax - bx) + (ay - by) * (ay - by));

  if (length === 0) {
    return null;
  }

  let divLine = document.createElement("div");

  divLine.style.position = "absolute";
  divLine.style.left = ax.toString() + "px";
  divLine.style.top = ay.toString() + "px";
  divLine.style.width = length.toString() + "px";
  divLine.style.height = "0px";
  divLine.style.transform = "rotate(" + angle.toString() + "deg)";
  divLine.style.transformOrigin = "0% 0%";
  divLine.style.boxShadow = "0px 0px 2px 2px rgba(0, 0, 0, .1)";
  divLine.style.webkitTransform = "rotate(" + angle.toString() + "deg)";
  divLine.style.webkitTransformOrigin = "0% 0%";
  divLine.style.webkitBoxShadow = "0px 0px 2px 2px rgba(0, 0, 0, .1)";

  /*
  var style = "";
  style += "-ms-transform:rotate(" + angle.toString() + "deg);";
  style += "-moz-transform:rotate(" + angle.toString() + "deg);";
  style += "-moz-transform-origin:0% 0%;";
  style += "-o-transform:rotate(" + angle.toString() + "deg);";
  style += "-o-transform-origin:0% 0%;";
  */

  if (!iWidth) {
    iWidth = 1;
  }
  divLine.style.borderWidth = iWidth.toString() + "px";

  if (!sBorderStyle) {
    sBorderStyle = "solid";
  }
  divLine.style.borderStyle = sBorderStyle;

  if (!sColor) {
    sColor = "black";
  }
  divLine.style.borderColor = sColor;

  if (izIndex) {
    divLine.style.zIndex = izIndex.toString();
  }

  divLine.className = "tooltipLine";
  divLine.title = sTitle;

  return divLine;
}

function convertPosToPix(iX0, iY0, iX1, iY1, div, bHorizontal) {
  var iPos = [];

  if (bHorizontal) {
    iDivPixX = div.offsetWidth .toString();
    iDivPixY = div.offsetHeight.toString();

    iPos.push(( iX0       * iDivPixX) / 122);
    iPos.push(((iY0 + 25) * iDivPixY) /  50);
    iPos.push(( iX1       * iDivPixX) / 122);
    iPos.push(((iY1 + 25) * iDivPixY) /  50);
  } else {
    iDivPixX = div.offsetHeight.toString();
    iDivPixY = div.offsetWidth .toString();

    iPos.push(((iX0 + 25) * iDivPixY) /  50);
    iPos.push(( iY0       * iDivPixX) / 122);
    iPos.push(((iX1 + 25) * iDivPixY) /  50);
    iPos.push(( iY1       * iDivPixX) / 122);
  }

  return iPos;
}

function drawnode(x, y) {
  var ele = ""
  var style = "";
  style += "position:absolute;";
  style += "z-index:100;"
  ele += "<div class='relNode' style=" + style + ">";
  ele += "<span> Test Node</span>"
  ele += "<div>"

  $('#divDrawGame').show();
  var node = $(ele).appendTo('#divDrawGame');
  var width = node.width();
  var height = node.height();

  var centerX = width / 2;
  var centerY = height / 2;

  var startX = x - centerX;
  var startY = y - centerY;

  node.css("left", startX).css("top", startY);
}

var opacity = 0;
var intervalID = 0;
window.fadeIn = (el, interval) => {
  setInterval(show, interval, el);
}

function show(el) {
  opacity = Number(window.getComputedStyle(el).getPropertyValue("opacity"));
  if (opacity < 1) {
    opacity = opacity + 0.1;
    el.style.opacity = opacity
  } else {
    clearInterval(intervalID);
  }
}

function plotStars(iStars, iStarsMax = 0, iStarsSize = 24, sMarginBottom = "0px") {
  var divStars = document.createElement("div");
  divStars.style.position = "relative";
  divStars.style.top = "0px";
  divStars.style.width = "100%";
  divStars.style.height = iStarsSize.toString() + "px";
  divStars.style.marginBottom = sMarginBottom;
  divStars.style.fontSize = "0px";

  var iEnd = iStars;
  if (iStarsMax > 0) {
    iEnd = iStarsMax;
  }

  var i = 0;
  for (i = 0; i < iEnd; i++) {
    var imgStar = document.createElement("img");
    imgStar.style.position = "relative";
    //imgStar.style.left = (i * (iStarsSize + 4)).toFixed(0) + "px";
    imgStar.style.top = "0px";
    imgStar.style.width = iStarsSize.toString() + "px";
    imgStar.style.marginRight = "2px";
    if (i < iStars) {
      imgStar.src = "/Content/Icons/star.ico";
      imgStar.title = (i + 1).toString();
    } else {
      imgStar.src = "/Content/Icons/star_empty.png";
    }
    divStars.appendChild(imgStar);
  }

  return divStars;
}

function radioButtonList(rbActive, ltRbs, bnEnable1, bnEnable2) {
  for (let i = 0; i < ltRbs.length; i++) {
    ltRbs[i].classList.remove("active");
  }
  rbActive.className += " active";

  if (bnEnable1) {
    bnEnable1.disabled = false;
  }
  if (bnEnable2) {
    bnEnable2.disabled = false;
  }
}
