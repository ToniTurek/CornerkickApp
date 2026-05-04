let iParentWidth = 0;
window.drawPlayerBest = (_teamData, parent, bMobile) => {
  if (!parent) { return; }
  parent.innerHTML = '';

  const teamData = JSON.parse(_teamData);

  // Store div width for partial reload
  if (parent.offsetWidth > 0) {
    iParentWidth = parent.offsetWidth;
  }

  if (teamData) {  // check if data is defined
    for (var iPl = 0; iPl < teamData.ltPlayer2.length; iPl++) {
      const player = teamData.ltPlayer2[iPl];

      const divBoxPl = window.getBoxFormationDOM(
        player.iId, player.ptPos, player.sName, iPl + 1, player.sSkillAve, 0, false, -1, player.iPos, iParentWidth, 0.5, player.sTeamname, player.sAge, player.sNat, false, player.sPortrait, bMobile, null
      );
      parent.appendChild(divBoxPl);
    }
  }
}
