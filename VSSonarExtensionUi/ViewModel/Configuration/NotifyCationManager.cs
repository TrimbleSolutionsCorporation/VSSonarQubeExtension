// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotifyCationManager.cs" company="">
//   
// </copyright>
// <summary>
//   The notify cation manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtensionUi.ViewModel.Configuration
{
    using System;

    using VSSonarPlugins;

    /// <summary>The notify cation manager.</summary>
    public class NotifyCationManager : INotificationManager
    {
        /// <summary>The helper.</summary>
        private readonly IVsEnvironmentHelper helper;

        /// <summary>The model.</summary>
        private readonly SonarQubeViewModel model;

        /// <summary>Initializes a new instance of the <see cref="NotifyCationManager"/> class.</summary>
        /// <param name="model">The model.</param>
        /// <param name="helper">The helper.</param>
        public NotifyCationManager(SonarQubeViewModel model, IVsEnvironmentHelper helper)
        {
            this.model = model;
            this.helper = helper;
        }

        /// <summary>The report exception.</summary>
        /// <param name="ex">The ex.</param>
        public void ReportException(Exception ex)
        {
            this.helper.WriteToVisualStudioOutput(DateTime.Now + ":" + ex.Source + ":" + ex.Message + " : " + ex.StackTrace);
        }

        /// <summary>The report message.</summary>
        /// <param name="messages">The messages.</param>
        public void ReportMessage(Message messages)
        {
            this.helper.WriteToVisualStudioOutput(DateTime.Now + ":" + messages.Id + " : " + messages.Data);
        }
    }
}