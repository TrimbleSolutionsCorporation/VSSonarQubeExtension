namespace VSSonarExtension.MainView.SampleData.SampleDataForButtons
{
    using System.ComponentModel;

    /// <summary>
    /// The item.
    /// </summary>
    public class Item : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        ///     The _ is license enable.
        /// </summary>
        private bool isLicenseEnable;

        #endregion

        #region Public Events

        /// <summary>
        ///     The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether is license enable.
        /// </summary>
        public bool IsLicenseEnable
        {
            get
            {
                return this.isLicenseEnable;
            }

            set
            {
                if (this.isLicenseEnable != value)
                {
                    this.isLicenseEnable = value;
                    this.OnPropertyChanged("IsLicenseEnable");
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}