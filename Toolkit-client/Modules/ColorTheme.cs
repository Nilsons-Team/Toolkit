using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Toolkit_Client.Modules
{
    public class ColorTheme
    {
        public static ColorTheme LightColorTheme;
        public static ColorTheme DarkColorTheme;

        public static void InitializeThemes()
        {
            var Resources = Application.Current.Resources;
            LightColorTheme = new ColorTheme() 
            {
                ErrorColor            = (SolidColorBrush) Resources["ErrorColor"],
                WarningColor          = (SolidColorBrush) Resources["WarningColor"],
                SuccessColor          = (SolidColorBrush) Resources["SuccessColor"],

                WindowBackgroundColor = (SolidColorBrush) Resources["WindowBackgroundColor"],
                PrimaryColor          = (SolidColorBrush) Resources["PrimaryColor"],
                SecondaryColor        = (SolidColorBrush) Resources["SecondaryColor"],
                ButtonColor           = (SolidColorBrush) Resources["ButtonColor"],
                ButtonMouseOverColor  = (SolidColorBrush) Resources["ButtonMouseOverColor"],
                SelectionBrush        = (SolidColorBrush) Resources["SelectionBrush"],
                TextBoxColor          = (SolidColorBrush) Resources["TextBoxColor"]
            };

            DarkColorTheme = new ColorTheme() 
            {
                ErrorColor            = (SolidColorBrush) Resources["ErrorColor"],
                WarningColor          = (SolidColorBrush) Resources["WarningColor"],
                SuccessColor          = (SolidColorBrush) Resources["SuccessColor"],

                WindowBackgroundColor = GetSolidColorBrushFromString("#3b3b3b"),
                PrimaryColor          = (SolidColorBrush) Resources["PrimaryColor"],
                SecondaryColor        = (SolidColorBrush) Resources["SecondaryColor"],
                ButtonColor           = (SolidColorBrush) Resources["ButtonColor"],
                ButtonMouseOverColor  = (SolidColorBrush) Resources["ButtonMouseOverColor"],
                SelectionBrush        = (SolidColorBrush) Resources["SelectionBrush"],
                TextBoxColor          = (SolidColorBrush) Resources["TextBoxColor"]
            };
        }

        public static SolidColorBrush GetSolidColorBrushFromString(string color) {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        public SolidColorBrush ErrorColor;
        public SolidColorBrush WarningColor;
        public SolidColorBrush SuccessColor;

        public SolidColorBrush WindowBackgroundColor;
        public SolidColorBrush PrimaryColor;
        public SolidColorBrush SecondaryColor;
        public SolidColorBrush ButtonColor;
        public SolidColorBrush ButtonMouseOverColor;
        public SolidColorBrush SelectionBrush;
        public SolidColorBrush TextBoxColor;
    }
}
