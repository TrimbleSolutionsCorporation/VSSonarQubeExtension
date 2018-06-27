// --------------------------------------------------------------------------------------------------------------------
// <copyright file="tageditorviewmodel.cs" company="Copyright © 2014 jmecsoftware">
//   Copyright (C) 2014 [jmecsoftware, jmecsoftware2014@tekla.com]
// </copyright>
// <summary>
//   The tag editor view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SqaleUi.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    using VSSonarPlugins;
    using VSSonarPlugins.Types;

    using PropertyChanged;

    using SqaleUi.Menus;

    /// <summary>
    ///     The tag editor view model.
    /// </summary>
    [ImplementPropertyChanged]
    public class TagEditorViewModel : IViewModelTheme
    {
        #region Fields

        /// <summary>
        ///     The conf.
        /// </summary>
        private readonly ISonarConfiguration conf;

        /// <summary>
        ///     The model.
        /// </summary>
        private readonly ISqaleGridVm model;

        /// <summary>
        ///     The service.
        /// </summary>
        private readonly ISonarRestService service;

        /// <summary>
        ///     The selected tag in rule.
        /// </summary>
        private string selectedTagInRule;

        /// <summary>
        ///     The selected tag in server.
        /// </summary>
        private string selectedTagInServer;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TagEditorViewModel" /> class.
        /// </summary>
        public TagEditorViewModel()
        {
            this.SelectedTags = new List<string>();
            this.AvailableTags = new ObservableCollection<string>();
            this.TagsInRule = new ObservableCollection<string>();
            this.CanExecuteAddSelectedTags = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagEditorViewModel"/> class.
        /// </summary>
        /// <param name="conf">
        /// The conf.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        public TagEditorViewModel(ISonarConfiguration conf, ISonarRestService service, ISqaleGridVm model)
        {
            this.conf = conf;
            this.service = service;
            this.model = model;

            this.SelectedTags = new List<string>();
            this.AvailableTags = new ObservableCollection<string>();
            this.TagsInRule = new ObservableCollection<string>();

            this.CanExecuteAddSelectedTags = false;
            this.CanExecuteRefreshTags = false;
            this.CanExecuteRemoveSelected = false;
            this.AddSelectedTagCommand = new RelayCommand<object>(this.ExecuteAddSelectedTags);
            this.RefreshTagsCommand = new RelayCommand<object>(this.RefreshAvailableTagsInServer);
            this.RemoveSelectedCommand = new RelayCommand<object>(this.ExecuteRemoveSelected);

            this.SelectionChangedCommand = new RelayCommand<List<string>>(items => { this.SelectedTags = items; });

            this.RefreshAvailableTagsInServer(null);

            this.RefreshTagsInRule();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the add selected tags command.
        /// </summary>
        public ICommand AddSelectedTagCommand { get; set; }

        /// <summary>
        ///     The available tags.
        /// </summary>
        public ObservableCollection<string> AvailableTags { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can execute add selected tags.
        /// </summary>
        public bool CanExecuteAddSelectedTags { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can execute refresh tags.
        /// </summary>
        public bool CanExecuteRefreshTags { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether can execute remove selected.
        /// </summary>
        public bool CanExecuteRemoveSelected { get; set; }

        /// <summary>
        ///     Gets or sets the refresh tags command.
        /// </summary>
        public ICommand RefreshTagsCommand { get; set; }

        /// <summary>
        ///     Gets or sets the remove selected command.
        /// </summary>
        public ICommand RemoveSelectedCommand { get; set; }

        /// <summary>
        ///     Gets or sets the selected tag in rule.
        /// </summary>
        public string SelectedTagInRule
        {
            get
            {
                return this.selectedTagInRule;
            }

            set
            {
                this.selectedTagInRule = value;
                if (value == null)
                {
                    this.CanExecuteRemoveSelected = false;
                }
                else
                {
                    this.CanExecuteRemoveSelected = true;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the selected tag in server.
        /// </summary>
        public string SelectedTagInServer
        {
            get
            {
                return this.selectedTagInServer;
            }

            set
            {
                this.selectedTagInServer = value;
                if (value == null)
                {
                    this.CanExecuteAddSelectedTags = false;
                }
                else
                {
                    this.CanExecuteAddSelectedTags = true;
                }
            }
        }

        /// <summary>
        ///     The selected tags.
        /// </summary>
        public List<string> SelectedTags { get; set; }

        /// <summary>
        ///     Gets the selection changed command.
        /// </summary>
        public RelayCommand<List<string>> SelectionChangedCommand { get; private set; }

        /// <summary>
        ///     Gets or sets the tags in rule.
        /// </summary>
        public ObservableCollection<string> TagsInRule { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The refresh available tags in server.
        /// </summary>
        /// <param name="data">The data.</param>
        public void RefreshAvailableTagsInServer(object data)
        {
            var tags = this.service.GetAllTags(this.conf);
            tags.Sort();
            this.AvailableTags.Clear();

            foreach (var tag in tags)
            {
                this.AvailableTags.Add(tag);
            }
        }

        /// <summary>
        ///     The refresh tags in rule.
        /// </summary>
        public void RefreshTagsInRule()
        {
            if (this.model.SelectedRule == null)
            {
                return;
            }

            this.TagsInRule.Clear();
            var usorted = new List<string>();
            usorted.AddRange(this.model.SelectedRule.Tags);
            usorted.Sort();
            foreach (var tag in usorted)
            {
                this.TagsInRule.Add(tag);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The aggregate list.
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
        private string AggregateList(string arg1, string arg2)
        {
            return arg1 + "\r\n" + arg2;
        }

        /// <summary>
        ///     The execute add selected tags.
        /// </summary>
        private void ExecuteAddSelectedTags(object data)
        {
            if (this.SelectedTagInServer == null)
            {
                return;
            }

            var newList = new List<string>();

            foreach (var tag in this.TagsInRule)
            {
                if (tag.EndsWith(this.SelectedTagInServer))
                {
                    return;
                }

                newList.Add(tag);
            }

            newList.Add(this.SelectedTagInServer);

            this.SetTagsInRule(newList);
            this.RefreshTagsInRule();
        }

        /// <summary>
        ///     The execute remove selected.
        /// </summary>
        private void ExecuteRemoveSelected(object data)
        {
            if (this.SelectedTagInRule == null)
            {
                return;
            }

            var newList = new List<string>();

            foreach (var tag in this.TagsInRule)
            {
                if (tag == null || tag.Equals(this.SelectedTagInRule))
                {
                    continue;
                }

                newList.Add(tag);
            }

            this.SetTagsInRule(newList);
            this.model.SelectedRule.Tags.Clear();

            foreach (var tag in newList)
            {
                this.model.SelectedRule.Tags.Add(tag);
            }
            
            this.RefreshTagsInRule();
        }

        /// <summary>
        /// The set tags in rule.
        /// </summary>
        /// <param name="newList">
        /// The new list.
        /// </param>
        private void SetTagsInRule(List<string> newList)
        {
            var errorMessage = this.service.UpdateTags(this.conf, this.model.SelectedRule, newList);

            if (errorMessage.Count == 0)
            {
                this.model.SelectedRule.Tags.Add(this.SelectedTagInServer);
            }
            else
            {
                MessageBox.Show("Error: " + errorMessage.Aggregate(this.AggregateList));
            }
        }

        /// <summary>
        /// The update colors.
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
        /// Gets or sets the back ground color.
        /// </summary>
        public Color BackGroundColor { get; set; }

        /// <summary>
        /// Gets or sets the fore ground color.
        /// </summary>
        public Color ForeGroundColor { get; set; }

        #endregion
    }
}