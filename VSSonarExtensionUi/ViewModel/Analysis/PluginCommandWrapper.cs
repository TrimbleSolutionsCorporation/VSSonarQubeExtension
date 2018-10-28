

namespace VSSonarExtensionUi.ViewModel.Analysis
{
    using GalaSoft.MvvmLight.Command;
    using System.Windows.Input;
    using VSSonarPlugins.Types;
    using System;
    using VSSonarPlugins;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;

    public class PluginCommandWrapper
    {
        private readonly LocalViewModel model;
        private readonly INotificationManager notification;
        private bool isRunning;

        public PluginCommandWrapper(LocalViewModel model, INotificationManager manager)
        {
            this.notification = manager;
            this.model = model;
            this.ExecuteCommand = new RelayCommand<VsFileItem>(this.RunCommand);
        }

        private void RunCommand(VsFileItem itemInView)
        {
            if(this.isRunning)
            {
                return;
            }

            this.isRunning = true;
            this.model.CanRunAnalysis = false;
            this.model.AnalysisIsRunning = true;
            try
            {
                var bw = new BackgroundWorker { WorkerReportsProgress = true };

                bw.RunWorkerCompleted += delegate
                {
                    this.isRunning = false;
                    this.model.CanRunAnalysis = true;
                    this.model.AnalysisIsRunning = false;
                };

                bw.DoWork +=
                    delegate {
                        var commandIssues = PluginOperation.ExecuteCommand(itemInView);
                        Application.Current.Dispatcher.Invoke(
                            delegate
                            {
                                this.model.UpdateLocalIssues(this, new LocalAnalysisEventCommandAnalsysisComplete(itemInView, commandIssues));
                            });
                    };

                bw.RunWorkerAsync();
            }
            catch(Exception ex)
            {
                this.notification.ReportMessage(new Message() { Id = "PluginWrapper", Data = "Failed to Run Command : " + this.Name });
                this.notification.ReportException(ex);
            }
        }

        public IPluginCommand PluginOperation { get; set; }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public ICommand ExecuteCommand { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Name { get; set; }


    }
}