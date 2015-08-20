using PropertyChanged;
using System.Collections.ObjectModel;
using System.Windows.Media;
using VSSonarExtensionUi.Model.Configuration;
using VSSonarExtensionUi.ViewModel.Helpers;
using System;

namespace VSSonarExtensionUi.ViewModel.Configuration
{
    [ImplementPropertyChanged]
    public class RoslynDiagnosticInterpretation
    {
        public RoslynDiagnosticInterpretation()
        {
        }

        public string Name { get; set; }
    }

    [ImplementPropertyChanged]
    public class RoslynManagerViewModel : IOptionsViewModelBase
    {
        private readonly RoslynManagerModel model;

        public RoslynManagerViewModel(RoslynManagerModel model)
        {
            this.Header = "Roslyn Manager";
            this.model = model;
            this.BackGroundColor = Colors.White;
            this.ForeGroundColor = Colors.Black;
            this.AvailableDllDiagnostics = new ObservableCollection<VSSonarExtensionDiagnostic>();
            this.AvailableChecksInDll = new ObservableCollection<RoslynDiagnosticInterpretation>();

            foreach (var item in model.ExtensionDiagnostics)
            {
                AvailableDllDiagnostics.Add(item.Value);
            }

            SonarQubeViewModel.RegisterNewViewModelInPool(this);
        }

        public VSSonarExtensionDiagnostic SelectedDiagnostic { get; set; }

        public ObservableCollection<RoslynDiagnosticInterpretation> AvailableChecksInDll { get; set; }

        public ObservableCollection<VSSonarExtensionDiagnostic> AvailableDllDiagnostics { get; set; }

        public Color BackGroundColor { get; set; }

        public Color ForeGroundColor { get; set; }

        public string Header { get; set; }

        public void UpdateColours(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        public void OnSelectedDiagnosticChanged()
        {
            this.AvailableChecksInDll.Clear();
            foreach (var item in this.SelectedDiagnostic.AvailableChecks)
            {
                var interpretation = new RoslynDiagnosticInterpretation() { Name = item.ToString()};
                this.AvailableChecksInDll.Add(interpretation);
            }
        }

        public object GetAvailableModel()
        {
            throw new NotImplementedException();
        }
    }
}
