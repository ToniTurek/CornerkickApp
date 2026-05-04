using CornerkickApp.Shared.Models;
using static CornerkickApp.Shared.Models.LayoutModel;

namespace CornerkickApp.Controllers.Member
{
  public class TransferController
  {
    public static TransferModel Get(CornerkickManager.User _usr)
    {
      TransferModel mdTransfer = new TransferModel();

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return mdTransfer;

      mdTransfer.bSound = true;
      if (_usr.lti.Count > UserOptionsModel.iUserOptionsIxSound) mdTransfer.bSound = _usr.lti[UserOptionsModel.iUserOptionsIxSound] > 0;

      mdTransfer.bTransferlistOpen = CkAppShared.ckMng.dtDatum.Date.CompareTo(CkAppShared.ckMng.dtSeasonStart.Date) > 0;

      mdTransfer.iContractYears = 1;

      mdTransfer.bNation = clb.bNation;
      mdTransfer.bNominationPossible = App.getWcNominationDeadline(CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdWc)).CompareTo(CkAppShared.ckMng.dtDatum) > 0;

      mdTransfer.iFilterNation = clb.bNation ? clb.iLand : -9;
      mdTransfer.sliFilterNations.Add(new SelectListItem { Text = "Alle", Value = "-9" });
      foreach (byte iN in CkAppShared.iNations) {
        mdTransfer.sliFilterNations.Add(new SelectListItem { Text = CornerkickManager.Main.sLand[iN], Value = iN.ToString() });
      }
      mdTransfer.iFilterClub = -9;

      return mdTransfer;
    }

    public static List<TransferModel.TransferItem>? GetTransferList(CornerkickManager.User? _usr, int iPos, int iFType, int iFValue, bool bJouth, int iType, bool bFixTransferFee, bool bEndingContract, int iClubId = -9, int iNation = -1)
    {
      if (_usr == null) return null;

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      List<CornerkickManager.Player>? ltPlayer = null;
      if (iType == 1) {
        ltPlayer = CkAppShared.ckMng.ltPlayer;
      } else if (iType == 2) {
        if (_usr == null) return null;

        ltPlayer = _usr.ltPlayerFavorites;
      }

      //The table or entity I'm querying
      List<TransferModel.TransferItem> ltDeTransfer = new List<TransferModel.TransferItem>();

      int iTr = 0;
      foreach (CornerkickManager.Transfer.Item transfer in CkAppShared.ckMng.ui.filterTransferlist(sName: "",
                                                                                                   iClubId: iClubId,
                                                                                                   iPos: iPos,
                                                                                                   iFType: iFType,
                                                                                                   iF: iFValue,
                                                                                                   bJouth: bJouth,
                                                                                                   ltPlayer: ltPlayer,
                                                                                                   bFixTransferFee: bFixTransferFee,
                                                                                                   bEndingContract: bEndingContract,
                                                                                                   iNation: iNation,
                                                                                                   clubUser: clb)) {
        if (transfer.player == null) continue;

        try {
          string sClub = "vereinslos";
          if (transfer.player.contract?.club != null) {
            if (transfer.player.contract.club.bNation) continue;

            sClub = transfer.player.contract.club.sName;
          }

          int iOffer = 0; // -2: not on transfer list, -1: negotiation cancelled, +1: already offered, +2: own player with offers, +3: own player of nation
          int iFixtransferfee = 0;

          bool bOwnPlayer = CornerkickManager.PlayerTool.ownPlayer(clb, transfer.player);

          if (AdminModel.checkUserIsAdmin(_usr.id) && ((transfer.ltOffers != null && transfer.ltOffers.Count > 0) || clb.bNation)) {
            iOffer = +9;
          } else if (clb.bNation && bOwnPlayer) {
            iOffer = +3;
          } else if (CkAppShared.ckMng.tr.negotiationCancelled(clb, transfer.player)) {
            iOffer = -1;
          } else if (CornerkickManager.Transfer.alreadyOffered(clb, transfer.player, CkAppShared.ckMng.ltTransfer)) {
            iOffer = +1;
          } else if (transfer.ltOffers != null && transfer.ltOffers.Count > 0 && bOwnPlayer) {
            if (transfer.ltOffers.Count == 1 && transfer.ltOffers[0].contractOffered != null && transfer.ltOffers[0].contractOffered.club.iId == clb.iId) continue;
            iOffer = +2;
          } else if (transfer.player.contract?.club != null && transfer.player.contract.iFixTransferFee < 1 && !CkAppShared.ckMng.plt.onTransferlist(transfer.player)) {
            // If player has a club (and a contract) and not fixed transfer fee and is not on transfer list: iOffer = -2
            iOffer = -2;
          }

          if (transfer.player.contract != null) iFixtransferfee = transfer.player.contract.iFixTransferFee;

          string sDatePutOnTl = "-";
          if (transfer.dt.Year > 1) sDatePutOnTl = transfer.dt.ToString("d", MemberController.getCi(clb));

          float[]? fSkills = null;
          if (_usr.bScouting) {
            CornerkickManager.Main.Staff? staff = ClubController.getClubStaff(clb);
            if (staff != null) fSkills = staff.getScoutedSkills(transfer.player.plGame);
          }

          List<TransferModel.TransferItem.Offer>? ltOffers = null;
          if (transfer.ltOffers != null) {
            ltOffers = new List<TransferModel.TransferItem.Offer>();

            int iIx = 0;
            foreach (CornerkickManager.Transfer.Offer off in transfer.ltOffers.FindAll(t => t.iFee > 0 && t.contract != null)) {
              if (bOwnPlayer || (off.contract?.club != null && off.contract.club.iId == clb.iId)) {
                ltOffers.Add(new TransferModel.TransferItem.Offer() {
                  iIx = ++iIx,
                  dt = off.dt,
                  iClubId = off.contract != null ? off.contract.club.iId : -1,
                  sClubName = off.contract != null ? off.contract.club.sName : "vereinslos",
                  iFee = off.iFee,
                  iFeeSecret = off.iFeeSecret,
                  bNextSeason = off.bNextSeason
                });
              }
            }
          }
          ltDeTransfer.Add(new TransferModel.TransferItem {
            ltOffers = ltOffers,
            iPlayerId = transfer.player.plGame.iId,
            bOwnPlayer = CornerkickManager.PlayerTool.ownPlayer(clb, transfer.player),
            iOffer = iOffer,
            iIx = iTr + 1,
            dt = transfer.dt,
            sName = transfer.player.plGame.sName,
            sPos = CornerkickManager.PlayerTool.getStrPos(transfer.player),
            fStrength = CornerkickGame.Tool.getAveSkill(transfer.player.plGame, bIdeal: false, fSkills: fSkills),
            fStrengthIdeal = CornerkickGame.Tool.getAveSkill(transfer.player.plGame, bIdeal: true, fSkills: fSkills),
            fAge = transfer.player.plGame.getAge(CkAppShared.ckMng.dtDatum),
            fTalentAve = transfer.player.getTalentAve() + 1f,
            iValue = transfer.player.getValue(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd) * 1000,
            iFixtransferfee = iFixtransferfee,
            sClubName = sClub,
            sNat = CornerkickManager.Main.sLandShort[transfer.player.iNat1],
            bEndingContract = CornerkickManager.PlayerTool.checkIfContractIsEnding(transfer.player, CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd) && transfer.player.contractNext == null
          });
        } catch (Exception e) {
          CkAppShared.ckMng.tl.writeLog("Error in getTableTransfer(), iTr: " + iTr.ToString() + Environment.NewLine + e.Message + Environment.NewLine + e.Source + Environment.NewLine + e.Data + Environment.NewLine + e.StackTrace, CornerkickManager.Main.sErrorFile);
        }

        iTr++;
      }

      return ltDeTransfer;
    }

    /// <summary>
    /// Adds or removes a player from the transfer list, depending on their current status.
    /// </summary>
    /// <remarks>If the player is already on the transfer list, they will be removed from it. If the player is
    /// not on the transfer list, they will be added to it. Additional actions may be triggered if the player is
    /// considered a top-tier player, such as generating news updates.</remarks>
    /// <param name="_usr">The user initiating the operation. This parameter cannot be null.</param>
    /// <param name="iPlayerId">The unique identifier of the player to be added or removed from the transfer list.</param>
    /// <param name="sRet">An output parameter that contains a message describing the result of the operation.</param>
    /// <returns>-1: error, 0: player was put on transfer list, 1: player was taken from transfer list</returns>
    public static int PutOnTakeFromTransferList(CornerkickManager.User? _usr, int iPlayerId, out string? sRet)
    {
      return PutOnTakeFromTransferList(_usr, CkAppShared.ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId), out sRet);
    }
    /// <summary>
    /// Adds or removes a player from the transfer list, depending on their current status.
    /// </summary>
    /// <remarks>If the player is already on the transfer list, they will be removed from it. If the player is
    /// not on the transfer list, they will be added to it. Additional actions may be triggered if the player is
    /// considered a top-tier player, such as generating news updates.</remarks>
    /// <param name="_usr">The user initiating the action. Can be <see langword="null"/>.</param>
    /// <param name="pl">The player to be added to or removed from the transfer list. Cannot be <see langword="null"/>.</param>
    /// <param name="sRet">An output parameter that contains a message describing the result of the operation. This will be <see
    /// <returns>-1: error, 0: player was put on transfer list, 1: player was taken from transfer list</returns>
    public static int PutOnTakeFromTransferList(CornerkickManager.User? _usr, CornerkickManager.Player? pl, out string? sRet)
    {
      sRet = null;

      if (pl == null) return -1;

      // If player is on transfer-list already
      if (CkAppShared.ckMng.plt.onTransferlist(pl)) {
        for (int iT = 0; iT < CkAppShared.ckMng.ltTransfer.Count; iT++) {
          CornerkickManager.Transfer.Item transfer = CkAppShared.ckMng.ltTransfer[iT];

          if (transfer.player == pl) {
            CkAppShared.ckMng.ltTransfer.RemoveAt(iT);
            break;
          }
        }

        sRet = "Der Spieler " + pl.plGame.sName + " wurde von der Transferliste genommen";
        return 1;
      }

      // If player is not on transfer-list already
      if (CkAppShared.ckMng.tr.putPlayerOnTransferlist(pl, 0) == 2) {
        sRet = "Der Spieler " + pl.plGame.sName + " kann in dieser Saison den Verein nicht mehr wechseln";
        return -1;
      }

      // Create news for top players
      if (checkIfTop10Player(pl)) {
        string sNewsPaper1 = pl.plGame.sName + " steht zum Verkauf!";

        string sNewsPaper2 = "";
        CornerkickManager.Club? clbPlayer = null;
        if (pl.contract != null) clbPlayer = pl.contract.club;
        if (clbPlayer != null) {
          sNewsPaper2 = "Nach über&shy;ein&shy;stimmenden Medien&shy;berichten stehen die Zeichen zwischen ";
          sNewsPaper2 += clbPlayer.sName.Replace(" ", "&nbsp;");
          sNewsPaper2 += " und " + pl.plGame.sName + " (" + ((int)pl.plGame.getAge(CkAppShared.ckMng.dtDatum)).ToString() + " Jahre, " + CornerkickManager.PlayerTool.getStrPos(pl) + ", " + (pl.getValue(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd) / 1000.0).ToString("0.0") + " mio. MW) auf Abschied.";
          //sNewsPaper2 += " Die kolportierte Ablösesumme soll bei ca. " + (pl.getValue(CkAppShared.ckMng.dtDatum) / 1000).ToString("0.0") + " mio. liegen";
        }
        CkAppShared.ckMng.sendNews(CkAppShared.ckMng.ltUser[0], sNewsPaper1 + "#" + sNewsPaper2, iType: 200, iId: pl.plGame.iId);
      }

      sRet = "Der Spieler " + pl.plGame.sName + " wurde auf die Transferliste gesetzt";

      return 0;
    }

    private static bool checkIfTop10Player(CornerkickManager.Player pl)
    {
      for (byte iPos = 1; iPos <= 11; iPos++) {
        foreach (CornerkickManager.Player plB in CkAppShared.ckMng.getBestPlayer(iPlCount: 10, iPos: iPos, fAgeMax: pl.plGame.getAge(CkAppShared.ckMng.dtDatum))) {
          if (pl.plGame.iId == plB.plGame.iId) return true;
        }
      }

      return false;
    }

    public static string? MakeOffer(CornerkickManager.User? _usr, int iPlayerId, int iTransferFee, int iTransferFeeSecret)
    {
      CornerkickManager.Player? pl = CkAppShared.ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl == null) return null;

      string sReturn = "Error";

      CornerkickManager.Club? club = MemberController.ckClub(_usr);
      if (club == null) return null;

      CornerkickManager.Club? clbGive = null;
      if (pl.contract != null) clbGive = pl.contract.club;

      // If no club ...
      if (clbGive == null) {
        // ... and not on transferlist already --> put on transferlist
        if (!CkAppShared.ckMng.plt.onTransferlist(pl)) CkAppShared.ckMng.tr.putPlayerOnTransferlist(pl, 0);
      }

      for (int iT = 0; iT < CkAppShared.ckMng.ltTransfer.Count; iT++) {
        CornerkickManager.Transfer.Item transfer = CkAppShared.ckMng.ltTransfer[iT];

        if (transfer.player == pl) {
          if (transfer.ltOffers != null) {
            for (int iO = 0; iO < transfer.ltOffers.Count; iO++) {
              CornerkickManager.Transfer.Offer offer = transfer.ltOffers[iO];

              if (offer.contract.club == club) {
                if (iTransferFee > 0 && !CkAppShared.ckMng.fz.checkDispoLimit(iTransferFee, club)) {
                  transfer.ltOffers.Remove(offer);
                  return "Ihr Kreditrahmen ist leider nicht hoch genug";
                }

                if (iTransferFeeSecret > club.iBalanceSecret) {
                  transfer.ltOffers.Remove(offer);
                  return "Sie haben nicht genug Schwarzgeld...";
                }

                // No club
                if (clbGive == null) {
                  offer.iFee = 0;
                  offer.iFeeSecret = 0;
                  if (CkAppShared.ckMng.tr.transferPlayer(iPlayerId, club)) {
                    sReturn = "Sie haben den vereinslosen Spieler " + pl.plGame.sName + " ablösefrei unter Vertrag genommen.";
                  }
                  break;
                }

                // Ending contract
                if (offer.bNextSeason && CornerkickManager.PlayerTool.checkIfContractIsEnding(transfer.player, CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd)) {
                  offer.iFee = 0;
                  offer.iFeeSecret = 0;
                  if (CkAppShared.ckMng.tr.transferPlayer(iPlayerId, club, bNextSeason: true)) {
                    sReturn = "Sie haben den Spieler " + pl.plGame.sName + " ablösefrei für die nächste Saison verpflichtet.";
                    createNewspaperPlayerTransfer(pl, club, -1);
                  }
                  break;
                }

                // Fix transfer fee
                if (pl.contract != null && pl.contract.iFixTransferFee > 0) {
                  offer.iFee = pl.contract.iFixTransferFee;
                  offer.iFeeSecret = 0;
                  if (CkAppShared.ckMng.tr.transferPlayer(iPlayerId, club, iTransferIx: iT)) {
                    sReturn = "Sie haben den Spieler " + pl.plGame.sName + " für die festgeschriebene Ablöse von " + offer.iFee.ToString("N0", MemberController.getCi(club)) + " verpflichtet.";
                    CkAppShared.ckMng.sendNews(clbGive.user, "Ihr Spieler " + pl.plGame.sName + " wechselt mit sofortiger Wirkung für die festgeschriebene Ablöse von " + offer.iFee.ToString("N0", MemberController.getCi(club)) + " zu " + club.sName, iType: CornerkickManager.Main.iNewsTypePlayerTransferOfferAccept, iId: iPlayerId);

                    createNewspaperPlayerTransfer(pl, club, offer.iFee);
                  }
                  break;
                }

                offer.dt = CkAppShared.ckMng.dtDatum;

                offer.iFee = iTransferFee;
                offer.iFeeSecret = iTransferFeeSecret;

                transfer.ltOffers[iO] = offer;
                CkAppShared.ckMng.ltTransfer[iT] = transfer;

                CkAppShared.ckMng.tr.informUser(transfer, offer);

                if (clbGive != null) {
                  CkAppShared.ckMng.sendNews(clbGive.user, "Sie haben ein neues Transferangebot für den Spieler " + pl.plGame.sName + " erhalten!", iType: CornerkickManager.Main.iNewsTypePlayerTransferNewOffer, iId: iPlayerId);
                  sReturn = "Sie haben das Transferangebot für dem Spieler " + pl.plGame.sName + " erfolgreich abgegeben.";

                  bool bNewspaperTalent = pl.getTalentAve() > 5f && pl.plGame.getAge(CkAppShared.ckMng.dtDatum) < 23;
                  if (bNewspaperTalent || checkIfTop10Player(pl)) {
                    string sTalent = "";
                    if (bNewspaperTalent) sTalent = "Talent ";
                    string sNewsPaper1 = club.sName + " vor Verpflichtung von " + sTalent + pl.plGame.sName;
                    string sNewsPaper2 = "Angeblich steht " + club.sName + " kurz vor der Verpflichtung von " + pl.plGame.sName + " (" + ((int)pl.plGame.getAge(CkAppShared.ckMng.dtDatum)).ToString() + " Jahre, " + CornerkickManager.PlayerTool.getStrPos(pl) + ", " + (pl.getValue(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd) / 1000.0).ToString("0.0") + " mio. MW).";
                    CkAppShared.ckMng.sendNews(CkAppShared.ckMng.ltUser[0], sNewsPaper1 + "#" + sNewsPaper2, iType: 200, iId: pl.plGame.iId);
                  }
                }

                pl.plGame.character.fMoney += 0.05f;

                break;
              }
            }
          }
        }
      }

      return sReturn;
    }

    private static void createNewspaperPlayerTransfer(CornerkickManager.Player pl, CornerkickManager.Club clbTake, int iTransferFee)
    {
      if (pl == null) return;
      if (clbTake == null) return;

      // Create news
      if (checkIfTop10Player(pl)) {
        if (iTransferFee < 0) {
          string sNewsPaper1 = pl.plGame.sName + " wechselt zu " + clbTake.sName;
          string sNewsPaper2 = "Wie heute bekannt gegeben wurde, schließt sich " + pl.plGame.sName + " (" + ((int)pl.plGame.getAge(CkAppShared.ckMng.dtDatum)).ToString() + " Jahre, " + CornerkickManager.PlayerTool.getStrPos(pl) + ", " + (pl.getValue(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd) / 1000.0).ToString("0.0") + " mio. MW) zur neuen Saison " + clbTake.sName + " an.";
          CkAppShared.ckMng.sendNews(CkAppShared.ckMng.ltUser[0], sNewsPaper1 + "#" + sNewsPaper2, iType: 200, iId: pl.plGame.iId);
        } else {
          string sNewsPaper1 = pl.plGame.sName + " bei " + clbTake.sName + " vorgestellt";
          string sNewsPaper2 = "Auf der heutigen Presse&shy;konferenz wurde " + pl.plGame.sName + " (" + ((int)pl.plGame.getAge(CkAppShared.ckMng.dtDatum)).ToString() + " Jahre, " + CornerkickManager.PlayerTool.getStrPos(pl) + ", " + (pl.getValue(CkAppShared.ckMng.dtDatum, CkAppShared.ckMng.dtSeasonEnd) / 1000.0).ToString("0.0") + " mio. MW) offiziell vorgestellt. Die Ablöse&shy;summe soll angeblich bei " + (iTransferFee / 1000000.0).ToString("0.0") + " mio. liegen.";
          CkAppShared.ckMng.sendNews(CkAppShared.ckMng.ltUser[0], sNewsPaper1 + "#" + sNewsPaper2, iType: 200, iId: pl.plGame.iId);
        }
      }
    }

    public static string? AcceptOffer(int iPlayerId, int iClubId)
    {
      string sReturn = "Error";

      CornerkickManager.Player? pl = CkAppShared.ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl == null) return null;

      CornerkickManager.Club? clubTake = CkAppShared.ckMng.ltClubs.Find(c => c.iId == iClubId);
      CornerkickManager.Transfer.Offer offer = CkAppShared.ckMng.tr.getOffer(pl, clubTake);

      if (CkAppShared.ckMng.tr.transferPlayer(iPlayerId, clubTake)) {
        // Create news
        createNewspaperPlayerTransfer(pl, clubTake, offer.iFee);

        sReturn = "Sie haben das Transferangebot für dem Spieler " + pl.plGame.sName + " angenommen. Er wechselt mit sofortiger Wirkung zu " + clubTake.sName;
      }

      return sReturn;
    }

    public static string? CancelOffer(CornerkickManager.User? _usr, int iPlayerId)
    {
      string sReturn = "Error";

      CornerkickManager.Club? clb = MemberController.ckClub(_usr);
      if (clb == null) return null;

      CornerkickManager.Player? pl = CkAppShared.ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (pl == null) return null;

      if (CkAppShared.ckMng.tr.cancelTransferOffer(pl, clb)) {
        pl.plGame.character.fMoney -= 0.05f;
        sReturn = "Sie haben Ihr Transferangebot für dem Spieler " + pl.plGame.sName + " zurückgezogen.";
      }

      return sReturn;
    }

    public static bool AddToRemFromFavorites(CornerkickManager.User? _usr, int iPlayerId)
    {
      if (_usr == null) return false;

      CornerkickManager.Player? plFav = CkAppShared.ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (plFav == null) return false;

      if (_usr.ltPlayerFavorites.IndexOf(plFav) >= 0) _usr.ltPlayerFavorites.Remove(plFav);
      else _usr.ltPlayerFavorites.Add(plFav);

      return true;
    }

    public static bool NominatePlayer(CornerkickManager.User? _usr, int iPlayerId, out string sMsg)
    {
      sMsg = "";

      CornerkickManager.Club? nation = MemberController.ckClub(_usr);

      if (nation == null) {
        sMsg = "No nation";
        return false;
      }
      if (!nation.bNation) {
        sMsg = "Not a nation";
        return false;
      }

      CornerkickManager.Player? plNom = CkAppShared.ckMng.ltPlayer.Find(p => p.plGame.iId == iPlayerId);
      if (plNom == null) {
        sMsg = "Player not found";
        return false;
      }

      if (plNom.iNat1 != nation.iLand) {
        sMsg = "Players nation not correct";
        return false;
      }

      if (App.getWcNominationDeadline(CkAppShared.ckMng.tl.getCup(CkAppShared.iCupIdWc)).CompareTo(CkAppShared.ckMng.dtDatum) < 0) {
        sMsg = "Sie sind zu spät dran. Das finale Nominierungsdatum wurde verpasst.";
        return false;
      }

      bool bNominate = !CornerkickManager.PlayerTool.ownPlayer(nation, plNom);
      if (bNominate && nation.ltPlayer.Count >= CkAppShared.nPlayerNatMax) {
        sMsg = "Sie dürfen maximal " + CkAppShared.nPlayerNatMax.ToString() + " nominieren.";
        return false;
      }

      if (bNominate) nation.ltPlayer.Add(plNom);
      else           nation.ltPlayer.Remove(plNom);

      return bNominate;
    }

  }
}
