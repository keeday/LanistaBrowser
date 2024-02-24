using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanistaBrowserV1.Functions
{
    public class InfoDockPanel
    {
        public static DockPanel BuildInfoDockPanel(string title, string content)
        {
            DockPanel dockPanel = new();
            TextBlock titleText = new();
            TextBlock contentText = new();
            Grid containerGrid = new();

            containerGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            containerGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            dockPanel.Margin = new Avalonia.Thickness(3, 0, 0, 3);

            if (title.Contains("Requires Legend"))
            {
                titleText.Text = $"{title}";
            }
            else
            {
                titleText.Text = $"{title}: ";
            }
            titleText.FontWeight = Avalonia.Media.FontWeight.Bold;
            titleText.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

            contentText.Text = content;
            contentText.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;

            containerGrid.Children.Add(titleText);
            containerGrid.Children.Add(contentText);

            Grid.SetColumn(titleText, 0);
            Grid.SetColumn(contentText, 1);

            dockPanel.Children.Add(containerGrid);

            return dockPanel;
        }
    }
}