namespace VSSonarExtensionUi.View.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using VSSonarPlugins;

    using SonarRestService.Types;
    using SonarRestService;
    using VSSonarExtensionUi.Model.Helpers;
    using System.Threading;

    /// <summary>
    /// Interaction logic for SearchComponenetDialog.xaml
    /// </summary>
    public partial class SearchComponenetDialog
    {
        /// <summary>
        /// The conf
        /// </summary>
        private readonly ISonarConfiguration conf;

        /// <summary>
        /// The rest
        /// </summary>
        private readonly ISonarRestService rest;

        /// <summary>
        /// The available projects
        /// </summary>
        private readonly ObservableCollection<ResourceInComponentDialog> availableProjects;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly IRestLogger logger;

        /// <summary>
        /// The selected items
        /// </summary>
        private readonly ObservableCollection<ResourceInComponentDialog> selectedItems;

        /// <summary>
        /// The cached resource data
        /// </summary>
        private readonly Dictionary<string, List<ResourceInComponentDialog>> cachedResourceData = new Dictionary<string, List<ResourceInComponentDialog>>();

        /// <summary>
        /// The component list
        /// </summary>
        private readonly ObservableCollection<ResourceInComponentDialog> componentList;

        /// <summary>
        /// teams data
        /// </summary>
        private readonly ObservableCollection<Team> teamsCollection;

        /// <summary>
        /// The vshelper
        /// </summary>
        private readonly IVsEnvironmentHelper vshelper;

        /// <summary>
        ///  cancelation token
        /// </summary>
        private CancellationTokenSource ct;

        /// <summary>
        /// sets dialog ok or false
        /// </summary>
        public bool DialogResultOk { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchComponenetDialog" /> class.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="rest">The rest.</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="listofSaveComp">The listof save comp.</param>
        /// <param name="vshelper">The vshelper.</param>
        public SearchComponenetDialog(ISonarConfiguration conf,
            ISonarRestService rest,
            List<Resource> availableProjectsIn,
            List<Resource> listofSaveComp,
            List<Team> teams,
            IVsEnvironmentHelper vshelper,
            IRestLogger logger)
        {
            this.logger = logger;
            this.selectedItems = new ObservableCollection<ResourceInComponentDialog>();
            this.availableProjects = new ObservableCollection<ResourceInComponentDialog>();
            foreach (var item in availableProjectsIn)
            {
                var addIn = new ResourceInComponentDialog
                {
                    BranchName = item.BranchName,
                    IsBranch = item.IsBranch,
                    Key = item.Key,
                    Name = item.Name,
                    Qualifier = item.Qualifier
                };

                foreach (var branchResource in item.BranchResources)
                {
                    addIn.BranchResources.Add(ToResourceComponentDialog(branchResource));
                }

                this.availableProjects.Add(addIn);
            }
            
            this.componentList = new ObservableCollection<ResourceInComponentDialog>();
            this.teamsCollection = new ObservableCollection<Team>(teams);

            this.conf = conf;
            this.rest = rest;
            this.vshelper = vshelper;

            InitializeComponent();

            this.Teams.ItemsSource = this.teamsCollection;
            this.Projects.ItemsSource = this.availableProjects;
            this.SelectedDataGrid.ItemsSource = this.selectedItems;
            this.SearchDataGrid.ItemsSource = this.componentList;

            if (listofSaveComp != null)
            {
                foreach (var item in listofSaveComp)
                {
                    try
                    {
                        IEnumerable<ResourceInComponentDialog> resourcesWithSameKey =
                                from resource in this.availableProjects
                                where this.CompareKeysInProjects(resource, item.Key)
                                select resource;

                        var element = resourcesWithSameKey.First();

                        if (element.BranchResources.Count != 0)
                        {
                            element = element.BranchResources.First(x => x.Key.Equals(item.Key)) as ResourceInComponentDialog;
                        }

                        element.Qualifier = "TRK";
                        this.selectedItems.Add(element);
                        element.Valid = true;
                    }
                    catch (Exception ex)
                    {
                        var elementToSelect = ToResourceComponentDialog(item);
                        elementToSelect.Valid = false;
                        this.selectedItems.Add(elementToSelect);
                        var message = $"Failed to import {item.Key}, not found likely in Sonar : {ex.Message}";
                        logger.ReportMessage(message);
                        this.StatusLabel.Content = message;
                    }
                }

                this.SearchDataGrid.Items.Refresh();
            }

            this.MouseLeftButtonDown += this.MouseLeftButtonDownPressed;
            this.SearchData.KeyDown += new KeyEventHandler(this.KeyboardKeyDown);
        }

        /// <summary>
        /// Compares the keys in projects.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private bool CompareKeysInProjects(ResourceInComponentDialog resource, string key)
        {
            if (resource.BranchResources.Count != 0)
            {
                try
                {
                    return resource.BranchResources.FirstOrDefault(x => x.Key.Equals(key)) != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return resource.Key.Equals(key);
            }
        }

        /// <summary>
        /// Searches the components.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="rest">The rest.</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="listofSaveComp">The listof save comp.</param>
        /// <param name="helper">The helper.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>
        /// returns saved component list
        /// </returns>
        public static List<Resource> SearchComponents(
            ISonarConfiguration conf,
            ISonarRestService rest,
            List<Resource> availableProjects,
            List<Resource> listofSaveComp,
            List<Team> teams,
            IVsEnvironmentHelper helper,
            IRestLogger logger)
        {
            var savedList = new List<Resource>();
            foreach (var item in listofSaveComp)
            {
                savedList.Add(item);
            }

            try
            {
                var searchComponenetDialog = new SearchComponenetDialog(conf, rest, availableProjects, listofSaveComp, teams, helper, logger);
                searchComponenetDialog.ShowDialog();

                if (searchComponenetDialog.DialogResultOk == true)
                {
                    var resources = searchComponenetDialog.SelectedDataGrid.Items.OfType<ResourceInComponentDialog>().ToList();
                    var resourcesDAta = new List<Resource>();
                    foreach (var item in resources)
                    {
                        resourcesDAta.Add(SearchComponenetDialog.ToResource(item));
                    }

                    return resourcesDAta;
                }
                else
                {
                    return savedList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ups Something wrong has happened. Please Report : " + ex.Message + " -> " + ex.StackTrace);
            }

            return new List<Resource>();
        }

        /// <summary>
        /// Handles the KeyDown event of the tb control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        private async void KeyboardKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SearchData.IsEnabled = false;
                this.ProgressBar.IsIndeterminate = true;
                var searchData = this.SearchData.Text;
                var selectedProject = this.Projects.SelectedItem;
                var list = new List<ResourceInComponentDialog>();

                if (selectedProject != null)
                {
                    await this.SearchInProject(list, selectedProject as ResourceInComponentDialog, searchData);
                }
                else
                {
                    foreach (var project in this.availableProjects)
                    {
                        await this.SearchInProject(list, project, searchData);
                        this.StatusLabel.Content = $"Searching : {project.Name} : Done";
                    }
                }

                this.componentList.Clear();
                foreach (var item in list)
                {
                    this.componentList.Add(item);
                }

                this.SearchData.IsEnabled = true;
                this.ProgressBar.IsIndeterminate = false;
                this.SearchDataGrid.Items.Refresh();
                this.StatusLabel.Content = "Search Completed.";
            }
        }

        /// <summary>
        /// Reports the status.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ProgressChangedEventArgs"/> instance containing the event data.</param>
        private void ReportStatus(object sender, ProgressChangedEventArgs e)
        {
            this.StatusLabel.Content = e.UserState;
        }

        /// <summary>
        /// The btn cancel_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResultOk = false;

            if (this.ct != null)
            {
                this.ct.Cancel();
            }

            try
            {
                this.Close();
            }
            catch (Exception)
            {
            }
        }

        private CancellationToken CreateNewTokenOrUseOldOne()
        {
            if (this.ct == null || this.ct.IsCancellationRequested)
            {
                this.ct = new CancellationTokenSource();
            }

            return this.ct.Token;
        }

        /// <summary>
        /// Determines whether [contains] [the specified source].
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="toCheck">To check.</param>
        /// <returns></returns>
        private bool Contains(string source, string toCheck)
        {
            return source.IndexOf(toCheck, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        /// <summary>
        /// Searches the in project.
        /// </summary>
        /// <param name="comps">The comps.</param>
        /// <param name="project">The project.</param>
        private async Task SearchInProject(List<ResourceInComponentDialog> comps, ResourceInComponentDialog project, string searchMessage)
        {
            var mainProj = GetMainProject(project);
            if (Contains(mainProj.Key, searchMessage))
            {
                comps.Add(mainProj);
            }

            if (this.cachedResourceData.ContainsKey(mainProj.Key))
            {
                var resources = this.cachedResourceData[mainProj.Key];

                IEnumerable<ResourceInComponentDialog> compsdirs =
                        from resource in resources
                        where Contains(resource.Name, searchMessage)
                        select resource;

                comps.AddRange(compsdirs);
            }
            else
            {
                try
                {
                    if (mainProj.Key.EndsWith("_Main"))
                    {
                        return;
                    }

                    var resourcesServer = await this.rest.IndexServerResources(this.conf, ToResource(mainProj), CreateNewTokenOrUseOldOne(), this.logger);
                    foreach (var item in resourcesServer)
                    {
                        if (!this.cachedResourceData.ContainsKey(mainProj.Key))
                        {
                            var listData = new List<ResourceInComponentDialog>();
                            listData.Add(ToResourceComponentDialog(item));
                            this.cachedResourceData.Add(mainProj.Key, listData);
                        }
                        else
                        {
                            this.cachedResourceData[mainProj.Key].Add(ToResourceComponentDialog(item));
                        }
                    }

                    IEnumerable<Resource> compsdirs =
                            from resource in resourcesServer
                            where Contains(resource.Name, searchMessage)
                            select resource;

                    foreach (var item in compsdirs)
                    {
                        comps.Add(ToResourceComponentDialog(item));
                    }
                }
                catch (Exception ex)
                {
                    var message = $"Failed to retrieve Project {project.Key} => {project.Name}, not found likely in Sonar : {ex.Message}";
                    logger.ReportMessage(message);
                    this.StatusLabel.Content = message;
                }
            }
        }

        /// <summary>
        /// Gets the main project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        private ResourceInComponentDialog GetMainProject(ResourceInComponentDialog project)
        {
            if (project.IsBranch)
            {
                foreach (var branch in project.BranchResources)
                {
                    if (branch.BranchName.Equals("master"))
                    {
                        return branch;
                    }
                }
            }
            else
            {
                return project;
            }

            return project;
        }
        /// <summary>
        /// Handles the MouseLeftButtonDown event of the YourWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void MouseLeftButtonDownPressed(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// Removes the selected to list button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void RemoveSelectedToListButton(object sender, RoutedEventArgs e)
        {
            foreach (ResourceInComponentDialog item in selectedItems.ToList())
            {
                foreach (var selected in this.SelectedDataGrid.SelectedItems)
                {
                    if (selected.Equals(item))
                    {
                        this.selectedItems.Remove(item);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Sends the selected to list button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SendSelectedToListButton(object sender, RoutedEventArgs e)
        {
            var selectedItems = this.SearchDataGrid.SelectedItems;

            foreach (ResourceInComponentDialog item in selectedItems)
            {
                bool found = false;
                foreach (ResourceInComponentDialog picked in this.SelectedDataGrid.Items)
                {
                    if (item.Key == picked.Key)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    item.Valid = true;
                    this.selectedItems.Add(item);
                }
            }

            this.SelectedDataGrid.Items.Refresh();
        }

        /// <summary>
        /// The btn ok_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResultOk = true;
            this.Close();
        }

        private async void GetCoverageReportDetailed(object sender, RoutedEventArgs e)
        {
            this.SearchData.IsEnabled = false;
            this.ProgressBar.IsIndeterminate = true;

            var completeData = new Dictionary<string, CoverageReport>();
            foreach (var item in this.selectedItems)
            {
                if (item.Qualifier == "TRK")
                {
                    var newRes = new ResourceInComponentDialog
                    {
                        BranchName = item.BranchName,
                        IsBranch = item.IsBranch,
                        Key = item.Key,
                        Name = item.Name,
                        Qualifier = item.Qualifier
                    };

                    foreach (var rep in await this.rest.GetCoverageReport(this.conf, ToResource(item), CreateNewTokenOrUseOldOne(), this.logger))
                    {
                        if (this.ct.IsCancellationRequested)
                        {
                            this.SearchData.IsEnabled = true;
                            this.ProgressBar.IsIndeterminate = false;
                            return;
                        }

                        completeData.Add(rep.Key, rep.Value);
                    }
                }
            }

            var reportHtml = HtmlHelpers.GenerateOverallCoverageHtmlReport(completeData);

            System.Windows.Forms.SaveFileDialog savefile = new System.Windows.Forms.SaveFileDialog();
            // set a default file name
            savefile.FileName = "coveragereport.html";
            // set filters - this can be done in properties as well
            savefile.Filter = "Create Html (*.html)|*.html|All files (*.*)|*.*";

            if (savefile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(savefile.FileName))
                    sw.WriteLine(reportHtml);
            }

            this.vshelper.NavigateToResource(savefile.FileName);
            this.SearchData.IsEnabled = true;
            this.ProgressBar.IsIndeterminate = false;
        }

        public static Resource ToResource(ResourceInComponentDialog item)
        {
            return new Resource
            {
                BranchName = item.BranchName,
                IsBranch = item.IsBranch,
                Key = item.Key,
                Name = item.Name,
                Qualifier = item.Qualifier
            };
        }

        private ResourceInComponentDialog ToResourceComponentDialog(Resource item)
        {
            return new ResourceInComponentDialog
            {
                BranchName = item.BranchName,
                IsBranch = item.IsBranch,
                Key = item.Key,
                Name = item.Name,
                Qualifier = item.Qualifier
            };
        }

        private async void GetDetailedHtmlReportButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.DatePick.SelectedDate.HasValue)
            {
                this.StatusLabel.Content = "Please pick date.";
                return;
            }

            this.SearchData.IsEnabled = false;
            this.ProgressBar.IsIndeterminate = true;

            var completeData = new Dictionary<string, CoverageDifferencial>();
            foreach (var item in this.selectedItems)
            {
                if (item.Qualifier == "TRK")
                {
                    foreach (var rep in await this.rest.GetCoverageReportOnNewCodeOnLeak(this.conf, ToResource(item), CreateNewTokenOrUseOldOne(), this.logger))
                    {
                        if (this.ct.IsCancellationRequested)
                        {
                            this.SearchData.IsEnabled = true;
                            this.ProgressBar.IsIndeterminate = false;
                            return;
                        }

                        completeData.Add(rep.Key, rep.Value);
                    }
                }
            }

            var teamsSelected = this.teamsCollection.Where(x => x.Selected).ToList();
            var teamsReportHtml = HtmlHelpers.GenerateTeamsCoverageReport(completeData, this.teamsCollection, teamsSelected, this.DatePick.SelectedDate.Value);
            System.Windows.Forms.SaveFileDialog savefileTeams = new System.Windows.Forms.SaveFileDialog();
            // set a default file name
            savefileTeams.FileName = "teamsreport.html";
            // set filters - this can be done in properties as well
            savefileTeams.Filter = "Create Html (*.html)|*.html|All files (*.*)|*.*";

            if (savefileTeams.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(savefileTeams.FileName))
                    sw.WriteLine(teamsReportHtml);
            }

            this.vshelper.NavigateToResource(savefileTeams.FileName);

            var reportHtml = HtmlHelpers.GenerateDetailedHtmlReport(completeData, this.teamsCollection, this.teamsCollection.Where(x => x.Selected).ToList(), this.DatePick.SelectedDate.Value);
            System.Windows.Forms.SaveFileDialog savefile = new System.Windows.Forms.SaveFileDialog();
            // set a default file name
            savefile.FileName = "report.html";
            // set filters - this can be done in properties as well
            savefile.Filter = "Create Html (*.html)|*.html|All files (*.*)|*.*";

            if (savefile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(savefile.FileName))
                    sw.WriteLine(reportHtml);
            }

            this.vshelper.NavigateToResource(savefile.FileName);


            var reportHtmlCompact = HtmlHelpers.GenerateCompactHtmlReport(completeData, this.teamsCollection, this.teamsCollection.Where(x => x.Selected).ToList(), this.DatePick.SelectedDate.Value);
            savefile = new System.Windows.Forms.SaveFileDialog();
            // set a default file name
            savefile.FileName = "report-compact.html";
            // set filters - this can be done in properties as well
            savefile.Filter = "Create Html (*.html)|*.html|All files (*.*)|*.*";

            if (savefile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(savefile.FileName))
                    sw.WriteLine(reportHtmlCompact);
            }

            this.vshelper.NavigateToResource(savefile.FileName);
            this.SearchData.IsEnabled = true;
            this.ProgressBar.IsIndeterminate = false;
        }

        private async void CreateVersionClick(object sender, RoutedEventArgs e)
        {
            if (!this.DatePick.SelectedDate.HasValue)
            {
                this.StatusLabel.Content = "Please pick date.";
                return;
            }

            if (string.IsNullOrEmpty(this.VersionName.Text))
            {
                this.StatusLabel.Content = "Please write a version.";
                return;
            }

            this.SearchData.IsEnabled = false;
            this.ProgressBar.IsIndeterminate = true;

            var date = this.DatePick.SelectedDate.Value;
            var version = this.VersionName.Text;

            this.CreateNewTokenOrUseOldOne();
            foreach (var item in this.selectedItems)
            {
                if (this.ct.IsCancellationRequested)
                {
                    this.SearchData.IsEnabled = true;
                    this.ProgressBar.IsIndeterminate = false;
                    return;
                }

                if (item.Qualifier == "TRK")
                {
                    try
                    {
                        this.StatusLabel.Content = await this.rest.CreateVersion(this.conf, ToResource(item), version, date, this.ct.Token, this.logger);
                    }
                    catch (Exception ex)
                    {
                        this.logger.ReportMessage($"Failed to create version: {item.Name} => {ex.Message}");
                        this.StatusLabel.Content = $"Failed to create version: {item.Name}  => {ex.Message}";
                    }
                }
            }

            this.SearchData.IsEnabled = true;
            this.ProgressBar.IsIndeterminate = false;
        }

        private void BtnCancelRequestClick(object sender, RoutedEventArgs e)
        {
            if (this.ct != null)
            {
                this.ct.Cancel();
            }		
        }
    }
}
