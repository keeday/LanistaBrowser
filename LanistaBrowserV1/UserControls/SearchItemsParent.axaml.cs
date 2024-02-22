using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using LanistaBrowserV1.ViewModels;

namespace LanistaBrowserV1.UserControls
{
    public partial class SearchItemsParent : UserControl
    {
        public SearchItemsParent()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                MainViewModel? viewModel = DataContext as MainViewModel;
                if (viewModel == null)
                {
                    return;
                }

                var tabControl = (TabStrip)sender;
                int selectedIndex = tabControl.SelectedIndex;

                switch (selectedIndex)
                {
                    case 0:
                        viewModel.IsWeaponsVisible = true;
                        break;

                    case 1:
                        viewModel.IsArmorVisible = true;
                        break;

                    case 2:
                        viewModel.IsConsumablesVisible = true;
                        break;
                }
            }
        }
    }
}