using System;
using System.Threading;
using System.Windows;
using Toolkit_Shared;

namespace Toolkit_Client.Windows
{
    public class ToolkitWindow : Window
    {
        protected Logger logger;

        public ToolkitWindow() : base()
        {
            Closed += Window_Closed;
            Loaded += Window_Loaded;
        }

        public ToolkitWindow(Logger logger) : this()
        {
            this.logger = logger;
        }

        public Logger GetLogger() 
        {
            return logger;
        }

        protected void Window_Closed(object sender, EventArgs e)
        {
            logger.Info($"Closed window '{Name}' (\"{Title}\").");
        }

        protected void Window_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Info($"Loaded window '{Name}' (\"{Title}\").");
        }
    }
}
