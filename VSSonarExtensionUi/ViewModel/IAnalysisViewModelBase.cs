namespace VSSonarExtensionUi.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Documents;

    using ExtensionTypes;

    using SonarRestService;

    using VSSonarPlugins;

    public interface IAnalysisViewModelBase
    {
        void RefreshDataForResource(Resource file, string documentInView);

        List<Issue> GetIssuesForResource(Resource file, string fileContent);

        void InitDataAssociation(Resource associatedProject, ISonarConfiguration userConnectionConfig, string workingDir);

        void UpdateServices(ISonarRestService restServiceIn, IVsEnvironmentHelper vsenvironmenthelperIn, IVSSStatusBar statusBar, IServiceProvider provider);

    }
}