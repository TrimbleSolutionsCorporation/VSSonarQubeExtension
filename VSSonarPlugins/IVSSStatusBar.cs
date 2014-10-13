namespace VSSonarPlugins
{
    public interface IVSSStatusBar
    {
        void ShowIcons();

        void DisplayMessage(string message);

        void DisplayAndShowProgress(string message);

        void DisplayAndShowIcon(string message);
    }
}