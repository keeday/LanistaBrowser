using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LanistaBrowserV1.UserControls
{
    public partial class TheoryCraftingLevelDisplay : UserControl
    {
        private int tacticId;
        private int level;

        public TheoryCraftingLevelDisplay(int id, int lvl)
        {
            InitializeComponent();
            tacticId = id;
            level = lvl;
        }

        private void UserControl_Loaded(object Sender, RoutedEventArgs e)
        {
            TestBlock.Text = "Tactic ID: " + tacticId + " Level: " + level;
        }
    }
}