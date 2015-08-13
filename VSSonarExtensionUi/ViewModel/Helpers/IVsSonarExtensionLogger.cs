using System;

namespace VSSonarExtensionUi.ViewModel.Helpers
{
    public interface IVsSonarExtensionLogger
    {
        void WriteException(Exception ex);
        void WriteMessage(string msg);
    }
}