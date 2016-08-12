namespace VSSonarExtensionUi.View.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using VSSonarPlugins;
    using VSSonarPlugins.Types;

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
        private readonly ObservableCollection<Resource> componentList = new ObservableCollection<Resource>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchComponenetDialog" /> class.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="rest">The rest.</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="listofSaveComp">The listof save comp.</param>
        public SearchComponenetDialog(ISonarConfiguration conf, ISonarRestService rest, List<Resource> availableProjects, List<Resource> listofSaveComp)
        {
            this.selectedItems = new ObservableCollection<Resource>();
            this.availableProjects = availableProjects;
            this.conf = conf;
            this.rest = rest;

            InitializeComponent();
            this.SearchDataGrid.ItemsSource = this.componentList;
            this.SearchDataGrid.Items.Refresh();
            this.Projects.ItemsSource = availableProjects;
            this.SelectedDataGrid.ItemsSource = this.selectedItems;
            if (listofSaveComp != null)
            {
                foreach (var item in listofSaveComp)
                {
                    this.selectedItems.Add(item);
                }
            }

            
            this.MouseLeftButtonDown += this.MouseLeftButtonDownPressed;
            this.SearchData.KeyDown += new KeyEventHandler(this.KeyboardKeyDown);
        }

        /// <summary>
        /// Searches the components.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="rest">The rest.</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="listofSaveComp">The listof save comp.</param>
        /// <returns>returns saved component list</returns>
        public static List<Resource> SearchComponents(ISonarConfiguration conf, ISonarRestService rest, List<Resource> availableProjects, List<Resource> listofSaveComp)
        {
            var savedList = new List<Resource>();
            foreach (var item in listofSaveComp)
            {
                savedList.Add(item);
            }


            try
            {
                var searchComponenetDialog = new SearchComponenetDialog(conf, rest, availableProjects, listofSaveComp);
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

                using (var bw = new BackgroundWorker{ WorkerReportsProgress = true })
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
                            this.SearchInProject(list, selectedProject as Resource, searchData);
                        }
                        else
                        {
                            var tasks = new Task[projects.Count];
                            var i = 0;
                            foreach (var project in projects)
                            {
                                tasks[i] = Task.Run(() =>
                                    {
                                        try
                                        {
                                            this.SearchInProject(list, project, searchData);
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

                            Application.Current.Dispatcher.Invoke(
                                delegate
                                {
                                    this.componentList.Clear();
                                    foreach (var item in list)
                                    {
                                        this.componentList.Add(item);
                                    }
                                });
                        }
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
            this.Close();
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
        private void SearchInProject(List<Resource> comps, Resource project, string searchMessage)
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
                        where Contains(resource.Key, searchMessage)
                        select resource;

                comps.AddRange(compsdirs);
            }
            else
            {
                var resources = this.rest.IndexServerResources(this.conf, mainProj);
                this.cachedResourceData.Add(mainProj.Key, resources);
                IEnumerable<Resource> compsdirs =
                        from resource in resources
                        where Contains(resource.Key, searchMessage)
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
            Application.Current.Dispatcher.Invoke(
                delegate
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
                });
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
    }
}
