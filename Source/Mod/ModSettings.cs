using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GeothermalPowerExtendersCompatibilityPack
{
    public class ModSettings : Verse.ModSettings
    {
        /// <summary>
        /// 允許 Extender 建築物蓋在周圍的地熱發電機集合。
        /// </summary>
        public Dictionary<string, ExtenderCompaitableGeothermalGenerator> AllowedSet = new Dictionary<string, ExtenderCompaitableGeothermalGenerator>();

        public bool Adjacent8Way = true;

        /// <summary>
        /// 負責保存和載入模組設定的數據。
        /// 使用 Scribe 系統對 `AllowedSet` 進行序列化處理。
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            // 是否可以在斜對角放置
            Scribe_Values.Look(ref Adjacent8Way, "Adjacent8Way", true, true);

            // 保存時，將 Dictionary 的鍵和值分別保存到兩個列表中
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                List<string> keys = AllowedSet.Keys.ToList();
                List<ExtenderCompaitableGeothermalGenerator> values = AllowedSet.Values.ToList();
                Scribe_Collections.Look(ref keys, "keys", LookMode.Value);
                Scribe_Collections.Look(ref values, "values", LookMode.Deep);
            }

            // 載入時，從兩個列表中重建 Dictionary
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<string> keys = null;
                List<ExtenderCompaitableGeothermalGenerator> values = null;
                Scribe_Collections.Look(ref keys, "keys", LookMode.Value);
                Scribe_Collections.Look(ref values, "values", LookMode.Deep);

                if (keys != null && values != null && keys.Count == values.Count)
                {
                    AllowedSet = new Dictionary<string, ExtenderCompaitableGeothermalGenerator>();
                    for (int i = 0; i < keys.Count; i++)
                    {
                        AllowedSet[keys[i]] = values[i];
                    }
                }
                else
                {
                    AllowedSet = new Dictionary<string, ExtenderCompaitableGeothermalGenerator>();
                }
            }

            // 如果 AllowedSet 為 null，則初始化為一個新的字典
            if (AllowedSet == null)
            {
                AllowedSet = new Dictionary<string, ExtenderCompaitableGeothermalGenerator>();
            }

            Log.Message("[GeothermalPowerExtendersCompatibilityPack] Load or Save Scribe Data.");
        }
    }
}
