// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectAProjectCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The view options command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExtensionView.Commands
{
    using System;
    using System.Windows.Input;

    using ExtensionView;

    using ExtensionViewModel.ViewModel;

    using SonarRestService;

    /// <summary>
    /// The view options command.
    /// </summary>
    public class ConfigureIssuesWindowCommand : ICommand
    {
        /// <summary>
        /// The model.
        /// </summary>
        private readonly ExtensionDataModel model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureIssuesWindowCommand"/> class.
        /// </summary>
        public ConfigureIssuesWindowCommand()
        {
            var handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureIssuesWindowCommand"/> class. 
        /// Initializes a new instance of the <see cref="SelectAProjectCommand"/> class.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public ConfigureIssuesWindowCommand(ExtensionDataModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// The can execute changed.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// The can execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        public void Execute(object parameter)
        {
            var data = new IssuesGridSettingsDialog(this.model);
            data.ShowDialog();
        }
    }
}
