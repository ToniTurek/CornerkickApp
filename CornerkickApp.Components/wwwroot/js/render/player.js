class Player {
  constructor(scene, ptPos, iHeight, cl1, cl2, cl3) {
    const fPlSecHeight1 = 0.45;
    const fPlSecHeight2 = 0.20;
    const fPlSecHeight3 = 0.35;

    this.ptPos = ptPos;
    this.ptPosLast = null;
    this.iHeight = iHeight;
    this.parts = [];

    if (!cl1) { cl1 = 0xff0000; }
    const matPl1 = new THREE.MeshStandardMaterial({ color: cl1 });
    const geomPl1 = new THREE.CylinderGeometry(0.2, 0.2, this.iHeight * fPlSecHeight1, 32);
    const mshPl1 = new THREE.Mesh(geomPl1, matPl1);
    mshPl1.castShadow = true;
    mshPl1.rotation.x = 90 * (Math.PI / 180);
    mshPl1.position.x = this.ptPos.x;
    mshPl1.position.y = this.ptPos.y;
    mshPl1.position.z = this.iHeight * (fPlSecHeight3 + fPlSecHeight2 + (fPlSecHeight1 / 2));
    this.parts.push(mshPl1);
    scene.add(mshPl1);

    if (!cl2) { cl2 = 0x000000; }
    const matPl2 = new THREE.MeshStandardMaterial({ color: cl2 });
    const geomPl2 = new THREE.CylinderGeometry(0.2, 0.2, this.iHeight * fPlSecHeight2, 32);
    const mshPl2 = new THREE.Mesh(geomPl2, matPl2);
    mshPl2.castShadow = true;
    mshPl2.rotation.x = 90 * (Math.PI / 180);
    mshPl2.position.x = this.ptPos.x;
    mshPl2.position.y = this.ptPos.y;
    mshPl2.position.z = this.iHeight * (fPlSecHeight3 + (fPlSecHeight2 / 2));
    this.parts.push(mshPl2);
    scene.add(mshPl2);

    if (!cl3) { cl3 = 0x000000; }
    const matPl3 = new THREE.MeshStandardMaterial({ color: cl3 });
    const geomPl3 = new THREE.CylinderGeometry(0.2, 0.2, this.iHeight * fPlSecHeight3, 32);
    const mshPl3 = new THREE.Mesh(geomPl3, matPl3);
    mshPl3.castShadow = true;
    mshPl3.rotation.x = 90 * (Math.PI / 180);
    mshPl3.position.x = this.ptPos.x;
    mshPl3.position.y = this.ptPos.y;
    mshPl3.position.z = this.iHeight * (fPlSecHeight3 / 2);
    this.parts.push(mshPl3);
    scene.add(mshPl3);
  }

  updatePos(ptPos) {
    for (let i = 0; i < this.parts.length; i++) {
      this.parts[i].position.x = ptPos.x;
      this.parts[i].position.y = ptPos.y;
    }
  }
}

export { Player };
