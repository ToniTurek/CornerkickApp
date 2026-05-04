import { THREEx } from "../../lib/three/threex.keyboardstate.js";
import { Stadium } from "./stadium.js";
import { Player } from "./player.js";
import { Stats } from "./Stats.js";

function render2(renderContainer, posBall0X, posBall0Y, posBall0Z, txtLeft = null, txtRight = null, txtRenderShootResult = null) {
  Renderer(
    renderContainer,
    new THREE.Vector3(posBall0X, posBall0Y, posBall0Z),
    txtLeft,
    txtRight,
    txtRenderShootResult
  );
}

// ltStadiumBlocks: [0] - total seats, [1] - block type, [2] - specs, [3] - roof (0 - no roof, 1 - roof)
function Render3D(renderContainer, txtLeft = null, txtRight = null, txtRenderShootResult = null, ltStadiumBlocks) {
  var renderer;
  var scene;
  var camera;
  var mshBall;
  var mshBallTarget;
  var mshKeeperH;
  var mshKeeperA;

  var posBall0 = { x: 0, y: 0, z: 0 }; // Ball position at start of phase
  var posBall1 = { x: 0, y: 0, z: 0 }; // Ball position at end of phase
  var alpha_Ball = -1;
  var fBallPositionZ0 = 0;
  const fBallDampingPerBounce = 0.1;
  var fBallDamping = 0;

  var controls;
  var keyboard = new THREEx.KeyboardState();
  var stats;

  const iSceneWidth  = renderContainer.offsetWidth  - 8;
  const iSceneHeight = renderContainer.offsetHeight - 8;

  //const fPitchX = 98.7552 / 2;
  //const fPitchY = fPitchX * (50 / 61);
  const fPitchX = 61;
  const fPitchY = 50;

  const fGoalWidth  = 7.32; // 8 yards
  const fGoalHeight = 2.44; // 8 feet
  const fGoalDepth  = 2;
  const fGoalPostRad = 0.06;
  //const fBallRadius = (0.7 / Math.PI) / 2; // Radius of ball
  const fBallRadius = (1 / Math.PI) / 2; // Radius of ball
  const fKeeperRad = 0.6; // Keeper "radius"
  this.fGoalWidth  = fGoalWidth;
  this.fGoalHeight = fGoalHeight;
  this.fGoalPostRad = fGoalPostRad;

  const MASS = 0.1;
  const fBallMeshEffect = 4;

  const GRAVITY = 9.81;
  const gravity = new THREE.Vector3(0, 0, -GRAVITY).multiplyScalar(MASS);

  var t = 0;
  var fFramesPerSec = 60;
  const TIMESTEP = 18 / 1000;
  const TIMESTEP_SQ = TIMESTEP * TIMESTEP;

  const diff = new THREE.Vector3();
  var ltMeshGoals = [];
  var ltMeshGoalHA = [];

  var ballPosition = new THREE.Vector3(0, 0, 0);
  var TeamH = null;
  var TeamA = null;
  this.TeamH = null;
  this.TeamA = null;

  class Team {
    constructor() {
      this.cl1;
      this.cl2;
      this.cl3;
      this.ltPlayer = [];
    }

    addPlayer(ptPosCk = null, ptPosLastCk = null) {
      if (!ptPosCk) {
        ptPosCk = { x: fPitchX, y: 0 };
      }

      let pl = new Player(scene, { x: convertXToRenderCoord(ptPosCk.x), y: ptPosCk.y }, 1.8, this.cl1, this.cl2, this.cl3);
      if (ptPosLastCk) {
        pl.ptPosLast = { x: convertXToRenderCoord(ptPosLastCk.x), y: ptPosLastCk.y };
      }
      this.ltPlayer.push(pl);
      //console.log("Add player to pos: " + pl.ptPos.x + ", " + pl.ptPos.y);
    }

    applyColorToPlayer(cl1, cl2, cl3) {
      for (let i = 0; i < this.ltPlayer.length; i++) {
        if (this.ltPlayer[i] != null) {
          this.ltPlayer[i].parts[0].material.color.set(cl1);
          this.ltPlayer[i].parts[1].material.color.set(cl2);
          this.ltPlayer[i].parts[2].material.color.set(cl3);
        }
      }
    }
  }

  const init = function (ltStadiumBlocks) {
    console.log("Initiate rendering...");
    /*
    console.log("Render existent: " + renderContainer.getElementsByTagName("canvas").length > 0);
    if (renderContainer.getElementsByTagName("canvas").length > 0) {
      console.log("return");
      return;
    }
    */
    renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    //console.log("scene width/height: " + iSceneWidth + "/" + iSceneHeight);
    renderer.setSize(iSceneWidth, iSceneHeight);
    //renderer.setPixelRatio(window.devicePixelRatio);
    renderer.shadowMap.enabled = true;
    renderContainer.appendChild(renderer.domElement);

    scene = new THREE.Scene();
    //scene.remove.apply(scene, scene.children);
    scene.background = new THREE.Color(0xcce0ff);
    //scene.fog = new THREE.Fog(0xcce0ff, 500, 10000);

    ////////////////////////////////////////////////////////////////////
    // Lights
    ////////////////////////////////////////////////////////////////////
    //const lightAmb = new THREE.AmbientLight(0x404040); // soft white light
    //lightAmb.castShadow = true;
    //scene.add(lightAmb);

    const light = new THREE.DirectionalLight(0xdfebff, 1);
    light.position.set(-10, 10, 50);
    //light.position.multiplyScalar(1.3);

    light.castShadow = true;
    //light.shadowDarkness = 1.0;
    //light.shadowCameraVisible = true;

    light.shadow.mapSize.width  = 1024;
    light.shadow.mapSize.height = 1024;

    const d = fPitchX * 1.5;

    light.shadow.camera.left = -d;
    light.shadow.camera.right = d;
    light.shadow.camera.top = d;
    light.shadow.camera.bottom = -d;

    light.shadow.camera.far = 70;

    scene.add(light);

    /*
    const light2 = new THREE.DirectionalLight(0xdfebff, 1);
    light2.position.set(5, 20, 10);
    light2.position.multiplyScalar(1.3);

    light2.castShadow = true;

    light2.shadow.mapSize.width = 1024;
    light2.shadow.mapSize.height = 1024;

    const d2 = 30;

    light2.shadow.camera.left = -d2;
    light2.shadow.camera.right = d2;
    light2.shadow.camera.top = d2;
    light2.shadow.camera.bottom = -d2;

    light2.shadow.camera.far = 1000;

    scene.add(light2);
    */

    const lightHemi = new THREE.HemisphereLight(0xffffbb, 0x080820, 1);
    //lightHemi.castShadow = true;
    scene.add(lightHemi);

    // Stadium
    const stadium = new Stadium(scene, ltStadiumBlocks);
    ltMeshGoals = stadium.ltMeshGoal;

    const txtLoader = new THREE.TextureLoader();
    ////////////////////////////////////////////////////////////////////
    // Ball
    ////////////////////////////////////////////////////////////////////
    // THREE.SphereGeometry(radius, widthSegments, heightSegments);
    const geomBall = new THREE.SphereGeometry(fBallRadius, 32, 16);

    // Load and apply ball texture
    // load a texture, set wrap mode to repeat
    const txtBall = txtLoader.load("./_content/CornerkickApp.Components/Content/Images/render/ball_texture.jpg");
    //txtBall.wrapS = THREE.RepeatWrapping;
    //txtBall.wrapT = THREE.RepeatWrapping;
    //txtBall.repeat.set(4, 4);
    const matBall = new THREE.MeshBasicMaterial({ map: txtBall/*, color: 0xffff00*/ });

    // Create ball mesh
    mshBall = new THREE.Mesh(geomBall, matBall);
    mshBall.castShadow = true; //default is false
    mshBall.receiveShadow = true;
    scene.add(mshBall);

    const matBallTarget = new THREE.MeshBasicMaterial({ color: 0xffff00 });
    mshBallTarget = new THREE.Mesh(geomBall, matBallTarget);
    mshBallTarget.castShadow = false;
    mshBallTarget.receiveShadow = false;
    //scene.add(mshBallTarget);
    ////////////////////////////////////////////////////////////////////
    // End ball
    ////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////
    // Keeper
    ////////////////////////////////////////////////////////////////////
    const geomKeeper = new THREE.CylinderGeometry(fKeeperRad, fKeeperRad, 0.2, 32);
    //const matKeeper = new THREE.MeshStandardMaterial({ color: 0x009dff });
    const matKeeperH = new THREE.MeshStandardMaterial({ color: 0xff0000 });
    //const matKeeper = new THREE.MeshBasicMaterial({ color: 0xff0000 });
    mshKeeperH = new THREE.Mesh(geomKeeper, matKeeperH);
    mshKeeperH.castShadow = true;
    mshKeeperH.rotation.z = 90 * (Math.PI / 180);
    mshKeeperH.position.x = +fPitchX - 0.2;
    mshKeeperH.position.y = 0;
    mshKeeperH.position.z = 0.8;
    scene.add(mshKeeperH);

    const matKeeperA = new THREE.MeshStandardMaterial({ color: 0x009dff });
    //const matKeeper = new THREE.MeshBasicMaterial({ color: 0xff0000 });
    mshKeeperA = new THREE.Mesh(geomKeeper, matKeeperA);
    mshKeeperA.castShadow = true;
    mshKeeperA.rotation.z = 90 * (Math.PI / 180);
    mshKeeperA.position.x = -fPitchX + 0.2;
    mshKeeperA.position.y = 0;
    mshKeeperA.position.z = 0.8;
    scene.add(mshKeeperA);
    ////////////////////////////////////////////////////////////////////
    // End keeper
    ////////////////////////////////////////////////////////////////////

    // Teams
    console.log("  Create teams");
    TeamH = new Team();
    TeamA = new Team();

    iBounceCount = 0;

    stats = createStats();
    /* DEBUG
    renderContainer.appendChild(stats.domElement);
    */
  } // init()

  const updateSize = function () {
    const iSceneWidth  = renderContainer.offsetWidth  - 8;
    const iSceneHeight = renderContainer.offsetHeight - 8;

    if (!camera) {
      setCamera(3);
    }
    camera.aspect = iSceneWidth / iSceneHeight;
    camera.updateProjectionMatrix();
    //camera = new THREE.PerspectiveCamera(60, iSceneWidth / iSceneHeight, 0.1, 1000);

    renderer.setSize(iSceneWidth, iSceneHeight);
  }
  this.updateSize = updateSize;

  var ballTargetInt = { x: 0, y: 0, z: 0 };
  var fShootPower = 1;

  let iBounceCount = 0;
  let xBallLast = 0;
  let zBall = 0;
  let zBallLast = 0;

  let iGoals = 0;
  let iAways = 0;
  let iSaves = 0;
  let iAlu = 0;
  let jShootResult = -1; // 0: missed, 1: goal, 2: catched by keeper, 3: saved by keeper (bounce), 4: saved by keeper (cornerkick) - handle as 3 for now, 5: post, 6: bar

  let iPhase = 0;
  const update = function (ballPosition, posBallCk0, posBallCkm1, iShootResult, TeamH, TeamA, bBounce = false) {
    //console.log("ballPosition: " + ballPosition.x + ", " + ballPosition.y + ", " + ballPosition.z + ", " + fPitchX + fGoalDepth + ", Bounce: " + iBounceCount.toString());
    //console.log("  jShootResult: " + jShootResult);
    //console.log("  iPhase: " + iPhase);

    t += 1 / fFramesPerSec;

    /*
    // Ini info text
    if (txtLeft) {
      txtLeft.innerText = "Bounce " + iBounceCount.toString();
    }
    //console.log(ballTargetInt.y);
    */

    // Rotate ball
    mshBall.rotation.x += 0.01;
    mshBall.rotation.y += 0.01;

    mshBallTarget.position.copy(posBall1);

    /*
    // Correct z ball target if shoot result is away but rendered as on goal
    if (iShootResult == 0 && checkOnGoal(posBall1, true)) {
      posBall1.z = fGoalHeight * 1.2;
    }
    */

    // Do ball bounce
    if (bBounce && iBounceCount < 2) {
      zBall += 0.02;
      if (checkBounce(zBall, zBallLast)) {
        iBounceCount += 1;
      }

      if (txtLeft) {
        txtLeft.innerText = "Bounce " + iBounceCount.toString();
      }

      ballPosition.z = fBallRadius + Math.abs(Math.sin(zBall));

      return true;
    }

    if (zBall > 0) {
      zBall = 0;
    }

    // Move ball
    var fBallDistPerFrame = (0.5 * fShootPower) + 0.4;
    //fBallDistPerFrame *= 0.5;
    if (iPhase < 1) { fBallDistPerFrame = 0.1; }

    const v0_Ball = fBallDistPerFrame * fFramesPerSec;

    // Calculate alpha ball
    if (alpha_Ball < 0) {
      alpha_Ball = 10;

      const dx = Math.abs(posBall1.x - posBall0.x);
      const dz = posBall1.z - fBallPositionZ0;
      for (let ii = 0; ii < 20; ii++) {
        const alpha_tan = (dz + ((GRAVITY / (2 * Math.pow(v0_Ball, 2) * Math.pow(Math.cos(alpha_Ball * (Math.PI / 180)), 2))) * Math.pow(dx, 2))) / dx;
        if (alpha_tan < 1) {
          alpha_Ball = Math.atan(alpha_tan) * (180 / Math.PI);
        }
        //const z_Test = z0_Test + (Math.tan(alpha_Test * (Math.PI / 180)) * dx_Test) - ((GRAVITY / (2 * Math.pow(v0_Test, 2) * Math.pow(Math.cos(alpha_Test * (Math.PI / 180)), 2))) * Math.pow(dx_Test, 2));
      }
    }
    //console.log(alpha_Ball.toFixed(5) + ", " + v0_Ball.toFixed(5) + ", " + (posBall1.x - posBall0.x).toFixed(5) + ", " + (posBall0.z - posBall1.z).toFixed(5));

    let iSign = 1;
    if (posBall0.x > posBall1.x) {
      iSign = -1;
    }
    if (jShootResult == 3 || jShootResult == 4 || jShootResult == 5 || jShootResult == 6 || jShootResult == 7) {
      //iSign *= -1;
    }

    const fBallXOld = ballPosition.x;
    const fRelXOld = (fPitchX - fBallXOld) / (fPitchX - posBall0.x);
    var fBallDeltaX = fBallDistPerFrame * Math.cos(alpha_Ball * (Math.PI / 180));
    fBallDeltaX *= iSign;
    ballPosition.x += fBallDeltaX;

    // Start of shoot
    if (iShootResult >= 0 && iPhase == 0) {
      if ((fBallXOld < posBall1.x && ballPosition.x > posBall1.x) ||
          (fBallXOld > posBall1.x && ballPosition.x < posBall1.x)) {
        iPhase += 1;
        //console.log("Increment phase to " + iPhase);

        t = 0;
        alpha_Ball = -1; // Recalculate alpha
        fBallDamping = 0;
        fBallPositionZ0 = Math.max(posBall1.z, fBallRadius);

        posBall0 = {
          x: convertXToRenderCoord(posBallCk0.x),
          y: posBallCk0.y,
          z: posBallCk0.z
        };

        // Create ball target at x = end of pitch
        let ballTarget1X = -fPitchX;
        if (ballTargetInt.x > 0) {
          ballTarget1X = fPitchX;
        }
        posBall1 = {
          x: ballTarget1X,
          y: (ballTargetInt.y - posBall0.y) * ((ballTarget1X - posBall0.x) / (ballTargetInt.x - posBall0.x)) + posBall0.y,
          //z: (ballTargetInt.z - posBallCk0.z) * ((fPitchX - posBall0.x) / (ballTargetInt.x - posBall0.x)) + posBallCk0.z
          z: ballTargetInt.z
        };
        //console.log("ballTarget: " + ballTargetInt.x.toFixed(5) + "/" + ballTargetInt.y.toFixed(5) + "/" + ballTargetInt.z.toFixed(5) + ", posBall1: " + posBall1.x.toFixed(5) + "/" + posBall1.y.toFixed(5) + "/" + posBall1.z.toFixed(5));

        fShootPower = Math.random();
        return true;
      }
    }

    let fRelX = 0;
    if (Math.abs(posBall1.x - posBall0.x) > 0) {
      fRelX = Math.abs((ballPosition.x - posBall0.x) / (posBall1.x - posBall0.x));
    }

    ballPosition.y = (posBall1.y * fRelX) + (posBall0.y * (1 - fRelX));

    // Schräger Wurf f(x)
    //const dx = Math.abs(ballPosition.x - fBallHitGroundX);
    //ballPosition.z = fBallPositionZ0 + (Math.tan(alpha_Ball * (Math.PI / 180)) * dx) - ((GRAVITY / (2 * Math.pow(v0_Ball, 2) * Math.pow(Math.cos(alpha_Ball * (Math.PI / 180)), 2))) * Math.pow(dx, 2));

    if (jShootResult == 2) {
      ballPosition.z = posBall1.z;
    } else {
      // Schräger Wurf f(t)
      ballPosition.z = fBallPositionZ0 + (v0_Ball * Math.max(1 - fBallDamping, 0) * Math.sin(alpha_Ball * (Math.PI / 180)) * t) - (0.5 * GRAVITY * Math.pow(t, 2));
    }
    /*
    console.log(
      posBall0.x.toFixed(5) + "/" + posBall0.y.toFixed(5) + "/" + posBall0.z.toFixed(5) + ", " +
      posBall1.x.toFixed(5) + "/" + posBall1.y.toFixed(5) + "/" + posBall1.z.toFixed(5) + ", " +
      ballPosition.x.toFixed(5) + "/" + ballPosition.y.toFixed(5) + " / " + ballPosition.z.toFixed(5) + ", " + fRelX.toFixed(5));
    */
    //console.log(iPhase + ", " + t.toFixed(5) + ", " + v0_Ball.toFixed(5) + ", " + ballPosition.x.toFixed(5) + ", " + ballPosition.y.toFixed(5) + ", " + ballPosition.z.toFixed(5) + ", " + fBallPositionZ0.toFixed(5));

    // If ball hits ground --> reset parabel
    if (ballPosition.z < fBallRadius) {
      t = 0;
      fBallPositionZ0 = fBallRadius;

      if (iPhase > 0) {
        fBallDamping += fBallDampingPerBounce;
      }
    }

    zBall -= 0.02;

    let mshKeeper = mshKeeperH;
    if (ballTargetInt && ballTargetInt.x < 0) mshKeeper = mshKeeperA;

    if (jShootResult == 1) {
      if (ballTargetInt.x > 0) {
        ballPosition.x = Math.min(ballPosition.x, +fPitchX + fGoalDepth + 0.1);
      } else {
        ballPosition.x = Math.max(ballPosition.x, -fPitchX - fGoalDepth - 0.1);
      }
      ballPosition.z = Math.min(ballPosition.z, fGoalHeight);
    } else if (jShootResult == 2) {
      if (ballTargetInt.x > 0) {
        ballPosition.x = Math.min(ballPosition.x, mshKeeper.position.x - fBallRadius);
      } else {
        ballPosition.x = Math.max(ballPosition.x, mshKeeper.position.x + fBallRadius);
      }
      //ballPosition.z -= 0.02;
    }

    // Move player on pitch
    //console.log("render | update player pos., iPhase: " + iPhase + ", fRelX: " + fRelX);
    if (iPhase < 1 && fRelX < 1.0) {
      if (TeamH) {
        for (let i = 0; i < TeamH.ltPlayer.length; i++) {
          let plH = TeamH.ltPlayer[i];
          //console.log("plH: " + plH);
          if (plH != null) {
            plH.updatePos(
              {
                x: (plH.ptPos.x * fRelX) + (plH.ptPosLast.x * (1 - fRelX)),
                y: (plH.ptPos.y * fRelX) + (plH.ptPosLast.y * (1 - fRelX))
              });
            //console.log("plH ptPos.x: " + plH.ptPos.x, plH.ptPosLast.x, fRelX);
          }
        }
          //console.log(TeamH.ltPlayer[9].ptPos, fRelX);
      }

      if (TeamA) {
        for (let i = 0; i < TeamA.ltPlayer.length; i++) {
          let plA = TeamA.ltPlayer[i];
          if (plA != null) {
            plA.updatePos(
              {
                x: (plA.ptPos.x * fRelX) + (plA.ptPosLast.x * (1 - fRelX)),
                y: (plA.ptPos.y * fRelX) + (plA.ptPosLast.y * (1 - fRelX))
              });
          }
        }
      }
    }

    /*
    if (iShootResult < 0) {
      setCamera(3, posBallCk0);
    }
    */

    // Move keeper
    if (iPhase > 0) {
      if (Math.abs(mshKeeper.position.y) < fGoalWidth / 2 && mshKeeper.position.z < fGoalHeight && jShootResult < 0) {
        let fKeeperSpeed = 0.04;
        if (iShootResult == 2 || iShootResult == 3 || iShootResult == 4) {
          const fBallDeltaXRel = (fRelXOld - fRelX) / fRelXOld; // Relative step of ball towards goal
          const fKeeperDistToBallTarget = Math.max(Math.sqrt(Math.pow(posBall1.y - mshKeeper.position.y, 2) + Math.pow(posBall1.z - mshKeeper.position.z, 2)) - (fKeeperRad * 0.9), 0); // Distance of keeper to ball target position
          fKeeperSpeed = Math.max(fKeeperDistToBallTarget * fBallDeltaXRel, fKeeperSpeed);
        }
        const fAlphaKeeperBall = Math.atan((posBall1.z - mshKeeper.position.z) / (posBall1.y - mshKeeper.position.y));
        //console.log("Keeper pos: " + mshKeeper.position.y.toFixed(5) + "/" + mshKeeper.position.z.toFixed(5) + ", Ball tar pos: " + ballTargetInt.y.toFixed(5) + "/" + ballTargetInt.z.toFixed(5) + ", alpha: " + fAlphaKeeperBall.toFixed(5) + ", " + Math.cos(fAlphaKeeperBall).toFixed(5) + ", " + Math.sin(fAlphaKeeperBall).toFixed(5));

        if      (mshKeeper.position.y < posBall1.y) { mshKeeper.position.y += fKeeperSpeed * Math.abs(Math.cos(fAlphaKeeperBall)); }
        else if (mshKeeper.position.y > posBall1.y) { mshKeeper.position.y -= fKeeperSpeed * Math.abs(Math.cos(fAlphaKeeperBall)); }
        if      (mshKeeper.position.z < posBall1.z) { mshKeeper.position.z += fKeeperSpeed * Math.abs(Math.sin(fAlphaKeeperBall)); }
        else if (mshKeeper.position.z > posBall1.z) { mshKeeper.position.z -= fKeeperSpeed * Math.abs(Math.sin(fAlphaKeeperBall)); }
      }
    }

    if (txtLeft) {
      txtLeft.innerText = "x = " + (fPitchX - ballPosition.x).toFixed(1) + "m, y = " + ballPosition.y.toFixed(1) + "m, z = " + ballPosition.z.toFixed(1) + "m";
    }

    // This should never happen
    if (ballPosition.x < -fPitchX * 2) {
      return false;
    }
    if (ballPosition.x > +fPitchX * 2) {
      return false;
    }

    if ((jShootResult < 2 && ((ballTargetInt.x > 0 && ballPosition.x > fPitchX + fGoalDepth) || (ballTargetInt.x < 0 && ballPosition.x < -fPitchX - fGoalDepth))) ||
        jShootResult == 2 || jShootResult == 3 || jShootResult == 4 || jShootResult == 5 || jShootResult == 6 || jShootResult == 7) {
      //console.log("ballPosition.x: " + ballPosition.x + ", " + fPitchX + fGoalDepth);
      if (checkBounce(-zBall, -zBallLast)) {
        /*
        // Reset
        zBall = 0;
        iBounceCount = 0;
        jShootResult = -1;
        set();
        setRndBallTarget(ballTargetInt, fGoalWidth * 3, fGoalHeight * 2);
        if (fBall1RelY > -1 && fBall1RelY < +1) {
          ballTargetInt.y = fBall1RelY * (fGoalWidth / 2);
        }
        fShootPower = Math.random();
        ballPosition.x = posBallCk0.x;
        ballPosition.y = posBallCk0.y;
        ballPosition.z = posBallCk0.z;

        if (txtRenderShootResult) {
          txtRenderShootResult.innerText = "";
        }
        */

        return false;
      }
    } else if (iShootResult == 7 &&
               ((fBallXOld < ballTargetInt.x && ballPosition.x > ballTargetInt.x) ||
                (fBallXOld > ballTargetInt.x && ballPosition.x < ballTargetInt.x))) {
      jShootResult = iShootResult;

      iPhase = 2;

      if (ballTargetInt.x > posBall0.x) {
        posBall1.x = ballPosition.x - 20;
      } else {
        posBall1.x = ballPosition.x + 20;
      }
      posBall0.x = ballPosition.x;
      posBall0.y = ballPosition.y;
    } else if ((iShootResult == 2 || iShootResult == 3 || iShootResult == 4 || iShootResult > 8) &&
               ((ballTargetInt.x > 0 && ballPosition.x + fBallRadius >= mshKeeper.position.x) ||
                (ballTargetInt.x < 0 && ballPosition.x - fBallRadius <= mshKeeper.position.x)) &&
               checkHit(ballPosition, mshKeeper.position, fKeeperRad)) {
      if (txtRenderShootResult) {
        txtRenderShootResult.innerText = "Gehalten";
      }
      iSaves += 1;
      if (iShootResult > 7) {
        jShootResult = 2;
      } else {
        jShootResult = iShootResult;
      }

      iPhase = 2;

      if (iShootResult == 3) { // Save bounce
        posBall0.x = ballPosition.x;
        posBall0.y = ballPosition.y;
        if (ballTargetInt.x > 0) {
          posBall1.x = ballTargetInt.x - 20;
        } else {
          posBall1.x = ballTargetInt.x + 20;
        }
      } else if (iShootResult == 4) { // Save to cornerkick
        posBall0.x = ballPosition.x;
        posBall0.y = ballPosition.y;
        if (ballTargetInt.x > 0) {
          posBall1.x = ballTargetInt.x - 1;
        } else {
          posBall1.x = ballTargetInt.x + 1;
        }
        if (ballPosition.y > 0) {
          posBall1.y = ballPosition.y + 1;
        } else {
          posBall1.y = ballPosition.y - 1;
        }
      }

      if (txtRight) {
        txtRight.innerHTML = "Schüsse: " + (iAways + iGoals + iSaves + iAlu).toString() + "</br>Tore: " + iGoals.toString() + "</br>Gehalten: " + iSaves.toString() + "</br>Pfosten/Latte: " + iAlu.toString();
      }
    } else if ((iShootResult == 5 ||
               (iShootResult > 8 && Math.abs(ballPosition.y) > (fGoalWidth / 2) - fGoalPostRad &&
                                    Math.abs(ballPosition.y) < (fGoalWidth / 2) + fGoalPostRad &&
                                    ballPosition.z < fGoalHeight)) &&
               ((ballTargetInt.x > 0 && ballPosition.x + fBallRadius >= fPitchX) ||
                (ballTargetInt.x < 0 && ballPosition.x - fBallRadius <= -fPitchX))) {
      if (txtRenderShootResult) {
        txtRenderShootResult.innerText = "Pfosten!";
      }
      iAlu += 1;
      jShootResult = 5;

      iPhase = 2;

      //console.log(posBall0, ballPosition);
      posBall0.x = ballPosition.x;
      posBall0.y = ballPosition.y;
      if (ballTargetInt.x > 0) {
        posBall1.x = ballTargetInt.x - 20;
      } else {
        posBall1.x = ballTargetInt.x + 20;
      }
      posBall1.y = (Math.random() * 30) - 15;

      if (txtRight) {
        txtRight.innerHTML = "Schüsse: " + (iAways + iGoals + iSaves + iAlu).toString() + "</br>Tore: " + iGoals.toString() + "</br>Gehalten: " + iSaves.toString() + "</br>Pfosten/Latte: " + iAlu.toString();
      }
    } else if ((iShootResult == 6 || iShootResult > 8) &&
               ((ballTargetInt.x > 0 && ballPosition.x + fBallRadius >= fPitchX) ||
                (ballTargetInt.x < 0 && ballPosition.x - fBallRadius <= -fPitchX)) &&
               ballPosition.z > fGoalHeight - fGoalPostRad &&
               ballPosition.z < fGoalHeight + fGoalPostRad &&
               Math.abs(ballPosition.y) < fGoalWidth / 2) {
      if (txtRenderShootResult) {
        txtRenderShootResult.innerText = "Latte!";
      }
      iAlu += 1;
      jShootResult = 6;

      iPhase = 2;

      posBall0.x = ballPosition.x;
      posBall0.y = ballPosition.y;
      if (ballTargetInt.x > 0) {
        posBall1.x = ballTargetInt.x - 20;
      } else {
        posBall1.x = ballTargetInt.x + 20;
      }
      //posBall1.y = (Math.random() * 30) - 15;

      if (txtRight) {
        txtRight.innerHTML = "Schüsse: " + (iAways + iGoals + iSaves + iAlu).toString() + "</br>Tore: " + iGoals.toString() + "</br>Gehalten: " + iSaves.toString() + "</br>Pfosten/Latte: " + iAlu.toString();
      }
    } else if ((iShootResult == 1 || iShootResult > 8) &&
               ((ballTargetInt.x > 0 && ballPosition.x > +fPitchX && xBallLast < +fPitchX) ||
                (ballTargetInt.x < 0 && ballPosition.x < -fPitchX && xBallLast > -fPitchX))) {
      if (iShootResult == 1 || (iShootResult > 8 && checkOnGoal(ballPosition, false))) {
        if (txtRenderShootResult) {
          txtRenderShootResult.innerText = "Tor!";
        }
        iGoals += 1;
        jShootResult = 1;
      } else {
        if (txtRenderShootResult) {
          txtRenderShootResult.innerText = Math.abs(ballPosition.y) < fGoalWidth / 2 ? "Drüber..." : "Daneben...";
        }
        iAways += 1;
        jShootResult = 0;
      }

      if (txtRight) {
        txtRight.innerHTML = "Schüsse: " + (iAways + iGoals + iSaves + iAlu).toString() + "</br>Tore: " + iGoals.toString() + "</br>Gehalten: " + iSaves.toString() + "</br>Pfosten/Latte: " + iAlu.toString();
      }
    }

    xBallLast = ballPosition.x;
    zBallLast = zBall;

    // Cloth
    for (let h = 0; h < ltMeshGoalHA.length; h++) {
      var mshGoal = ltMeshGoalHA[h];

      const particles = mshGoal.cloth.particles;

      for (let i = 0, il = particles.length; i < il; i++) {
        const particle = particles[i];
        particle.addForce(gravity);

        particle.integrate(TIMESTEP_SQ);
      }

      // Start Constraints
      const constraints = mshGoal.cloth.constraints;
      const il = constraints.length;

      for (let i = 0; i < il; i++) {
        const constraint = constraints[i];
        satisfyConstraints(constraint[0], constraint[1], constraint[2]);
      }

      // Ball Constraints
      for (let i = 0, il = particles.length; i < il; i++) {
        const particle = particles[i];
        const pos = particle.position;
        diff.subVectors(pos, ballPosition);
        if (diff.length() < fBallRadius * fBallMeshEffect) {
          // collided
          diff.normalize().multiplyScalar(fBallRadius * fBallMeshEffect);
          pos.copy(ballPosition).add(diff);
        }

        if (particle.fixed) {
          particle.position.copy(particle.original);
          particle.previous.copy(particle.original);
        }
      }

      // Floor Constraints
      for (let i = 0, il = particles.length; i < il; i++) {
        const particle = particles[i];
        const pos = particle.position;
        if (pos.z < 0) {
          pos.z = 0;
        }
      }
    }

    //camera.lookAt(mshBall.position);

    if (!controls) { setCamera(3, posBallCk0); }
    if (controls) { controls.update(); }

    // functionality provided by THREEx.KeyboardState.js
    if (keyboard.pressed) {
      //console.log(keyboard.pressed);
      if (keyboard.pressed("1")) {
        setCamera(1, posBallCk0);
      } else if (keyboard.pressed("2")) {
        setCamera(2);
      } else if (keyboard.pressed("3")) {
        setCamera(3);
      } else if (keyboard.pressed("enter")) {
        animate(posBallCk0, null, false, null);
      }
    }

    return true;
  }; // update()

  function satisfyConstraints(p1, p2, distance) {
    diff.subVectors(p2.position, p1.position);
    const currentDist = diff.length();
    if (currentDist === 0) return; // prevents division by 0
    const correction = diff.multiplyScalar(1 - distance / currentDist);
    const correctionHalf = correction.multiplyScalar(0.5);
    p1.position.add(correctionHalf);
    p2.position.sub(correctionHalf);
  }

  const checkBounce = function (zBall, zBallLast) {
    if (Math.sin(zBall) < 0 && Math.sin(zBallLast) >= 0) {
      return true;
    } else if (Math.sin(zBall) > 0 && Math.sin(zBallLast) <= 0) {
      return true;
    }

    return false;
  }

  const checkHit = function (posBall, posKeeper, fKeeperRad) {
    return Math.sqrt(Math.pow(posBall.y - posKeeper.y, 2) + Math.pow(posBall.z - posKeeper.z, 2)) < fKeeperRad;
  }

  const checkOnGoal = function (ballPosition, bInclPost = false) {
    if (bInclPost) {
      return Math.abs(ballPosition.y) < (fGoalWidth / 2) + fGoalPostRad && ballPosition.z < fGoalHeight + fGoalPostRad;
    }

    return Math.abs(ballPosition.y) < (fGoalWidth / 2) - fGoalPostRad && ballPosition.z < fGoalHeight - fGoalPostRad;
  }
  this.checkOnGoal = checkOnGoal;

  const set = function (posBallCk0, posBallCkm1, ballTarget, ballPosition) {
    if (!posBallCk0) { return; }

    // Set time to 0
    t = 0;

    if (stats.FramesPerSec) {
      fFramesPerSec = stats.FramesPerSec;
    }

    // Set ball initial position
    if (posBallCkm1 &&
        !(Math.abs(posBallCk0.x - posBallCkm1.x) < 0.01 &&
          Math.abs(posBallCk0.y - posBallCkm1.y) < 0.01 &&
          Math.abs(posBallCk0.z - posBallCkm1.z) < 0.01)) {
      posBall0 = {
        x: convertXToRenderCoord(posBallCkm1.x),
        y: posBallCkm1.y,
        z: posBallCkm1.z
      };

      iPhase = 0;

      fShootPower = 0;
    } else {
      posBall0 = {
        x: convertXToRenderCoord(posBallCk0.x),
        y: posBallCk0.y,
        z: posBallCk0.z
      };

      iPhase = 1;

      fShootPower = Math.random();
    }

    posBall1 = {
      x: convertXToRenderCoord(posBallCk0.x),
      y: posBallCk0.y,
      z: posBallCk0.z
    };

    Object.assign(ballPosition, posBall0);

    //console.log(" Set ball pos. to: " + posBallCk0.x + ", " + ballPosition.x.toFixed(5) + ", " + ballPosition.y.toFixed(5) + ", " + ballPosition.z.toFixed(5) + ", shoot power: " + fShootPower.toFixed(5) + ", iPhase: " + iPhase);

    alpha_Ball = -1; // Recalculate alpha
    fBallPositionZ0 = ballPosition.z;
    fBallDamping = 0;

    //console.log("  ballTarget: " + JSON.stringify(ballTargetInt, null, 4));
    if (ballTarget) {
      // Set x ball target
      ballTargetInt.x = ballTarget.x;

      // Set y ball target
      if (ballTarget.y > -fPitchY && ballTarget.y < +fPitchY) {
        ballTargetInt.y = ballTarget.y;
      }

      // Set z ball target
      if (ballTarget.z > 0) {
        ballTargetInt.z = ballTarget.z;
      }

      //console.log("  Ball target set to: " + ballTargetInt.x + ", " + ballTargetInt.y + ", " + ballTargetInt.z);
    }

    // Set active goal
    ltMeshGoalHA = ltMeshGoals[ballTarget.x > 0 ? 0 : 1];

    // Keeper
    mshKeeperH.position.y = 0;
    mshKeeperH.position.z = 0.8;
    mshKeeperA.position.y = 0;
    mshKeeperA.position.z = 0.8;

    if (!camera) {
      setCamera(1, posBallCk0);
    }

    iBounceCount = 0;
    jShootResult = -1;

    if (txtRenderShootResult) {
      txtRenderShootResult.innerText = "";
    }
  } // set

  const setCamera = function (iType, posBallCk) {
    if (!camera) {
      console.log("Create new camera");

      camera = new THREE.PerspectiveCamera(60, iSceneWidth / iSceneHeight, 0.1, 1000);
    }

    //////////////
    // CONTROLS //
    //////////////
    // move mouse and: left   click to rotate,
    //                 middle click to zoom,
    //                 right  click to pan
    //controls = new THREE.OrbitControls(camera, renderer.domElement);

    camera.up = new THREE.Vector3(0, 0, 1);

    if (iType == 1 && posBallCk) {
      //console.log("Set camera to type: " + iType);

      let posBall = { x: convertXToRenderCoord(posBallCk.x), y: posBallCk.y };
      //console.log("  posBall.x: " + posBall.x);
      if (posBall.x < 0) {
        camera.position.x = posBall.x + 5;
        camera.lookAt(new THREE.Vector3(-fPitchX - fGoalDepth, 0, 0));
        //controls.target = new THREE.Vector3(-fPitchX - fGoalDepth, 0, 0);
      } else {
        camera.position.x = posBall.x - 5;
        camera.lookAt(new THREE.Vector3(+fPitchX + fGoalDepth, 0, 0));
        //controls.target = new THREE.Vector3(+fPitchX + fGoalDepth, 0, 0);
      }
      camera.position.y = posBall.y - ((camera.position.x - posBall.x) / (fPitchX - posBall.x));
      camera.position.z = 3;
    } else if (iType == 2) {
      //console.log("Set camera to type: " + iType);

      camera.position.set(
        fPitchX + 10,
        0,
        5
      );
      camera.lookAt(new THREE.Vector3(fPitchX / 2, 0, 0));
      //controls.target = new THREE.Vector3(fPitchX / 2, 0, 0);
    } else if (iType == 3) {
      //console.log("Set camera to type: " + iType);

      camera.position.set(
        0,
        (fPitchY / 2) + 20,
        25
      );

      if (posBallCk) {
        let posBall = { x: convertXToRenderCoord(posBallCk.x), y: posBallCk.y };
        camera.lookAt(new THREE.Vector3(posBall.x, posBall.y, posBall.z));
        //console.log(" lookAt.x: " + posBall.x);
      } else {
        camera.lookAt(new THREE.Vector3(0, 0, 0));
      }
      //controls.target = new THREE.Vector3(0, 0, 0);
      /*
      camera.position.set(0, fPitchY / 2, 1);
      camera.lookAt(new THREE.Vector3(1, 0, 0));
      controls.target = new THREE.Vector3(0, fPitchY / 2, 1);
      */
    }
  }
  this.setCamera = setCamera;

  function animate2(ballPosition, posBallCk0, posBallCkm1, iShootResult = -1, fRnd = 0, iSideY = 1, ballTarget = null, bLoop = false, _callbackFct = null) {
    if (update(ballPosition, posBallCk0, posBallCkm1, iShootResult, TeamH, TeamA)) {
      requestAnimationFrame(function () { animate2(ballPosition, posBallCk0, posBallCkm1, iShootResult, fRnd, iSideY, ballTarget, bLoop, _callbackFct); });
      render(ballPosition);
      return;
    }

    if (_callbackFct) {
      _callbackFct();
    }

    if (bLoop) {
      animate(posBallCk0, posBallCkm1, iShootResult, fRnd, iSideY, ballTarget, bLoop, _callbackFct);
    }
  };

  function animate(posBallCk0, posBallCkm1, iShootResult = -1, fRnd = 0, iSideY = 1, ballTarget = null, bLoop = false, _callbackFct = null) {
    let sDebugInfo = "  Animate (shoot result: " + iShootResult;
    if (ballTarget) {
      sDebugInfo += ", ballTarget: " + ballTarget.x + "/" + ballTarget.y + "/" + ballTarget.z;
    }
    sDebugInfo += ", loop: " + bLoop + ") ...";
    console.log(sDebugInfo);

    if (iShootResult >= 0 && (!ballTarget || bLoop)) {
      //jShootResult = iShootResult;
      let iSideX = posBallCk0.x > fPitchX ? +1 : -1;
      if (iShootResult > 7) {
        ballTarget = getRndBallTarget(fGoalWidth * 3, fGoalHeight * 1.5, iSideX);
      } else if (!ballTarget) {
        ballTarget = getBallTarget(iShootResult, fRnd, iSideX, iSideY);
      }
    }

    //TeamH = this.TeamH;
    //TeamA = this.TeamA;

    set(posBallCk0, posBallCkm1, ballTarget, ballPosition);
    animate2(ballPosition, posBallCk0, posBallCkm1, iShootResult, fRnd, iSideY, ballTarget, bLoop, _callbackFct);
  }
  this.animate = animate;

  // Convert to render coord. system
  const convertXToRenderCoord = function (posCkX) {
    var iPosRenderX = posCkX;
    iPosRenderX *= fPitchX / 61;
    iPosRenderX -= fPitchX;
    return iPosRenderX;
  }
  this.convertXToRenderCoord = convertXToRenderCoord;

  const getBallTarget = function (iShootResult, fRnd = -1, iSideX = 1, iSideY = 0) {
    if (iShootResult < 0) {
      return null;
    }

    let ballTargetTmp = { x: iSideX * fPitchX, y: 0, z: 0 };

    if (fRnd < 0) {
      fRnd = Math.random();
    }

    if (iShootResult == 0) {
      ballTargetTmp.y = ((fPitchY - ((fGoalWidth / 2) + fGoalPostRad)) * fRnd) + (fGoalWidth / 2) + fGoalPostRad;
    } else if (iShootResult < 5) {
      ballTargetTmp.y = (fGoalWidth / 2) * fRnd;
      ballTargetTmp.z = fGoalHeight * fRnd;
    } else if (iShootResult == 5) {
      ballTargetTmp.y = fGoalWidth / 2;
      ballTargetTmp.z = fGoalHeight * fRnd;
    } else if (iShootResult == 6) {
      ballTargetTmp.y = (fGoalWidth / 2) * fRnd;
      ballTargetTmp.z = fGoalHeight;
    }

    if (iSideY == 0) {
      iSideY = Math.random() < 0.5 ? -1 : +1;
    }

    ballTargetTmp.y *= iSideY;

    return ballTargetTmp;
  }
  this.getBallTarget = getBallTarget;

  const render = function (ballPosition) {
    //for (mshGoal in ltMeshGoalHA) {
    for (let h = 0; h < ltMeshGoalHA.length; h++) {
      var mshGoal = ltMeshGoalHA[h];

      const p = mshGoal.cloth.particles;

      for (let i = 0, il = p.length; i < il; i++) {
        const v = p[i].position;

        mshGoal.geom.attributes.position.setXYZ(i, v.x, v.y, v.z);
      }

      mshGoal.geom.attributes.position.needsUpdate = true;
      mshGoal.geom.computeVertexNormals();
    }
    mshBall.position.copy(ballPosition);

    renderer.render(scene, camera);

    stats.update();
  }

  const getRndBallTarget = function (fMarginY, fMarginZ, iSideX = 1) {
    let ballTargetTmp = { x: iSideX * fPitchX, y: 0, z: 0 };

    ballTargetTmp.y = ((Math.random() * fMarginY) + (Math.random() * fMarginY) + (Math.random() * fMarginY)) / 3;
    ballTargetTmp.z = Math.random() * fMarginZ;
    ballTargetTmp.y -= fMarginY / 2;

    return ballTargetTmp;
  }
  this.getRndBallTarget = getRndBallTarget;

  init(ltStadiumBlocks);
  //setCamera(1);
  //camera = ltCamera[0];
  //set(posBallCk0);
  //animate();

  this.TeamH = TeamH;
  this.TeamA = TeamA;
}

function createStats() {
  var stats = new Stats();
  stats.setMode(0);

  stats.domElement.style.position = 'absolute';
  stats.domElement.style.left = '0';
  stats.domElement.style.top = '0';

  return stats;
}

export { Render3D };
