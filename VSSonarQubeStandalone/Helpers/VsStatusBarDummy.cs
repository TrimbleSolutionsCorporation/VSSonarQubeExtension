using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSonarPlugins;

namespace VSSonarQubeStandalone.Helpers
{
    class VsStatusBarDummy : IVSSStatusBar
    {
        public void DisplayAndShowIcon(string message)
        {
            throw new NotImplementedException();
        }

        public void DisplayAndShowProgress(string message)
        {
            throw new NotImplementedException();
        }

        public void DisplayMessage(string message)
        {
            throw new NotImplementedException();
        }

        public void ShowIcons()
        {
            throw new NotImplementedException();
        }
    }
}
