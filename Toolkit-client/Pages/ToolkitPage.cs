using System.Windows.Controls;

using Toolkit_Shared;

namespace Toolkit_Client.Pages
{
    public class ToolkitPage : Page
    {
        protected Logger logger;

        public ToolkitPage(Logger logger) : base()
        {
            this.logger = logger;
        }
    }
}
