const fPitchX = 61;
const fPitchY = 80;
const fGroundX = fPitchX * 1.2;
const fGroundY = fPitchY * 1.2;

const fGoalWidth = 7.32; // 8 yards
const fGoalHeight = 2.44; // 8 feet
const fGoalDepth = 2;
const fGoalPostRad = 0.06;

const ySegs = 20;
const zSegs = parseInt(Math.round(ySegs * (fGoalHeight / fGoalWidth)));
//console.log("Segments: " + ySegs + ", " + zSegs);
const restDistanceWidth = fGoalWidth / ySegs;
const restDistanceHeight = fGoalHeight / zSegs;
//const restDistanceWidth = 1;
//const restDistanceHeight = 1;
const ySegsSide = 2;

const DAMPING = 0.03;
const DRAG = 1 - DAMPING;
const MASS = 0.1;
const GRAVITY = 9.81;

const sImgDir = "./_content/CornerkickApp.Components/Content/Images/render/"

class Particle {
  constructor(x, y, z, mass, planeFunction, fixed) {
    this.position = new THREE.Vector3();
    this.previous = new THREE.Vector3();
    this.original = new THREE.Vector3();
    this.a = new THREE.Vector3(0, 0, 0); // acceleration
    this.mass = mass;
    this.invMass = 1 / mass;
    this.tmp = new THREE.Vector3();
    this.tmp2 = new THREE.Vector3();
    this.fixed = fixed;

    // init
    planeFunction(x, y, this.position); // position
    planeFunction(x, y, this.previous); // previous
    planeFunction(x, y, this.original);
  }

  // Force -> Acceleration
  addForce(force) {
    this.a.add(
      this.tmp2.copy(force).multiplyScalar(this.invMass)
    );
  }

  // Performs Verlet integration
  integrate(timesq) {
    const newPos = this.tmp.subVectors(this.position, this.previous);
    newPos.multiplyScalar(DRAG).add(this.position);
    newPos.add(this.a.multiplyScalar(timesq));

    this.tmp = this.previous;
    this.previous = this.position;
    this.position = newPos;

    this.a.set(0, 0, 0);
  }
}

class Goal {
  constructor(iSide = 1) {
    const matGoal = new THREE.MeshStandardMaterial({ color: 0xffffff, roughness: 0.5, metalness: 0.25 });
    const geomPost = new THREE.CylinderGeometry(fGoalPostRad, fGoalPostRad, fGoalHeight, 32);
    this.mshPostL = new THREE.Mesh(geomPost, matGoal);
    this.mshPostL.castShadow = true;
    this.mshPostL.rotation.x = 90 * (Math.PI / 180);
    this.mshPostL.position.x = iSide * fPitchX;
    this.mshPostL.position.y = fGoalWidth / 2;
    this.mshPostL.position.z = fGoalHeight / 2;
    this.mshPostR = new THREE.Mesh(geomPost, matGoal);
    this.mshPostR.castShadow = true;
    this.mshPostR.rotation.x = 90 * (Math.PI / 180);
    this.mshPostR.position.x = iSide * fPitchX;
    this.mshPostR.position.y = -fGoalWidth / 2;
    this.mshPostR.position.z = fGoalHeight / 2;
    const geomBar = new THREE.CylinderGeometry(fGoalPostRad, fGoalPostRad, fGoalWidth, 32);
    this.mshBar = new THREE.Mesh(geomBar, matGoal);
    this.mshBar.castShadow = true;
    //mshBar.rotation.x = 90 * (Math.PI / 180);
    this.mshBar.position.x = iSide * fPitchX;
    this.mshBar.position.y = 0;
    this.mshBar.position.z = fGoalHeight;
    this.ltMeshGoal = [];

    /////////////////////////////////////////////////////////////////////////////////
    // Mesh back
    /////////////////////////////////////////////////////////////////////////////////
    const txtLoader = new THREE.TextureLoader();
    const txtMesh = txtLoader.load(sImgDir + "mesh_pattern.png");
    //txtMesh.anisotropy = 16;
    txtMesh.wrapS = THREE.RepeatWrapping;
    txtMesh.wrapT = THREE.RepeatWrapping;
    txtMesh.repeat.set(parseInt(fGoalWidth * 6), parseInt(fGoalHeight * 6));

    // Mesh material
    const matMesh = new THREE.MeshBasicMaterial({
      map: txtMesh,
      side: THREE.DoubleSide,
      alphaTest: 0.5
    });

    // Mesh geometry
    const plnFctMesh = plane(fGoalWidth, fGoalHeight, iSide * (fPitchX + fGoalDepth), -fGoalWidth / 2, 0, 0);
    const cloth = new Cloth(ySegs, zSegs, plnFctMesh);
    const geomGoalMesh = new THREE.ParametricBufferGeometry(plnFctMesh, cloth.w, cloth.h);
    this.ltMeshGoal.push(new MeshGoal(cloth, geomGoalMesh));

    // cloth mesh
    this.mshGoalMesh = new THREE.Mesh(geomGoalMesh, matMesh);
    this.mshGoalMesh.position.set(0, 0, 0);
    //mshGoalMesh.castShadow = true;

    /////////////////////////////////////////////////////////////////////////////////
    // Mesh right
    /////////////////////////////////////////////////////////////////////////////////
    const txtMeshR = txtLoader.load(sImgDir + "mesh_pattern.png");
    //txtMesh.anisotropy = 16;
    txtMeshR.wrapS = THREE.RepeatWrapping;
    txtMeshR.wrapT = THREE.RepeatWrapping;
    txtMeshR.repeat.set(parseInt(fGoalDepth * 6), parseInt(fGoalHeight * 6));
    const matMeshR = new THREE.MeshBasicMaterial({
      map: txtMeshR,
      side: THREE.DoubleSide,
      alphaTest: 0.5
    });

    // Mesh geometry
    const plnFctMeshR = plane(iSide * fGoalDepth, fGoalHeight, iSide * fPitchX, +fGoalWidth / 2, 0, 1);
    const clothSideR = new Cloth(ySegsSide, zSegs, plnFctMeshR);
    const geomGoalMeshR = new THREE.ParametricBufferGeometry(plnFctMeshR, clothSideR.w, clothSideR.h);
    this.ltMeshGoal.push(new MeshGoal(clothSideR, geomGoalMeshR));

    // cloth mesh
    this.mshGoalMeshR = new THREE.Mesh(geomGoalMeshR, matMeshR);
    this.mshGoalMeshR.position.set(0, 0, 0);
    //mshGoalMesh.castShadow = true;

    /////////////////////////////////////////////////////////////////////////////////
    // Mesh left
    /////////////////////////////////////////////////////////////////////////////////
    const txtMeshL = txtLoader.load(sImgDir + "mesh_pattern.png");
    //txtMesh.anisotropy = 16;
    txtMeshL.wrapS = THREE.RepeatWrapping;
    txtMeshL.wrapT = THREE.RepeatWrapping;
    txtMeshL.repeat.set(parseInt(fGoalDepth * 6), parseInt(fGoalHeight * 6));
    const matMeshL = new THREE.MeshBasicMaterial({
      map: txtMeshL,
      side: THREE.DoubleSide,
      alphaTest: 0.5
    });

    // Mesh geometry
    const plnFctMeshL = plane(iSide * fGoalDepth, fGoalHeight, iSide * fPitchX, -fGoalWidth / 2, 0, 1);
    const clothSideL = new Cloth(ySegsSide, zSegs, plnFctMeshL);
    const geomGoalMeshL = new THREE.ParametricBufferGeometry(plnFctMeshL, clothSideL.w, clothSideL.h);
    this.ltMeshGoal.push(new MeshGoal(clothSideL, geomGoalMeshL));

    // cloth mesh
    this.mshGoalMeshL = new THREE.Mesh(geomGoalMeshL, matMeshL);
    this.mshGoalMeshL.position.set(0, 0, 0);
    //mshGoalMesh.castShadow = true;

    /////////////////////////////////////////////////////////////////////////////////
    // Mesh top (no cloth)
    /////////////////////////////////////////////////////////////////////////////////
    const txtMeshT = txtLoader.load(sImgDir + "mesh_pattern.png");
    //txtMesh.anisotropy = 16;
    txtMeshT.wrapS = THREE.RepeatWrapping;
    txtMeshT.wrapT = THREE.RepeatWrapping;
    txtMeshT.repeat.set(parseInt(fGoalDepth * 6), parseInt(fGoalWidth * 6));
    const matMeshT = new THREE.MeshBasicMaterial({
      map: txtMeshT,
      side: THREE.DoubleSide,
      alphaTest: 0.5
    });

    // Mesh geometry
    const geomGoalMeshT = new THREE.PlaneGeometry(fGoalDepth, fGoalWidth);
    /*
    const plnFctMeshT = plane(fGoalDepth, fGoalWidth, fPitchX, -fGoalWidth / 2, fGoalHeight, 2);
    const clothSideT = new Cloth(ySegs, ySegsSide, plnFctMeshT);
    const geomGoalMeshT = new THREE.ParametricBufferGeometry(plnFctMeshT, clothSideT.w, clothSideT.h);
    ltMeshGoal.push(new MeshGoal(clothSideT, geomGoalMeshT));
    */

    // cloth mesh
    this.mshGoalMeshT = new THREE.Mesh(geomGoalMeshT, matMeshT);
    this.mshGoalMeshT.position.set(iSide * (fPitchX + (fGoalDepth / 2)), 0, fGoalHeight);
    //mshGoalMesh.castShadow = true;
  }

  addToScene(sceneTmp) {
    sceneTmp.add(this.mshPostL);
    sceneTmp.add(this.mshPostR);
    sceneTmp.add(this.mshBar);
    sceneTmp.add(this.mshGoalMesh);
    sceneTmp.add(this.mshGoalMeshR);
    sceneTmp.add(this.mshGoalMeshL);
    sceneTmp.add(this.mshGoalMeshT);
  }
}

class MeshGoal {
  constructor(cloth, geom) {
    this.cloth = cloth;
    this.geom = geom;
  }
}

class Cloth {
  constructor(w, h, planeFunction) {
    this.w = w;
    this.h = h;

    const particles = [];
    const constraints = [];

    // Create particles
    for (let v = 0; v <= h; v++) {
      for (let u = 0; u <= w; u++) {
        particles.push(
          new Particle(u / w, v / h, 0, MASS, planeFunction, v == h || u == 0 || u == w)
        );
      }
    }

    // Structural
    for (let v = 0; v < h; v++) {
      for (let u = 0; u < w; u++) {
        constraints.push([
          particles[index(u, v)],
          particles[index(u, v + 1)],
          restDistanceHeight
        ]);

        constraints.push([
          particles[index(u, v)],
          particles[index(u + 1, v)],
          restDistanceWidth
        ]);
      }
    }

    for (let u = w, v = 0; v < h; v++) {
      constraints.push([
        particles[index(u, v)],
        particles[index(u, v + 1)],
        restDistanceHeight
      ]);
    }

    for (let v = h, u = 0; u < w; u++) {
      constraints.push([
        particles[index(u, v)],
        particles[index(u + 1, v)],
        restDistanceWidth
      ]);
    }

    this.particles = particles;
    this.constraints = constraints;

    function index(u, v) {
      return u + v * (w + 1);
    }

    this.index = index;
  }
}

function plane(U, V, x0, y0, z0, iNormal = 0) {
  return function (u, v, target) {
    var x = x0;
    var y = y0;
    var z = z0;

    if (iNormal == 0) {
      y += u * U;
      z += v * V;
    } else if (iNormal == 1) {
      x += u * U;
      z += v * V;
    } else if (iNormal == 2) {
      x += u * U;
      y += v * V;
      console.log("plane: x=" + x.toFixed(3) + ", y=" + y.toFixed(3) + ", z=" + z.toFixed(3));
    }

    //console.log("plane: x=" + x.toFixed(3) + ", y=" + y.toFixed(3) + ", z=" + z.toFixed(3));

    target.set(x, y, z);
  };
}

class Board {
  constructor() {
    const fBoardWidth = 10;
    const fBoardHeight = 1.5;

    const matBoard = new THREE.MeshStandardMaterial({ color: 0x7f7f7f });
    const geomBoard = new THREE.BoxGeometry(fBoardWidth, 0.1, fBoardHeight);
    geomBoard.translate(0, 0, -fBoardHeight / 2);

    const txtLoader = new THREE.TextureLoader();
    const txtSponsor = txtLoader.load("/content/Images/sponsors/1.png");
    const matSponsor = new THREE.MeshStandardMaterial({ map: txtSponsor });

    this.msh = new THREE.Mesh(geomBoard, matSponsor);
    this.msh.castShadow = true; // default is false
    this.msh.receiveShadow = true;
    this.msh.rotation.y = +180 * (Math.PI / 180);
    this.msh.rotation.x = +10 * (Math.PI / 180);
  }
}

class Ground {
  constructor(scene) {
    const geomGround = new THREE.PlaneGeometry(fGroundX * 6, fGroundY * 4);
    const matGround = new THREE.MeshStandardMaterial({ color: 0x4d4c38/*color: 0x7f7f7f*/ });
    const mshGround = new THREE.Mesh(geomGround, matGround);
    mshGround.position.x = 0;
    mshGround.position.z = -0.05;
    mshGround.castShadow = false;
    mshGround.receiveShadow = true;
    scene.add(mshGround);

    const matWall = new THREE.MeshStandardMaterial({ color: 0x7f7f7f });

    const geomWallFront = new THREE.BoxGeometry(0.2, fGroundY, 1);
    const geomWallSide = new THREE.BoxGeometry(0.2, 2 * fGroundX, 1);

    const mshWall1 = new THREE.Mesh(geomWallFront, matWall);
    mshWall1.position.x = +fGroundX;
    mshWall1.position.z = 0.5 - 0.05;
    mshWall1.castShadow = true; // default is false
    mshWall1.receiveShadow = true;
    scene.add(mshWall1);

    const mshWall2 = new THREE.Mesh(geomWallFront, matWall);
    mshWall2.position.x = -fGroundX;
    mshWall2.position.z = 0.5 - 0.05;
    mshWall2.castShadow = true; // default is false
    mshWall2.receiveShadow = true;
    scene.add(mshWall2);

    const mshWall3 = new THREE.Mesh(geomWallSide, matWall);
    mshWall3.rotation.z = +90 * (Math.PI / 180);
    mshWall3.position.y = +fGroundY * 0.5;
    mshWall3.position.z = 0.5 - 0.05;
    mshWall3.castShadow = true; // default is false
    mshWall3.receiveShadow = true;
    scene.add(mshWall3);

    const mshWall4 = new THREE.Mesh(geomWallSide, matWall);
    mshWall4.rotation.z = +90 * (Math.PI / 180);
    mshWall4.position.y = -fGroundY * 0.5;
    mshWall4.position.z = 0.5 - 0.05;
    mshWall4.castShadow = true; // default is false
    mshWall4.receiveShadow = true;
    scene.add(mshWall4);

    // Add sponsor boards
    for (let iSideX = -1; iSideX < 2; iSideX += 2) {
      for (let iBoard = 0; iBoard < 6; iBoard++) {
        let board = new Board();
        board.msh.rotateOnWorldAxis(new THREE.Vector3(0, 0, 1), iSideX * 90 * (Math.PI / 180));
        board.msh.position.x = iSideX * (fPitchX + 8);
        board.msh.position.y = -30 + (iBoard * 12);
        scene.add(board.msh);
      }
    }
  }
}

class Pitch {
  constructor(scene) {
    const fPitchSideX = 2.5;
    const fPitchSideY = 1;
    const txtLoader = new THREE.TextureLoader();
    const geomPitch = new THREE.PlaneGeometry(fPitchY + (2 * fPitchSideY), fPitchX + fPitchSideX);
    //const txtPitch = new THREE.TextureLoader().load("/Content/Images/stadium/field.png");
    const txtPitch = txtLoader.load(sImgDir + "football_pitch-half.jpg");
    const matPitch = new THREE.MeshLambertMaterial({ map: txtPitch });
    //const matPitch = new THREE.MeshLambertMaterial({ color: 0xffff00 });
    const mshPitchL = new THREE.Mesh(geomPitch, matPitch);
    mshPitchL.receiveShadow = true;
    mshPitchL.rotation.z = +90 * (Math.PI / 180);
    mshPitchL.position.x = (fPitchX + fPitchSideX) / 2;
    scene.add(mshPitchL);
    const mshPitchR = new THREE.Mesh(geomPitch, matPitch);
    mshPitchR.receiveShadow = true;
    mshPitchR.rotation.z = -90 * (Math.PI / 180);
    mshPitchR.position.x = -(fPitchX + fPitchSideX) / 2;
    scene.add(mshPitchR);
  }
}

class Blocks {
  constructor(scene, ltBlocks) {
    const fStadiumBlockHeightFront = (2 * fGroundX) / 3;
    const fStadiumBlockCornerExtensionFactor = 1.25;
    const fStadiumBlockSteepAngle = 40;
    const fRoofHeight = 0.2;
    const fRoofWidthBasis = 4;
    const fTopringGap = 20; // Topring blocks will start 20m behind lower blocks
    const txtLoader = new THREE.TextureLoader();
    const clBlock = 0x7f7f7f;
    const clRoof = 0x3d3d3d;
    const clOutsideWall = clBlock;
    const clTest = 0xff0000;

    // Materials
    const matBlock = new THREE.MeshStandardMaterial({ color: clBlock/*, side: THREE.DoubleSide*/ });
    const matRoof = new THREE.MeshStandardMaterial({ color: clRoof });
    const matOutsideWall = new THREE.MeshStandardMaterial({ color: clOutsideWall });

    if (!scene) {
      return;
    }

    var bTopring = false;
    if (ltBlocks) {
      for (let iB = 10; iB < 24; iB++) {
        if (iB >= ltBlocks.length) { break; }

        if (ltBlocks[iB][0] > 0) {
          bTopring = true;
          break;
        }
      }
    }

    var fTopringHeight = 0;
    if (ltBlocks && bTopring) {
      for (let iB = 0; iB < 10; iB++) {
        if (iB >= ltBlocks.length) { break; }

        fTopringHeight = Math.max(fTopringHeight, ltBlocks[iB][0]);
      }

      // Scale topring height by block steep angle
      fTopringHeight = fTopringHeight * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180));
    }

    // Predefine lower block connectors
    var geomOutsideWallCon1 = new THREE.BufferGeometry();
    geomOutsideWallCon1.setAttribute('position', new THREE.BufferAttribute(new Float32Array(6 * 3), 3));
    geomOutsideWallCon1.attributes.position.needsUpdate = true;
    const mshOutsideWallCon1 = new THREE.Mesh(geomOutsideWallCon1, matOutsideWall);
    scene.add(mshOutsideWallCon1);

    var geomOutsideWallCon2 = new THREE.BufferGeometry();
    geomOutsideWallCon2.setAttribute('position', new THREE.BufferAttribute(new Float32Array(6 * 3), 3));
    geomOutsideWallCon2.attributes.position.needsUpdate = true;
    geomOutsideWallCon2.computeVertexNormals();
    const mshOutsideWallCon2 = new THREE.Mesh(geomOutsideWallCon2, matOutsideWall);
    scene.add(mshOutsideWallCon2);

    var geomOutsideWallCon3 = new THREE.BufferGeometry();
    geomOutsideWallCon3.setAttribute('position', new THREE.BufferAttribute(new Float32Array(6 * 3), 3));
    geomOutsideWallCon3.attributes.position.needsUpdate = true;
    geomOutsideWallCon3.computeVertexNormals();
    const mshOutsideWallCon3 = new THREE.Mesh(geomOutsideWallCon3, matOutsideWall);
    scene.add(mshOutsideWallCon3);

    var geomOutsideWallCon4 = new THREE.BufferGeometry();
    geomOutsideWallCon4.setAttribute('position', new THREE.BufferAttribute(new Float32Array(6 * 3), 3));
    geomOutsideWallCon4.attributes.position.needsUpdate = true;
    geomOutsideWallCon4.computeVertexNormals();
    const mshOutsideWallCon4 = new THREE.Mesh(geomOutsideWallCon4, matOutsideWall);
    scene.add(mshOutsideWallCon4);

    // Create blocks
    for (let iB = 0; iB < 24; iB++) {
      var fStadiumSideBlockX = fGroundX;
      var fStadiumBlockWidthFront = fGroundY / 2;
      console.log(iB);

      if (iB > 9) {
        if (!bTopring) { break; }

        fStadiumSideBlockX += fTopringGap;
        fStadiumBlockWidthFront += fTopringGap;
      }

      let block = [0, 0, 0, 0]; // Block height, block type, crowd height, roof
      if (ltBlocks && ltBlocks.length > iB) {
        block = ltBlocks[iB];
      }
      if (block[0] < 1) {
        continue;
      }

      let fStadiumBlockWidthCorner = block[0] * 0.5;

      const fBlockWidth = block[0] * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180));

      let fRoofWidth = fRoofWidthBasis;
      if (iB < 10 && bTopring) { fRoofWidth = fBlockWidth + fRoofWidthBasis + 2 - fTopringGap; } // If topring
      else if (block[3] > 0) { fRoofWidth = fBlockWidth + fRoofWidthBasis + 2; } // If roof

      var geomBlock;
      var geomCrowd;
      var geomRoof;
      var geomOutsideWall;

      var txtCrowd = null;
      var txtCrowd2 = null;
      if (block[1] === 2) {
        txtCrowd = txtLoader.load(sImgDir + "stadium_VIP_box.png");
        txtCrowd2 = txtLoader.load(sImgDir + "stadium_VIP_box.png");
      } else {
        txtCrowd = txtLoader.load(sImgDir + "crowd.jpg");
        txtCrowd2 = txtLoader.load(sImgDir + "crowd.jpg");
      }

      var txtSeats = null;
      if (block[1] === 0) {
        txtSeats = txtLoader.load(sImgDir + "bench.png");
      } else if (block[1] === 1) {
        txtSeats = txtLoader.load(sImgDir + "seat.png");
      } else if (block[1] === 2) {
        txtSeats = txtLoader.load(sImgDir + "stadium_VIP_box_empty.png");
      }

      txtCrowd.wrapS = THREE.RepeatWrapping;
      txtCrowd.wrapT = THREE.RepeatWrapping;
      txtCrowd2.wrapS = THREE.RepeatWrapping;
      txtCrowd2.wrapT = THREE.RepeatWrapping;
      if (txtSeats) {
        txtSeats.wrapS = THREE.RepeatWrapping;
        txtSeats.wrapT = THREE.RepeatWrapping;
      }

      var nRepeatsSeats = 1;
      var nRepeatsCrowd = Math.min(10, Math.floor(block[2] / 2));
      if (block[1] === 2) {
        nRepeatsSeats *= 6;
        nRepeatsCrowd /= 2;
      }
      let fGeomWidth = 0;
      if (iB === 3  || iB ===  4 || iB ===  8 || iB ===  9 ||
          iB === 13 || iB === 14 || iB === 18 || iB === 19) {
        fGeomWidth = fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor;
      } else if (iB ===  0 || iB ===  2 || iB ===  5 || iB ===  7 ||
                 iB === 10 || iB === 12 || iB === 15 || iB === 17) {
        fGeomWidth = fStadiumBlockHeightFront * fStadiumBlockCornerExtensionFactor;
      } else {
        fGeomWidth = fStadiumBlockHeightFront;
      }
      geomBlock = new THREE.PlaneGeometry(fGeomWidth, block[0]);
      geomCrowd = new THREE.PlaneGeometry(fGeomWidth, block[2]);
      if (txtSeats) { txtSeats.repeat.set(parseInt(fGeomWidth) / nRepeatsSeats, block[0] / nRepeatsSeats); }
      if (txtCrowd) { txtCrowd.repeat.set(parseInt(fGeomWidth / nRepeatsCrowd), parseInt(block[2] / nRepeatsCrowd)); }
      geomRoof = new THREE.BoxGeometry(fGeomWidth, fRoofWidth, fRoofHeight);
      geomOutsideWall = new THREE.PlaneGeometry(fGeomWidth, block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180)));

      const matSeats = new THREE.MeshLambertMaterial({
        color: clBlock,
        map: txtSeats,
        onBeforeCompile: shader => {
          shader.fragmentShader = shader.fragmentShader.replace(
            "#include <alphatest_fragment>",
            "if ( diffuseColor.a < 0.9 ) diffuseColor = vec4(vec3(0.0,0.0,0.5), 1.0);"
          );
        }
      });
      const matCrowd = new THREE.MeshLambertMaterial({ map: txtCrowd });

      let msh = new THREE.Mesh(geomBlock, matSeats);
      msh.castShadow = true;
      msh.receiveShadow = true;

      let msh2 = null;

      let mshCrowd = new THREE.Mesh(geomCrowd, matCrowd);
      mshCrowd.castShadow = false;
      mshCrowd.receiveShadow = true;

      let mshCrowd2 = null;

      // Roof
      let mshRoof = new THREE.Mesh(geomRoof, matRoof);
      mshRoof.castShadow = true;  // default is false
      mshCrowd.receiveShadow = false;

      // Outside wall
      let mshOutsideWall = null;
      if (iB < 10) {
        mshOutsideWall = new THREE.Mesh(geomOutsideWall, matOutsideWall);
        mshOutsideWall.castShadow = true;  // default is false
        mshOutsideWall.receiveShadow = false;
      }

      if (iB === 0 || iB === 10) { // A or A1
        const cornerpt_x = +fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2)) + (fGeomWidth / 2);
        const cornerpt_y = +fStadiumBlockWidthFront + fBlockWidth + fRoofWidthBasis;

        msh.rotation.x = +fStadiumBlockSteepAngle * (Math.PI / 180);
        msh.position.x = +fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2));
        msh.position.y = +fStadiumBlockWidthFront + ((block[0] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180)));

        mshCrowd.rotation.copy(msh.rotation);
        mshCrowd.position.copy(msh.position);
        mshCrowd.position.y = +fStadiumBlockWidthFront + ((block[2] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180))) - 0.1;

        // Lower block connections
        if (iB < 10) {
          let geomBlock2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[0]);
          geomBlock2.translate(-fStadiumBlockWidthCorner / 2, block[0] / 2, 0);
          msh2 = new THREE.Mesh(geomBlock2, matSeats);
          msh2.castShadow = true;
          msh2.receiveShadow = true;
          msh2.rotation.z = -90 * (Math.PI / 180);
          msh2.position.x = +fGroundX;
          msh2.position.y = +fStadiumBlockWidthFront;
          msh2.rotation.y = -fStadiumBlockSteepAngle * (Math.PI / 180);
          msh2.rotateOnWorldAxis(new THREE.Vector3(0, 0, 1), +45 * (Math.PI / 180));

          let geomCrowd2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[2]);
          geomCrowd2.translate(-fStadiumBlockWidthCorner / 2, block[2] / 2, 0);
          txtCrowd2.repeat.set(parseInt(fStadiumBlockWidthCorner / 10), parseInt(block[2] / 10));
          const matCrowd2 = new THREE.MeshLambertMaterial({ map: txtCrowd2 });
          mshCrowd2 = new THREE.Mesh(geomCrowd2, matCrowd2);
          mshCrowd2.castShadow = true;
          mshCrowd2.receiveShadow = true;
          mshCrowd2.rotation.copy(msh2.rotation);
          mshCrowd2.position.copy(msh2.position);
          mshCrowd2.position.x -= 0.05;
          mshCrowd2.position.y -= 0.05;

          if (mshOutsideWall) {
            mshOutsideWall.rotation.x = -Math.PI / 2;
            mshOutsideWall.position.x = +fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2));
            mshOutsideWall.position.y = +fStadiumBlockWidthFront + fBlockWidth + fRoofWidthBasis;
          }

          // Modify outside wall block connector
          geomOutsideWallCon4.attributes.position.setXYZ(
            0,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
          geomOutsideWallCon4.attributes.position.setXYZ(
            4,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
          geomOutsideWallCon4.attributes.position.setXYZ(
            5,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
        }

        mshRoof.position.x = cornerpt_x - (fGeomWidth / 2);
        mshRoof.position.y = cornerpt_y - (fRoofWidth / 2);
      } else if (iB === 1 || iB === 11) { // B or B1
        msh.rotation.x = +fStadiumBlockSteepAngle * (Math.PI / 180);
        msh.position.x = 0.0;
        msh.position.y = +fStadiumBlockWidthFront + ((block[0] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180)));

        mshCrowd.rotation.copy(msh.rotation);
        mshCrowd.position.copy(msh.position);
        mshCrowd.position.y = +fStadiumBlockWidthFront + ((block[2] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180))) - 0.1;

        mshRoof.position.x = 0.0;
        mshRoof.position.y = +fStadiumBlockWidthFront + fBlockWidth - (fRoofWidth / 2) + fRoofWidthBasis;

        if (mshOutsideWall) {
          mshOutsideWall.rotation.x = -Math.PI / 2;
          mshOutsideWall.position.x = 0.0;
          mshOutsideWall.position.y = +fStadiumBlockWidthFront + fBlockWidth + fRoofWidthBasis;
        }
      } else if (iB === 2 || iB === 12) { // C or C1
        const cornerpt_x = -fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2)) - (fGeomWidth / 2);
        const cornerpt_y = +fStadiumBlockWidthFront + fBlockWidth + fRoofWidthBasis;

        msh.rotation.x = +fStadiumBlockSteepAngle * (Math.PI / 180);
        msh.position.x = -fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2));
        msh.position.y = +fStadiumBlockWidthFront + ((block[0] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180)));

        mshCrowd.rotation.copy(msh.rotation);
        mshCrowd.position.copy(msh.position);
        mshCrowd.position.y = +fStadiumBlockWidthFront + ((block[2] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180))) - 0.1;

        // Lower block connections
        if (iB < 10) {
          let geomBlock2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[0]);
          geomBlock2.translate(+fStadiumBlockWidthCorner / 2, block[0] / 2, 0);
          msh2 = new THREE.Mesh(geomBlock2, matSeats);
          msh2.castShadow = true;
          msh2.receiveShadow = true;
          msh2.rotation.z = -90 * (Math.PI / 180);
          msh2.position.x = -fGroundX;
          msh2.position.y = +fStadiumBlockWidthFront;
          msh2.rotation.y = -fStadiumBlockSteepAngle * (Math.PI / 180);
          msh2.rotateOnWorldAxis(new THREE.Vector3(0, 0, 1), 135 * (Math.PI / 180));

          let geomCrowd2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[2]);
          geomCrowd2.translate(+fStadiumBlockWidthCorner / 2, block[2] / 2, 0);
          txtCrowd2.repeat.set(parseInt(fStadiumBlockWidthCorner / 10), parseInt(block[2] / 10));
          const matCrowd2 = new THREE.MeshLambertMaterial({ map: txtCrowd2 });
          mshCrowd2 = new THREE.Mesh(geomCrowd2, matCrowd2);
          mshCrowd2.castShadow = true;
          mshCrowd2.receiveShadow = true;
          mshCrowd2.rotation.copy(msh2.rotation);
          mshCrowd2.position.copy(msh2.position);
          mshCrowd2.position.x += 0.05;
          mshCrowd2.position.y -= 0.05;

          if (mshOutsideWall) {
            mshOutsideWall.rotation.x = -Math.PI / 2;
            mshOutsideWall.position.x = -fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2));
            mshOutsideWall.position.y = +fStadiumBlockWidthFront + fBlockWidth + fRoofWidthBasis;
          }

          // Modify outside wall block connector
          geomOutsideWallCon1.attributes.position.setXYZ(
            1,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
          geomOutsideWallCon1.attributes.position.setXYZ(
            2,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
          geomOutsideWallCon1.attributes.position.setXYZ(
            3,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
        }

        mshRoof.position.x = cornerpt_x + (fGeomWidth / 2);
        mshRoof.position.y = cornerpt_y - (fRoofWidth / 2);
      } else if (iB === 3 || iB === 13) { // D or D1
        const cornerpt_x = -fStadiumSideBlockX - fBlockWidth - fRoofWidthBasis;
        const cornerpt_y = +fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor;

        msh.rotation.z = +90 * (Math.PI / 180);
        msh.rotation.y = +fStadiumBlockSteepAngle * (Math.PI / 180);
        msh.position.x = -fStadiumSideBlockX - ((block[0] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180)));
        msh.position.y = +(fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor) / 2;

        mshCrowd.rotation.copy(msh.rotation);
        mshCrowd.position.copy(msh.position);
        mshCrowd.position.x = -fStadiumSideBlockX - ((block[2] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180))) + 0.1;

        // Lower block connections
        if (iB < 10) {
          let geomBlock2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[0]);
          geomBlock2.translate(-fStadiumBlockWidthCorner / 2, block[0] / 2, 0);
          msh2 = new THREE.Mesh(geomBlock2, matSeats);
          msh2.castShadow = true;
          msh2.receiveShadow = true;
          msh2.rotation.z = -90 * (Math.PI / 180);
          msh2.position.x = -fGroundX;
          msh2.position.y = +fStadiumBlockWidthFront;
          msh2.rotation.y = -fStadiumBlockSteepAngle * (Math.PI / 180);
          msh2.rotateOnWorldAxis(new THREE.Vector3(0, 0, 1), 135 * (Math.PI / 180));

          let geomCrowd2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[2]);
          geomCrowd2.translate(-fStadiumBlockWidthCorner / 2, block[2] / 2, 0);
          txtCrowd2.repeat.set(parseInt(fStadiumBlockWidthCorner / 10), parseInt(block[2] / 10));
          const matCrowd2 = new THREE.MeshLambertMaterial({ map: txtCrowd2 });
          mshCrowd2 = new THREE.Mesh(geomCrowd2, matCrowd2);
          mshCrowd2.castShadow = true;
          mshCrowd2.receiveShadow = true;
          mshCrowd2.rotation.copy(msh2.rotation);
          mshCrowd2.position.copy(msh2.position);
          mshCrowd2.position.x += 0.05;
          mshCrowd2.position.y -= 0.05;

          if (mshOutsideWall) {
            mshOutsideWall.rotation.x = -Math.PI / 2;
            mshOutsideWall.rotation.y = -Math.PI / 2;
            mshOutsideWall.position.x = -fStadiumSideBlockX - fBlockWidth - fRoofWidthBasis;
            mshOutsideWall.position.y = +(fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor) / 2;
          }

          // Modify outside wall block connector
          geomOutsideWallCon1.attributes.position.setXYZ(
            0,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
          geomOutsideWallCon1.attributes.position.setXYZ(
            4,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
          geomOutsideWallCon1.attributes.position.setXYZ(
            5,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
        }

        mshRoof.rotation.z = -90 * (Math.PI / 180);
        mshRoof.position.x = cornerpt_x + (fRoofWidth / 2);
        mshRoof.position.y = cornerpt_y / 2;
      } else if (iB === 4 || iB === 14) { // E or E1
        const cornerpt_x = -fStadiumSideBlockX - fBlockWidth - fRoofWidthBasis;
        const cornerpt_y = -fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor;

        msh.rotation.z = +90 * (Math.PI / 180);
        msh.rotation.y = +fStadiumBlockSteepAngle * (Math.PI / 180);
        msh.position.x = -fStadiumSideBlockX - ((block[0] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180)));
        msh.position.y = -(fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor) / 2;

        mshCrowd.rotation.copy(msh.rotation);
        mshCrowd.position.copy(msh.position);
        mshCrowd.position.x = -fStadiumSideBlockX - ((block[2] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180))) + 0.1;

        // Lower block connections
        if (iB < 10) {
          let geomBlock2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[0]);
          geomBlock2.translate(+fStadiumBlockWidthCorner / 2, block[0] / 2, 0);
          msh2 = new THREE.Mesh(geomBlock2, matSeats);
          msh2.castShadow = true;
          msh2.receiveShadow = true;
          msh2.rotation.z = -90 * (Math.PI / 180);
          msh2.position.x = -fGroundX;
          msh2.position.y = -fStadiumBlockWidthFront;
          msh2.rotation.y = -fStadiumBlockSteepAngle * (Math.PI / 180);
          msh2.rotateOnWorldAxis(new THREE.Vector3(0, 0, 1), -135 * (Math.PI / 180));

          let geomCrowd2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[2]);
          geomCrowd2.translate(+fStadiumBlockWidthCorner / 2, block[2] / 2, 0);
          txtCrowd2.repeat.set(parseInt(fStadiumBlockWidthCorner / 10), parseInt(block[2] / 10));
          const matCrowd2 = new THREE.MeshLambertMaterial({ map: txtCrowd2 });
          mshCrowd2 = new THREE.Mesh(geomCrowd2, matCrowd2);
          mshCrowd2.castShadow = true;
          mshCrowd2.receiveShadow = true;
          mshCrowd2.rotation.copy(msh2.rotation);
          mshCrowd2.position.copy(msh2.position);
          mshCrowd2.position.x += 0.05;
          mshCrowd2.position.y += 0.05;

          if (mshOutsideWall) {
            mshOutsideWall.rotation.x = -Math.PI / 2;
            mshOutsideWall.rotation.y = -Math.PI / 2;
            mshOutsideWall.position.x = -fStadiumSideBlockX - fBlockWidth - fRoofWidthBasis;
            mshOutsideWall.position.y = -(fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor) / 2;
          }

          // Modify outside wall block connector
          geomOutsideWallCon2.attributes.position.setXYZ(
            1,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
          geomOutsideWallCon2.attributes.position.setXYZ(
            2,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
          geomOutsideWallCon2.attributes.position.setXYZ(
            3,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
        }

        mshRoof.rotation.z = -90 * (Math.PI / 180);
        mshRoof.position.x = cornerpt_x + (fRoofWidth / 2);
        mshRoof.position.y = cornerpt_y / 2;
      } else if (iB === 5 || iB === 15) { // F or F1
        msh.rotation.z = 180 * (Math.PI / 180);
        msh.rotation.x = -fStadiumBlockSteepAngle * (Math.PI / 180);
        msh.position.x = -fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2));
        msh.position.y = -fStadiumBlockWidthFront - ((block[0] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180)));

        mshCrowd.rotation.copy(msh.rotation);
        mshCrowd.position.copy(msh.position);
        mshCrowd.position.y = -fStadiumBlockWidthFront - ((block[2] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180))) + 0.1;

        const cornerpt_x = -fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2)) - (fGeomWidth / 2);
        const cornerpt_y = -fStadiumBlockWidthFront - fBlockWidth - fRoofWidthBasis;

        // Lower block connections
        if (iB < 10) {
          let geomBlock2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[0]);
          geomBlock2.translate(-fStadiumBlockWidthCorner / 2, block[0] / 2, 0);
          msh2 = new THREE.Mesh(geomBlock2, matSeats);
          msh2.castShadow = true;
          msh2.receiveShadow = true;
          msh2.rotation.z = -90 * (Math.PI / 180);
          msh2.position.x = -fGroundX;
          msh2.position.y = -fStadiumBlockWidthFront;
          msh2.rotation.y = -fStadiumBlockSteepAngle * (Math.PI / 180);
          msh2.rotateOnWorldAxis(new THREE.Vector3(0, 0, 1), -135 * (Math.PI / 180));

          let geomCrowd2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[2]);
          geomCrowd2.translate(-fStadiumBlockWidthCorner / 2, block[2] / 2, 0);
          txtCrowd2.repeat.set(parseInt(fStadiumBlockWidthCorner / 10), parseInt(block[2] / 10));
          const matCrowd2 = new THREE.MeshLambertMaterial({ map: txtCrowd2 });
          mshCrowd2 = new THREE.Mesh(geomCrowd2, matCrowd2);
          mshCrowd2.castShadow = true;
          mshCrowd2.receiveShadow = true;
          mshCrowd2.rotation.copy(msh2.rotation);
          mshCrowd2.position.copy(msh2.position);
          mshCrowd2.position.x += 0.05;
          mshCrowd2.position.y += 0.05;

          if (mshOutsideWall) {
            mshOutsideWall.rotation.x = +Math.PI / 2;
            mshOutsideWall.position.x = -fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2));
            mshOutsideWall.position.y = -fStadiumBlockWidthFront - fBlockWidth - fRoofWidthBasis;
          }

          // Modify outside wall block connector
          geomOutsideWallCon2.attributes.position.setXYZ(
            0,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
          geomOutsideWallCon2.attributes.position.setXYZ(
            4,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
          geomOutsideWallCon2.attributes.position.setXYZ(
            5,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
        }

        mshRoof.position.x = cornerpt_x + (fGeomWidth / 2);
        mshRoof.position.y = cornerpt_y + (fRoofWidth / 2);
      } else if (iB === 6 || iB === 16) { // G or G1
        msh.rotation.z = 180 * (Math.PI / 180);
        msh.rotation.x = -fStadiumBlockSteepAngle * (Math.PI / 180);
        msh.position.x = 0.0;
        msh.position.y = -fStadiumBlockWidthFront - ((block[0] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180)));

        mshCrowd.rotation.copy(msh.rotation);
        mshCrowd.position.copy(msh.position);
        mshCrowd.position.y = -fStadiumBlockWidthFront - ((block[2] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180))) + 0.1;

        mshRoof.position.x = 0.0;
        mshRoof.position.y = -fStadiumBlockWidthFront - fBlockWidth + (fRoofWidth / 2) - fRoofWidthBasis;

        if (mshOutsideWall) {
          mshOutsideWall.rotation.x = +Math.PI / 2;
          mshOutsideWall.position.x = 0.0;
          mshOutsideWall.position.y = -fStadiumBlockWidthFront - fBlockWidth - fRoofWidthBasis;
        }
      } else if (iB === 7 || iB === 17) { // H or H1
        const cornerpt_x = +fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2)) + (fGeomWidth / 2);
        const cornerpt_y = -fStadiumBlockWidthFront - fBlockWidth - fRoofWidthBasis;

        msh.rotation.z = 180 * (Math.PI / 180);
        msh.rotation.x = -fStadiumBlockSteepAngle * (Math.PI / 180);
        msh.position.x = +fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2));
        msh.position.y = -fStadiumBlockWidthFront - ((block[0] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180)));

        mshCrowd.rotation.copy(msh.rotation);
        mshCrowd.position.copy(msh.position);
        mshCrowd.position.y = -fStadiumBlockWidthFront - ((block[2] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180))) + 0.1;

        // Lower block connections
        if (iB < 10) {
          let geomBlock2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[0]);
          geomBlock2.translate(+fStadiumBlockWidthCorner / 2, block[0] / 2, 0);
          msh2 = new THREE.Mesh(geomBlock2, matSeats);
          msh2.castShadow = true;
          msh2.receiveShadow = true;
          msh2.rotation.z = -90 * (Math.PI / 180);
          msh2.position.x = +fGroundX;
          msh2.position.y = -fStadiumBlockWidthFront;
          msh2.rotation.y = -fStadiumBlockSteepAngle * (Math.PI / 180);
          msh2.rotateOnWorldAxis(new THREE.Vector3(0, 0, 1), -45 * (Math.PI / 180));

          let geomCrowd2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[2]);
          geomCrowd2.translate(+fStadiumBlockWidthCorner / 2, block[2] / 2, 0);
          txtCrowd2.repeat.set(parseInt(fStadiumBlockWidthCorner / 10), parseInt(block[2] / 10));
          const matCrowd2 = new THREE.MeshLambertMaterial({ map: txtCrowd2 });
          mshCrowd2 = new THREE.Mesh(geomCrowd2, matCrowd2);
          mshCrowd2.castShadow = true;
          mshCrowd2.receiveShadow = true;
          mshCrowd2.rotation.copy(msh2.rotation);
          mshCrowd2.position.copy(msh2.position);
          mshCrowd2.position.x -= 0.05;
          mshCrowd2.position.y += 0.05;

          if (mshOutsideWall) {
            mshOutsideWall.rotation.x = +Math.PI / 2;
            mshOutsideWall.position.x = +fStadiumBlockHeightFront * (1 + ((fStadiumBlockCornerExtensionFactor - 1) / 2));
            mshOutsideWall.position.y = -fStadiumBlockWidthFront - fBlockWidth - fRoofWidthBasis;
          }

          // Modify outside wall block connector
          geomOutsideWallCon3.attributes.position.setXYZ(
            1,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
          geomOutsideWallCon3.attributes.position.setXYZ(
            2,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
          geomOutsideWallCon3.attributes.position.setXYZ(
            3,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
        }

        mshRoof.position.x = cornerpt_x - (fGeomWidth / 2);
        mshRoof.position.y = cornerpt_y + (fRoofWidth / 2);
      } else if (iB === 8 || iB === 18) { // I or I1
        const cornerpt_x = +fStadiumSideBlockX + fBlockWidth + fRoofWidthBasis;
        const cornerpt_y = -fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor;

        msh.rotation.z = -90 * (Math.PI / 180);
        msh.rotation.y = -fStadiumBlockSteepAngle * (Math.PI / 180);
        msh.position.x = +fStadiumSideBlockX + ((block[0] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180)));
        msh.position.y = -(fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor) / 2;

        mshCrowd.rotation.copy(msh.rotation);
        mshCrowd.position.copy(msh.position);
        mshCrowd.position.x = +fStadiumSideBlockX + ((block[2] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180))) - 0.1;

        // Lower block connections
        if (iB < 10) {
          let geomBlock2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[0]);
          geomBlock2.translate(-fStadiumBlockWidthCorner / 2, block[0] / 2, 0);
          msh2 = new THREE.Mesh(geomBlock2, matBlock);
          msh2.castShadow = true;
          msh2.receiveShadow = true;
          msh2.rotation.z = -90 * (Math.PI / 180);
          msh2.position.x = +fGroundX;
          msh2.position.y = -fStadiumBlockWidthFront;
          msh2.rotation.y = -fStadiumBlockSteepAngle * (Math.PI / 180);
          msh2.rotateOnWorldAxis(new THREE.Vector3(0, 0, 1), -45 * (Math.PI / 180));

          let geomCrowd2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[2]);
          geomCrowd2.translate(-fStadiumBlockWidthCorner / 2, block[2] / 2, 0);
          txtCrowd2.repeat.set(parseInt(fStadiumBlockWidthCorner / 10), parseInt(block[2] / 10));
          const matCrowd2 = new THREE.MeshLambertMaterial({ map: txtCrowd2 });
          mshCrowd2 = new THREE.Mesh(geomCrowd2, matCrowd2);
          mshCrowd2.castShadow = true;
          mshCrowd2.receiveShadow = true;
          mshCrowd2.rotation.copy(msh2.rotation);
          mshCrowd2.position.copy(msh2.position);
          mshCrowd2.position.x -= 0.05;
          mshCrowd2.position.y += 0.05;

          if (mshOutsideWall) {
            mshOutsideWall.rotation.x = -Math.PI / 2;
            mshOutsideWall.rotation.y = +Math.PI / 2;
            mshOutsideWall.position.x = +fStadiumSideBlockX + fBlockWidth + fRoofWidthBasis;
            mshOutsideWall.position.y = -(fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor) / 2;
          }

          // Modify outside wall block connector
          geomOutsideWallCon3.attributes.position.setXYZ(
            0,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
          geomOutsideWallCon3.attributes.position.setXYZ(
            4,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
          geomOutsideWallCon3.attributes.position.setXYZ(
            5,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
        }

        mshRoof.rotation.z = -90 * (Math.PI / 180);
        mshRoof.position.x = cornerpt_x - (fRoofWidth / 2);
        mshRoof.position.y = cornerpt_y / 2;
      } else if (iB === 9 || iB === 19) { // J or J1
        msh.rotation.z = -90 * (Math.PI / 180);
        msh.rotation.y = -fStadiumBlockSteepAngle * (Math.PI / 180);
        msh.position.x = +fStadiumSideBlockX + ((block[0] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180)));
        msh.position.y = +(fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor) / 2;

        mshCrowd.rotation.copy(msh.rotation);
        mshCrowd.position.copy(msh.position);
        mshCrowd.position.x = +fStadiumSideBlockX + ((block[2] / 2) * Math.cos(fStadiumBlockSteepAngle * (Math.PI / 180))) - 0.1;

        const cornerpt_x = +fStadiumSideBlockX + fBlockWidth + fRoofWidthBasis;
        const cornerpt_y = +fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor;

        // Lower block connections
        if (iB < 10) {
          let geomBlock2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[0]);
          geomBlock2.translate(+fStadiumBlockWidthCorner / 2, block[0] / 2, 0);
          msh2 = new THREE.Mesh(geomBlock2, matBlock);
          msh2.castShadow = true;
          msh2.receiveShadow = true;
          msh2.rotation.z = -90 * (Math.PI / 180);
          msh2.position.x = +fGroundX;
          msh2.position.y = +fStadiumBlockWidthFront;
          msh2.rotation.y = -fStadiumBlockSteepAngle * (Math.PI / 180);
          msh2.rotateOnWorldAxis(new THREE.Vector3(0, 0, 1), +45 * (Math.PI / 180));

          let geomCrowd2 = new THREE.PlaneGeometry(fStadiumBlockWidthCorner, block[2]);
          geomCrowd2.translate(+fStadiumBlockWidthCorner / 2, block[2] / 2, 0);
          txtCrowd2.repeat.set(parseInt(fStadiumBlockWidthCorner / 10), parseInt(block[2] / 10));
          const matCrowd2 = new THREE.MeshLambertMaterial({ map: txtCrowd2 });
          mshCrowd2 = new THREE.Mesh(geomCrowd2, matCrowd2);
          mshCrowd2.castShadow = true;
          mshCrowd2.receiveShadow = true;
          mshCrowd2.rotation.copy(msh2.rotation);
          mshCrowd2.position.copy(msh2.position);
          mshCrowd2.position.x -= 0.05;
          mshCrowd2.position.y -= 0.05;

          if (mshOutsideWall) {
            mshOutsideWall.rotation.x = -Math.PI / 2;
            mshOutsideWall.rotation.y = +Math.PI / 2;
            mshOutsideWall.position.x = +fStadiumSideBlockX + fBlockWidth + fRoofWidthBasis;
            mshOutsideWall.position.y = +(fStadiumBlockWidthFront * fStadiumBlockCornerExtensionFactor) / 2;
          }

          // Modify outside wall block connector
          geomOutsideWallCon4.attributes.position.setXYZ(
            1,
            cornerpt_x,
            cornerpt_y,
            block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))
          );
          geomOutsideWallCon4.attributes.position.setXYZ(
            2,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
          geomOutsideWallCon4.attributes.position.setXYZ(
            3,
            cornerpt_x,
            cornerpt_y,
            0.0
          );
        }

        mshRoof.rotation.z = -90 * (Math.PI / 180);
        mshRoof.position.x = cornerpt_x - (fRoofWidth / 2);
        mshRoof.position.y = cornerpt_y / 2;
      } else {
        continue;
      }
      msh.position.z = (block[0] / 2) * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180));
      mshCrowd.position.z = (block[2] / 2) * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180));
      mshRoof.position.z = block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180));
      if (mshOutsideWall) {
        mshOutsideWall.position.z = (block[0] * Math.sin(fStadiumBlockSteepAngle * (Math.PI / 180))) / 2;
      }

      // Add topring height
      if (iB > 9) {
        msh.position.z += fTopringHeight;
        mshCrowd.position.z += fTopringHeight;
        mshRoof.position.z += fTopringHeight;
      }

      scene.add(msh);
      scene.add(mshCrowd);
      scene.add(mshRoof);

      if (msh2) {
        scene.add(msh2);
      }

      if (mshCrowd2) {
        scene.add(mshCrowd2);
      }

      // Add outside walls
      if (mshOutsideWall) {
        scene.add(mshOutsideWall);
      }
    }

    // Re- compute outside wall connector normals
    geomOutsideWallCon1.computeVertexNormals();
    geomOutsideWallCon2.computeVertexNormals();
    geomOutsideWallCon3.computeVertexNormals();
    geomOutsideWallCon4.computeVertexNormals();
  }
}

class Stadium {
  constructor(scene, ltBlocks) {
    this.ltMeshGoal = [];

    this.pitch = new Pitch(scene);
    this.ground = new Ground(scene);
    this.blocks = new Blocks(scene, ltBlocks);
    this.goalL = new Goal(+1);
    this.goalL.addToScene(scene);
    this.goalR = new Goal(-1);
    this.goalR.addToScene(scene);

    this.ltMeshGoal[0] = this.goalL.ltMeshGoal;
    this.ltMeshGoal[1] = this.goalR.ltMeshGoal;
  }
}

export { Stadium };
