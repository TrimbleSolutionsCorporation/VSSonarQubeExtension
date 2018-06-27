namespace CxxPlugin.Options
{
    using System.Windows.Forms;

    using global::CxxPlugin.Commands;

    /// <summary>
    ///     The io service.
    /// </summary>
    public class CxxService : ICxxIoService
    {
        #region Public Methods and Operators

        /// <summary>The open file dialog.</summary>
        /// <param name="filter">The filter.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string OpenFileDialog(string filter)
        {
            var openFileDialog = new OpenFileDialog { Filter = filter };
            return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : string.Empty;
        }

        #endregion
    }
}