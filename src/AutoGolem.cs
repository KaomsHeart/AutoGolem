using PoeHUD.Controllers;
using PoeHUD.Models;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.RemoteMemoryObjects;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace AutoGolem
{
    internal class AutoGolem : BaseSettingsPlugin<AutoGolemSettings>
    {
        private bool isTown;
        private KeyboardHelper keyboard;
        private Stopwatch stopwatch = new Stopwatch();
        private int highlightSkill = 0;
        private HashSet<EntityWrapper> nearbyMonsters = new HashSet<EntityWrapper>();

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
            Settings.NearbyMonsterRange.OnValueChanged += OnNearbyMonsterRangeChanged;
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

        private void OnNearbyMonsterRangeChanged()
        {
            highlightSkill = -1;
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

                    isTown = GameController.Area.CurrentArea.IsTown;

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
                if (highlightSkill == -1)
                {
                    var pos = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Render>().Pos;
                    DrawEllipseToWorld(pos, Settings.NearbyMonsterRange.Value, 50, 2, Color.Yellow);
                }
                else
                {
                    IngameUIElements ingameUiElements = GameController.Game.IngameState.IngameUi;
                    Graphics.DrawFrame(ingameUiElements.SkillBar[highlightSkill].GetClientRect(), 3f, Color.Yellow);
                }
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

                if (Settings.DontCastOnNearbyMonster.Value)
                {
                    Vector3 positionPlayer = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Render>().Pos;

                    foreach (EntityWrapper monster in nearbyMonsters)
                    {
                        if (monster.IsValid && monster.IsAlive)
                        {
                            Render positionMonster = monster.GetComponent<Render>();
                            int distance = (int)Math.Sqrt(Math.Pow((double)(positionPlayer.X - positionMonster.X), 2.0) + Math.Pow((double)(positionPlayer.Y - positionMonster.Y), 2.0));
                            if (distance <= Settings.NearbyMonsterRange.Value)
                            {
                                 return; //don't cast if monsters are nearby
                            }
                        }
                    }
                }

                IngameUIElements ingameUiElements = GameController.Game.IngameState.IngameUi;
                List<DeployedObject> deployedObjects = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Actor>().DeployedObjects;

                int countChaosGolem = 0;
                int countFireGolem = 0;
                int countIceGolem = 0;
                int countLightningGolem = 0;
                int countStoneGolem = 0;

                ActorSkill skillLightningGolem = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Actor>().ActorSkills.Find(x => x.Name == "SummonLightningGolem");

                foreach (DeployedObject deployedObject in deployedObjects)
                {

                    if (GameController.Game.IngameState.Data.EntityList.EntitiesAsDictionary.ContainsKey(deployedObject.ObjectKey))
                    {
                        var golemPathString = GameController.Game.IngameState.Data.EntityList.EntitiesAsDictionary[deployedObject.ObjectKey].Path;

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
            catch (Exception ex)
            {
                LogError(ex.Message, 3);
            }

        }

        public override void EntityAdded(EntityWrapper entity)
        {
            if (!Settings.Enable.Value)
                return;

            if (entity.IsAlive && entity.IsHostile && entity.HasComponent<Monster>())
            {
                entity.GetComponent<Positioned>();
                nearbyMonsters.Add(entity);
            }
        }

        public override void EntityRemoved(EntityWrapper entity)
        {
            if (!Settings.Enable.Value)
                return;

            this.nearbyMonsters.Remove(entity);
        }

        public void DrawEllipseToWorld(Vector3 vector3Pos, int radius, int points, int lineWidth, Color color)
        {
            var camera = GameController.Game.IngameState.Camera;

            var plottedCirclePoints = new List<Vector3>();
            var slice = 2 * Math.PI / points;
            for (var i = 0; i < points; i++)
            {
                var angle = slice * i;
                var x = (decimal)vector3Pos.X + decimal.Multiply((decimal)radius, (decimal)Math.Cos(angle));
                var y = (decimal)vector3Pos.Y + decimal.Multiply((decimal)radius, (decimal)Math.Sin(angle));
                plottedCirclePoints.Add(new Vector3((float)x, (float)y, vector3Pos.Z));
            }

            var rndEntity = GameController.Entities.FirstOrDefault(x =>
                x.HasComponent<Render>() && GameController.Player.Address != x.Address);

            for (var i = 0; i < plottedCirclePoints.Count; i++)
            {
                if (i >= plottedCirclePoints.Count - 1)
                {
                    var pointEnd1 = camera.WorldToScreen(plottedCirclePoints.Last(), rndEntity);
                    var pointEnd2 = camera.WorldToScreen(plottedCirclePoints[0], rndEntity);
                    Graphics.DrawLine(pointEnd1, pointEnd2, lineWidth, color);
                    return;
                }

                var point1 = camera.WorldToScreen(plottedCirclePoints[i], rndEntity);
                var point2 = camera.WorldToScreen(plottedCirclePoints[i + 1], rndEntity);
                Graphics.DrawLine(point1, point2, lineWidth, color);
            }
        }
    }
}
