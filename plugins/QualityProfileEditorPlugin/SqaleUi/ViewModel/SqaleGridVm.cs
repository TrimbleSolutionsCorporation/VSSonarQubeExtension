// --------------------------------------------------------------------------------------------------------------------
// <copyright file="sqalegridvm.cs" company="Copyright © 2014 jmecsoftware">
//   Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// <summary>
//   The filtering sub view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SqaleUi.ViewModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;

    using VSSonarPlugins.Types;

    using PropertyChanged;

    using SonarRestService;

    using SqaleManager;

    using SqaleUi.helpers;
    using SqaleUi.Menus;
    using SqaleUi.View;

    using VSSonarPlugins;

    public class RelayCommand<T> : ICommand
    {
        #region Fields

        readonly Action<T> _execute = null;
        readonly Predicate<T> _canExecute = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="execute">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        ///<summary>
        ///Defines the method that determines whether the command can execute in its current state.
        ///</summary>
        ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        ///<returns>
        ///true if this command can be executed; otherwise, false.
        ///</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        ///<summary>
        ///Occurs when changes occur that affect whether or not the command should execute.
        ///</summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        ///<summary>
        ///Defines the method to be called when the command is invoked.
        ///</summary>
        ///<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        #endregion
    }

    /// <summary>
    ///     The filtering sub view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class SqaleGridVm : IFilterOption, IDataModel, ISqaleGridVm, IViewModelTheme
    {
        #region Fields

        /// <summary>
        ///     The filter.
        /// </summary>
        private readonly IFilter filter;

        /// <summary>
        ///     The main model.
        /// </summary>
        private readonly SqaleEditorControlViewModel mainModel;

        /// <summary>
        ///     The selected rule.
        /// </summary>
        private Rule selectedRule;
        private ISonarConfiguration configuration;
        private ISonarRestService service;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SqaleGridVm"/> class.
        /// </summary>
        public SqaleGridVm()
        {
            this.ProfileRules = new ItemsChangeObservableCollection<Rule>(this);
            this.Profile = new CollectionViewSource { Source = this.ProfileRules }.View;
            this.RulesCounter = "0";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqaleGridVm"/> class.
        ///     Initializes a new instance of the
        ///     <see>
        ///         <cref>SqaleGridViewModel</cref>
        ///     </see>
        ///     class.
        /// </summary>
        /// <param name="mainModel">
        /// The main Model.
        /// </param>
        /// <param name="manager">
        /// The manager.
        /// </param>
        public SqaleGridVm(SqaleEditorControlViewModel mainModel, SqaleManager manager, ISonarRestService service, ISonarConfiguration configuration)
        {
            this.service = service;
            this.configuration = configuration;
            this.mainModel = mainModel;
            this.SqaleManager = manager;
            this.ProfileRules = new ItemsChangeObservableCollection<Rule>(this);
            this.Profile = new CollectionViewSource { Source = this.ProfileRules }.View;
            this.RulesCounter = "0";

            this.filter = new RuleFilter(this);
            this.RestService = new SonarRestService(new JsonSonarConnector());

            this.ContextMenuItems = this.CreateGridContextMenu();

            this.FilterTermDescription = string.Empty;
            this.FilterTermConfigKey = string.Empty;
            this.FilterTermRepo = string.Empty;
            this.FilterTermName = string.Empty;
            this.FilterTermKey = string.Empty;
            this.FilterTermEnabled = null;
            this.FilterTermCategory = null;
            this.FilterTermSubCategory = null;
            this.FilterTermSeverity = null;
            this.FilterTermRemediationFunction = null;

            this.FilterApplyCommand = new RelayCommand<object>(this.OnFilterApply);

            this.FilterClearAllCommand = new RelayCommand<object>(this.OnFilterRemoveAll);
            this.FilterClearConfigKeyCommand = new RelayCommand<object>(this.OnFilterRemoveConfigKey);
            this.FilterClearKeyCommand = new RelayCommand<object>(this.OnFilterRemoveKey);
            this.FilterClearNameCommand = new RelayCommand<object>(this.OnFilterRemoveName);
            this.FilterClearRepoCommand = new RelayCommand<object>(this.OnFilterRemoveRepo);
            this.FilterClearTagCommand = new RelayCommand<object>(this.OnFilterRemoveTag);
            this.FilterClearDescriptionCommand = new RelayCommand<object>(this.OnFilterRemoveDescription);

            this.FilterClearEnabledCommand = new RelayCommand<object>(this.OnFilterRemoveEnabledKey);
            this.FilterClearSeverityCommand = new RelayCommand<object>(this.OnFilterRemoveSeverityKey);
            this.FilterClearRemediationFunctionCommand = new RelayCommand<object>(this.OnFilterRemoveRemediationFunctionKey);
            this.FilterClearCategoryCommand = new RelayCommand<object>(this.OnFilterRemoveCategoryKey);
            this.FilterClearSubCategoryCommand = new RelayCommand<object>(this.OnFilterRemoveSubCategoryKey);

            // handling events
            this.MouseEventCommand = new RelayCommand<object>(this.OnMouseInteraction);
            this.SelectionChangedCommand = new RelayCommand<IList>(
                items =>
                    {
                        this.SelectedItems = items;
                        this.RulesCounter = this.ProfileRules.Count.ToString(CultureInfo.InvariantCulture);
                        SendItemToWorkAreaMenu.RefreshMenuItemsStatus(this.ContextMenuItems, items != null);
                        SelectKeyMenuItem.RefreshMenuItemsStatus(this.ContextMenuItems, this.SelectedItems.Count == 1);
                        CreateTagMenuItem.RefreshMenuItemsStatus(
                            this.ContextMenuItems, 
                            this.SelectedItems.Count == 1 && this.mainModel.ConnectedToServer);
                    });

            this.IsDirty = false;

            // project options
            this.CanSendToWorkAreaCommand = false;
            this.CanSendToProject = false;
            this.CanAddNewRuleCommand = true;
            this.CanRemoveRuleCommand = false;
            this.SendToWorkAreaCommand = new RelayCommand<object>(this.ExecuteSendToWorkAreaCommand);
            this.SendToProject = new RelayCommand<object>(this.ExecuteSendToProject);
            this.AddNewRuleCommand = new RelayCommand<object>(this.ExecuteAddNewRuleCommand);
            this.RemoveRuleCommand = new RelayCommand<object>(this.ExecuteRemoveRuleCommand);

            // import export
            this.CanImportXmlProfileCommand = true;
            this.CanImportProfileCommand = true;
            this.CanImportSqaleModelCommand = true;
            this.CanExportSaqleModelCommand = true;
            this.ImportXmlProfileCommand = new RelayCommand<object>(this.ExecuteImportXmlProfileCommand);
            this.ImportProfileCommand = new RelayCommand<object>(this.ExecuteImportProfileCommand);
            this.ImportSqaleModelCommand = new RelayCommand<object>(this.ExecuteImportSqaleModelCommand);
            this.ExportSaqleModelCommand = new RelayCommand<object>(this.ExecuteExportSaqleModelCommand);

            // server imports
            this.CanImportFromSonarServer = true;
            this.ImportServerQualityProfileFromProjectCommand = new RelayCommand<object>(
                this.ExecuteImportServerQualityProfileFromProjectCommand);
            this.ImportServerQualityProfileCommand = new RelayCommand<object>(
                this.ExecuteImportServerQualityProfileCommand);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the back ground colour.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        /// Gets or sets the fore ground colour.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        /// <summary>
        ///     Gets or sets the add new rule command.
        /// </summary>
        public ICommand AddNewRuleCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can add new rule command.
        /// </summary>
        public bool CanAddNewRuleCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can export saqle model command.
        /// </summary>
        public bool CanExportSaqleModelCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can import export from server.
        /// </summary>
        public bool CanImportFromSonarServer { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can import profile command.
        /// </summary>
        public bool CanImportProfileCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can import sqale model command.
        /// </summary>
        public bool CanImportSqaleModelCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can import xml profile command.
        /// </summary>
        public bool CanImportXmlProfileCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can remove rule command.
        /// </summary>
        public bool CanRemoveRuleCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can send to project.
        /// </summary>
        public bool CanSendToProject { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can send to work area command.
        /// </summary>
        public bool CanSendToWorkAreaCommand { get; set; }

        /// <summary>
        ///     Gets or sets the configuration.
        /// </summary>
        public ISonarConfiguration Configuration { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether syncing data with sonar model.
        /// </summary>
        public bool ConnectedToSonarServer { get; set; }

        /// <summary>
        ///     Gets or sets the profile.
        /// </summary>
        public ObservableCollection<IMenuItem> ContextMenuItems { get; set; }

        /// <summary>
        /// Gets or sets the create rules model.
        /// </summary>
        public CreateRuleViewModel CreateRulesModel { get; set; }

        /// <summary>
        ///     Gets or sets the export saqle model command.
        /// </summary>
        public ICommand ExportSaqleModelCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether filter active.
        /// </summary>
        public bool FilterActive { get; set; }

        /// <summary>
        ///     Gets or sets the filter apply command.
        /// </summary>
        public ICommand FilterApplyCommand { get; set; }

        /// <summary>
        /// Gets or sets the filter clear all command.
        /// </summary>
        public ICommand FilterClearAllCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter clear remediation category command.
        /// </summary>
        public ICommand FilterClearCategoryCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter clear config key command.
        /// </summary>
        public ICommand FilterClearConfigKeyCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter clear description command.
        /// </summary>
        public ICommand FilterClearDescriptionCommand { get; set; }

        /// <summary>
        /// Gets or sets the filter clear enabled command.
        /// </summary>
        public ICommand FilterClearEnabledCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter clear key command.
        /// </summary>
        public ICommand FilterClearKeyCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter clear name command.
        /// </summary>
        public ICommand FilterClearNameCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter clear remediation function command.
        /// </summary>
        public ICommand FilterClearRemediationFunctionCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter clear repo command.
        /// </summary>
        public ICommand FilterClearRepoCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter clear severity command.
        /// </summary>
        public ICommand FilterClearSeverityCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter clear remediation sub category command.
        /// </summary>
        public ICommand FilterClearSubCategoryCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter clear tag command.
        /// </summary>
        public ICommand FilterClearTagCommand { get; set; }

        /// <summary>
        ///     Gets or sets the filter term category.
        /// </summary>
        public Category? FilterTermCategory { get; set; }

        /// <summary>
        ///     Gets or sets the filter term config key.
        /// </summary>
        public string FilterTermConfigKey { get; set; }

        /// <summary>
        ///     Gets or sets the filter term description.
        /// </summary>
        public string FilterTermDescription { get; set; }

        /// <summary>
        ///     Gets or sets the filter term enabled.
        /// </summary>
        public string FilterTermEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the filter term key.
        /// </summary>
        public string FilterTermKey { get; set; }

        /// <summary>
        ///     Gets or sets the filter term name.
        /// </summary>
        public string FilterTermName { get; set; }

        /// <summary>
        ///     Gets or sets the filter term remdiation unit offset.
        /// </summary>
        public RemediationUnit? FilterTermRemdiationUnitOffset { get; set; }

        /// <summary>
        ///     Gets or sets the filter term remdiation unit val.
        /// </summary>
        public RemediationUnit? FilterTermRemdiationUnitVal { get; set; }

        /// <summary>
        ///     Gets or sets the filter term remediation function.
        /// </summary>
        public RemediationFunction? FilterTermRemediationFunction { get; set; }

        /// <summary>
        ///     Gets or sets the filter term repo.
        /// </summary>
        public string FilterTermRepo { get; set; }

        /// <summary>
        ///     Gets or sets the filter term severity.
        /// </summary>
        public Severity? FilterTermSeverity { get; set; }

        /// <summary>
        ///     Gets or sets the filter term sub category.
        /// </summary>
        public SubCategory? FilterTermSubCategory { get; set; }

        /// <summary>
        ///     Gets or sets the filter term tag.
        /// </summary>
        public string FilterTermTag { get; set; }

        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        ///     Gets or sets the import profile command.
        /// </summary>
        public ICommand ImportProfileCommand { get; set; }

        /// <summary>
        ///     Gets or sets the import server quality profile command.
        /// </summary>
        public ICommand ImportServerQualityProfileCommand { get; set; }

        /// <summary>
        ///     Gets or sets the import server quality profile command.
        /// </summary>
        public ICommand ImportServerQualityProfileFromProjectCommand { get; set; }

        /// <summary>
        ///     Gets or sets the import sqale model command.
        /// </summary>
        public ICommand ImportSqaleModelCommand { get; set; }

        /// <summary>
        ///     Gets or sets the import xml profile command.
        /// </summary>
        public ICommand ImportXmlProfileCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is dirty.
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        ///     Gets or sets the mouse event command.
        /// </summary>
        public ICommand MouseEventCommand { get; set; }

        /// <summary>
        ///     Gets or sets the profile.
        /// </summary>
        public ICollectionView Profile { get; set; }

        /// <summary>
        ///     Gets or sets the profile.
        /// </summary>
        [AlsoNotifyFor("RulesCounter")]
        public ItemsChangeObservableCollection<Rule> ProfileRules { get; set; }

        /// <summary>
        /// Gets or sets the profile selection window.
        /// </summary>
        public QualityProfileViewer ProfileSelectionWindow { get; set; }

        /// <summary>
        ///     Gets or sets the project file.
        /// </summary>
        public string ProjectFile { get; set; }

        /// <summary>
        ///     Gets or sets the profile selection window.
        /// </summary>
        public ProjectProfileViewer ProjectProfileSelectionWindow { get; set; }

        /// <summary>
        /// Gets or sets the project quality viewer model.
        /// </summary>
        public QualityViewerViewModel ProjectQualityViewerModel { get; set; }

        /// <summary>
        ///     Gets or sets the quality viewer model.
        /// </summary>
        public QualityViewerViewModel QualityViewerModel { get; set; }

        /// <summary>
        ///     Gets or sets the remove rule command.
        /// </summary>
        public ICommand RemoveRuleCommand { get; set; }

        /// <summary>
        ///     Gets or sets the rest service.
        /// </summary>
        public ISonarRestService RestService { get; set; }

        /// <summary>
        ///     Gets or sets the rules counter.
        /// </summary>
        public string RulesCounter { get; set; }

        /// <summary>
        ///     Gets or sets the selected items.
        /// </summary>
        public IList SelectedItems { get; set; }

        /// <summary>
        /// Gets or sets the selected profile.
        /// </summary>
        public Profile SelectedProfile { get; set; }

        /// <summary>
        ///     Gets or sets the selected rule.
        /// </summary>
        public Rule SelectedRule
        {
            get
            {
                return this.selectedRule;
            }

            set
            {
                this.CanRemoveRuleCommand = value != null;

                this.selectedRule = value;
            }
        }

        /// <summary>
        ///     Gets the selection changed command.
        /// </summary>
        public ICommand SelectionChangedCommand { get; private set; }

        /// <summary>
        ///     Gets or sets the send to project.
        /// </summary>
        public ICommand SendToProject { get; set; }

        /// <summary>
        ///     Gets or sets the send to work area.
        /// </summary>
        public ICommand SendToWorkArea { get; set; }

        /// <summary>
        ///     Gets or sets the send to work area command.
        /// </summary>
        public ICommand SendToWorkAreaCommand { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether show context menu.
        /// </summary>
        public bool ShowContextMenu { get; set; }

        /// <summary>
        ///     Gets or sets the solution.
        /// </summary>
        public Resource Solution { get; set; }

        /// <summary>
        ///     Gets or sets the sqale project.
        /// </summary>
        public SqaleManager SqaleManager { get; set; }

        /// <summary>
        ///     Gets or sets the v shelper.
        /// </summary>
        public IConfigurationHelper VShelper { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is connected.
        /// </summary>
        public bool IsConnected { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The aggregate error strings.
        /// </summary>
        /// <param name="arg1">
        /// The arg 1.
        /// </param>
        /// <param name="arg2">
        /// The arg 2.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string AggregateErrorStrings(string arg1, string arg2)
        {
            return arg1 + "\r\n" + arg2;
        }

        /// <summary>
        /// The update colours.
        /// </summary>
        /// <param name="background">
        /// The background.
        /// </param>
        /// <param name="foreground">
        /// The foreground.
        /// </param>
        public void UpdateColors(Color background, Color foreground)
        {
            this.BackGroundColor = background;
            this.ForeGroundColor = foreground;
        }

        /// <summary>
        ///     The apply.
        /// </summary>
        public void Apply()
        {
            this.FilterActive = true;
        }

        /// <summary>
        ///     The clear category.
        /// </summary>
        public void ClearCategory()
        {
            this.FilterTermCategory = null;
            this.FilterActive = this.SetFilterActive();
        }

        /// <summary>
        ///     The clear config key.
        /// </summary>
        public void ClearConfigKey()
        {
            this.FilterTermConfigKey = string.Empty;
            this.FilterActive = this.SetFilterActive();
        }

        /// <summary>
        ///     The clear description.
        /// </summary>
        public void ClearDescription()
        {
            this.FilterTermDescription = string.Empty;
            this.FilterActive = this.SetFilterActive();
        }

        /// <summary>
        ///     The clear key.
        /// </summary>
        public void ClearKey()
        {
            this.FilterTermKey = string.Empty;
            this.FilterActive = this.SetFilterActive();
        }

        /// <summary>
        ///     The clear name.
        /// </summary>
        public void ClearName()
        {
            this.FilterTermName = string.Empty;
            this.FilterActive = this.SetFilterActive();
        }

        /// <summary>
        ///     The clear remediation function.
        /// </summary>
        public void ClearRemediationFunction()
        {
            this.FilterTermRemediationFunction = null;
            this.FilterActive = this.SetFilterActive();
        }

        /// <summary>
        ///     The clear repo.
        /// </summary>
        public void ClearRepo()
        {
            this.FilterTermRepo = string.Empty;
            this.FilterActive = this.SetFilterActive();
        }

        /// <summary>
        ///     The clear severity.
        /// </summary>
        public void ClearSeverity()
        {
            this.FilterTermSeverity = null;
            this.FilterActive = this.SetFilterActive();
        }

        /// <summary>
        ///     The clear sub category.
        /// </summary>
        public void ClearSubCategory()
        {
            this.FilterTermSubCategory = null;
            this.FilterActive = this.SetFilterActive();
        }

        /// <summary>
        ///     The clear repo.
        /// </summary>
        public void ClearTag()
        {
            this.FilterTermTag = string.Empty;
            this.FilterActive = this.SetFilterActive();
        }

        /// <summary>
        ///     The create new key.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string CreateNewKey()
        {
            string key = PromptUserData.Prompt("Insert New Key", "Key Request", string.Empty);

            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (this.ProfileRules.Any(profileRule => profileRule.Key.Equals(key)))
            {
                MessageBox.Show("Key allready in use");
                return string.Empty;
            }

            return key;
        }

        /// <summary>
        /// The merge rule.
        /// </summary>
        /// <param name="rule">
        /// The rule.
        /// </param>
        public void MergeRule(Rule rule)
        {
            bool found = false;
            foreach (Rule profileRule in this.ProfileRules)
            {
                if (rule.Key.Equals(profileRule.Key))
                {
                    profileRule.MergeRule(rule);
                    found = true;
                }
            }

            if (!found)
            {
                this.ProfileRules.Add(CopyRule(rule));
            }
        }

        /// <summary>
        /// The add rules from sqale mode to work area.
        /// </summary>
        /// <param name="rules">
        /// The rules.
        /// </param>
        public void MergeRulesIntoProject(List<Rule> rules)
        {
            foreach (Rule rule in rules)
            {
                bool found = false;
                foreach (Rule profileRule in this.ProfileRules)
                {
                    if (profileRule.Key.Equals(rule.Key))
                    {
                        profileRule.Repo = rule.Repo;
                        profileRule.RemediationFactorTxt = rule.RemediationFactorTxt;
                        profileRule.RemediationFactorVal = rule.RemediationFactorVal;
                        profileRule.RemediationFunction = rule.RemediationFunction;
                        profileRule.RemediationOffsetTxt = rule.RemediationOffsetTxt;
                        profileRule.RemediationOffsetVal = rule.RemediationOffsetVal;
                        profileRule.Category = rule.Category;
                        profileRule.Subcategory = rule.Subcategory;

                        found = true;
                    }
                }

                if (!found)
                {
                    this.ProfileRules.Add(rule);
                }
            }

            this.RefreshView();
            this.RefreshMenus();
        }

        /// <summary>
        /// The process changes.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="propertyChangedEventArgs">
        /// The property changed event args.
        /// </param>
        public void ProcessChanges(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.IsDirty = true;
            var rule = sender as Rule;
            if (this.ConnectedToSonarServer && rule != null)
            {
                if (propertyChangedEventArgs.PropertyName.Equals("Enabled"))
                {
                    if (rule.Enabled)
                    {
                        List<string> reply = this.RestService.ActivateRule(this.Configuration, rule.Key, rule.Severity.ToString(), this.SelectedProfile.Key);
                        if (reply != null && reply.Count != 0)
                        {
                            MessageBox.Show("Cannot Update Status Of Data in Server: " + reply.Aggregate(this.AggregateErrorStrings));
                        }
                    }
                    else
                    {
                        List<string> reply = this.RestService.DisableRule(this.Configuration, rule, this.SelectedProfile.Key);

                        if (reply != null && reply.Count != 0)
                        {
                            MessageBox.Show("Cannot Update Status Of Data in Server: " + reply.Aggregate(this.AggregateErrorStrings));
                        }
                    }
                }

                if (propertyChangedEventArgs.PropertyName.Equals("Severity") && rule.Enabled)
                {
                    this.RestService.ActivateRule(this.Configuration, rule.Key, rule.Severity.ToString(), this.SelectedProfile.Key);
                }

                if (propertyChangedEventArgs.PropertyName.Equals("Subcategory"))
                {
                    var dic = new Dictionary<string, string>
                                  {
                                      {
                                          "debt_sub_characteristic", 
                                          rule.Subcategory.Equals(SubCategory.UNDEFINED)
                                              ? string.Empty
                                              : rule.Subcategory.ToString()
                                      }
                                  };

                    List<string> reply = this.RestService.UpdateRule(this.Configuration, rule.Key, dic);
                    if (reply != null && reply.Count != 0)
                    {
                        MessageBox.Show("Cannot Update Status Of Data in Server: " + reply.Aggregate(this.AggregateErrorStrings));
                    }
                }

                if ((propertyChangedEventArgs.PropertyName.Equals("RemediationFunction")
                     || propertyChangedEventArgs.PropertyName.Equals("RemediationOffsetVal")
                     || propertyChangedEventArgs.PropertyName.Equals("RemediationOffsetTxt")
                     || propertyChangedEventArgs.PropertyName.Equals("RemediationFactorVal")
                     || propertyChangedEventArgs.PropertyName.Equals("RemediationFactorTxt"))
                    && !rule.RemediationFunction.Equals(RemediationFunction.UNDEFINED))
                {
                    var dic = new Dictionary<string, string> { { "debt_remediation_fn_type", rule.RemediationFunction.ToString() } };

                    if (rule.RemediationFactorTxt != RemediationUnit.UNDEFINED)
                    {
                        dic.Add(
                            "debt_remediation_fy_coeff", 
                            rule.RemediationFactorVal + rule.RemediationFactorTxt.ToString().ToLower().Replace("mn", "min"));
                    }

                    if (rule.RemediationOffsetTxt != RemediationUnit.UNDEFINED)
                    {
                        dic.Add(
                            "debt_remediation_fn_offset", 
                            rule.RemediationOffsetVal + rule.RemediationOffsetTxt.ToString().ToLower().Replace("mn", "min"));
                    }

                    List<string> reply = this.RestService.UpdateRule(this.Configuration, rule.Key, dic);
                    if (reply != null && reply.Count != 0)
                    {
                        MessageBox.Show("Cannot Update Status Of Data in Server: " + reply.Aggregate(this.AggregateErrorStrings));
                        if (propertyChangedEventArgs.PropertyName.Equals("RemediationFunction"))
                        {
                            rule.RemediationFunction = RemediationFunction.UNDEFINED;
                        }

                        if (propertyChangedEventArgs.PropertyName.Equals("RemediationOffsetTxt"))
                        {
                            rule.RemediationOffsetTxt = RemediationUnit.UNDEFINED;
                        }

                        if (propertyChangedEventArgs.PropertyName.Equals("RemediationFactorTxt"))
                        {
                            rule.RemediationFactorTxt = RemediationUnit.UNDEFINED;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     The refresh menus.
        /// </summary>
        public void RefreshMenus()
        {
            if (this.SelectedItems != null)
            {
                SelectKeyMenuItem.RefreshMenuItemsStatus(this.ContextMenuItems, this.SelectedItems.Count == 1);
            }
            else
            {
                SelectKeyMenuItem.RefreshMenuItemsStatus(this.ContextMenuItems, false);
            }

            SendItemToWorkAreaMenu.RefreshMenuItemsStatus(this.ContextMenuItems, this.SelectedItems != null);
            SendItemToWorkAreaMenu.RefreshMenuItems(this.ContextMenuItems, this.mainModel, this, this.SelectedItems != null);
        }

        /// <summary>
        ///     The refresh view.
        /// </summary>
        public void RefreshView()
        {
            try
            {
                if (this.Profile.SourceCollection == null)
                {
                    this.Profile = new CollectionViewSource { Source = this.ProfileRules }.View;
                }

                this.Profile.Refresh();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// The send selected items to work area.
        /// </summary>
        /// <param name="commandText">
        /// The command text.
        /// </param>
        public void SendSelectedItemsToWorkArea(string commandText)
        {
            this.CancelAnyEdit();
            SendItemToWorkAreaMenu.RefreshMenuItemsStatus(this.ContextMenuItems, this.SelectedItems != null);

            if (this.SelectedItems == null)
            {
                return;
            }

            if (commandText.Contains("New Work Area"))
            {
                SqaleGridVm workArea = this.mainModel.CreateNewWorkArea(false);
                bool isOk = true;
                foreach (Rule rule in this.SelectedItems)
                {
                    if (string.IsNullOrEmpty(rule.Key))
                    {
                        isOk = false;
                    }
                    else
                    {
                        workArea.ProfileRules.Add(CopyRule(rule));
                    }
                }

                if (!isOk)
                {
                    System.Windows.MessageBox.Show("Not all rules have been imported, to import a rule the key must be defined");
                }

                this.RefreshMenus();
                return;
            }

            foreach (SqaleGridVm tab in this.mainModel.Tabs)
            {
                if (tab.Header.Equals(commandText))
                {
                    bool isOk = true;
                    foreach (Rule rule in this.SelectedItems)
                    {
                        if (string.IsNullOrEmpty(rule.Key))
                        {
                            isOk = false;
                        }
                        else
                        {
                            SqaleGridVm sqaleGridVm = tab;
                            sqaleGridVm.MergeRule(rule);
                        }
                    }

                    if (!isOk)
                    {
                        System.Windows.MessageBox.Show("Not all rules have been merged, to import a rule the key must be defined");
                    }
                }
            }

            this.RefreshMenus();
        }

        /// <summary>
        /// The set connected to server.
        /// </summary>
        /// <param name="b">
        /// The b.
        /// </param>
        public void SetConnectedToServer(bool b)
        {
            this.ConnectedToSonarServer = b;
            this.DisableImportXmls();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The copy rule.
        /// </summary>
        /// <param name="rule">
        /// The rule.
        /// </param>
        /// <returns>
        /// The <see cref="Rule"/>.
        /// </returns>
        private static Rule CopyRule(Rule rule)
        {
            var newrule = new Rule
                              {
                                  Category = rule.Category, 
                                  ConfigKey = rule.ConfigKey, 
                                  HtmlDescription = rule.HtmlDescription,
                                  MarkDownDescription = rule.MarkDownDescription, 
                                  Key = rule.Key, 
                                  Name = rule.Name, 
                                  RemediationFactorTxt = rule.RemediationFactorTxt, 
                                  RemediationFactorVal = rule.RemediationFactorVal, 
                                  RemediationFunction = rule.RemediationFunction, 
                                  RemediationOffsetTxt = rule.RemediationOffsetTxt, 
                                  RemediationOffsetVal = rule.RemediationOffsetVal, 
                                  Repo = rule.Repo, 
                                  Severity = rule.Severity, 
                                  Subcategory = rule.Subcategory
                              };
            return newrule;
        }

        /// <summary>
        /// The add rules from profile to work area.
        /// </summary>
        /// <param name="rules">
        /// The rules.
        /// </param>
        private void AddRulesFromProfileToWorkArea(IEnumerable<Rule> rules)
        {
            foreach (Rule rule in rules)
            {
                bool found = false;
                foreach (Rule profileRule in this.ProfileRules)
                {
                    if (profileRule.Key.Equals(rule.Key))
                    {
                        profileRule.Repo = rule.Repo;
                        profileRule.Severity = rule.Severity;
                        found = true;
                    }
                }

                if (!found)
                {
                    this.ProfileRules.Add(CopyRule(rule));
                }
            }

            this.RefreshView();
            this.RefreshMenus();
        }

        /// <summary>
        /// The add rules from rules xml definition to work area.
        /// </summary>
        /// <param name="rules">
        /// The rules.
        /// </param>
        private void AddRulesFromRulesXmlDefinitionToWorkArea(IEnumerable<Rule> rules)
        {
            foreach (Rule rule in rules)
            {
                bool found = false;
                foreach (Rule profileRule in this.ProfileRules)
                {
                    if (profileRule.Key.Equals(rule.Key))
                    {
                        profileRule.ConfigKey = rule.ConfigKey;
                        profileRule.Name = rule.Name;
                        profileRule.HtmlDescription = rule.HtmlDescription;
                        profileRule.MarkDownDescription = rule.MarkDownDescription;
                        found = true;
                    }
                }

                if (!found)
                {
                    this.ProfileRules.Add(CopyRule(rule));
                }
            }

            this.RefreshView();
            this.RefreshMenus();
        }

        /// <summary>
        /// The add rules from sqale mode to work area.
        /// </summary>
        /// <param name="rules">
        /// The rules.
        /// </param>
        private void AddRulesFromSqaleModeToWorkArea(IEnumerable<Rule> rules)
        {
            foreach (Rule rule in rules)
            {
                bool found = false;
                foreach (Rule profileRule in this.ProfileRules)
                {
                    if (profileRule.Key.Equals(rule.Key))
                    {
                        profileRule.Repo = rule.Repo;
                        profileRule.RemediationFactorTxt = rule.RemediationFactorTxt;
                        profileRule.RemediationFactorVal = rule.RemediationFactorVal;
                        profileRule.RemediationFunction = rule.RemediationFunction;
                        profileRule.RemediationOffsetTxt = rule.RemediationOffsetTxt;
                        profileRule.RemediationOffsetVal = rule.RemediationOffsetVal;
                        profileRule.Category = rule.Category;
                        profileRule.Subcategory = rule.Subcategory;
                        found = true;
                    }
                }

                if (!found)
                {
                    this.ProfileRules.Add(CopyRule(rule));
                }
            }

            this.RefreshView();
            this.RefreshMenus();
        }

        /// <summary>
        ///     The cancel any edit.
        /// </summary>
        private void CancelAnyEdit()
        {
            if (this.Profile is IEditableCollectionView)
            {
                var myEditableCollectionView = this.Profile as IEditableCollectionView;
                if (myEditableCollectionView.IsAddingNew)
                {
                    myEditableCollectionView.CommitNew();
                }

                if (myEditableCollectionView.IsEditingItem)
                {
                    myEditableCollectionView.CommitEdit();
                }
            }
        }

        /// <summary>
        /// The clear enabled.
        /// </summary>
        private void ClearEnabled()
        {
            this.FilterTermEnabled = null;
            this.FilterActive = this.SetFilterActive();
        }

        /// <summary>
        ///     The clear filter.
        /// </summary>
        private void ClearFilter()
        {
            if (!this.filter.IsEnabled())
            {
                this.CancelAnyEdit();
                this.Profile.Filter = null;
            }
        }

        /// <summary>
        ///     The create grid context menu.
        /// </summary>
        /// <returns>
        ///     The
        ///     <see>
        ///         <cref>ObservableCollection</cref>
        ///     </see>
        ///     .
        /// </returns>
        private ObservableCollection<IMenuItem> CreateGridContextMenu()
        {
            var menu = new ObservableCollection<IMenuItem>
                           {
                               SendItemToWorkAreaMenu.MakeMenu(this, this.mainModel), 
                               SelectKeyMenuItem.MakeMenu(this), 
                               CreateTagMenuItem.MakeMenu(this)
                           };

            return menu;
        }

        /// <summary>
        /// The disable import xmls.
        /// </summary>
        private void DisableImportXmls()
        {
            this.CanImportSqaleModelCommand = false;
            this.CanImportXmlProfileCommand = false;
            this.CanImportProfileCommand = false;
        }

        /// <summary>
        ///     The execute add new rule command.
        /// </summary>
        private void ExecuteAddNewRuleCommand(object data)
        {
            if (this.ConnectedToSonarServer)
            {
                if (this.CreateRulesModel == null)
                {
                    this.CreateRulesModel = new CreateRuleViewModel(this, this.service, this.configuration);
                }

                var rulecreationWindow = new CreateRuleWindow(this.CreateRulesModel);
                rulecreationWindow.Show();
            }
            else
            {
                string key = this.CreateNewKey();
                if (string.IsNullOrEmpty(key))
                {
                    return;
                }

                this.ProfileRules.Add(new Rule { Key = key });
            }
        }

        /// <summary>
        ///     The execute export saqle model command.
        /// </summary>
        private void ExecuteExportSaqleModelCommand(object data)
        {
            if (this.ProfileRules.Count == 0)
            {
                System.Windows.MessageBox.Show("Current View is Empty, Cannot create model");
                return;
            }

            var saveFileDialog = new SaveFileDialog { Filter = "xml files (*.xml)|*.xml", FilterIndex = 1, RestoreDirectory = true };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SqaleModel modelToExport = this.SqaleManager.CreateModelFromRules(this.ProfileRules);
                    this.SqaleManager.WriteSqaleModelToFile(modelToExport, saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Cannot Import: " + ex.Message);
                }
            }
        }

        /// <summary>
        ///     The execute import profile command.
        /// </summary>
        private void ExecuteImportProfileCommand(object data)
        {
            // Do something 
            var filedialog = new OpenFileDialog { Filter = @"Xml Profile|*.xml" };

            DialogResult result = filedialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            try
            {
                SqaleModel tempModel = this.SqaleManager.GetDefaultSqaleModel();
                this.SqaleManager.AddProfileDefinition(tempModel, filedialog.FileName);

                this.AddRulesFromProfileToWorkArea(tempModel.GetProfile().GetAllRules());
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot import: ", ex.Message);
            }
        }

        /// <summary>
        ///     The execute import server quality profile command.
        /// </summary>
        private void ExecuteImportServerQualityProfileCommand(object data)
        {
            if (this.ProfileRules.Count > 0)
            {
                DialogResult result = MessageBox.Show(
                    "Will Clear current rules in view?", 
                    "Confirm", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    this.ProfileRules.Clear();
                }
            }

            this.CanImportFromSonarServer = false;

            if (this.Configuration == null)
            {
                this.CanImportFromSonarServer = true;
                return;
            }

            if (this.QualityViewerModel == null)
            {
                this.QualityViewerModel = new QualityViewerViewModel(this.Configuration, this, true);
            }

            var profileSelectionWindow = new QualityProfileViewer(this.QualityViewerModel);
            this.CanImportFromSonarServer = true;
            profileSelectionWindow.ShowDialog();
        }

        /// <summary>
        ///     The execute import server quality profile command.
        /// </summary>
        private void ExecuteImportServerQualityProfileFromProjectCommand(object data)
        {
            if (this.ProfileRules.Count > 0)
            {
                DialogResult result = MessageBox.Show(
                    "Will Clear current rules in view?", 
                    "Confirm", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    this.ProfileRules.Clear();
                }
            }

            this.CanImportFromSonarServer = false;

            if (this.Configuration == null)
            {
                this.CanImportFromSonarServer = true;
                return;
            }

            if (this.ProjectQualityViewerModel == null)
            {
                this.ProjectQualityViewerModel = new QualityViewerViewModel(this.Configuration, this);
            }

            if (this.ProjectProfileSelectionWindow == null)
            {
                this.ProjectProfileSelectionWindow = new ProjectProfileViewer(this.ProjectQualityViewerModel);
            }

            this.CanImportFromSonarServer = true;
            try
            {
                this.ProjectProfileSelectionWindow.Show();
            }
            catch (Exception)
            {
                this.ProjectProfileSelectionWindow = new ProjectProfileViewer(this.ProjectQualityViewerModel);
                this.ProjectProfileSelectionWindow.Show();
            }
        }

        /// <summary>
        ///     The execute import sqale model command.
        /// </summary>
        private void ExecuteImportSqaleModelCommand(object data)
        {
            // Do something 
            var filedialog = new OpenFileDialog { Filter = @"Xml Sqale model|*.xml" };

            DialogResult result = filedialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            try
            {
                SqaleModel tempModel = this.SqaleManager.ParseSqaleModelFromXmlFile(filedialog.FileName);

                this.AddRulesFromSqaleModeToWorkArea(tempModel.GetProfile().GetAllRules());
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot import: " + ex.Message);
            }
        }

        /// <summary>
        ///     The execute import xml profile command.
        /// </summary>
        private void ExecuteImportXmlProfileCommand(object data)
        {
            // Do something 
            var filedialog = new OpenFileDialog { Filter = @"Rules Xml Definition|*.xml" };

            DialogResult result = filedialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            try
            {
                SqaleModel tempModel = this.SqaleManager.GetDefaultSqaleModel();

                string repo = PromptUserData.Prompt("Specify Repository Name", "Repo Name", Path.GetFileNameWithoutExtension(filedialog.FileName));
                this.SqaleManager.AddAProfileFromFileToSqaleModel(repo, tempModel, filedialog.FileName);

                this.AddRulesFromRulesXmlDefinitionToWorkArea(tempModel.GetProfile().GetAllRules());
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot import file:", ex.Message);
            }
        }

        /// <summary>
        ///     The execute remove rule command.
        /// </summary>
        private void ExecuteRemoveRuleCommand(object data)
        {
            if (this.SelectedRule != null)
            {
                if (this.ConnectedToSonarServer)
                {
                    List<string> errors = this.RestService.DeleteRule(this.Configuration, this.SelectedRule);
                    if (errors != null && errors.Count != 0)
                    {
                        MessageBox.Show("Cannot Delete Rule: " + errors.Aggregate(this.AggregateErrorStrings));
                        return;
                    }
                }

                this.ProfileRules.Remove(this.SelectedRule);
            }
        }

        /// <summary>
        ///     The execute send to project.
        /// </summary>
        private void ExecuteSendToProject(object data)
        {
            this.IsDirty = false;

            SqaleGridVm project = this.mainModel.Tabs[0];
            Rule templateRule = null;

            if (project.ConnectedToSonarServer)
            {
                var model = new CustomRuleSectorViewModel();
                var profile = new Profile(this.service, this.configuration);
                profile.Key = project.QualityViewerModel.SelectedProfile.Key;
                profile.Name = project.QualityViewerModel.SelectedProfile.Name;
                project.RestService.GetTemplateRules(project.Configuration, profile);

                foreach (Rule rule in profile.GetAllRules())
                {
                    model.CustomRules.Add(rule);
                }

                var window = new CustomRuleSelector(model);
                window.ShowDialog();

                templateRule = model.SelectedRule;
            }

            var importViewerModel = new ImportLogViewModel();

            foreach (Rule rule in this.ProfileRules)
            {
                if (this.filter.FilterFunction(rule))
                {
                    bool found = false;

                    foreach (Rule ruleinProject in project.ProfileRules)
                    {
                        if (ruleinProject.Key.Equals(rule.Key))
                        {
                            found = true;
                            ruleinProject.MergeRule(rule);
                        }
                    }

                    if (!found)
                    {
                        if (templateRule != null)
                        {
                            List<string> errors = this.RestService.CreateRule(project.Configuration, rule, templateRule);
                            if (errors.Count > 0)
                            {
                                foreach (string error in errors)
                                {
                                    importViewerModel.ImportLog.Add(new ImportLogEntry { exceptionMessage = rule.Key, line = 0, message = error });
                                }
                            }
                            else
                            {
                                project.ProfileRules.Add(CopyRule(rule));
                            }
                        }
                        else
                        {
                            project.ProfileRules.Add(CopyRule(rule));
                        }
                    }

                    this.RefreshView();
                }
            }

            if (importViewerModel.ImportLog.Count > 0)
            {
                var importLogWindow = new ImportLogView(importViewerModel);
                importLogWindow.Show();
            }
        }

        /// <summary>
        ///     The execute send to work area command.
        /// </summary>
        private void ExecuteSendToWorkAreaCommand(object data)
        {
            SqaleGridVm workArea = this.mainModel.CreateNewWorkArea(false, !this.ConnectedToSonarServer);

            bool errorsFound = false;
            foreach (Rule rule in this.ProfileRules)
            {
                if (string.IsNullOrEmpty(rule.Key))
                {
                    errorsFound = true;
                    continue;
                }

                if (this.filter.FilterFunction(rule))
                {
                    workArea.ProfileRules.Add(CopyRule(rule));
                }
            }

            if (errorsFound)
            {
                System.Windows.MessageBox.Show("Some items were not copied, key not defined");
            }

            SendItemToWorkAreaMenu.RefreshMenuItems(this.ContextMenuItems, this.mainModel, this, false);
        }

        /// <summary>
        /// The on filter apply.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterApply(object data)
        {
            this.Apply();

            this.CancelAnyEdit();

            this.Profile.Filter = this.filter.FilterFunction;
        }

        /// <summary>
        /// The on filter remove all.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnFilterRemoveAll(object obj)
        {
            this.ClearConfigKey();
            this.ClearKey();
            this.ClearCategory();
            this.ClearDescription();
            this.ClearEnabled();
            this.ClearFilter();
            this.ClearName();
            this.ClearRemediationFunction();
            this.ClearRepo();
            this.ClearSeverity();
            this.ClearSubCategory();
            this.ClearTag();
            this.ClearFilter();
        }

        /// <summary>
        /// The on filter remove category key.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterRemoveCategoryKey(object data)
        {
            this.ClearCategory();
            this.ClearFilter();
        }

        /// <summary>
        /// The on filter remove config key.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterRemoveConfigKey(object data)
        {
            this.ClearConfigKey();
            this.ClearFilter();
        }

        /// <summary>
        /// The on filter remove description.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterRemoveDescription(object data)
        {
            this.ClearDescription();
            this.ClearFilter();
        }

        /// <summary>
        /// The on filter remove enabled key.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterRemoveEnabledKey(object data)
        {
            this.ClearEnabled();
            this.ClearFilter();
        }

        /// <summary>
        /// The on filter remove key.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterRemoveKey(object data)
        {
            this.ClearKey();
            this.ClearFilter();
        }

        /// <summary>
        /// The on filter remove name.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterRemoveName(object data)
        {
            this.ClearName();
            this.ClearFilter();
        }

        /// <summary>
        /// The on filter remove remediation function key.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterRemoveRemediationFunctionKey(object data)
        {
            this.ClearRemediationFunction();
            this.ClearFilter();
        }

        /// <summary>
        /// The on filter remove repo.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterRemoveRepo(object data)
        {
            this.ClearRepo();
            this.ClearFilter();
        }

        /// <summary>
        /// The on filter remove severity key.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterRemoveSeverityKey(object data)
        {
            this.ClearSeverity();
            this.ClearFilter();
        }

        /// <summary>
        /// The on filter remove sub category key.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterRemoveSubCategoryKey(object data)
        {
            this.ClearSubCategory();
            this.ClearFilter();
        }

        /// <summary>
        /// The on filter remove tag.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void OnFilterRemoveTag(object data)
        {
            this.ClearTag();
            this.ClearFilter();
        }

        /// <summary>
        /// The on mouse interaction.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        private void OnMouseInteraction(object obj)
        {
            this.CancelAnyEdit();
        }

        /// <summary>
        ///     The set filter active.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        private bool SetFilterActive()
        {
            return !this.FilterTermDescription.Equals(string.Empty) || !this.FilterTermConfigKey.Equals(string.Empty)
                   || !this.FilterTermKey.Equals(string.Empty) || !this.FilterTermRepo.Equals(string.Empty)
                   || !this.FilterTermName.Equals(string.Empty) || this.FilterTermRemediationFunction != null || this.FilterTermCategory != null
                   || this.FilterTermSubCategory != null || this.FilterTermSeverity != null;
        }

        #endregion
    }
}