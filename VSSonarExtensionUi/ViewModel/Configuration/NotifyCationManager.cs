using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSSonarPlugins;

namespace VSSonarExtensionUi.ViewModel.Configuration
{
    public class NotifyCationManager : INotificationManager
    {
        private readonly SonarQubeViewModel model;
        public NotifyCationManager(SonarQubeViewModel model)
        {
            this.model = model;
        }

        public void ReportException(Exception ex)
        {
            this.model.ErrorMessage = ex.Message;
        }

        public void ReportMessage(Message messages)
        {
            this.model.ErrorMessage = messages.Data;
        }
    }
}
