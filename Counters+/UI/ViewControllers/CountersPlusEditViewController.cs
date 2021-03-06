﻿using CustomUI.BeatSaber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using VRUI;
using CountersPlus.Config;
using UnityEngine;
using TMPro;
using CustomUI.Settings;
using CountersPlus.Custom;
using System.Threading;
using BS_Utils.Gameplay;
using System.Collections;

namespace CountersPlus.UI
{
    class CountersPlusEditViewController : VRUIViewController
    {
        public static CountersPlusEditViewController Instance;
        private static RectTransform rect;
        private static TextMeshProUGUI settingsTitle;
        private static SubMenu container;
        
        internal static List<GameObject> loadedElements = new List<GameObject>(); //Mass clearing
        private static List<ListSettingsController> loadedSettings = new List<ListSettingsController>(); //Mass initialization
        internal static int settingsCount = 0; //Spacing

        internal class PositionSettingsViewController : TupleViewController<Tuple<ICounterPositions, string>> { }
        static List<Tuple<ICounterPositions, string>> positions = new List<Tuple<ICounterPositions, string>> {
            {ICounterPositions.BelowCombo, "Below Combo" },
            {ICounterPositions.AboveCombo, "Above Combo" },
            {ICounterPositions.BelowMultiplier, "Below Multi." },
            {ICounterPositions.AboveMultiplier, "Above Multi." },
            {ICounterPositions.BelowEnergy, "Below Energy" },
            {ICounterPositions.AboveHighway, "Over Highway" }
        };

        static Action<RectTransform, float, float, float, float, float> setPositioning = delegate (RectTransform r, float x, float y, float w, float h, float pivotX)
        {
            r.anchorMin = new Vector2(x, y);
            r.anchorMax = new Vector2(x + w, y + h);
            r.pivot = new Vector2(pivotX, 1);
            r.sizeDelta = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
        };

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            rect = rectTransform;
            if (firstActivation)
            {
                Instance = this;
                CreateCredits();
            }
        }

        private static void CreateCredits()
        {
            ClearScreen();
            TextMeshProUGUI name, version, creator, contributorLabel;
            Dictionary<string, string> contributors = new Dictionary<string, string>()
            {
                { "Moon", "Bug fixing and code optimization" },
                { "Shoko84", "Bug fixing" },
                { "xhuytox", "Big helper in bug hunting - thanks man!" },
                { "Brian", "Saving Beat Saber Modding with CustomUI" },
                { "Assistant", "Stole some Custom Avatars UI code to help with settings" },
                { "Kyle1413", "Beat Saber Utils and for Progress/Score Counter code" },
                { "Ragesaq", "Speed Counter and Spinometer idea <i>(and some bug fixing on stream)</i>" },
                { "SkyKiwiTV", "Original version checking code" },
                { "Dracrius", "Bug fixing and beta testing" },
                { "Stackeror", "Creator of the original Progress Counter mod" },
            };
            string user = GetUserInfo.GetUserName();
            if (contributors.ContainsKey(user))
                contributors.Add($"<i>\"{user}\"</i>", "For enjoying this mod!");
            else contributors.Add(user, "For enjoying this mod!"); //Teehee :)
            name = BeatSaberUI.CreateText(rect, "Counters+", Vector2.zero);
            name.fontSize = 10;
            name.alignment = TextAlignmentOptions.Center;
            name.characterWidthAdjustment = 2;
            setPositioning(name.rectTransform, 0, 0.8f, 1, 0.166f, 0.5f);

            version = BeatSaberUI.CreateText(rect,
                $"Version <color={(Plugin.upToDate ? "#00FF00" : "#FF0000")}>{Plugin.Instance.Version}</color>", Vector2.zero);
            version.fontSize = 3;
            version.alignment = TextAlignmentOptions.Center;
            setPositioning(version.rectTransform, 0, 0.73f, 1, 0.166f, 0.5f);

            if (!Plugin.upToDate)
            {
                TextMeshProUGUI warning = BeatSaberUI.CreateText(rect,
                $"<color=#FF0000>Version {Plugin.webVersion} available for download!</color>", Vector2.zero);
                warning.fontSize = 3;
                warning.alignment = TextAlignmentOptions.Center;
                setPositioning(warning.rectTransform, 0, 0.7f, 1, 0.166f, 0.5f);
                loadedElements.Add(warning.gameObject);
            }

            creator = BeatSaberUI.CreateText(rect, "Developed by: <color=#00c0ff>Caeden117</color>", Vector2.zero);
            creator.fontSize = 5;
            creator.alignment = TextAlignmentOptions.Center;
            setPositioning(creator.rectTransform, 0, 0.64f, 1, 0.166f, 0.5f);

            contributorLabel = BeatSaberUI.CreateText(rect, "Thanks to these contributors for, directly or indirectly, helping make Counters+ what it is!", Vector2.zero);
            contributorLabel.fontSize = 3;
            contributorLabel.alignment = TextAlignmentOptions.Center;
            setPositioning(contributorLabel.rectTransform, 0, 0.55f, 1, 0.166f, 0.5f);

            foreach(var kvp in contributors)
            {
                TextMeshProUGUI contributor = BeatSaberUI.CreateText(rect, $"<color=#00c0ff>{kvp.Key}</color> | {kvp.Value}", Vector2.zero);
                contributor.fontSize = 3;
                contributor.alignment = TextAlignmentOptions.Left;
                setPositioning(contributor.rectTransform, 0.15f,
                    0.5f - (contributors.Keys.ToList().IndexOf(kvp.Key) * 0.05f), 1, 0.166f, 0.5f);
                loadedElements.Add(contributor.gameObject);
            }

            loadedElements.AddRange(new GameObject[] { name.gameObject, version.gameObject, creator.gameObject, contributorLabel.gameObject});
        }

        public static void UpdateSettings<T>(T settings, SettingsInfo info, bool isMain = false, bool isCredits = false) where T : IConfigModel
        {
            try
            {
                MockCounter.Highlight(settings);
                ClearScreen();
                if (!(info is null))
                {
                    if (info.IsCustom) container = CreateBase(settings, (settings as CustomConfigModel).RestrictedPositions);
                    else if (!isMain)
                    {
                        SubMenu sub = CreateBase(settings);
                        AdvancedCounterSettings.counterUIItems.Where(
                            (KeyValuePair<IConfigModel, Action<SubMenu, IConfigModel>> x) => (x.Key.DisplayName == settings.DisplayName)
                            ).First().Value(sub, settings);
                    }
                }
                if (!isCredits)
                {
                    settingsTitle = BeatSaberUI.CreateText(rect, $"{(isMain ? "Main" : settings.DisplayName)} Settings", Vector2.zero);
                    settingsTitle.fontSize = 6;
                    settingsTitle.alignment = TextAlignmentOptions.Center;
                    setPositioning(settingsTitle.rectTransform, 0, 0.85f, 1, 0.166f, 0.5f);
                    loadedElements.Add(settingsTitle.gameObject);
                    if (isMain)
                    {
                        SubMenu sub = new SubMenu(rect);
                        var enabled = AddList(ref sub, settings, "Enabled", "Toggles Counters+ on or off.", 2);
                        enabled.GetTextForValue = (v) => (v != 0f) ? "ON" : "OFF";
                        enabled.GetValue = () => CountersController.settings.Enabled ? 1f : 0f;
                        enabled.SetValue = (v) => CountersController.settings.Enabled = v != 0f;

                        var toggleCounters = AddList(ref sub, settings, "Advanced Mock Counters", "Allows the mock counters to display more settings. To increase preformance, and reduce chances of bugs, disable this option.", 2);
                        toggleCounters.GetTextForValue = (v) => (v != 0f) ? "ON" : "OFF";
                        toggleCounters.GetValue = () => CountersController.settings.AdvancedCounterInfo ? 1f : 0f;
                        toggleCounters.SetValue = (v) => CountersController.settings.AdvancedCounterInfo = v != 0f;

                        var comboOffset = AddList(ref sub, settings, "Combo Offset", "How far from the Combo counters should be before Distance is taken into account.", 20);
                        comboOffset.GetTextForValue = (v) => ((v - 10) / 10).ToString();
                        comboOffset.GetValue = () => (CountersController.settings.ComboOffset * 10) + 10;
                        comboOffset.SetValue = (v) => CountersController.settings.ComboOffset = ((v - 10) / 10);

                        var multiOffset = AddList(ref sub, settings, "Multiplier Offset", "How far from the Multiplier counters should be before Distance is taken into account.", 20);
                        multiOffset.GetTextForValue = (v) => ((v - 10) / 10).ToString();
                        multiOffset.GetValue = () => (CountersController.settings.MultiplierOffset * 10) + 10;
                        multiOffset.SetValue = (v) => CountersController.settings.MultiplierOffset = ((v - 10) / 10);
                        
                        toggleCounters.SetValue += (v) => CountersPlusSettingsFlowCoordinator.UpdateMockCounters();
                        comboOffset.SetValue += (v) => CountersPlusSettingsFlowCoordinator.UpdateMockCounters();
                        multiOffset.SetValue += (v) => CountersPlusSettingsFlowCoordinator.UpdateMockCounters();
                    }
                }
                else CreateCredits();
                foreach (ListViewController list in loadedSettings) list.Init();
            }
            catch(Exception e) { Plugin.Log(e.ToString(), Plugin.LogInfo.Fatal); }
        }

        private static SubMenu CreateBase<T>(T settings, params ICounterPositions[] restricted) where T : IConfigModel
        {
            SubMenu sub = new SubMenu(rect);
            List<Tuple<ICounterPositions, string>> restrictedList = new List<Tuple<ICounterPositions, string>>();
            try
            {
                foreach (ICounterPositions pos in restricted)
                    restrictedList.Add(Tuple.Create(pos, positions.Where((Tuple<ICounterPositions, string> x) => x.Item1 == pos).First().Item2));
            }
            catch { } //It most likely errors here. If it does, well no problem.

            var enabled = AddList(ref sub, settings, "Enabled", "Toggles this counter on or off.", 2);
            enabled.GetTextForValue = (v) => (v != 0f) ? "ON" : "OFF";
            enabled.GetValue = () => settings.Enabled ? 1f : 0f;
            enabled.SetValue += (v) => settings.Enabled = v != 0f;

            var position = AddList(ref sub, settings, "Position", "The relative position of common UI elements", (restrictedList.Count() == 0) ? positions.Count() : restrictedList.Count());
            position.GetTextForValue = (v) => {
                if (restrictedList.Count() == 0)
                    return positions[Mathf.RoundToInt(v)].Item2;
                else
                    return restrictedList[Mathf.RoundToInt(v)].Item2;
            };
            position.GetValue = () => {
                return positions.ToList().IndexOf(positions.Where((Tuple<ICounterPositions, string> x) => (x.Item1 == settings.Position)).First());
            };
            position.SetValue += (v) => {
                if (restrictedList.Count() == 0)
                    settings.Position = positions[Mathf.RoundToInt(v)].Item1;
                else
                    settings.Position = restrictedList[Mathf.RoundToInt(v)].Item1;
            };

            var index = AddList(ref sub, settings, "Distance", "How far from the position the counter will be. A higher number means farther way.", 7);
            index.GetTextForValue = (v) => Mathf.RoundToInt(v - 1).ToString();
            index.GetValue = () => settings.Index + 1;
            index.SetValue += (v) => settings.Index = Mathf.RoundToInt(v - 1);
            return sub;
        }

        internal static ListViewController AddList<T>(ref SubMenu sub, T settings, string Label, string HintText, int sizeCount) where T : IConfigModel
        {
            List<float> values = new List<float>() { };
            for (var i = 0; i < sizeCount; i++) values.Add(i);
            var list = sub.AddList(Label, values.ToArray(), HintText);
            list.applyImmediately = true;
            PositionElement(list.gameObject);
            loadedSettings.Add(list);
            if (!(settings is null)) list.SetValue = (v) => Instance.StartCoroutine(DelayedMockCounterUpdate(settings));
            return list;
        }

        private static IEnumerator DelayedMockCounterUpdate<T>(T settings) where T : IConfigModel
        {
            yield return new WaitForEndOfFrame();
            MockCounter.Update(settings);
        }

        private static void ClearScreen()
        {
            foreach (GameObject element in loadedElements) Destroy(element);
            loadedElements.Clear();
            loadedSettings.Clear();
            settingsCount = 0;
        }

        private static void PositionElement(GameObject element)
        {
            loadedElements.Add(element);
            setPositioning(element.transform as RectTransform, 0.05f, 0.75f - (settingsCount * 0.1f), 0.9f, 0.166f, 0f);
            settingsCount++;
        }
    }
}