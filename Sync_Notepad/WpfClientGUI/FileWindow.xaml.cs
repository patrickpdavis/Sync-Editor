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
using System.Windows.Shapes;

namespace WpfClientGUI
{
    
    /// <summary>
    /// Interaction logic for Filegroup.xaml
    /// </summary>
    public partial class FileWindow : Window
    {
        private MainWindow mainWindow;
        public FileWindow(List<string> files, MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            foreach(string item in files)
            {
                listBoxFiles.Items.Add(item);
            }
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.openFile(listBoxFiles.SelectedItem.ToString());
            this.Close();
        }
    }
}
