using CustomTranslator;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace GeothermalPowerExtendersCompatibilityPack
{
    public class Mod : Verse.Mod
    {
        /// <summary>
        /// 模組的設定
        /// </summary>
        public static ModSettings Settings { get; private set; }

        /// <summary>
        /// 查詢文字
        /// </summary>
        private string searchText = string.Empty;

        /// <summary>
        /// 用來記錄展開的群組狀態的字典，key 為模組的 packageId，值為是否展開的布林值。
        /// </summary>
        private Dictionary<string, bool> groupExpanded = new Dictionary<string, bool>();

        /// <summary>
        /// 用來記錄滾動位置的 Vector2 變數。
        /// </summary>
        private Vector2 scrollPosition;

        /// <summary>
        /// 用來記錄是否展開所有群組。
        /// </summary>
        private bool expandAll = false;

        public Mod(Verse.ModContentPack content) : base(content)
        {
            Settings = GetSettings<ModSettings>();
        }

        /// <summary>
        /// 顯示模組的設定視窗內容。
        /// </summary>
        /// <param name="inRect"></param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            // 創建一個 Listing_Standard 來顯示清單
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            // 顯示查詢輸入框和按鈕
            listingStandard.Label(Constants.ModSettingFilterLabel.TranslateOrFallback());
            Rect searchRect = listingStandard.GetRect(30f);
            Rect searchTextRect = new Rect(searchRect.x, searchRect.y, searchRect.width - 440f, searchRect.height);
            Rect searchButtonRect = new Rect(searchRect.x + searchRect.width - 410f, searchRect.y, 90f, searchRect.height);
            Rect selectAllButtonRect = new Rect(searchRect.x + searchRect.width - 310f, searchRect.y, 90f, searchRect.height);
            Rect deselectAllButtonRect = new Rect(searchRect.x + searchRect.width - 210f, searchRect.y, 90f, searchRect.height);
            Rect expandToggleButtonRect = new Rect(searchRect.x + searchRect.width - 110f, searchRect.y, 90f, searchRect.height);

            searchText = Widgets.TextField(searchTextRect, searchText);

            // 查詢按鈕
            if (Widgets.ButtonText(searchButtonRect, Constants.ModSettingFilterButton.TranslateOrFallback()))
            {
                FilterGenerators();
            }

            // 全部選擇按鈕
            if (Widgets.ButtonText(selectAllButtonRect, Constants.ModSettingSelectAll.TranslateOrFallback()))
            {
                CheckAll(true);
            }

            // 全部取消按鈕
            if (Widgets.ButtonText(deselectAllButtonRect, Constants.ModSettingDeselectAll.TranslateOrFallback()))
            {
                CheckAll(false);
            }

            // 全部展開/全部縮合所有群組的按鈕
            if (Widgets.ButtonText(expandToggleButtonRect, expandAll ? Constants.ModSettingCollapseAll.TranslateOrFallback() : Constants.ModSettingCollapseAll.TranslateOrFallback()))
            {
                expandAll = !expandAll;

                foreach (var key in groupExpanded.Keys.ToList())
                {
                    groupExpanded[key] = expandAll;
                }
            }

            listingStandard.Gap();

            // 將同樣 packageId 的物件分作一群
            var groupedGenerators = Main.GeothermalGenerators.Values.Where(generator => generator.IsShow).GroupBy(generator => generator.PackageId);

            Rect descriptionRect = new Rect(inRect.x, searchRect.yMax + 12f, inRect.width, 30f);
            int total = groupedGenerators.Sum(group => group.Count());
            // 計數器
            string counterLabel = Constants.ModSettingCounterLabel.TranslateOrFallback(groupedGenerators.Count(), total);
            listingStandard.Label(counterLabel);

            // 是否可以斜角擺放的勾選欄標籤
            string adjacentCheckBoxLabel = Constants.ModSettingAdjacentCheckBoxLabel.TranslateOrFallback();
            // 是否可以斜角擺放的勾選欄提示說明
            string adjacentCheckBoxTooltip = Constants.ModSettingAdjacentCheckBoxTooltip.TranslateOrFallback();
            listingStandard.CheckboxLabeled(adjacentCheckBoxLabel, ref Settings.Adjacent8Way, adjacentCheckBoxTooltip);

            listingStandard.GapLine(24f);

            // 創建一個可滾動的區域
            Rect scrollRect = new Rect(inRect.x, descriptionRect.yMax + 36f, inRect.width, inRect.height - searchRect.yMax - descriptionRect.yMax + 100f);
            // 調整 viewRect 的高度以符合整體畫面的高度
            Rect viewRect = new Rect(0, 0, inRect.width - 16f, inRect.height * 2);
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);

            listingStandard.Begin(viewRect);

            foreach (var group in groupedGenerators)
            {
                if (!groupExpanded.ContainsKey(group.Key))
                {
                    groupExpanded[group.Key] = true; // 預設為全部展開
                }

                bool expanded = groupExpanded[group.Key];
                Rect groupRect = listingStandard.GetRect(30f);
                Rect buttonRect = new Rect(groupRect.x, groupRect.y - 8f, 30f, groupRect.height);
                Rect labelRect = new Rect(groupRect.x + 35f, groupRect.y, groupRect.width - 35f, groupRect.height);

                if (Widgets.ButtonImage(buttonRect, expanded ? TexButton.Collapse : TexButton.Reveal))
                {
                    groupExpanded[group.Key] = !expanded;
                }

                string groupLabel = Constants.ModSettingListGroupLabel.TranslateOrFallback(group.First().ModName);
                Widgets.Label(labelRect, groupLabel);
                listingStandard.Gap();

                if (expanded)
                {
                    foreach (var generator in group)
                    {
                        bool allowed = generator.Allowed;
                        listingStandard.CheckboxLabeled(generator.Name, ref allowed, generator.ModName);
                        generator.Allowed = allowed;

                        if (allowed)
                        {
                            Settings.AllowedSet[generator.DefName] = generator;
                        }
                        else
                        {
                            Settings.AllowedSet.Remove(generator.DefName);
                        }

                        listingStandard.GapLine();
                    }
                    listingStandard.Gap();
                }
            }

            listingStandard.End();

            Widgets.EndScrollView();

            listingStandard.End();
        }

        private void FilterGenerators()
        {
            // 篩選出符合查詢文字的地熱發電機
            if (!string.IsNullOrEmpty(searchText))
            {
                foreach (var pair in Main.GeothermalGenerators)
                {
                    pair.Value.IsShow = pair.Value.Name.Contains(searchText) || pair.Value.ModName.Contains(searchText);
                }
            }
            // 清空查詢文字，顯示全部的地熱發電機
            else
            {
                foreach (var pair in Main.GeothermalGenerators)
                {
                    pair.Value.IsShow = true;
                }
            }
        }

        private void CheckAll(bool isAllowed)
        {
            // 全部允許/全部取消按鈕邏輯
            foreach (var pair in Main.GeothermalGenerators)
            {
                pair.Value.Allowed = isAllowed;
                Settings.AllowedSet[pair.Value.DefName] = pair.Value;
            }
        }

        public override string SettingsCategory()
        {
            return Constants.ModSettingHeaderTitle.TranslateOrFallback();
        }
    }
}
