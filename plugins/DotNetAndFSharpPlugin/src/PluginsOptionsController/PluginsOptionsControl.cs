namespace PluginsOptionsController
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using PropertyChanged;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using SonarRestService.Types;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Input;

    /// <summary>
    ///     The dummy options controller.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class PluginsOptionsControl : IPluginControlOption
    {
        #region Fields

        /// <summary>
        ///     The dummy control.
        /// </summary>
        private UserControl userControl;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginsOptionsControl" /> class.
        ///     Initializes a new instance of the
        ///     <see>
        ///         <cref>PluginsOptionsControl</cref>
        ///     </see>
        ///     class.
        /// </summary>
        public PluginsOptionsControl()
        {
            this.PluginProperties = new ObservableCollection<SonarQubeProperties>();

            this.InitCommanding();
            this.ForeGroundColor = System.Windows.Media.Colors.Black;
            this.BackGroundColor = System.Windows.Media.Colors.White;
        }

        /// <summary>
        ///     Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the add new prop trigger button command.
        /// </summary>
        public ICommand CreateExternalToolCommand { get; set; }

        /// <summary>
        ///     Gets or sets the configuration.
        /// </summary>
        public ISonarConfiguration Configuration { get; set; }

        /// <summary>
        ///     Gets or sets the excluded plugins.
        /// </summary>
        public string ExcludedPlugins { get; set; }

        /// <summary>
        ///     Gets or sets the language.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        ///     Gets or sets the text box.
        /// </summary>
        public ObservableCollection<SonarQubeProperties> PluginProperties { get; set; }

        /// <summary>
        ///     Gets or sets the project id.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>Gets the context type.</summary>
        public IEnumerable<Context> ContextType
        {
            get
            {
                return Enum.GetValues(typeof(Context)).Cast<Context>();
            }
        }

        /// <summary>Gets or sets a value indicating whether can modify data.</summary>
        public bool CanModifyData { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The is enabled.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool IsEnabled()
        {
            return true;
        }

        /// <summary>The get option control user interface.</summary>
        /// <returns>The <see cref="UserControl"/>.</returns>
        public UserControl GetOptionControlUserInterface()
        {
            return this.userControl ?? (this.userControl = new PluginsUserControl(this));
        }

        /// <summary>The refresh data in ui.</summary>
        /// <param name="project">The project.</param>
        /// <param name="helper">The helper.</param>
        public void RefreshDataInUi(Resource project, IConfigurationHelper helper)
        {
            this.PluginProperties.Clear();
            var stylecop = helper.ReadSetting(Context.AnalysisGeneral, "AnalysisPlugin", "StyleCopPath");
            if (stylecop != null)
            {
                this.PluginProperties.Add(stylecop);
            }
            
            this.CanModifyData = true;
        }

        /// <summary>The save data in ui.</summary>
        /// <param name="project">The project.</param>
        /// <param name="helper">The helper.</param>
        public void SaveDataInUi(Resource project, IConfigurationHelper helper)
        {
            foreach (var prop in this.PluginProperties)
            {
                helper.WriteSetting(prop);
            }
        }

        /// <summary>The refresh colours.</summary>
        /// <param name="foreground">The foreground.</param>
        /// <param name="background">The background.</param>
        public void RefreshColours(Color foreground, Color background)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        /// <summary>
        ///     The init commanding.
        /// </summary>
        private void InitCommanding()
        {
            this.CreateExternalToolCommand = new RelayCommand(this.OnCreateExternalToolCommand);
        }

        /// <summary>
        /// The on create external tool command.
        /// </summary>
        /// <param name="data">The data.</param>
        private void OnCreateExternalToolCommand(object data)
        {
            
        }

        #endregion
    }
}