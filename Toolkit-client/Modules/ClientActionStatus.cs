using System.Windows;
using System.Windows.Controls;

namespace Toolkit_Client.Modules
{
    public class ClientActionStatus
    {
        public static ColorTheme CurrentTheme;

        public static void UpdateColorTheme(ColorTheme theme) {
            CurrentTheme = theme;
        }

        public static void SetStatusError(Control? actionElement, TextBlock statusTextBlock, string? error)
        {
            if (string.IsNullOrEmpty(error)) {
                ClearStatus(actionElement, statusTextBlock);
                return;
            }

            statusTextBlock.Visibility = Visibility.Visible;
            statusTextBlock.Text       = error;
            statusTextBlock.Foreground = CurrentTheme.ErrorColor;

            if (actionElement != null)
                actionElement.BorderBrush  = CurrentTheme.ErrorColor;
        }

        public static void SetStatusWarning(Control? actionElement, TextBlock statusTextBlock, string? warning)
        {
            if (string.IsNullOrEmpty(warning)) {
                ClearStatus(actionElement, statusTextBlock);
                return;
            }

            statusTextBlock.Visibility = Visibility.Visible;
            statusTextBlock.Text       = warning;
            statusTextBlock.Foreground = CurrentTheme.WarningColor;

            if (actionElement != null)
                actionElement.BorderBrush  = CurrentTheme.WarningColor;
        }

        public static void SetStatusSuccess(Control? actionElement, TextBlock statusTextBlock, string? success)
        {
            if (string.IsNullOrEmpty(success)) {
                ClearStatus(actionElement, statusTextBlock);
                return;
            }

            statusTextBlock.Visibility = Visibility.Visible;
            statusTextBlock.Text       = success;
            statusTextBlock.Foreground = CurrentTheme.SuccessColor;

            if (actionElement != null)
                actionElement.BorderBrush  = CurrentTheme.TextBoxColor;
        }

        public static void ClearStatus(Control? actionElement, TextBlock statusTextBlock)
        {
            statusTextBlock.Visibility = Visibility.Collapsed;
            if (actionElement != null)
                actionElement.BorderBrush = CurrentTheme.TextBoxColor;
        }

        public static void ResetBorderBrushColor(Control element) {
            element.BorderBrush = CurrentTheme.TextBoxColor;
        }
    }
}
