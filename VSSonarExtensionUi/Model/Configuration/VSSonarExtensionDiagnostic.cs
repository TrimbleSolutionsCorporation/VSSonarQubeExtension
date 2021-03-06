﻿namespace VSSonarExtensionUi.Model.Configuration
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using VSSonarPlugins.Types;

    /// <summary>
    /// Diagnostic loader
    /// </summary>
    public class VSSonarExtensionDiagnostic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VSSonarExtensionDiagnostic"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        public VSSonarExtensionDiagnostic(string name)
        {
            this.Name = name;
            this.Enabled = true;
            this.AvailableChecks = new List<DiagnosticAnalyzerType>();
            this.ChecksInterpretation = new ObservableCollection<DiagnosticDescriptor>();
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the available checks.
        /// </summary>
        /// <value>
        /// The available checks.
        /// </value>
        public List<DiagnosticAnalyzerType> AvailableChecks { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="VSSonarExtensionDiagnostic"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets the checks interpretation.
        /// </summary>
        /// <value>
        /// The checks interpretation.
        /// </value>
        public ObservableCollection<DiagnosticDescriptor> ChecksInterpretation { get; private set; }

        /// <summary>
        /// path
        /// </summary>
        public string PathForDiagnostic { get; private set; }

        /// <summary>
        /// Loads the diagnostics.
        /// </summary>
        /// <param name="path">The path.</param>
        public async Task LoadDiagnostics(string path)
        {
            this.PathForDiagnostic = path;

            try
            {
                await this.LoadDependencies(Path.Combine(Directory.GetParent(path).ToString(), "deps"));
                await this.LoadDependencies(Directory.GetParent(path).ToString());

                var assembly = await Task.Run(() => Assembly.LoadFrom(path));

                var types2 = assembly.GetTypes();
                foreach (var typedata in types2)
                {
                    try
                    {
                        
                        if (typedata.IsSubclassOf(typeof(DiagnosticAnalyzer)) && !typedata.IsAbstract)
                        {
                            var data = Activator.CreateInstance(typedata) as DiagnosticAnalyzer;
                            var attribute = Attribute.GetCustomAttribute(typedata, typeof(DiagnosticAnalyzerAttribute)) as DiagnosticAnalyzerAttribute;
                            this.AvailableChecks.Add(new DiagnosticAnalyzerType() { Diagnostic = data, Languages = attribute.Languages });
                            foreach (var diagnostic in data.SupportedDiagnostics)
                            {
                                this.ChecksInterpretation.Add(diagnostic);
                            }
                        }
                        else
                        {
                            this.ErrorMessage += "Cannot Add Check: " + typedata.ToString() + " : null \r\n";
                        }                   
                    }
                    catch (Exception ex)
                    {
                        this.ErrorMessage += "Cannot Add Check: " + typedata.ToString() + "\r\n" + ex.Message + "\r\n";
                    }
                }
            }
            catch (System.Exception ex)
            {
                this.ErrorMessage += "Critical Error: Not loaded: \r\n" + ex.Message + "\r\n";
            }
        }

        /// <summary>
        /// Loads the dependencies.
        /// </summary>
        /// <param name="path">The path.</param>
        private async Task LoadDependencies(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            var dlls = Directory.GetFiles(path, "*.dll");
            foreach (var dll in dlls)
            {
                await Task.Run(() => Assembly.LoadFrom(dll));
            }
        }
    }
}
