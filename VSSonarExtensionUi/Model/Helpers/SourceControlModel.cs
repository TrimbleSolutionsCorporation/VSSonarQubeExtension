using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSonarPlugins;
using VSSonarPlugins.Types;

namespace VSSonarExtensionUi.Model.Helpers
{
    internal class SourceControlModel : ISourceControlProvider
    {
        private IEnumerable<ISourceVersionPlugin> plugins;

        public SourceControlModel()
        {
            this.plugins = new List<ISourceVersionPlugin>();
        }

        public SourceControlModel(IEnumerable<ISourceVersionPlugin> sourceControlPlugins)
        {
            this.plugins = sourceControlPlugins;
        }

        public string GetBranch(string basePath)
        {
            if (plugins == null)
            {
                return "";
            }

            var plugin = GetSupportedPlugin(basePath);
            if (plugin == null)
            {
                return "";
            }

            return plugin.GetBranch(basePath);
        }

        private ISourceVersionPlugin GetSupportedPlugin(string basePath)
        {
            foreach (var item in this.plugins)
            {
                if(item.IsSupported(basePath)) return item;
            }

            return null;
        }

        public IList<string> GetHistory(Resource item)
        {
            throw new NotImplementedException();
        }

        public void UpdatePlugins(IList<ISourceVersionPlugin> pluginsIn)
        {
            this.plugins = pluginsIn;
        }
    }
}
