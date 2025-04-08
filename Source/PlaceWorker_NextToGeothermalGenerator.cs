using CustomTranslator;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace GeothermalPowerExtendersCompatibilityPack
{
    public class PlaceWorker_NextToGeothermalGenerator : PlaceWorker
    {
        /// <summary>
        /// IsAllowed的規則是：
        /// 檢查給定的 ThingDef 是否在相容的地熱發電機列表中，且被設置為允許在周圍建造。
        /// </summary>
        private static Predicate<ThingDef> IsAllowed = thingDef => Mod.Settings.AllowedSet.ContainsKey(thingDef.defName);

        /// <summary>
        /// 取得周圍格子，Mod.Settings.Adjacent8Way 為 true 時，返回所有相鄰(包含斜角)的格子，否則僅返回上下左右的相鄰格子。
        /// </summary>
        private static Func<IntVec3, IEnumerable<IntVec3>> GetAdjacencies = cell =>
        {
            // 如果設置為允許斜對角放置，則返回所有相鄰的格子
            if (Mod.Settings.Adjacent8Way)
            {
                return GenAdj.CellsAdjacent8Way(cell, Rot4.North, new IntVec2(1, 1));
            }
            // 否則僅返回上下左右的相鄰格子
            else
            {
                return GenAdj.CellsAdjacentCardinal(cell, Rot4.North, new IntVec2(1, 1));
            }
        };

        /// <summary>
        /// 檢查是否允許在指定位置放置建築物。
        /// </summary>
        /// <param name="checkingDef">正在嘗試放置的建築物的定義。</param>
        /// <param name="loc">嘗試放置建築物的位置。</param>
        /// <param name="rot">建築物的旋轉方向。</param>
        /// <param name="map">地圖對象，表示建築物將被放置的地圖。</param>
        /// <param name="thingToIgnore">在檢查時要忽略的物件，通常是正在移動的物件。</param>
        /// <param name="thing">可能是正在放置的具體物件。</param>
        /// <returns>返回一個 AcceptanceReport，表示是否允許放置。</returns>
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            // 獲取放置的建築物的大小，Geothermal Power Extender 建築物應該為 2x2
            IntVec2 size = checkingDef.Size;

            bool isNextToGeothermalGenerator = GenAdj.OccupiedRect(loc, rot, size) // 建築物佔據的格子
                .EdgeCells // 建築物佔據的格子的周圍
                .SelectMany(cell => GetAdjacencies(cell)) // 獲取指定格子的周圍格子
                .Where(cell => cell.InBounds(map)) // 篩選是在地圖範圍內的格子
                .ToHashSet()  // 最終將集合轉換為 HashSet，能夠排除重覆
                .Select(cell => cell.GetTransmitter(map)) // 獲取每一個格子上的能夠傳輸電力的建築物(具有 CompProperties_Power 屬性的建築物)
                .Any(building => building != null && IsAllowed(building.def)); // 有任意一格子上有建築物，且符合 IsAllowed 規則的話，就回傳 true

            return isNextToGeothermalGenerator ? AcceptanceReport.WasAccepted : new AcceptanceReport(Constants.PlaceWorkerWarningMustPlaceExtenderNextToGenerator.TranslateOrFallback());
        }
    }
}
