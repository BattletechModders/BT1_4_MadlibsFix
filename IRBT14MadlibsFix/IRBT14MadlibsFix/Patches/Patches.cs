using BattleTech;
using BattleTech.Data;
using BattleTech.Framework;
using Harmony;
using HBS.Collections;
using System.Collections.Generic;

namespace IRBT14MadlibsFix.Patches {

    [HarmonyPatch(typeof(TeamOverride), "RunMadLibs")]
    public static class TeamOverride_RunMadLibs {
        public static Dictionary<int, TeamOverride> lanceOverrides = new Dictionary<int, TeamOverride>();

        public static bool Prefix(TeamOverride __instance, Contract contract, DataManager dataManager, 
            Contract ___contract, DataManager ___dataManager, string ___teamLeaderCastDefId, List<LanceOverride> ___lanceOverrideList, Faction ___faction) {
            ___contract = contract;
            ___dataManager = dataManager;

            if (___teamLeaderCastDefId == CastDef.castDef_TeamLeader_Current) {
                ___teamLeaderCastDefId = CastDef.GetCombatCastDefIdFromFaction(___faction, contract.BattleTechGame.DataManager);
            }

            for (int i = 0; i < ___lanceOverrideList.Count; ++i) {
                LanceOverride lanceOverride = ___lanceOverrideList[i];
                lanceOverrides[lanceOverride.GetHashCode()] = __instance;
                lanceOverride.RunMadLibs(contract);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(LanceOverride), "RunMadLibs")]
    public static class LanceOverride_RunMadLibs {
        public static Dictionary<int, TeamOverride> teamOverrides= new Dictionary<int, TeamOverride>();

        public static bool Prefix(LanceOverride __instance, Contract contract,
            TagSet ___lanceTagSet, TagSet ___lanceExcludedTagSet, List<UnitSpawnPointOverride> ___unitSpawnPointOverrideList) {

            TeamOverride teamOverride = TeamOverride_RunMadLibs.lanceOverrides[__instance.GetHashCode()];
            LanceOverride_RunMadLibs.teamOverrides[__instance.GetHashCode()] = teamOverride;
            contract.GameContext.SetObject(GameContextObjectTagEnum.CurrentTeam, teamOverride);

            contract.RunMadLib(___lanceTagSet);
            contract.RunMadLib(___lanceExcludedTagSet);

            for (int i = 0; i < ___unitSpawnPointOverrideList.Count; ++i) {
                ___unitSpawnPointOverrideList[i].RunMadLib(contract);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(LanceOverride), "RunMadLibsOnLanceDef")]
    public static class LanceOverride_RunMadLibsOnLanceDef {
        public static bool Prefix(LanceOverride __instance, Contract contract, LanceDef lanceDef) {
            if (contract != null) {
                // Setup CurrentTeam before running the madlibs
                TeamOverride teamOverride = LanceOverride_RunMadLibs.teamOverrides[__instance.GetHashCode()];
                contract.GameContext.SetObject(GameContextObjectTagEnum.CurrentTeam, teamOverride);
                contract.RunMadLib(lanceDef.LanceTags);

                foreach (LanceDef.Unit unit in lanceDef.LanceUnits) {
                    Traverse ensureTagSetsT = Traverse.Create(unit).Method("EnsureTagSets");
                    ensureTagSetsT.GetValue();                    

                    contract.RunMadLib(unit.unitTagSet);
                    contract.RunMadLib(unit.excludedUnitTagSet);
                    contract.RunMadLib(unit.pilotTagSet);
                    contract.RunMadLib(unit.excludedPilotTagSet);
                }
            }

            return false;
        }
    }

}
