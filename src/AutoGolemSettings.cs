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
            ChaosGolemMax = new RangeNode<int>(1, 1, 10);

            FireGolem = false;
            FireGolemKeyPressed = Keys.T;
            FireGolemMax = new RangeNode<int>(1, 1, 10);

            IceGolem = false;
            IceGolemKeyPressed = Keys.T;
            IceGolemMax = new RangeNode<int>(1, 1, 10);

            LightningGolem = false;
            LightningGolemKeyPressed = Keys.T;
            LightningGolemMax = new RangeNode<int>(1, 1, 10);

            StoneGolem = false;
            StoneGolemKeyPressed = Keys.T;
            StoneGolemMax = new RangeNode<int>(1, 1, 10);
        }

        //Menu
        [Menu("Chaos Golem", 1)]
        public ToggleNode ChaosGolem { get; set; }
        [Menu("Skill Hotkey", 10, 1)]
        public HotkeyNode ChaosGolemKeyPressed { get; set; }
        [Menu("Max Golems", 11, 1)]
        public RangeNode<int> ChaosGolemMax { get; set; }

        [Menu("Fire Golem", 2)]
        public ToggleNode FireGolem { get; set; }
        [Menu("Skill Hotkey", 20, 2)]
        public HotkeyNode FireGolemKeyPressed { get; set; }
        [Menu("Max Golems", 21, 2)]
        public RangeNode<int> FireGolemMax { get; set; }

        [Menu("Ice Golem", 3)]
        public ToggleNode IceGolem { get; set; }
        [Menu("Skill Hotkey", 30, 3)]
        public HotkeyNode IceGolemKeyPressed { get; set; }
        [Menu("Max Golems", 31, 3)]
        public RangeNode<int> IceGolemMax { get; set; }

        [Menu("Lightning Golem", 4)]
        public ToggleNode LightningGolem { get; set; }
        [Menu("Skill Hotkey", 40, 4)]
        public HotkeyNode LightningGolemKeyPressed { get; set; }
        [Menu("Max Golems", 41, 4)]
        public RangeNode<int> LightningGolemMax { get; set; }

        [Menu("Stone Golem", 5)]
        public ToggleNode StoneGolem { get; set; }
        [Menu("Skill Hotkey", 50, 5)]
        public HotkeyNode StoneGolemKeyPressed { get; set; }
        [Menu("Max Golems", 51, 5)]
        public RangeNode<int> StoneGolemMax { get; set; }
    }
}
