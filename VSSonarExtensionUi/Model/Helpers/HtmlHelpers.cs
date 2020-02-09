namespace VSSonarExtensionUi.Model.Helpers
{
	using SonarRestService.Types;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public static class HtmlHelpers
	{
		/// <summary>
		/// language metric
		/// </summary>
		private class PerLanguageMetric
		{
			public PerLanguageMetric()
			{
				this.TotalLines = 0;
				this.TotalConditions = 0;
				this.LinesHits = 0;
				this.CoveredConditions = 0;

				this.BlockerIssue = 0;
				this.CriticalIssue = 0;
				this.MinorIssue = 0;
				this.MajorIssue = 0;
				this.InfoIssue = 0;
			}

			public decimal TotalLines { get; set; }

			public decimal TotalConditions { get; set; }

			public decimal LinesHits { get; set; }

			public decimal CoveredConditions { get; set; }

			public decimal BlockerIssue { get; set; }

			public decimal CriticalIssue { get; set; }

			public decimal MajorIssue { get; set; }

			public decimal MinorIssue { get; set; }

			public decimal InfoIssue { get; set; }
		}

		/// <summary>
		/// internal data class for teams
		/// </summary>
		private class TeamData
		{
			public TeamData()
			{
				this.MetricsPerLanguage = new Dictionary<string, PerLanguageMetric>();
			}

			public Dictionary<string, PerLanguageMetric> MetricsPerLanguage { get; set; }


		}

		public static string GetLanguage(string path)
		{
			if (path.EndsWith(".cpp") || path.EndsWith(".hpp") || path.EndsWith(".c") || path.EndsWith(".h"))
			{
				return "c++";
			}

			if (path.EndsWith(".cs"))
			{
				return "C#";
			}

			if (path.EndsWith(".vb"))
			{
				return "VB.Net";
			}

			if (path.EndsWith(".fs") || path.EndsWith(".fsx") || path.EndsWith(".fsi"))
			{
				return "F#";
			}

			if (path.EndsWith(".py"))
			{
				return "Python";
			}

			return "UnknowLanguage";
		}

		public static string GenerateIssuesReportPerTeam(IEnumerable<Issue> allIssues)
		{
			StringBuilder reportdata = new StringBuilder();
			reportdata.Append(GenerateTeamsIssuesReportHeaderString());

			var dicTeam = new Dictionary<string, TeamData>();

			foreach (var issue in allIssues)
			{
				var team = issue.Team;
				if (string.IsNullOrEmpty(team))
				{
					team = "No Team";
				}
				var language = GetLanguage(issue.Component);
				if (!dicTeam.ContainsKey(team))
				{
					dicTeam.Add(team, new TeamData());
				}

				var teamData = dicTeam[team];
				if (!teamData.MetricsPerLanguage.ContainsKey(language))
				{
					teamData.MetricsPerLanguage.Add(language, new PerLanguageMetric());
				}

				var languageMetricData = teamData.MetricsPerLanguage[language];

				switch (issue.Severity)
				{
					case Severity.BLOCKER:
						languageMetricData.BlockerIssue += 1;
						break;
					case Severity.CRITICAL:
						languageMetricData.CriticalIssue += 1;
						break;
					case Severity.MAJOR:
						languageMetricData.MajorIssue += 1;
						break;
					case Severity.MINOR:
						languageMetricData.MinorIssue += 1;
						break;
					case Severity.INFO:
						languageMetricData.InfoIssue += 1;
						break;
					default:
						break;
				}
			}
			
			foreach (var item in dicTeam)
			{
				var enumerableElements = item.Value.MetricsPerLanguage.AsEnumerable();
                var totalIssues =
                    enumerableElements.Sum(x => x.Value.BlockerIssue) +
                    enumerableElements.Sum(x => x.Value.CriticalIssue) +
                    enumerableElements.Sum(x => x.Value.MajorIssue) +
                    enumerableElements.Sum(x => x.Value.MinorIssue) +
                    enumerableElements.Sum(x => x.Value.InfoIssue);

                reportdata.Append("<tr>");
				reportdata.Append("<td>" + item.Key + "</td>");
                reportdata.Append("<td>" + totalIssues + "</td>");
                reportdata.Append("<td>" + enumerableElements.Sum(x => x.Value.BlockerIssue) + "</td>");
				reportdata.Append("<td>" + enumerableElements.Sum(x => x.Value.CriticalIssue) + "</td>");
				reportdata.Append("<td>" + enumerableElements.Sum(x => x.Value.MajorIssue) + "</td>");
				reportdata.Append("<td>" + enumerableElements.Sum(x => x.Value.MinorIssue) + "</td>");
				reportdata.Append("<td>" + enumerableElements.Sum(x => x.Value.InfoIssue) + "</td>");
				reportdata.Append("</tr>");
			}

			return reportdata.ToString();
		}


		public static string GenerateTeamsCoverageReport(
			Dictionary<string, CoverageDifferencial> completeData,
			ObservableCollection<Team> teamsCollection,
			IEnumerable<Team> selectedTeams)
		{
			var dicTeam = new Dictionary<string, TeamData>();

			foreach (var item in completeData)
			{
				var language = GetLanguage(item.Value.resource.Path);

				foreach (var line in item.Value.resource.Lines)
				{
					var teamForAuthor = GetTeamForAuthor(line.Value.ScmAuthor, teamsCollection);

					if (!dicTeam.ContainsKey(teamForAuthor))
					{
						dicTeam.Add(teamForAuthor, new TeamData());
					}

					var teamData = dicTeam[teamForAuthor];
					if (!teamData.MetricsPerLanguage.ContainsKey(language))
					{
						teamData.MetricsPerLanguage.Add(language, new PerLanguageMetric());
					}

					var languageMetricData = teamData.MetricsPerLanguage[language];

					languageMetricData.TotalLines += 1;
					languageMetricData.LinesHits += line.Value.LineHits;
					languageMetricData.TotalConditions += line.Value.Conditions;
					languageMetricData.CoveredConditions += line.Value.CoveredConditions;
				}
			}

			StringBuilder reportdata = new StringBuilder();
			reportdata.Append(GenerateTeamsCoverageReportHeaderString());
			foreach (var item in dicTeam)
			{
				if (selectedTeams.Count() > 0 && selectedTeams.FirstOrDefault(x => x.Name.Equals(item.Key)) == null)
				{
					continue;
				}

				var enumerableElements = item.Value.MetricsPerLanguage.AsEnumerable();
				var totalLines = enumerableElements.Sum(x => x.Value.TotalLines);
				var totalConditions = enumerableElements.Sum(x => x.Value.TotalConditions);
				var lineHits = enumerableElements.Sum(x => x.Value.LinesHits);
				var coveredConditions = enumerableElements.Sum(x => x.Value.CoveredConditions);

				var lineCoveragePercentage = (int)(lineHits / totalLines) * 100;
				var conditionCoveragePercentage = (int)(coveredConditions / totalConditions) * 100;

				reportdata.Append("<tr>");
				reportdata.Append("<td>" + item.Key + "</td>");
				reportdata.Append("<td>" + totalLines + "</td>");
				reportdata.Append("<td>" + totalConditions + "</td>");
				reportdata.Append("<td>" + lineHits + "</td>");
				reportdata.Append("<td>" + coveredConditions + "</td>");
				reportdata.Append("<td>" + lineCoveragePercentage + "</td>");
				reportdata.Append("<td>" + conditionCoveragePercentage + "</td>");
				reportdata.Append("<td>" + string.Join("\r\n", item.Value.MetricsPerLanguage.Select(kvp => string.Format("{0} => {1}", kvp.Key, kvp.Value.TotalLines))) + "</td>");
				reportdata.Append("<td>" + string.Join("\r\n", item.Value.MetricsPerLanguage.Select(kvp => string.Format("{0} => {1}", kvp.Key, kvp.Value.TotalConditions))) + "</td>");
				reportdata.Append("<td>" + string.Join("\r\n", item.Value.MetricsPerLanguage.Select(kvp => string.Format("{0} => {1}", kvp.Key, kvp.Value.LinesHits))) + "</td>");
				reportdata.Append("<td>" + string.Join("\r\n", item.Value.MetricsPerLanguage.Select(kvp => string.Format("{0} => {1}", kvp.Key, kvp.Value.CoveredConditions))) + "</td>");
				reportdata.Append("</tr>");
			}

			reportdata.Append(GenerateFooter());

			return reportdata.ToString();
		}

		public static string GenerateDetailedHtmlReport(
			Dictionary<string, CoverageDifferencial> report,
			IEnumerable<Team> teams,
			IEnumerable<Team> selectedTeams)
		{
			StringBuilder reportdata = new StringBuilder();
			reportdata.Append(GenerateHeaderString());

			foreach (var item in report)
			{
				var path = item.Key;
				var rep = item.Value;

				if (rep.UncoveredLines == 0 && rep.UncoveredConditons == 0)
				{
					continue;
				}

				foreach (var line in rep.resource.Lines)
				{
					var teamForAuthor = GetTeamForAuthor(line.Value.ScmAuthor, teams);
					if (selectedTeams.Count() > 0 && selectedTeams.FirstOrDefault(x => x.Name.Equals(teamForAuthor)) == null)
					{
						continue;
					}

					reportdata.Append("<tr>");
					reportdata.Append("<td>" + path + "</td>");
					reportdata.Append("<td>" + rep.UncoveredLines + "</td>");
					reportdata.Append("<td>" + rep.UncoveredConditons + "</td>");
					reportdata.Append("<td>" + line.Key + "</td>");
					reportdata.Append("<td>" + line.Value.LineHits + "</td>");
					reportdata.Append("<td>" + line.Value.Conditions + "</td>");
					reportdata.Append("<td>" + line.Value.CoveredConditions + "</td>");
					reportdata.Append("<td>" + teamForAuthor + "</td>");
					reportdata.Append("<td>" + line.Value.ScmAuthor + "</td>");
					reportdata.Append("<td>" + line.Value.ScmDate + "</td>");
					reportdata.Append("<td>" + line.Value.ScmRevision + "</td>");
					reportdata.Append("<td>" + line.Value.Code + "</td>");
					reportdata.Append("</tr>");
				}
			}

			reportdata.Append(GenerateFooter());

			return reportdata.ToString();
		}

		private static string GetTeamForAuthor(string scmAuthor, IEnumerable<Team> teams)
		{
			foreach (var team in teams)
			{
				foreach (var user in team.Users)
				{
					if (user.Email.ToLower().Equals(scmAuthor.ToLower()) || user.AddionalEmails.FirstOrDefault(x => x.ToLower().Equals(scmAuthor.ToLower())) != null)
					{
						return team.Name;
					}
				}
			}

			return "No Team";
		}

		public static string GenerateCompactHtmlReport(
			Dictionary<string, CoverageDifferencial> report,
			IEnumerable<Team> teams,
			IEnumerable<Team> selectedTeams)
		{
			StringBuilder reportdata = new StringBuilder();
			reportdata.Append(GenerateHeaderCompactString());

			foreach (var item in report)
			{
				var path = item.Key;
				var rep = item.Value;
				if (rep.UncoveredLines == 0 && rep.UncoveredConditons == 0)
				{
					continue;
				}

				var authors = "";
				var teamsLine = "";
				foreach (var line in rep.resource.Lines)
				{
					if (authors.Contains(line.Value.ScmAuthor))
					{
						continue;
					}

					var teamForAuthor = GetTeamForAuthor(line.Value.ScmAuthor, teams);
					if (selectedTeams.Count() > 0 && selectedTeams.FirstOrDefault(x => x.Name.Equals(teamForAuthor)) == null)
					{
						continue;
					}

					authors += line.Value.ScmAuthor + " ";
					teamsLine += teamForAuthor + " ";
				}

				if (string.IsNullOrEmpty(authors))
				{
					continue;
				}

				reportdata.Append("<tr>");
				reportdata.Append("<td>" + path + "</td>");
				reportdata.Append("<td>" + rep.NewLinesToCover + "</td>");
				reportdata.Append("<td>" + rep.NewConditionsToCover + "</td>");
				reportdata.Append("<td>" + rep.UncoveredLines + "</td>");
				reportdata.Append("<td>" + rep.UncoveredConditons + "</td>");
				reportdata.Append("<td>");
				reportdata.Append(teamsLine);
				reportdata.Append("</td>");
				reportdata.Append("<td>");
				reportdata.Append(authors);
				reportdata.Append("</td>");
				reportdata.Append("</tr>");
			}

			reportdata.Append(GenerateFooter());

			return reportdata.ToString();
		}

		public static string GenerateOverallCoverageHtmlReport(Dictionary<string, CoverageReport> report)
		{
			StringBuilder reportdata = new StringBuilder();
			reportdata.Append(HtmlHelpers.GenerateOverallCoverageReportHeaderString());

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

			reportdata.Append(GenerateFooter());

			return reportdata.ToString();
		}

		/// <summary>
		/// Generates the header string.
		/// </summary>
		/// <returns></returns>
		private static string GenerateHeaderString()
		{
			return GenerateBasicHeader() + @"
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
    <th onclick='sortTable(7)'>Team</th>
	<th onclick='sortTable(7)'>Author</th>
    <th>Date</th>
    <th>Revision</th>
    <th>Code</th>
  </tr>
</thead>
  <tbody>";
		}

		/// <summary>
		/// Generates the header string.
		/// </summary>
		/// <returns></returns>
		private static string GenerateHeaderCompactString()
		{
			return GenerateBasicHeader() + @"
<body>
<input type='text' id='myInput2' onkeyup='myFunction2()' placeholder='Line Hit Threshold..'>
<input type='text' id='myInput' onkeyup='myFunction()' placeholder='Search for names..'>
<table id='myTable' class='table'>
<thead>
  <tr>
    <th>Path</th>
    <th>Lines To Cover</th>
    <th>Conditions to Cover</th>
    <th>Uncovered Lines</th>
    <th>Uncovered Conditions</th>
    <th>Teams</th>
	<th>Authors</th>
  </tr>
</thead>
  <tbody>";
		}

		/// <summary>
		/// Generates overall sumary html header
		/// </summary>
		/// <returns></returns>
		private static string GenerateOverallCoverageReportHeaderString()
		{
			return GenerateBasicHeader() + @"
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
		}

		private static string GenerateTeamsIssuesReportHeaderString()
		{
			return GenerateBasicHeader() + @"
<body>
<input type='text' id='myInput' onkeyup='myFunction()' placeholder='Search for names..'>
<table id='myTable' class='table'>
<thead>
  <tr>
    <th>Team</th>
    <th>Total Issues</th>
    <th>Blocker Issues</th>
    <th>Critical Issues</th>
    <th>Major Issues</th>
    <th>Minor Issues</th>
    <th>Info Issues</th>
  </tr>
</thead>
  <tbody>";
		}

		private static string GenerateTeamsCoverageReportHeaderString()
		{
			return GenerateBasicHeader() + @"
<body>
<input type='text' id='myInput' onkeyup='myFunction()' placeholder='Search for names..'>
<table id='myTable' class='table'>
<thead>
  <tr>
    <th>Team</th>
    <th>Total Lines</th>
    <th>Total Conditions</th>
    <th>Total Lines Covered</th>
    <th>Total Conditions Covered</th>
    <th>Percentage Lines Covered</th>
    <th>Percentage Conditions Covered</th>
    <th>Lines Per Language</th>
    <th>Total Conditions Per Language</th>
    <th>Total Lines Covered Per Language</th>
    <th>Total Conditions Covered Per Language</th>
  </tr>
</thead>
  <tbody>";
		}

		private static string GenerateBasicHeader()
		{
			return @"
<!DOCTYPE html>
<html>
<head>
<script type='text/javascript' src='https://code.jquery.com/jquery-3.2.1.min.js'></script>
<script type='text/javascript' src='https://cdnjs.cloudflare.com/ajax/libs/jquery.tablesorter/2.28.7/js/jquery.tablesorter.js'></script>" + SortingScript() + CssStyle() + @"
</head>";

		}



		private static string SortingScript()
		{
			return @"<script>
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
</script>";
		}

		private static string CssStyle()
		{
			return @"
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
";
		}

		private static string GenerateFooter()
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
