using PoeHUD.Controllers;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.RemoteMemoryObjects;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AutoGolem
{
    internal class AutoGolem : BaseSettingsPlugin<AutoGolemSettings>
    {
        private IngameData data;
        private bool isTown;
        private KeyboardHelper keyboard;

        public override void Initialise()
        {
            PluginName = "Auto Golem";

            OnSettingsToggle();
            Settings.Enable.OnValueChanged += OnSettingsToggle;
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

                    var checkThread = new Thread(CheckThread) { IsBackground = true };
                    checkThread.Start();
                }
                else
                {
                    GameController.Area.OnAreaChange -= OnAreaChange;
                }
            }
            catch (Exception)
            {
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

                if (Settings.ChaosGolem.Value && countChaosGolem < Settings.ChaosGolemMax.Value)
                {
                    keyboard.KeyPressRelease(Settings.ChaosGolemKeyPressed.Value);
                }
                if (Settings.FireGolem.Value && countFireGolem < Settings.FireGolemMax.Value)
                {
                    keyboard.KeyPressRelease(Settings.FireGolemKeyPressed.Value);
                }
                if (Settings.IceGolem.Value && countIceGolem < Settings.IceGolemMax.Value)
                {
                    keyboard.KeyPressRelease(Settings.IceGolemKeyPressed.Value);
                }
                if (Settings.LightningGolem.Value && countLightningGolem < Settings.LightningGolemMax.Value)
                {
                    keyboard.KeyPressRelease(Settings.LightningGolemKeyPressed.Value);
                }
                if (Settings.StoneGolem.Value && countStoneGolem < Settings.StoneGolemMax.Value)
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
