using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VSSonarExtensionUi.ViewModel.Configuration;

namespace VSSonarExtensionUi.View.Configuration
{
    /// <summary>
    /// Interaction logic for RoslynManagerView.xaml
    /// </summary>
    public partial class RoslynManagerView : UserControl
    {
        public RoslynManagerView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Register model to the view
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public RoslynManagerView(RoslynManagerViewModel controller)
        {
            this.InitializeComponent();
            this.DataContext = controller;
        }
    }
}
