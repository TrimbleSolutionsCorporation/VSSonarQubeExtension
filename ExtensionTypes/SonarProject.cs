namespace ExtensionTypes
{
    using System.Collections.Generic;

    using PropertyChanged;

    [ImplementPropertyChanged]
    public class SonarProject
    {
        public SonarProject()
        {
            this.Profiles = new List<Profile>();
        }

        public string Name { get; set; }

        public string Scope { get; set; }

        public string Key { get; set; }

        public string Qualifier { get; set; }

        public int Id { get; set; }

        public List<Profile> Profiles {get; set;}
    }
}
