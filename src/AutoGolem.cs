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

                    nearbyMonsters.Clear();
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

            var playerLife = GameController.Game?.IngameState?.Data?.LocalPlayer?.GetComponent<Life>();
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

                int countChaosGolem = 0;
                int countFireGolem = 0;
                int countIceGolem = 0;
                int countLightningGolem = 0;
                int countStoneGolem = 0;

                if (Settings.UseAlternativeDetectionMethod.Value)
                {
                    List<Buff> buffs = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Life>().Buffs;

                    Buff chaosGolem = buffs.FirstOrDefault(x => x.Name.Equals("chaos_elemental_buff"));
                    if (chaosGolem != null)
                    {
                        countChaosGolem = chaosGolem.Charges;
                    }

                    Buff fireGolem = buffs.FirstOrDefault(x => x.Name.Equals("fire_elemental_buff"));
                    if (fireGolem != null)
                    {
                        countFireGolem = fireGolem.Charges;
                    }

                    Buff iceGolem = buffs.FirstOrDefault(x => x.Name.Equals("ice_elemental_buff"));
                    if (iceGolem != null)
                    {
                        countIceGolem = iceGolem.Charges;
                    }

                    Buff lightningGolem = buffs.FirstOrDefault(x => x.Name.Equals("lightning_elemental_buff"));
                    if (lightningGolem != null)
                    {
                        countLightningGolem = lightningGolem.Charges;
                    }

                    Buff stoneGolem = buffs.FirstOrDefault(x => x.Name.Equals("rock_golem_buff"));
                    if (stoneGolem != null)
                    {
                        countStoneGolem = stoneGolem.Charges;
                    }
                   
                }
                else
                {
                    List<DeployedObject> deployedObjects = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Actor>().DeployedObjects;

                    countChaosGolem = deployedObjects.Count(x => x.Entity.Path.Contains("ChaosElemental"));
                    countFireGolem = deployedObjects.Count(x => x.Entity.Path.Contains("FireElemental"));
                    countIceGolem = deployedObjects.Count(x => x.Entity.Path.Contains("IceElemental"));
                    countLightningGolem = deployedObjects.Count(x => x.Entity.Path.Contains("LightningGolem"));
                    countStoneGolem = deployedObjects.Count(x => x.Entity.Path.Contains("RockGolem"));
                }

                List<ActorSkill> actorSkills = GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Actor>().ActorSkills;

                if (Settings.ChaosGolem.Value && countChaosGolem < Settings.ChaosGolemMax.Value)
                {
                    ActorSkill skillChaosGolem = actorSkills.FirstOrDefault(x => x.Name == "SummonChaosGolem" && x.CanBeUsed && x.SkillSlotIndex.Equals(Settings.ChaosGolemConnectedSkill.Value - 1));

                    if (skillChaosGolem != null)
                    {
                        keyboard.KeyPressRelease(Settings.ChaosGolemKeyPressed.Value);
                    }
                }
                if (Settings.FireGolem.Value && countFireGolem < Settings.FireGolemMax.Value)
                {
                    ActorSkill skillFireGolem = actorSkills.FirstOrDefault(x => x.Name == "SummonFireGolem" && x.CanBeUsed && x.SkillSlotIndex.Equals(Settings.FireGolemConnectedSkill.Value - 1));

                    if (skillFireGolem != null)
                    {
                        keyboard.KeyPressRelease(Settings.FireGolemKeyPressed.Value);
                    }
                }
                if (Settings.IceGolem.Value && countIceGolem < Settings.IceGolemMax.Value)
                {
                    ActorSkill skillIceGolem = actorSkills.FirstOrDefault(x => x.Name == "SummonIceGolem" && x.CanBeUsed && x.SkillSlotIndex.Equals(Settings.IceGolemConnectedSkill.Value - 1));

                    if (skillIceGolem != null)
                    {
                        keyboard.KeyPressRelease(Settings.IceGolemKeyPressed.Value);
                    }
                }
                if (Settings.LightningGolem.Value && countLightningGolem < Settings.LightningGolemMax.Value)
                {
                    ActorSkill skillLightningGolem = actorSkills.FirstOrDefault(x => x.Name == "SummonLightningGolem" && x.CanBeUsed && x.SkillSlotIndex.Equals(Settings.LightningGolemConnectedSkill.Value - 1));

                    if (skillLightningGolem != null)
                    {
                        keyboard.KeyPressRelease(Settings.LightningGolemKeyPressed.Value);
                    }
                }
                if (Settings.StoneGolem.Value && countStoneGolem < Settings.StoneGolemMax.Value)
                {
                    ActorSkill skillStoneGolem = actorSkills.FirstOrDefault(x => x.Name == "SummonRockGolem" && x.CanBeUsed && x.SkillSlotIndex.Equals(Settings.StoneGolemConnectedSkill.Value - 1));

                    if (skillStoneGolem != null)
                    {
                        keyboard.KeyPressRelease(Settings.StoneGolemKeyPressed.Value);
                    }
                }

            }
            catch (Exception ex)
            {
                //LogError(ex.Message, 3);
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
