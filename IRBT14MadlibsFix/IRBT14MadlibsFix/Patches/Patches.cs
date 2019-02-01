using BattleTech;
using BattleTech.Data;
using BattleTech.Framework;
using BattleTech.StringInterpolation;
using Harmony;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace IRBT14MadlibsFix.Patches {

    [HarmonyPatch(typeof(TeamOverride), "RunMadLibs")]
    public static class TeamOverride_RunMadLibs {
        public static Dictionary<int, TeamOverride> lanceOverrides = new Dictionary<int, TeamOverride>();
        public static bool HasLoggedFactions = false;

        public static bool Prefix(TeamOverride __instance, Contract contract, DataManager dataManager, 
            ref Contract ___contract, ref DataManager ___dataManager, ref string ___teamLeaderCastDefId, ref List<LanceOverride> ___lanceOverrideList, ref Faction ___faction) {

            LogDMFactions(dataManager);

            ___contract = contract;
            ___dataManager = dataManager;

            if (___teamLeaderCastDefId == CastDef.castDef_TeamLeader_Current) {
                ___teamLeaderCastDefId = CastDef.GetCombatCastDefIdFromFaction(___faction, contract.BattleTechGame.DataManager);
            }

            for (int i = 0; i < ___lanceOverrideList.Count; ++i) {
                LanceOverride lanceOverride = ___lanceOverrideList[i];
                lanceOverrides[lanceOverride.GetHashCode()] = __instance;
                IRBT14MadlibsFix.Logger.Log($"TO:RML setting teamOverride:{__instance.GetHashCode()} for lanceOverride:{lanceOverride.GetHashCode()}");
                lanceOverride.RunMadLibs(contract);
            }

            return false;
        }

        private static void LogDMFactions(DataManager dataManager) {
            if (!HasLoggedFactions) {
                foreach (KeyValuePair<string, FactionDef> kvp in dataManager.Factions) {
                    IRBT14MadlibsFix.Logger.Log($"{kvp.Key} has factionDef:{kvp.Value} with name:{kvp.Value?.Name} and denonym:{kvp.Value?.Demonym}");
                }
                HasLoggedFactions = true;
            }
        }
    }

    [HarmonyPatch(typeof(LanceOverride), "RunMadLibs")]
    public static class LanceOverride_RunMadLibs {
        public static Dictionary<int, TeamOverride> teamOverrides= new Dictionary<int, TeamOverride>();

        public static bool Prefix(LanceOverride __instance, Contract contract,
            ref TagSet ___lanceTagSet, ref TagSet ___lanceExcludedTagSet, ref List<UnitSpawnPointOverride> ___unitSpawnPointOverrideList) {

            TeamOverride teamOverride = TeamOverride_RunMadLibs.lanceOverrides[__instance.GetHashCode()];
            IRBT14MadlibsFix.Logger.Log($"LO:RML using teamOverride:{teamOverride.GetHashCode()} for lanceOverride:{__instance?.GetHashCode()}");
            LanceOverride_RunMadLibs.teamOverrides[__instance.GetHashCode()] = teamOverride;
            contract.GameContext.SetObject(GameContextObjectTagEnum.CurrentTeam, teamOverride);

            IRBT14MadlibsFix.Logger.Log($"LO:RML BEFORE madlibs lanceTagSet is:{___lanceTagSet}");
            contract.RunMadLib(___lanceTagSet);
            IRBT14MadlibsFix.Logger.Log($"LO:RML AFTER madlibs lanceTagSet is:{___lanceTagSet}");
            contract.RunMadLib(___lanceExcludedTagSet);

            for (int i = 0; i < ___unitSpawnPointOverrideList.Count; ++i) {
                ___unitSpawnPointOverrideList[i].RunMadLib(contract);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(LanceOverride), "RequestLance")]
    public static class LanceOverride_RequestLance {

        public static bool Prefix(LanceOverride __instance, Contract contract) {
            IRBT14MadlibsFix.Logger.Log($"LO:RL invoked with contract:{contract} with GUID:{contract?.GUID}");
            return true;
        }
    }

        [HarmonyPatch]
    public static class LanceOverride_RunMadLibsOnLanceDef {

        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(LanceOverride), "RunMadLibsOnLanceDef",
                new Type[] { typeof(Contract), typeof(LanceDef) });
        }

        public static bool Prefix(LanceOverride __instance, Contract contract, LanceDef lanceDef) {
            IRBT14MadlibsFix.Logger.Log($"LO:RMLOLD - entered.");
            if (contract != null) {
                // Setup CurrentTeam before running the madlibs
                TeamOverride teamOverride = LanceOverride_RunMadLibs.teamOverrides[__instance.GetHashCode()];
                IRBT14MadlibsFix.Logger.Log($"LO:RMLOLD using teamOverride:{teamOverride.GetHashCode()} for lanceOverride:{__instance?.GetHashCode()}");

                contract.GameContext.SetObject(GameContextObjectTagEnum.CurrentTeam, teamOverride);
                contract.RunMadLib(lanceDef.LanceTags);

                foreach (LanceDef.Unit unit in lanceDef.LanceUnits) {
                    Traverse ensureTagSetsT = Traverse.Create(unit).Method("EnsureTagSets");
                    ensureTagSetsT.GetValue();

                    IRBT14MadlibsFix.Logger.Log($"LO:RMLOLD BEFORE madlibs lanceTagSet is:{unit.unitTagSet}");
                    contract.RunMadLib(unit.unitTagSet);
                    IRBT14MadlibsFix.Logger.Log($"LO:RMLOLD AFTER madlibs lanceTagSet is:{unit.unitTagSet}");

                    contract.RunMadLib(unit.excludedUnitTagSet);
                    contract.RunMadLib(unit.pilotTagSet);
                    contract.RunMadLib(unit.excludedPilotTagSet);
                }
            }

            return false;
        }
    }


    [HarmonyPatch]
    public static class Interpolator_GetObjectByStringFromObject {

        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(Interpolator), "GetObjectByStringFromObject",
                new Type[] { typeof(string), typeof(object) });
        }
        
        public static bool Prefix(string expr, object obj) {
            IRBT14MadlibsFix.Logger.Log($"I:GOBSFO entered with expr:{expr} and obj:{obj}");
            return true;
        }
    }

}
