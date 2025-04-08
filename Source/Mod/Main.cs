using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace GeothermalPowerExtendersCompatibilityPack
{
    [StaticConstructorOnStartup]
    public class Main
    {
        /// <summary>
        /// 找出RimWorld以及所有模組中，包含地熱發電機的建築物
        /// </summary>
        public static Dictionary<string, ExtenderCompaitableGeothermalGenerator> GeothermalGenerators = new Dictionary<string, ExtenderCompaitableGeothermalGenerator>();

        static Main()
        {
            // 獲取當前執行的dll組件，印出組件建置編號以及釋出版本號
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Log.Message($"[GeothermalPowerExtendersCompatibilityPack] Current assembly build version is {executingAssembly.GetName().Version}.");

            // 使用反射獲取 AssemblyInformationalVersion
            var informationalVersionAttribute = executingAssembly
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .OfType<AssemblyInformationalVersionAttribute>()
                .FirstOrDefault();

            string productVersion = informationalVersionAttribute?.InformationalVersion ?? "";

            if (!string.IsNullOrEmpty(productVersion))
            {
                Log.Message($"[GeothermalPowerExtendersCompatibilityPack] Current assembly product version is {productVersion}.");
            }

            LongEventHandler.QueueLongEvent(Initialize, "Initializing", false, null);
        }

        private static void Initialize()
        {
            if (DefDatabase<ThingDef>.AllDefsListForReading == null)
            {
                Log.Message($"[GeothermalPowerExtendersCompatibilityPack] Not ready.");
            }
            else
            {
                GeothermalGenerators = DefDatabase<ThingDef>.AllDefsListForReading
                    // 能夠建造在蒸氣間歇泉上的建築物就會認定為是地熱發電機
                    .Where(def => def.placeWorkers != null && def.placeWorkers.Any(pw => typeof(PlaceWorker_OnSteamGeyser).IsAssignableFrom(pw)))
                    // 排除掉藍圖與框架類的建築物
                    .Where(def => !def.defName.StartsWith("Blueprint_") && !def.defName.StartsWith("Frame_"))
                    .Select(def => new ExtenderCompaitableGeothermalGenerator(def))
                    .ToDictionary(def => def.DefName, def => def);

                Log.Message($"[GeothermalPowerExtendersCompatibilityPack] Found {GeothermalGenerators.Count} kinds of geothermal generators.");
            }
        }
    }
}
