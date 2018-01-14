using PoeHUD.Controllers;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.RemoteMemoryObjects;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AutoGolem
{
    internal class AutoGolem : BaseSettingsPlugin<AutoGolemSettings>
    {
        private IngameData data;
        private bool isTown;
        private KeyboardHelper keyboard;
        private Stopwatch stopwatch = new Stopwatch();
        private int highlightSkill = 0;

        public override void Initialise()
        {
            PluginName = "Auto Golem";

            OnSettingsToggle();
            Settings.Enable.OnValueChanged += OnSettingsToggle;

            Settings.ChaosGolemConnectedSkill.OnValueChanged += OnChaosGolemConnectedSkillChanged;
            Settings.FireGolemConnectedSkill.OnValueChanged += OnFireGolemConnectedSkillChanged;
            Settings.IceGolemConnectedSkill.OnValueChanged += OnIceGolemConnectedSkillChanged;
            Settings.LightningGolemConnectedSkill.OnValueChanged += OnLightningGolemConnectedSkillChanged;
            Settings.StoneGolemConnectedSkill.OnValueChanged += OnStoneGolemConnectedSkillChanged;
        }

        private void OnChaosGolemConnectedSkillChanged()
        {
            highlightSkill = Settings.ChaosGolemConnectedSkill.Value - 1;
            stopwatch.Restart();
        }

        private void OnFireGolemConnectedSkillChanged()
        {
            highlightSkill = Settings.FireGolemConnectedSkill.Value - 1;
            stopwatch.Restart();
        }

        private void OnIceGolemConnectedSkillChanged()
        {
            highlightSkill = Settings.IceGolemConnectedSkill.Value - 1;
            stopwatch.Restart();
        }

        private void OnLightningGolemConnectedSkillChanged()
        {
            highlightSkill = Settings.LightningGolemConnectedSkill.Value - 1;
            stopwatch.Restart();
        }

        private void OnStoneGolemConnectedSkillChanged()
        {
            highlightSkill = Settings.StoneGolemConnectedSkill.Value - 1;
            stopwatch.Restart();
        }

        private void OnSettingsToggle()
        {
            try
            {
                if (Settings.Enable.Value)
                {
                    GameController.Area.OnAreaChange += OnAreaChange;
                    GameController.Area.RefreshState();

                    keyboard = new KeyboardHelper(GameController);

                    stopwatch.Reset();

                    var checkThread = new Thread(CheckThread) { IsBackground = true };
                    checkThread.Start();
                }
                else
                {
                    GameController.Area.OnAreaChange -= OnAreaChange;

                    stopwatch.Stop();
                }
            }
            catch (Exception)
            {
            }
        }

        public override void Render()
        {
            base.Render();
            if (!Settings.Enable.Value) return;

            if (stopwatch.IsRunning && stopwatch.ElapsedMilliseconds < 5000)
            {
                IngameUIElements ingameUiElements = GameController.Game.IngameState.IngameUi;
                Graphics.DrawFrame(ingameUiElements.SkillBar[highlightSkill].GetClientRect(), 3f, Color.Yellow);
            }
            else
            {
                stopwatch.Stop();
            }
        }

        private void OnAreaChange(AreaController area)
        {
            if (Settings.Enable.Value)
            {
                data = GameController.Game.IngameState.Data;
                isTown = area.CurrentArea.IsTown;
            }
        }

        private void CheckThread()
        {
            while (Settings.Enable.Value)
            {
                GolemMain();
                Thread.Sleep(1000);
            }
        }

        private void GolemMain()
        {
            if (GameController == null || GameController.Window == null || GameController.Game.IngameState.Data.LocalPlayer == null || GameController.Game.IngameState.Data.LocalPlayer.Address == 0x00)
                return;

            if (!GameController.Window.IsForeground())
                return;

            if (!GameController.Game.IngameState.Data.LocalPlayer.IsValid)
                return;

            var playerLife = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Life>();
            if (playerLife == null || isTown)
                return;

            try
            {
                IngameUIElements ingameUiElements = GameController.Game.IngameState.IngameUi;
                List<int> golems = data.LocalPlayer.GetComponent<Actor>().Minions;

                int countChaosGolem = 0;
                int countFireGolem = 0;
                int countIceGolem = 0;
                int countLightningGolem = 0;
                int countStoneGolem = 0;

                foreach (var golemId in golems)
                {
                    if (data.EntityList.EntitiesAsDictionary.ContainsKey(golemId))
                    {
                        var golemPathString = data.EntityList.EntitiesAsDictionary[golemId].Path;

                        if (golemPathString.Contains("ChaosElemental"))
                            countChaosGolem++;
                        if (golemPathString.Contains("FireElemental"))
                            countFireGolem++;
                        if (golemPathString.Contains("IceElemental"))
                            countIceGolem++;
                        if (golemPathString.Contains("LightningGolem"))
                            countLightningGolem++;
                        if (golemPathString.Contains("RockGolem"))
                            countStoneGolem++;
                    }
                }

                if (Settings.ChaosGolem.Value && countChaosGolem < Settings.ChaosGolemMax.Value && ingameUiElements.SkillBar[Settings.ChaosGolemConnectedSkill.Value - 1].SkillIconPath.Contains("ChaosElementalSummon"))
                {
                    keyboard.KeyPressRelease(Settings.ChaosGolemKeyPressed.Value);
                }
                if (Settings.FireGolem.Value && countFireGolem < Settings.FireGolemMax.Value && ingameUiElements.SkillBar[Settings.FireGolemConnectedSkill.Value - 1].SkillIconPath.Contains("FireElementalSummon"))
                {
                    keyboard.KeyPressRelease(Settings.FireGolemKeyPressed.Value);
                }
                if (Settings.IceGolem.Value && countIceGolem < Settings.IceGolemMax.Value && ingameUiElements.SkillBar[Settings.IceGolemConnectedSkill.Value - 1].SkillIconPath.Contains("IceElementalSummon"))
                {
                    keyboard.KeyPressRelease(Settings.IceGolemKeyPressed.Value);
                }
                if (Settings.LightningGolem.Value && countLightningGolem < Settings.LightningGolemMax.Value && ingameUiElements.SkillBar[Settings.LightningGolemConnectedSkill.Value - 1].SkillIconPath.Contains("LightningGolem"))
                {
                    keyboard.KeyPressRelease(Settings.LightningGolemKeyPressed.Value);
                }
                if (Settings.StoneGolem.Value && countStoneGolem < Settings.StoneGolemMax.Value && ingameUiElements.SkillBar[Settings.StoneGolemConnectedSkill.Value - 1].SkillIconPath.Contains("RockGolemSummon"))
                {
                    keyboard.KeyPressRelease(Settings.StoneGolemKeyPressed.Value);
                }
            }
            catch (Exception)
            {
            }

        }
    }

}
