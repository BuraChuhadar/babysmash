using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Baby_Smash
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    // SettingsDialog.xaml.cs
    public partial class SettingsDialog : Window
    {
        public bool DarkMode { get; set; }
        public int FadeSpeed { get; set; }

        public SettingsDialog(bool currentMode, int currentFadeSpeed)
        {
            InitializeComponent();
            DarkModeCheckBox.IsChecked = currentMode;
            FadeSpeedSlider.Value = currentFadeSpeed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the properties with the values from the controls
            DarkMode = DarkModeCheckBox.IsChecked == true;
            FadeSpeed = (int)FadeSpeedSlider.Value;

            // Save settings
            Properties.Settings.Default.IsDarkModeEnabled = DarkMode;
            Properties.Settings.Default.FadeSpeed = FadeSpeed;
            Properties.Settings.Default.Save();

            this.DialogResult = true;
            this.Close();
        }
    }
}
