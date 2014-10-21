namespace VSSonarExtensionUi.ViewModel.Configuration
{
    public interface IOptionsViewModelBase
    {
        void SaveAndClose();

        void Exit();

        void Apply();
    }
}