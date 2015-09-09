namespace VSSonarExtensionUi.Model.Configuration
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Reflection;

    public class VSSonarExtensionDiagnostic
    {
        public VSSonarExtensionDiagnostic(string name, string path)
        {
            this.Name = name;
            this.Path = path;
            this.Enabled = true;
            this.AvailableChecks = new List<DiagnosticAnalyzer>();
            this.ChecksInterpretation = new ObservableCollection<DiagnosticDescriptor>();
            this.LoadDiagnostics(path);
        }

        public string ErrorMessage { get; private set; }

        public string Name { get; private set; }

        public string Path { get; private set; }

        public List<DiagnosticAnalyzer> AvailableChecks { get; private set; }

        public bool Enabled { get; set; }

        public ObservableCollection<DiagnosticDescriptor> ChecksInterpretation { get; private set; }

        private void LoadDiagnostics(string path)
        {
            try
            {
                var assembly = Assembly.LoadFrom(path);
                var types2 = assembly.GetExportedTypes();
                foreach (var typedata in types2)
                {
                    try
                    {
                        var data = (Activator.CreateInstance(typedata) as DiagnosticAnalyzer);
                        if (data != null)
                        {
                            this.AvailableChecks.Add(data);
                            foreach (var diagnostic in data.SupportedDiagnostics)
                            {
                                ChecksInterpretation.Add(diagnostic);
                            }
                        }
                        else
                        {
                            ErrorMessage += "Cannot Add Check: " + typedata.ToString() + " : null \r\n";
                        }                   
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage += "Cannot Add Check: " + typedata.ToString() + "\r\n" + ex.Message + "\r\n";
                    }
                }

            }
            catch (System.Exception ex)
            {
                ErrorMessage += "Critical Error: Not loaded: \r\n" + ex.Message + "\r\n";
            }
        }
    }
}
