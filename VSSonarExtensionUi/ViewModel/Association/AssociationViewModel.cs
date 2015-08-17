namespace VSSonarExtensionUi.ViewModel.Association
{
    using VSSonarPlugins.Types;
    using System.Collections.ObjectModel;
    using GalaSoft.MvvmLight.Command;
    using VSSonarExtensionUi.Model.Association;
    using VSSonarExtensionUi.ViewModel.Helpers;
    using System.Windows.Media;
    using PropertyChanged;
    using System;
    using System.Windows;

    [ImplementPropertyChanged]
    public class AssociationViewModel : IViewModelBase
    {
        private readonly AssociationModel model;

        public AssociationViewModel()
        {
            this.AssignProjectCommand = new RelayCommand(this.OnAssignProjectCommand);
            this.AvailableProjects = new ObservableCollection<Resource>();
            this.Header = "Association Dialog";
            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
        }

        public AssociationViewModel(AssociationModel model)
        {
            this.model = model;
            this.AssignProjectCommand = new RelayCommand(this.OnAssignProjectCommand);
            this.AvailableProjects = new ObservableCollection<Resource>();
            this.Header = "Association Dialog";
        }

        /// <summary>
        ///     Gets or sets the available projects.
        /// </summary>
        public ObservableCollection<Resource> AvailableProjects { get; set; }

        /// <summary>
        ///     Gets or sets the assign project command.
        /// </summary>
        public RelayCommand AssignProjectCommand { get; set; }

        /// <summary>
        ///     Gets or sets the selected project.
        /// </summary>
        public Resource SelectedProject { get; set; }

        public Resource SelectedBranchProject { get; set; }       

        public Color BackGroundColor { get; set; }

        public Color ForeGroundColor { get; set; }

        public string Header { get; set; }

        public string StatusMessage { get; set; }

        public event Action<object, object> RequestClose;

        protected void OnRequestClose(object arg1, object arg2)
        {
            var handler = this.RequestClose;
            if (handler != null)
            {
                handler(arg1, arg2);
            }
        }

        public void OnSelectedProjectChanged()
        {
            if (this.SelectedProject.IsBranch)
            {
                var branch = this.model.CurrentBranch();
                if (string.IsNullOrEmpty(branch))
                {
                    this.StatusMessage = "This is multi branch project, however there is not plugin configured that supports the currect source control";
                }
                else
                {
                    this.StatusMessage = "This is multi branch project, checkout branch is " + branch;
                }
            }
            else
            {
                this.StatusMessage = "";
            }
        }

        /// <summary>
        ///     The on assign project command.
        /// </summary>
        private void OnAssignProjectCommand()
        {
            if (this.SelectedProject.IsBranch && this.SelectedBranchProject == null)
            {
                MessageBox.Show("Please select a feature branch as the in use branch");
                return;
            }

            if (this.model.AssignASonarProjectToSolution(this.SelectedProject, this.SelectedBranchProject))
            {
                this.StatusMessage = "Associated : " + this.model.AssociatedProject.Name;
                this.OnRequestClose(this, "Exit");
                return;
            }

            this.StatusMessage = "Error : Not Associated";
        }

        public void UpdateColours(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        public object GetAvailableModel()
        {
            throw new NotImplementedException();
        }
    }
}
