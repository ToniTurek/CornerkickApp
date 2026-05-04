var CornerkickApp = CornerkickApp || {};

//import { Render3D } from "/_content/CornerkickApp.Components/js/render/render.js"

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}

export function alertTitle() {
  var title = $("#myTitle").val();
  alert("the title is :" + title);
}

// wwwroot/platform.js
window.getPlatform = () => {
  if (navigator.userAgent.includes("Android")) return "Android";
  if (navigator.userAgent.includes("Windows")) return "Windows";
  return "Other";
};

//export function init(dtCounterStart2, bWebApp) {
window.init = (DotNetRef, dtCounterStart2, bWebApp) => {
  const dtCounterStart = JSON.parse(dtCounterStart2);

  DotNetRef.invokeMethodAsync("movePlayerJs", 1, 2);

  if (bWebApp) {
    //setView(showRender);
    setView();
  } else {
    setView();
  }

  var counter_container = document.getElementById("counter_container");
  var dt_diff = new Date(dtCounterStart - Date.now());
  if (dt_diff > 0) {
    document.getElementById("counter_date_season_start").innerText = dtCounterStart.toLocaleDateString('de-DE', { weekday: 'short', year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' }) + " Uhr!";
    counter_container.style.display = "inline-block";
    var counter_season_start = document.getElementById("counter_season_start");
    setCounter(dtCounterStart, counter_season_start);
  }

  /*
  $(window).resize(function () {
    setView();
  });
  */

  function setView(_callback) {
    let main = document.getElementById("divMain");
    let divImg = document.getElementById("divImg");
    let renderContainer = document.getElementById("renderContainer");
    let mainTextRectangle;
    if (bWebApp) {
      mainTextRectangle = main.getBoundingClientRect();
    }

    if (!divImg) return;

    const fWidth = window.screen.width;
    if (fWidth < 1200) {
      divImg.style.float = "left"
    } else {
      divImg.style.float = "right"
    }

    if (fWidth < 450) {
      divImg.style.width = "100%"
      divImg.style.top = "-20px"

      var ltEl = document.getElementsByName("txtTeaser");
      for (var iEl = 0; iEl < ltEl.length; iEl++) {
        ltEl[iEl].className = " ckTeaserSmall";
      }
    } else {
      if (fWidth <= 750) {
        divImg.style.width = "420px"
      } else {
        divImg.style.width = "580px"
      }
      divImg.style.top = "-40px"

      var ltEl = document.getElementsByName("txtTeaser");
      for (var iEl = 0; iEl < ltEl.length; iEl++) {
        ltEl[iEl].className = " ckTeaser";
      }
    }

    if (bWebApp) {
      // Render box
      renderContainer.style.left = (main.offsetWidth + mainTextRectangle.left + 20) + "px";
      //renderContainer.style.height = (renderContainer.offsetWidth * (9 / 16)) + "px";
    }

    if (_callback) {
      renderContainer.style.display = "block";
      _callback();
    }
  }

  function showRender() {
    let renderContainer = document.getElementById("renderContainer");
    let txtRenderInfo = document.getElementById("txtRenderInfo");
    let txtRenderCount = document.getElementById("txtRenderCount");
    let txtRenderShootResult = document.getElementById("txtRenderShootResult");

    var render3D = new Render3D(
      renderContainer,
      txtRenderInfo, txtRenderCount, txtRenderShootResult,
      [[20, 1, 15, 1], [15, 2, 12, 1], [25, 1, 17, 1], [30, 0, 20, 0], [20, 0, 16, 0], [25, 1, 18, 1], [15, 1, 13, 1], [10, 1, 10, 1], [20, 0, 15, 1], [30, 0, 19, 1]]
    );
    //render3D.setCamera(3);

    const bAway = false;

    // Set player of team away
    render3D.TeamA.cl1 = 0x009dff;
    render3D.TeamA.cl2 = 0xffffff;
    render3D.TeamA.cl3 = 0x009dff;
    if (bAway) {
      render3D.TeamA.addPlayer({ x: 9, y: -2 }, { x: 8, y: -3 });
    } else {
      render3D.TeamA.addPlayer({ x: 111, y: -2 }, { x: 112, y: -3 });
    }
    const iShootRes = 99;
    //render3D.set({ x: 99, y: 2, z: 0 }, null);
    let ballPosm1 = { x: 103, y: 4, z: 1 };
    let ballPos0 = { x: 99, y: 2, z: 0 };
    if (bAway) {
      ballPosm1.x = 122 - ballPosm1.x;
      ballPos0.x = 122 - ballPos0.x;;
    }

    render3D.animate(
      ballPos0,
      ballPosm1,
      iShootRes, -1, 0,
      iShootRes == 7 ? { x: render3D.TeamA.ltPlayer[0].ptPos.x, y: -2, z: -1 } : null,
      true,
      null
    );
  }
}

export function zoomImg(img) {
  var small = { height: "200px" };
  var large = { height: "800px" };

  if (img.height() == "200") {
    img.animate(large);
  } else {
    img.animate(small);
  }
}

export function zoomVideo(div, iType) {
  if (div.offsetWidth === 200) {
    div.innerHTML = '';
    var video = document.createElement("video");
    video.width = "472";
    video.height = "626";
    video.src = "/Content/Video/game_example_" + iType.toString() + ".mp4";
    div.appendChild(video);
    div.style.width = "800px";
  } else {
    div.style.width = "200px";
  }
}
