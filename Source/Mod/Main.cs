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
        }
    }
}
