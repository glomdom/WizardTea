using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace WizardTea.Visualizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
    }

    private void FileOpen_Click(object sender, RoutedEventArgs e) {
        OpenFileDialog ofd = new OpenFileDialog {
            Filter = "NIF Files (*.nif)|*.nif"
        };

        if (ofd.ShowDialog() == true) {
            // parse stuff
        }
    }

    private void FileExit_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}