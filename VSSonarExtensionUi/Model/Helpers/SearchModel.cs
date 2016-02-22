

namespace VSSonarExtensionUi.Model.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using VSSonarExtensionUi.ViewModel.Helpers;

    /// <summary>
    /// issue search model, search data model
    /// </summary>
    public class SearchModel
    {
        /// <summary>
        /// The user data folder
        /// </summary>
        private readonly string userDataFolder;

        /// <summary>
        /// The data file
        /// </summary>
        private readonly string dataFile;

        /// <summary>
        /// The data from model
        /// </summary>
        private readonly ObservableCollection<string> dataFromModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchModel" /> class.
        /// </summary>
        /// <param name="dataFromModel">The data from model.</param>
        public SearchModel(ObservableCollection<string> dataFromModel)
        {
            this.dataFromModel = dataFromModel;
            this.userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "VSSonarExtension");
            this.dataFile = Path.Combine(this.userDataFolder, "saved-search.dat");

            if (!Directory.Exists(this.userDataFolder))
            {
                Directory.CreateDirectory(this.userDataFolder);
            }

            this.LoadSavedSearchs();
            this.SyncCollections();
        }

        /// <summary>
        /// Gets or sets the saved searchs.
        /// </summary>
        /// <value>
        /// The saved searchs.
        /// </value>
        private List<Search> SavedSearchs { get; set; }

        /// <summary>
        /// Saves the search to file.
        /// </summary>
        /// <param name="data">The data.</param>
        public void SaveSearch(Search data)
        {
            foreach (var item in this.SavedSearchs)
            {
                if (data.Name.Equals(item.Name))
                {
                    this.SavedSearchs.Remove(item);
                    break;
                }
            }

            this.SavedSearchs.Add(data);
            this.SyncSaveSearchToFile();
            this.SyncCollections();
        }

        /// <summary>
        /// Deletes the search.
        /// </summary>
        /// <param name="name">The name.</param>
        public void DeleteSearch(string name)
        {
            var data = this.GetSearchByName(name);

            foreach (var search in this.SavedSearchs)
            {
                if (data.Name.Equals(search.Name))
                {
                    this.SavedSearchs.Remove(search);
                    break;
                }
            }

            this.SyncSaveSearchToFile();
            this.SyncCollections();
        }

        /// <summary>
        /// Gets the name of the search by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>returns the search item</returns>
        public Search GetSearchByName(string name)
        {
            return this.SavedSearchs.Single(i => i.Name == name);
        }

        /// <summary>
        /// Synchronizes the collections.
        /// </summary>
        private void SyncCollections()
        {
            this.dataFromModel.Clear();
            foreach (var search in this.SavedSearchs)
            {
                this.dataFromModel.Add(search.Name);
            }
        }

        /// <summary>
        /// Loads the saved search.
        /// </summary>
        private void LoadSavedSearchs()
        {
            try
            {
                using (Stream stream = File.Open(this.dataFile, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    this.SavedSearchs = (List<Search>)bin.Deserialize(stream);
                }
            }
            catch (Exception)
            {

            }

            if (this.SavedSearchs == null)
            {
                this.SavedSearchs = new List<Search>();
            }
        }

        /// <summary>
        /// Synchronizes the save search to file.
        /// </summary>
        private void SyncSaveSearchToFile()
        {
            try
            {
                using (Stream stream = File.Open(this.dataFile, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, this.SavedSearchs);
                }
            }
            catch (IOException)
            {
            }
        }
    }
}
