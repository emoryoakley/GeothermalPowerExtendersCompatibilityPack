using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GeothermalPowerExtendersCompatibilityPack
{
    public class ExtenderCompaitableGeothermalGenerator : IExposable
    {
        /// <summary>
        /// 地熱發電機的定義名稱。
        /// </summary>
        private string defName = string.Empty;
        public string DefName
        {
            get => defName;
            set => defName = value;
        }

        /// <summary>
        /// 地熱發電機的名稱。
        /// </summary>
        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// 是否允許讓 extender 在該建築物旁邊建設。
        /// </summary>
        private bool allowed = false;
        public bool Allowed
        {
            get => allowed;
            set => allowed = value;
        }

        /// <summary>
        /// 該地熱發電機來自於哪一個模組名稱。
        /// </summary>
        private string modName = string.Empty;
        public string ModName
        {
            get => modName;
            set => modName = value;
        }

        /// <summary>
        /// 該地熱發電機來自於哪一個模組 packageId。
        /// </summary>
        private string packageId = string.Empty;
        public string PackageId
        {
            get => packageId;
            set => packageId = value;
        }

        /// <summary>
        /// 是否顯示在清單中。
        /// </summary>
        private bool isShow = true;
        public bool IsShow
        {
            get => isShow;
            set => isShow = value;
        }

        public ExtenderCompaitableGeothermalGenerator()
        {

        }

        public ExtenderCompaitableGeothermalGenerator(ThingDef thingDef)
        {
            this.defName = thingDef.defName;
            this.name = thingDef.label;
            this.allowed = Mod.Settings.AllowedSet.ContainsKey(defName);
            this.modName = thingDef.modContentPack?.Name ?? string.Empty;
            this.packageId = thingDef.modContentPack?.PackageId ?? string.Empty;
            this.isShow = true;
        }

        public override bool Equals(object obj)
        {
            if (obj is ExtenderCompaitableGeothermalGenerator other)
            {
                return this.DefName == other.DefName && this.PackageId == other.PackageId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (PackageId + "_" + DefName).GetHashCode();
        }

        /// <summary>
        /// 實現 IExposable 介面的方法，用於序列化和反序列化數據。
        /// </summary>
        public void ExposeData()
        {
            // 使用 Scribe_Values.Look 方法來保存和載入字段值
            Scribe_Values.Look(ref defName, "DefName", string.Empty);
            Scribe_Values.Look(ref allowed, "Allowed", true);
            Scribe_Values.Look(ref packageId, "PackageId", string.Empty);
        }
    }
}
