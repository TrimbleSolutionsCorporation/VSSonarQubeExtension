﻿namespace VSSonarExtensionUi.Model.Configuration
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;    
    using System.IO;
    using System.Reflection;
    

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
        public VSSonarExtensionDiagnostic(string name, string path)
        {
            this.Name = name;
            this.Path = path;
            this.Enabled = true;
            this.AvailableChecks = new List<DiagnosticAnalyzer>();
            this.ChecksInterpretation = new ObservableCollection<DiagnosticDescriptor>();
            this.LoadDiagnostics(path);
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
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the available checks.
        /// </summary>
        /// <value>
        /// The available checks.
        /// </value>
        public List<DiagnosticAnalyzer> AvailableChecks { get; private set; }

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
        /// Loads the diagnostics.
        /// </summary>
        /// <param name="path">The path.</param>
        private void LoadDiagnostics(string path)
        {
            try
            {
                this.LoadDependencies(System.IO.Path.Combine(Directory.GetParent(path).ToString(), "deps"));

                var assembly = Assembly.LoadFrom(path);
                var types2 = assembly.GetExportedTypes();
                foreach (var typedata in types2)
                {
                    try
                    {
                        var data = Activator.CreateInstance(typedata) as DiagnosticAnalyzer;
                        if (data != null)
                        {
                            this.AvailableChecks.Add(data);
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
        private void LoadDependencies(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            var dlls = Directory.GetFiles(path, "*.dll");
            foreach (var dll in dlls)
            {
                Assembly.LoadFrom(dll);
            }
        }
    }
}
