namespace VSSonarExtensionUi.View.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
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
        /// Initializes a new instance of the <see cref="SearchComponenetDialog" /> class.
        /// </summary>
        /// <param name="conf">The conf.</param>
        /// <param name="rest">The rest.</param>
        /// <param name="availableProjects">The available projects.</param>
        /// <param name="listofSaveComp">The listof save comp.</param>
        public SearchComponenetDialog(ISonarConfiguration conf, ISonarRestService rest, List<Resource> availableProjects, List<Resource> listofSaveComp)
        {
            this.availableProjects = availableProjects;
            this.conf = conf;
            this.rest = rest;
           
            InitializeComponent();
            this.Projects.ItemsSource = availableProjects;
            if (listofSaveComp != null)
            {
                this.SearchDataGrid.ItemsSource = listofSaveComp;
                this.SearchDataGrid.Items.Refresh();
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
            var searchComponenetDialog = new SearchComponenetDialog(conf, rest, availableProjects, listofSaveComp);
            searchComponenetDialog.ShowDialog();

            if (searchComponenetDialog.DialogResult == true)
            {
                return searchComponenetDialog.SelectedDataGrid.Items.OfType<Resource>().ToList();
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
                var comps = this.rest.SearchComponent(this.conf, this.SearchData.Text, true, "master");

                if (this.availableProjects != null)
                {
                    if (this.Projects.SelectedItem != null)
                    {
                        this.SearchInProject(comps, this.Projects.SelectedItem as Resource);
                    }
                    else
                    {
                        foreach (var project in this.availableProjects)
                        {
                            this.SearchInProject(comps, project);
                        }
                    }
                }

                this.SearchDataGrid.ItemsSource = comps;
                this.SearchDataGrid.Items.Refresh();
                this.SearchData.IsEnabled = true;
            }
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
        /// Searches the in project.
        /// </summary>
        /// <param name="comps">The comps.</param>
        /// <param name="project">The project.</param>
        private void SearchInProject(List<Resource> comps, Resource project)
        {
            if (project.IsBranch)
            {
                foreach (var branch in project.BranchResources)
                {
                    if (branch.BranchName.Equals("master"))
                    {
                        var compsdirs = this.rest.SearchComponent(this.conf, branch.Key + ":" + this.SearchData.Text, true, "master");
                        comps.AddRange(compsdirs);
                    }
                }
            }
            else
            {
                var compsdirs = this.rest.SearchComponent(this.conf, project.Key + ":" + this.SearchData.Text, true, "master");
                comps.AddRange(compsdirs);
            }
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
            while (SelectedDataGrid.SelectedItems.Count > 0)
            {
                if (SelectedDataGrid.SelectedItem == CollectionView.NewItemPlaceholder)
                {
                    SelectedDataGrid.SelectedItems.Remove(SelectedDataGrid.SelectedItem);
                }
                else
                {
                    SelectedDataGrid.Items.Remove(SelectedDataGrid.SelectedItem);
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
                    this.SelectedDataGrid.Items.Add(item);
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
    }
}
