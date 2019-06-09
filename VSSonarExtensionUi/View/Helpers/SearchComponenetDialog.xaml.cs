namespace VSSonarExtensionUi.View.Helpers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Input;
	using VSSonarPlugins;
	using VSSonarPlugins.Types;

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
		private readonly IEnumerable<Resource> availableProjects;

		/// <summary>
		/// The logger
		/// </summary>
		private readonly IRestLogger logger;

		/// <summary>
		/// The selected items
		/// </summary>
		private readonly ObservableCollection<Resource> selectedItems;

		/// <summary>
		/// The cached resource data
		/// </summary>
		private readonly Dictionary<string, List<Resource>> cachedResourceData = new Dictionary<string, List<Resource>>();

		/// <summary>
		/// The component list
		/// </summary>
		private readonly ObservableCollection<Resource> componentList;

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
			this.selectedItems = new ObservableCollection<Resource>();
			this.availableProjects = new ObservableCollection<Resource>(availableProjectsIn);
			this.componentList = new ObservableCollection<Resource>();
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
						IEnumerable<Resource> resourcesWithSameKey =
								from resource in this.availableProjects
								where this.CompareKeysInProjects(resource, item.Key)
								select resource;

						var element = resourcesWithSameKey.First();

						if (element.BranchResources.Count != 0)
						{
							element = element.BranchResources.First(x => x.Key.Equals(item.Key));
						}

						element.Qualifier = "TRK";

						this.selectedItems.Add(element);
					}
					catch (Exception ex)
					{
						Debug.WriteLine("Failed to import project: ", ex.Message);
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
		private bool CompareKeysInProjects(Resource resource, string key)
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

				if (searchComponenetDialog.DialogResult == true)
				{
					return searchComponenetDialog.SelectedDataGrid.Items.OfType<Resource>().ToList();
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
        private void KeyboardKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SearchData.IsEnabled = false;
                this.ProgressBar.IsIndeterminate = true;

                using (var bw = new BackgroundWorker { WorkerReportsProgress = true })
                {
                    var projects = new List<Resource>();
                    var searchData = this.SearchData.Text;
                    projects.AddRange(this.availableProjects);
                    var selectedProject = this.Projects.SelectedItem;

                    bw.RunWorkerCompleted += delegate
                    {
                        this.SearchData.IsEnabled = true;
                        this.ProgressBar.IsIndeterminate = false;

                        Application.Current.Dispatcher.Invoke(
                            delegate
                            {
                                this.SearchDataGrid.Items.Refresh();
                                this.StatusLabel.Content = "Search Completed.";
                            });
                    };

                    bw.DoWork += delegate
                    {
                        var list = new List<Resource>();
                        if (selectedProject != null)
                        {
                            this.SearchInProject(list, selectedProject as Resource, searchData).GetAwaiter().GetResult();
                        }
                        else
                        {
                            var tasks = new Task[projects.Count];
                            var i = 0;
                            foreach (var project in projects)
                            {
                                tasks[i] = Task.Run(async () =>
                                {
                                    try
                                    {
                                        await this.SearchInProject(list, project, searchData);
                                    }
                                    catch (Exception)
                                    {
                                        // ignore data
                                    }

                                    bw.ReportProgress(0, "Searching : " + project.Name + " : Done");
                                });
                                i++;
                            }

                            foreach (var task in tasks)
                            {
                                task.Wait();
                            }
                        }

                        Application.Current.Dispatcher.Invoke(
                            delegate
                            {
                                this.componentList.Clear();
                                foreach (var item in list)
                                {
                                    this.componentList.Add(item);
                                }
                            });
                    };

                    bw.ProgressChanged += this.ReportStatus;

                    bw.RunWorkerAsync();
                }
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
			this.DialogResult = false;

			if (this.ct == null)
			{
				return;
			}

			this.ct.Cancel();

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
		private async Task SearchInProject(List<Resource> comps, Resource project, string searchMessage)
		{
			var mainProj = GetMainProject(project);
			if (Contains(mainProj.Key, searchMessage))
			{
				comps.Add(mainProj);
			}

			if (this.cachedResourceData.ContainsKey(mainProj.Key))
			{
				var resources = this.cachedResourceData[mainProj.Key];

				IEnumerable<Resource> compsdirs =
						from resource in resources
						where Contains(resource.Name, searchMessage)
						select resource;

				comps.AddRange(compsdirs);
			}
			else
			{
				var resourcesServer = await this.rest.IndexServerResources(this.conf, mainProj, CreateNewTokenOrUseOldOne(), this.logger);
				this.cachedResourceData.Add(mainProj.Key, resourcesServer);
				IEnumerable<Resource> compsdirs =
						from resource in resourcesServer
						where Contains(resource.Name, searchMessage)
						select resource;

				comps.AddRange(compsdirs);
			}
		}

		/// <summary>
		/// Gets the main project.
		/// </summary>
		/// <param name="project">The project.</param>
		/// <returns></returns>
		private Resource GetMainProject(Resource project)
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
			foreach (Resource item in selectedItems.ToList())
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

			foreach (Resource item in selectedItems)
			{
				bool found = false;
				foreach (Resource picked in this.SelectedDataGrid.Items)
				{
					if (item.Key == picked.Key)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
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
			this.DialogResult = true;
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
					foreach (var rep in await this.rest.GetCoverageReport(this.conf, item, CreateNewTokenOrUseOldOne(), this.logger))
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

		private async void GetDetailedHtmlReportButtonClick(object sender, RoutedEventArgs e)
		{
			this.SearchData.IsEnabled = false;
			this.ProgressBar.IsIndeterminate = true;

			var completeData = new Dictionary<string, CoverageDifferencial>();
			foreach (var item in this.selectedItems)
			{
				if (item.Qualifier == "TRK")
				{
					foreach (var rep in await this.rest.GetCoverageReportOnNewCodeOnLeak(this.conf, item, CreateNewTokenOrUseOldOne(), this.logger))
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
			var teamsReportHtml = HtmlHelpers.GenerateTeamsCoverageReport(completeData, this.teamsCollection, teamsSelected);
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

			var reportHtml = HtmlHelpers.GenerateDetailedHtmlReport(completeData, this.teamsCollection, this.teamsCollection.Where(x => x.Selected).ToList());
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


			var reportHtmlCompact = HtmlHelpers.GenerateCompactHtmlReport(completeData, this.teamsCollection, this.teamsCollection.Where(x => x.Selected).ToList());
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
					this.StatusLabel.Content = await this.rest.CreateVersion(this.conf, item, version, date, this.ct.Token, this.logger);
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
