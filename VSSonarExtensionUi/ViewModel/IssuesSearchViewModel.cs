using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSonarExtensionUi.ViewModel
{
    using GalaSoft.MvvmLight;

    using PropertyChanged;

    [ImplementPropertyChanged]
    public class IssuesSearchViewModel : IViewModelBase
    {
        public IssuesSearchViewModel()
        {
            this.Header = "Issues";
        }

        public string Header { get; set; }
    }
}
