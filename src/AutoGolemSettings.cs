using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;
using System.Windows.Forms;

namespace AutoGolem
{
    internal class AutoGolemSettings : SettingsBase
    {
        public AutoGolemSettings()
        {
            //plugin itself
            Enable = false;

            //Golems
            ChaosGolem = false;
            ChaosGolemKeyPressed = Keys.T;
            ChaosGolemConnectedSkill = new RangeNode<int>(1, 1, 8);
            ChaosGolemMax = new RangeNode<int>(1, 1, 10);

            FireGolem = false;
            FireGolemKeyPressed = Keys.T;
            FireGolemConnectedSkill = new RangeNode<int>(1, 1, 8);
            FireGolemMax = new RangeNode<int>(1, 1, 10);

            IceGolem = false;
            IceGolemKeyPressed = Keys.T;
            IceGolemConnectedSkill = new RangeNode<int>(1, 1, 8);
            IceGolemMax = new RangeNode<int>(1, 1, 10);

            LightningGolem = false;
            LightningGolemKeyPressed = Keys.T;
            LightningGolemConnectedSkill = new RangeNode<int>(1, 1, 8);
            LightningGolemMax = new RangeNode<int>(1, 1, 10);

            StoneGolem = false;
            StoneGolemKeyPressed = Keys.T;
            StoneGolemConnectedSkill = new RangeNode<int>(1, 1, 8);
            StoneGolemMax = new RangeNode<int>(1, 1, 10);
        }

        //Menu
        [Menu("Chaos Golem", 1)]
        public ToggleNode ChaosGolem { get; set; }
        [Menu("Skill Hotkey", "Set the hotkey of the Summon Chaos Golem skill", 10, 1)]
        public HotkeyNode ChaosGolemKeyPressed { get; set; }
        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 11, 1)]
        public RangeNode<int> ChaosGolemConnectedSkill { get; set; }
        [Menu("Max Golems", "Maximum number of golems to summon for this type", 12, 1)]
        public RangeNode<int> ChaosGolemMax { get; set; }

        [Menu("Flame Golem", 2)]
        public ToggleNode FireGolem { get; set; }
        [Menu("Skill Hotkey", "Set the hotkey of the Summon Flame Golem skill", 20, 2)]
        public HotkeyNode FireGolemKeyPressed { get; set; }
        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 21, 2)]
        public RangeNode<int> FireGolemConnectedSkill { get; set; }
        [Menu("Max Golems", "Maximum number of golems to summon for this type", 22, 2)]
        public RangeNode<int> FireGolemMax { get; set; }

        [Menu("Ice Golem", 3)]
        public ToggleNode IceGolem { get; set; }
        [Menu("Skill Hotkey", "Set the hotkey of the Summon Ice Golem skill", 30, 3)]
        public HotkeyNode IceGolemKeyPressed { get; set; }
        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 31, 3)]
        public RangeNode<int> IceGolemConnectedSkill { get; set; }
        [Menu("Max Golems", "Maximum number of golems to summon for this type", 32, 3)]
        public RangeNode<int> IceGolemMax { get; set; }

        [Menu("Lightning Golem", 4)]
        public ToggleNode LightningGolem { get; set; }
        [Menu("Skill Hotkey", "Set the hotkey of the Summon Lightning Golem skill", 40, 4)]
        public HotkeyNode LightningGolemKeyPressed { get; set; }
        [Menu("Connected Skill", "Set the skill slot (1 = top left, 8 = bottom right)", 41, 4)]
        public RangeNode<int> LightningGolemConnectedSkill { get; set; }
        [Menu("Max Golems", "Maximum number of golems to summon for this type", 42, 4)]
        public RangeNode<int> LightningGolemMax { get; set; }

        [Menu("Stone Golem", 5)]
        public ToggleNode StoneGolem { get; set; }
        [Menu("Skill Hotkey", "Set the hotkey of the Summon Stone Golem skill", 50, 5)]
        public HotkeyNode StoneGolemKeyPressed { get; set; }
        [Menu("Connected Skill", 51, 5)]
        public RangeNode<int> StoneGolemConnectedSkill { get; set; }
        [Menu("Max Golems", "Maximum number of golems to summon for this type", 52, 5)]
        public RangeNode<int> StoneGolemMax { get; set; }
    }
}
