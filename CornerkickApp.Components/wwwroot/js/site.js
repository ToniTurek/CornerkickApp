// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

var sContentDir = "./_content/CornerkickApp.Components/Content";

function ScrollTo(elementId) {
  var element = document.getElementById(elementId);
  element.scrollIntoView({
    behavior: 'smooth'
  });
}

window.getDimensions = function () {
  return {
    width:  window.innerWidth,
    height: window.innerHeight
  };
};

function setFontSize(iFontSize) {
  if (iFontSize) {
    if (parseInt(iFontSize) > 0) {
      var root = document.querySelector(':root');

      if (root) {
        root.style.setProperty('--ckRelSize', iFontSize / 100);
        root.style.setProperty('--ckFontSize', iFontSize.toString() + '%');
        root.style.setProperty('--ckTextContainerHeight', (26 * (parseInt(iFontSize) / 100)).toFixed(0) + 'px');

        return true;
      }
    }
  }
  return false;
}

var getClosestElement = function (elem, selector) {
  // Element.matches() polyfill
  if (!Element.prototype.matches) {
    Element.prototype.matches =
      Element.prototype.matchesSelector ||
      Element.prototype.mozMatchesSelector ||
      Element.prototype.msMatchesSelector ||
      Element.prototype.oMatchesSelector ||
      Element.prototype.webkitMatchesSelector ||
      function (s) {
        var matches = (this.document || this.ownerDocument).querySelectorAll(s),
          i = matches.length;
        while (--i >= 0 && matches.item(i) !== this) { }
        return i > -1;
      };
  }

  // Get the closest matching element
  for (; elem && elem !== document; elem = elem.parentNode) {
    if (elem.matches(selector)) return elem;
  }

  return null;
};

// Returns the first parent element which matches the tag
function getParentElement(startElement, tagName) {
  let currentElm = startElement;
  while (currentElm != document.body) {
    if (currentElm.tagName.toLowerCase() == tagName.toLowerCase()) { return currentElm; }
    currentElm = currentElm.parentElement;
  }
  return false;
}

function setCounter(dt_target, counter_el) {
  if (!counter_el) {
    return;
  }

  var dt_diff = new Date(dt_target - Date.now());

  if (dt_diff < 0) {
    return;
  }

  const dys = Math.floor(dt_diff / (1000 * 60 * 60 * 24));
  const hrs = Math.floor((dt_diff - (dys * (1000 * 60 * 60 * 24))) / (1000 * 60 * 60));
  const min = Math.floor((dt_diff - (dys * (1000 * 60 * 60 * 24)) - (hrs * (1000 * 60 * 60))) / (1000 * 60));;
  const sec = Math.floor((dt_diff - (dys * (1000 * 60 * 60 * 24)) - (hrs * (1000 * 60 * 60)) - (min * (1000 * 60))) / (1000));;

  counter_el.innerText = dys.toString() + "T " + hrs.toString().padStart(2, '0') + ":" + min.toString().padStart(2, '0') + ":" + sec.toString().padStart(2, '0');

  setTimeout(setCounter, 1000, dt_target, counter_el);
}

window.getElemDimensions = function (elem) {
  return {
    width: elem.offsetWidth,
    height: elem.offsetHeight
  };
};

window.PlaySound = function (sound) {
  var audioCash = new Audio(sContentDir + "/Sounds/" + sound + ".wav");
  if (audioCash) {
    audioCash.volume = 0.5;
    audioCash.play();
  }
}
