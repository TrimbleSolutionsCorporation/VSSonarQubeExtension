// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CxxLocalExtension.cs" company="Copyright © 2014 jmecsoftware">
//     Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------

namespace CxxPlugin.LocalExtensions
{
    using GalaSoft.MvvmLight.Command;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using SonarRestService;
    using SonarRestService.Types;
    using SonarLocalAnalyser;
    using System.Threading.Tasks;

    /// <summary>
    ///     The cxx server extension.
    /// </summary>
    [ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    public class CxxLocalExtension : WaitHandle, IFileAnalyser
    {
        /// <summary>
        ///     The command plugin.
        /// </summary>
        private readonly IAnalysisPlugin commandPlugin;

        /// <summary>
        ///     The lock this.
        /// </summary>
        private readonly object lockThis = new object();

        /// <summary>
        ///     The sensors.
        /// </summary>
        private readonly Dictionary<string, ASensor> sensors;

        /// <summary>
        /// The notification manager
        /// </summary>
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfigurationHelper configurationHelper;

        /// <summary>
        /// The sonar rest service
        /// </summary>
        private readonly ISonarRestService sonarRestService;

        /// <summary>
        /// The profile
        /// </summary>
        private Dictionary<string, Profile> profile;

        /// <summary>
        /// The is loading
        /// </summary>
        private bool isLoading;
        private readonly IVsEnvironmentHelper vshelper;
        private readonly ClangTidySensor clangSensor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CxxLocalExtension"/> class.
        /// </summary>
        /// <param name="commandPlugin">The command plugin.</param>
        /// <param name="notificationManager">The notification manager.</param>
        /// <param name="configurationHelper">The configuration helper.</param>
        /// <param name="sonarRestService">The sonar rest service.</param>
        public CxxLocalExtension(
            IAnalysisPlugin commandPlugin,
            INotificationManager notificationManager,
            IConfigurationHelper configurationHelper,
            ISonarRestService sonarRestService,
            IVsEnvironmentHelper vshelper)
        {
            this.clangSensor = new ClangTidySensor(vshelper, configurationHelper, notificationManager);
            this.commandPlugin = commandPlugin;
            this.notificationManager = notificationManager;
            this.configurationHelper = configurationHelper;
            this.sonarRestService = sonarRestService;
            this.vshelper = vshelper;

            this.sensors = new Dictionary<string, ASensor>
                               {
                                   {
                                       CppCheckSensor.SKey, 
                                       new CppCheckSensor(notificationManager, configurationHelper, sonarRestService)
                                   }, 
                                   { RatsSensor.SKey, new RatsSensor(notificationManager, configurationHelper, sonarRestService) }, 
                                   { VeraSensor.SKey, new VeraSensor(notificationManager, configurationHelper, sonarRestService) },
                                   { CxxLintSensor.SKey, new CxxLintSensor(notificationManager, configurationHelper, sonarRestService, this.vshelper) },
                                   { PcLintSensor.SKey, new PcLintSensor(notificationManager, configurationHelper, sonarRestService) }, 
                                   {
                                       CxxExternalSensor.SKey, 
                                       new CxxExternalSensor(notificationManager, configurationHelper, sonarRestService)
                                   }
                               };
        }

        /// <summary>
        ///     The std outevent.
        /// </summary>
        public event EventHandler StdOutEvent;

        /// <summary>
        /// The execute analysis on file.
        /// </summary>
        /// <param name="itemInView">The item In View.</param>
        /// <param name="project">The project.</param>
        /// <param name="conf">The conf.</param>
        /// <returns>
        /// The <see><cref>List</cref></see>
        /// .
        /// </returns>
        public List<Issue> ExecuteAnalysisOnFile(VsFileItem itemInView, Resource project, ISonarConfiguration conf, bool fromSave)
        {
            var threads = new List<Thread>();
            var allIssues = new List<Issue>();

            foreach (var sensor in this.sensors)
            {
                CxxPlugin.WriteLogMessage(
                    this.notificationManager,
                    this.GetType().ToString(), 
                    "Launching  Analysis on: " + sensor.Key + " " + itemInView.FilePath);

                threads.Add(
                    this.RunSensorThread(
                        this.StdOutEvent, 
                        itemInView, 
                        sensor,
                        this.profile["c++"], 
                        allIssues,
                        new VSSonarQubeCmdExecutor(60000)));
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            return allIssues;
        }

        /// <summary>
        /// The run sensor thread.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="file">The file.</param>
        /// <param name="sensor">The sensor.</param>
        /// <param name="profileIn">The profile in.</param>
        /// <param name="issuesToReturn">The issues to return.</param>
        /// <param name="exec">The execute.</param>
        /// <returns>
        /// The <see cref="Thread" />.
        /// </returns>
        public Thread RunSensorThread(
            EventHandler output, 
            VsFileItem file, 
            KeyValuePair<string, ASensor> sensor, 
            Profile profileIn, 
            List<Issue> issuesToReturn,
            IVSSonarQubeCmdExecutor exec)
        {
            var t =
                new Thread(
                    () =>
                    this.RunSensor(output, file, sensor, profileIn, issuesToReturn, exec));
            t.Start();
            return t;
        }

        /// <summary>
        /// The stop all execution.
        /// </summary>
        /// <param name="runningThread">
        /// The running thread.
        /// </param>
        public void StopAllExecution(Thread runningThread)
        {
        }

        /// <summary>
        /// The is issue supported.
        /// </summary>
        /// <param name="issue">The issue.</param>
        /// <param name="conf">The conf.</param>
        /// <returns>
        /// The <see cref="bool" />.
        /// </returns>
        public bool IsIssueSupported(Issue issue, ISonarConfiguration conf)
        {
            return CxxPlugin.IsSupported(issue.Component);
        }


        /// <summary>
        /// Updates the profile.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="profileIn">The profile in.</param>
        public async Task<bool> UpdateProfile(
            Resource project,
            ISonarConfiguration configuration,
            Dictionary<string, Profile> profileIn,
            string vsVersion)
        {
            if (!this.isLoading)
            {
                this.isLoading = true;
                try
                {
                    this.profile = profileIn;
                    foreach(var sensor in this.sensors)
                    {
                        sensor.Value.UpdateProfile(project, configuration, profileIn, vsVersion);
                    }

                    await this.clangSensor.UpdateProfile(project, configuration, profileIn, vsVersion);

                }
                catch(Exception ex)
                {
                    this.isLoading = false;
                    throw ex;
                }

                this.isLoading = false;
            }

            return true;
        }

        /// <summary>
        /// The process sensors issues.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="sensorReportedLines">The sensor reported lines.</param>
        /// <param name="itemInView">The item in view.</param>
        /// <param name="profileIn">The profile in.</param>
        /// <param name="issuesToReturn">The issues to return.</param>
        private void ProcessSensorsIssues(
            string key, 
            List<string> sensorReportedLines, 
            VsFileItem itemInView, 
            Profile profileIn, 
            List<Issue> issuesToReturn)
        {
            var issuesPerTool = new List<Issue>();
            try
            {
                var data = this.configurationHelper.ReadSetting(Context.GlobalPropsId, OwnersId.ApplicationOwnerId, GlobalIds.ExtensionDebugModeEnabled).Value;
                if (data.ToLower().Equals("true"))
                {
                    foreach (string line in sensorReportedLines)
                    {
                        CxxPlugin.WriteLogMessage(this.notificationManager, this.GetType().ToString(), "[" + key + "] : " + line);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
            try
            {
                List<Issue> issuesInTool = this.sensors[key].GetViolations(sensorReportedLines);
                foreach (Issue issue in issuesInTool)
                {
                    string path1 = Path.GetFullPath(itemInView.FilePath);
                    string path2 = issue.Component;
                    if (path1.Equals(path2))
                    {
                        issue.Component = this.commandPlugin.GetResourceKey(itemInView, false);
                        Rule ruleInProfile = profileIn.GetRule(issue.Rule);
                        if (ruleInProfile != null)
                        {
                            issue.Effort = ruleInProfile.DebtRemFnCoeff;
                            issue.Severity = ruleInProfile.Severity;
                            issuesPerTool.Add(issue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CxxPlugin.WriteLogMessage(this.notificationManager, this.GetType().ToString(), "Exception: " + key + " " + ex.StackTrace);
            }

            lock (this.lockThis)
            {
                issuesToReturn.AddRange(issuesPerTool);
            }
        }

        /// <summary>
        /// The run sensor.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="file">The file.</param>
        /// <param name="sensor">The sensor.</param>
        /// <param name="profileIn">The profile in.</param>
        /// <param name="issuesToReturn">The issues to return.</param>
        /// <param name="exec">The execute.</param>
        private void RunSensor(
            EventHandler output,
            VsFileItem file,
            KeyValuePair<string, ASensor> sensor,
            Profile profileIn,
            List<Issue> issuesToReturn,
            IVSSonarQubeCmdExecutor exec)
        {
            try
            {
                List<string> lines = sensor.Value.LaunchSensor(this, output, file.FilePath, exec);
                this.ProcessSensorsIssues(
                    sensor.Key,
                    lines,
                    file,
                    profileIn,
                    issuesToReturn);
            }
            catch (Exception ex)
            {
                CxxPlugin.WriteLogMessage(
                    this.notificationManager,
                    this.GetType().ToString(),
                    sensor.Key + " : Exception on Analysis Plugin : " + file.FilePath + " " + ex.StackTrace);
                CxxPlugin.WriteLogMessage(this.notificationManager, this.GetType().ToString(), sensor.Key + " : StdError: " + exec.GetStdError());
                CxxPlugin.WriteLogMessage(this.notificationManager, this.GetType().ToString(), sensor.Key + " : StdOut: " + exec.GetStdError());
            }
        }

        internal List<IPluginCommand> GetAdditionalCommands(Dictionary<string, Profile> profile)
        {
            if(!profile.ContainsKey("c++"))
            {
                return new List<IPluginCommand>();
            }

            var rules = profile["c++"].GetAllRules();

            var listofCmds = new List<IPluginCommand>();
            // create commands for clang-tidy
            var commandExecute = new PluginCommand(this.notificationManager, this.clangSensor, "");
            commandExecute.Name = "clang-tidy";
            listofCmds.Add(commandExecute);

            commandExecute = new PluginCommand(this.notificationManager, this.clangSensor, "--fix --checks=llvm-*");
            commandExecute.Name = "clang-tidy --fix llvm-*";

            var enabled = rules.Any(rule => rule.Key.Contains("llvm-"));
            if(enabled)
            {
                listofCmds.Add(commandExecute);
            }

            commandExecute = new PluginCommand(this.notificationManager, this.clangSensor, "--fix --checks=google-*");
            commandExecute.Name = "clang-tidy --fix google";
            enabled = rules.Any(rule => rule.Key.Contains("google-"));
            if(enabled)
            {
                listofCmds.Add(commandExecute);
            }

            commandExecute = new PluginCommand(this.notificationManager, this.clangSensor, "--fix --checks=modernize-*,-modernize-use-using");
            commandExecute.Name = "clang-tidy --fix modernize";
            enabled = rules.Any(rule => rule.Key.Contains("modernize-"));
            if(enabled)
            {
                listofCmds.Add(commandExecute);
            }

            commandExecute = new PluginCommand(this.notificationManager, this.clangSensor, "--fix --checks=misc-*");
            commandExecute.Name = "clang-tidy --fix misc";
            enabled = rules.Any(rule => rule.Key.Contains("misc-"));
            if(enabled)
            {
                listofCmds.Add(commandExecute);
            }

            return listofCmds;
        }
    }

    internal class PluginCommand : IPluginCommand
    {
        /// <summary>
        /// The fix
        /// </summary>
        private readonly INotificationManager notificationManager;
        private bool isRunning;
        private readonly ClangTidySensor sensor;
        private readonly string additionalArgs;

        public PluginCommand(INotificationManager manager, ClangTidySensor sensor, string additionalArgs)
        {
            this.sensor = sensor;
            this.notificationManager = manager;
            this.additionalArgs = additionalArgs;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get ; set; }

        public List<Issue> ExecuteCommand(VsFileItem resource)
        {
            if(resource == null)
            {
                this.notificationManager.WriteMessageToLog("no resource in view, skip clang-tidy");
                return new List<Issue>();
            }

            if(this.isRunning)
            {
                this.notificationManager.WriteMessageToLog("clang-tidy already running");
                return new List<Issue>();
            }

            this.isRunning = true;
            var issues = this.sensor.ExecuteClangTidy(resource, new VSSonarQubeCmdExecutor(60000), this.additionalArgs);
            this.isRunning = false;
            return issues;
        }
    }
}