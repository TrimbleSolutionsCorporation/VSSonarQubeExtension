using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionViewModel.License
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using ExtensionTypes;

    using ExtensionViewModel.Annotations;

    /// <summary>
    /// The license viewer controller.
    /// </summary>
    public class LicenseViewerController : INotifyPropertyChanged
    {
        /// <summary>
        /// The available licenses.
        /// </summary>
        private ObservableCollection<VsLicense> availableLicenses = new ObservableCollection<VsLicense>();

        /// <summary>
        /// The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseViewerController"/> class.
        /// </summary>
        public LicenseViewerController()
        {
            
        }

        /// <summary>
        ///     Gets the available plugins collection.
        /// </summary>
        public ObservableCollection<VsLicense> AvailableLicenses
        {
            get
            {
                return this.availableLicenses;
            }

            set
            {
                this.availableLicenses = value;
                this.OnPropertyChanged("AvailableLicenses");
            }
        }


        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        protected void OnPropertyChanged(string name)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
