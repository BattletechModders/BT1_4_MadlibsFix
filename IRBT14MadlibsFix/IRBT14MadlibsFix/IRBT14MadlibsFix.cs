using Harmony;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace LowVisibility {
    public class IRBT14MadlibsFix {

        public const string HarmonyPackage = "us.frostraptor.IRBT14MadlibsFix";

        public static Logger Logger;
        public static string ModDir;
        public static ModConfig Config;

        public static string CampaignSeed;

        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON) {
            ModDir = modDirectory;

            Exception settingsE;
            try {
                IRBT14MadlibsFix.Config = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            } catch (Exception e) {
                settingsE = e;
                IRBT14MadlibsFix.Config = new ModConfig();
            }

            Logger = new Logger(modDirectory, "irbt14madlibsfix");
            Logger.Log($"ModDir is:{modDirectory}");
            Logger.Log($"mod.json settings are:({settingsJSON})");
            Logger.Log($"mergedConfig is:{IRBT14MadlibsFix.Config}");

            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }
}
