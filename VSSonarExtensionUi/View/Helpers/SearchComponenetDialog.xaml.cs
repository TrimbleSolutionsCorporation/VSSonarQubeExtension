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
        private readonly INotificationManager logger;

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
        /// The vshelper
        /// </summary>
        private readonly IVsEnvironmentHelper vshelper;

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
            IVsEnvironmentHelper vshelper,
            INotificationManager logger)
        {
            this.logger = logger;
            this.selectedItems = new ObservableCollection<Resource>();
            this.availableProjects = new ObservableCollection<Resource>(availableProjectsIn);
            this.componentList = new ObservableCollection<Resource>();

            this.conf = conf;
            this.rest = rest;
            this.vshelper = vshelper;

            InitializeComponent();

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

                        if(element.BranchResources.Count != 0)
                        {
                            element = element.BranchResources.First(x => x.Key.Equals(item.Key));
                        }

                        element.Qualifier = "TRK";

                        this.selectedItems.Add(element);
                    }
                    catch(Exception ex)
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
            if(resource.BranchResources.Count != 0)
            {
                try
                {
                    return resource.BranchResources.First(x => x.Key.Equals(key)) != null;
                }
                catch(Exception)
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
            IVsEnvironmentHelper helper,
            INotificationManager logger)
        {
            var savedList = new List<Resource>();
            foreach (var item in listofSaveComp)
            {
                savedList.Add(item);
            }


            try
            {
                var searchComponenetDialog = new SearchComponenetDialog(conf, rest, availableProjects, listofSaveComp, helper, logger);
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
                        }

                        Application.Current.Dispatcher.Invoke(
                            delegate
                            {
                                this.componentList.Clear();
                                foreach(var item in list)
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
            try
            {
                this.Close();
            }
            catch(Exception)
            {
            }
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
                        where Contains(resource.Name, searchMessage)
                        select resource;

                comps.AddRange(compsdirs);
            }
            else
            {
                var resourcesServer = this.rest.IndexServerResources(this.conf, mainProj);
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

        private void GetCoverageReportDetailed(object sender, RoutedEventArgs e)
        {
            var completeData = new Dictionary<string, CoverageReport>();
            foreach (var item in this.selectedItems)
            {
                if (item.Qualifier == "TRK")
                {
                    foreach (var rep in this.rest.GetCoverageReport(this.conf, item))
                    {
                        completeData.Add(rep.Key, rep.Value);
                    }
                }
            }

            var reportHtml = this.GenerateCoverageHtmlReport(completeData);

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
        }

        private void GetCoverageOnLeakClick(object sender, RoutedEventArgs e)
        {
            var completeData = new Dictionary<string, CoverageDifferencial>();
            foreach (var item in this.selectedItems)
            {
                if (item.Qualifier == "TRK")
                {
                    foreach (var rep in this.rest.GetCoverageReportOnNewCodeOnLeak(this.conf, item, this.logger))
                    {
                        completeData.Add(rep.Key, rep.Value);
                    }
                }
            }

            var reportHtml = this.GenerateHtmlReport(completeData);
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


            var reportHtmlCompact = this.GenerateHtmlReportCompact(completeData);
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
        }

        private void CreateVersionClick(object sender, RoutedEventArgs e)
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

            var date = this.DatePick.SelectedDate.Value;
            var version = this.VersionName.Text;

            foreach (var item in this.selectedItems)
            {
                if (item.Qualifier == "TRK")
                {
                    this.StatusLabel.Content = this.rest.CreateVersion(this.conf, item, version, date);
                }
            }
        }

        private string GenerateCoverageHtmlReport(Dictionary<string, CoverageReport> report)
        {
            StringBuilder reportdata = new StringBuilder();
            reportdata.Append(this.GenerateCoverageReportHeaderString());

            foreach (var item in report)
            {
                var path = item.Key;
                var rep = item.Value;

                reportdata.Append("<tr>");
                reportdata.Append("<td>" + rep.resource.Key + "</td>");
                if (rep.NewCoverage < 60 && rep.NewLines > 5)
                {
                    reportdata.Append("<td>Failed</td>");
                }
                else
                {
                    reportdata.Append("<td>Passed</td>");
                }
                    
                reportdata.Append("<td>" + rep.LinesOfCode + "</td>");
                reportdata.Append("<td>" + rep.NewLines + "</td>");
                reportdata.Append("<td>" + Convert.ToInt32(rep.Coverage) + "</td>");
                reportdata.Append("<td>" + Convert.ToInt32(rep.NewCoverage) + "</td>");
                reportdata.Append("</tr>");

            }

            reportdata.Append(this.GenerateFooter());

            return reportdata.ToString();
        }

        private string GenerateHtmlReport(Dictionary<string, CoverageDifferencial> report)
        {
            StringBuilder reportdata = new StringBuilder();
            reportdata.Append(this.GenerateHeaderString());

            foreach (var item in report)
            {
                var path = item.Key;
                var rep = item.Value;
                foreach (var line in rep.resource.Lines)
                {
                    reportdata.Append("<tr>");
                    reportdata.Append("<td>" + path + "</td>");
                    reportdata.Append("<td>" + rep.UncoveredLines + "</td>");
                    reportdata.Append("<td>" + rep.UncoveredConditons + "</td>");
                    reportdata.Append("<td>" + line.Key + "</td>");
                    reportdata.Append("<td>" + line.Value.LineHits + "</td>");
                    reportdata.Append("<td>" + line.Value.Conditions + "</td>");
                    reportdata.Append("<td>" + line.Value.CoveredConditions + "</td>");
                    reportdata.Append("<td>" + line.Value.ScmAuthor + "</td>");
                    reportdata.Append("<td>" + line.Value.ScmDate + "</td>");
                    reportdata.Append("<td>" + line.Value.ScmRevision + "</td>");
                    reportdata.Append("<td>" + line.Value.Code + "</td>");
                    reportdata.Append("</tr>");
                }
            }

            reportdata.Append(this.GenerateFooter());

            return reportdata.ToString();
        }

        private string GenerateHtmlReportCompact(Dictionary<string, CoverageDifferencial> report)
        {
            StringBuilder reportdata = new StringBuilder();
            reportdata.Append(this.GenerateHeaderCompactString());

            foreach (var item in report)
            {
                var path = item.Key;
                var rep = item.Value;
                reportdata.Append("<tr>");
                reportdata.Append("<td>" + path + "</td>");
                reportdata.Append("<td>" + rep.UncoveredLines + "</td>");
                reportdata.Append("<td>" + rep.UncoveredConditons + "</td>");
                reportdata.Append("<td>");
                var authors = "";
                foreach (var line in rep.resource.Lines)
                {
                    if (authors.Contains(line.Value.ScmAuthor))
                    {
                        continue;
                    }

                    authors += line.Value.ScmAuthor + " ";
                }
                reportdata.Append(authors);
                reportdata.Append("</td>");
                reportdata.Append("</tr>");
            }

            reportdata.Append(this.GenerateFooter());

            return reportdata.ToString();
        }

        private string GenerateCoverageReportHeaderString()
        {
            var header = @"
<!DOCTYPE html>
<html>
<head>
<script type='text/javascript' src='https://code.jquery.com/jquery-3.2.1.min.js'></script>
<script type='text/javascript' src='https://cdnjs.cloudflare.com/ajax/libs/jquery.tablesorter/2.28.7/js/jquery.tablesorter.js'></script> 

<script>
$(document).ready(function() 
    { 
        $('#myTable').tablesorter(); 
    } 
);

function myFunction() {
  // Declare variables 
  var input, filter, table, tr, td, i;
  input = document.getElementById('myInput');
  filter = input.value.toUpperCase();
  table = document.getElementById('myTable');
  tr = table.getElementsByTagName('tr');

  // Loop through all table rows, and hide those who don't match the search query
  for (i = 0; i < tr.length; i++) {
    td = tr[i].getElementsByTagName('td')[0];
    if (td) {
      if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
        tr[i].style.display = '';
      } else {
        tr[i].style.display = 'none';
      }
    } 
  }
}
function myFunction2() {
  // Declare variables 
  var input, filter, table, tr, td, i;
  input = document.getElementById('myInput2');
  filter = input.value.toUpperCase();
  table = document.getElementById('myTable');
  tr = table.getElementsByTagName('tr');

  // Loop through all table rows, and hide those who don't match the search query
  for (i = 0; i < tr.length; i++) {
    td = tr[i].getElementsByTagName('td')[1];
    if (td) {
      var inputData = input.value;
      if (input.value === '') {
        inputData = '0';
      }
      if (parseInt(td.innerHTML) > parseInt(inputData)) {
        tr[i].style.display = '';
      } else {
        tr[i].style.display = 'none';
      }
    } 
  }
}
</script>
<link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/css/bootstrap.min.css' integrity='sha384-rwoIResjU2yc3z8GV/NPeZWAv56rSmLldC3R/AZzGRnGxQQKnKkoFVhFQhNUwEyJ' crossorigin='anonymous'>
<style media='screen' type='text/css'>
#myInput {
    background-position: 10px 12px; /* Position the search icon */
    background-repeat: no-repeat; /* Do not repeat the icon image */
    width: 100%; /* Full-width */
    font-size: 16px; /* Increase font-size */
    padding: 12px 20px 12px 40px; /* Add some padding */
    border: 1px solid #ddd; /* Add a grey border */
    margin-bottom: 12px; /* Add some space below the input */
}
#myInput2 {
    background-position: 10px 12px; /* Position the search icon */
    background-repeat: no-repeat; /* Do not repeat the icon image */
    width: 100%; /* Full-width */
    font-size: 16px; /* Increase font-size */
    padding: 12px 20px 12px 40px; /* Add some padding */
    border: 1px solid #ddd; /* Add a grey border */
    margin-bottom: 12px; /* Add some space below the input */
}
</style>
</head>
<body>
<input type='text' id='myInput' onkeyup='myFunction()' placeholder='Search for names..'>
<table id='myTable' class='table'>
<thead>
  <tr>
    <th>Project</th>
    <th>Gate Status</th>
    <th>Lines of Code</th>
    <th>New Lines</th>
    <th>Coverage</th>
    <th>Coverage on New Lines</th>
  </tr>
</thead>
  <tbody>";


            return header;
        }


        /// <summary>
        /// Generates the header string.
        /// </summary>
        /// <returns></returns>
        private string GenerateHeaderString()
        {
            var header = @"
<!DOCTYPE html>
<html>
<head>
<script type='text/javascript' src='https://code.jquery.com/jquery-3.2.1.min.js'></script>
<script type='text/javascript' src='https://cdnjs.cloudflare.com/ajax/libs/jquery.tablesorter/2.28.7/js/jquery.tablesorter.js'></script> 

<script>
$(document).ready(function() 
    { 
        $('#myTable').tablesorter(); 
    } 
);

function myFunction() {
  // Declare variables 
  var input, filter, table, tr, td, i;
  input = document.getElementById('myInput');
  filter = input.value.toUpperCase();
  table = document.getElementById('myTable');
  tr = table.getElementsByTagName('tr');

  // Loop through all table rows, and hide those who don't match the search query
  for (i = 0; i < tr.length; i++) {
    td = tr[i].getElementsByTagName('td')[0];
    if (td) {
      if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
        tr[i].style.display = '';
      } else {
        tr[i].style.display = 'none';
      }
    } 
  }
}
function myFunction2() {
  // Declare variables 
  var input, filter, table, tr, td, i;
  input = document.getElementById('myInput2');
  filter = input.value.toUpperCase();
  table = document.getElementById('myTable');
  tr = table.getElementsByTagName('tr');

  // Loop through all table rows, and hide those who don't match the search query
  for (i = 0; i < tr.length; i++) {
    td = tr[i].getElementsByTagName('td')[1];
    if (td) {
      var inputData = input.value;
      if (input.value === '') {
        inputData = '0';
      }
      if (parseInt(td.innerHTML) > parseInt(inputData)) {
        tr[i].style.display = '';
      } else {
        tr[i].style.display = 'none';
      }
    } 
  }
}
</script>
<link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/css/bootstrap.min.css' integrity='sha384-rwoIResjU2yc3z8GV/NPeZWAv56rSmLldC3R/AZzGRnGxQQKnKkoFVhFQhNUwEyJ' crossorigin='anonymous'>
<style media='screen' type='text/css'>
#myInput {
    background-position: 10px 12px; /* Position the search icon */
    background-repeat: no-repeat; /* Do not repeat the icon image */
    width: 100%; /* Full-width */
    font-size: 16px; /* Increase font-size */
    padding: 12px 20px 12px 40px; /* Add some padding */
    border: 1px solid #ddd; /* Add a grey border */
    margin-bottom: 12px; /* Add some space below the input */
}
#myInput2 {
    background-position: 10px 12px; /* Position the search icon */
    background-repeat: no-repeat; /* Do not repeat the icon image */
    width: 100%; /* Full-width */
    font-size: 16px; /* Increase font-size */
    padding: 12px 20px 12px 40px; /* Add some padding */
    border: 1px solid #ddd; /* Add a grey border */
    margin-bottom: 12px; /* Add some space below the input */
}
</style>
</head>
<body>
<input type='text' id='myInput2' onkeyup='myFunction2()' placeholder='Line Hit Threshold..'>
<input type='text' id='myInput' onkeyup='myFunction()' placeholder='Search for names..'>
<table id='myTable' class='table'>
<thead>
  <tr>
    <th>Path</th>
    <th>Uncovered Lines</th>
    <th>Uncovered Conditions</th>
    <th>Line</th>
    <th>Line Hits</th>
    <th>Conditions</th>
    <th>Covered Conditions</th>
    <th onclick='sortTable(7)'>Author</th>
    <th>Date</th>
    <th>Revision</th>
    <th>Code</th>
  </tr>
</thead>
  <tbody>";


            return header;
        }

        /// <summary>
        /// Generates the header string.
        /// </summary>
        /// <returns></returns>
        private string GenerateHeaderCompactString()
        {
            var header = @"
<!DOCTYPE html>
<html>
<head>
<script type='text/javascript' src='https://code.jquery.com/jquery-3.2.1.min.js'></script>
<script type='text/javascript' src='https://cdnjs.cloudflare.com/ajax/libs/jquery.tablesorter/2.28.7/js/jquery.tablesorter.js'></script> 

<script>
$(document).ready(function() 
    { 
        $('#myTable').tablesorter(); 
    } 
);

function myFunction() {
  // Declare variables 
  var input, filter, table, tr, td, i;
  input = document.getElementById('myInput');
  filter = input.value.toUpperCase();
  table = document.getElementById('myTable');
  tr = table.getElementsByTagName('tr');

  // Loop through all table rows, and hide those who don't match the search query
  for (i = 0; i < tr.length; i++) {
    td = tr[i].getElementsByTagName('td')[0];
    if (td) {
      if (td.innerHTML.toUpperCase().indexOf(filter) > -1) {
        tr[i].style.display = '';
      } else {
        tr[i].style.display = 'none';
      }
    } 
  }
}
function myFunction2() {
  // Declare variables 
  var input, filter, table, tr, td, i;
  input = document.getElementById('myInput2');
  filter = input.value.toUpperCase();
  table = document.getElementById('myTable');
  tr = table.getElementsByTagName('tr');

  // Loop through all table rows, and hide those who don't match the search query
  for (i = 0; i < tr.length; i++) {
    td = tr[i].getElementsByTagName('td')[1];
    if (td) {
      var inputData = input.value;
      if (input.value === '') {
        inputData = '0';
      }
      if (parseInt(td.innerHTML) > parseInt(inputData)) {
        tr[i].style.display = '';
      } else {
        tr[i].style.display = 'none';
      }
    } 
  }
}
</script>
<link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/css/bootstrap.min.css' integrity='sha384-rwoIResjU2yc3z8GV/NPeZWAv56rSmLldC3R/AZzGRnGxQQKnKkoFVhFQhNUwEyJ' crossorigin='anonymous'>
<style media='screen' type='text/css'>
#myInput {
    background-position: 10px 12px; /* Position the search icon */
    background-repeat: no-repeat; /* Do not repeat the icon image */
    width: 100%; /* Full-width */
    font-size: 16px; /* Increase font-size */
    padding: 12px 20px 12px 40px; /* Add some padding */
    border: 1px solid #ddd; /* Add a grey border */
    margin-bottom: 12px; /* Add some space below the input */
}
#myInput2 {
    background-position: 10px 12px; /* Position the search icon */
    background-repeat: no-repeat; /* Do not repeat the icon image */
    width: 100%; /* Full-width */
    font-size: 16px; /* Increase font-size */
    padding: 12px 20px 12px 40px; /* Add some padding */
    border: 1px solid #ddd; /* Add a grey border */
    margin-bottom: 12px; /* Add some space below the input */
}
</style>
</head>
<body>
<input type='text' id='myInput2' onkeyup='myFunction2()' placeholder='Line Hit Threshold..'>
<input type='text' id='myInput' onkeyup='myFunction()' placeholder='Search for names..'>
<table id='myTable' class='table'>
<thead>
  <tr>
    <th>Path</th>
    <th>Uncovered Lines</th>
    <th>Uncovered Conditions</th>
    <th>Authors</th>
  </tr>
</thead>
  <tbody>";

            return header;
        }

        private string GenerateFooter()
        {
            var footer = @"
  </tbody>
</table>
</body>
</html>";
            return footer;
        }
    }
}
