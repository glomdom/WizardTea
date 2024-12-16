using System.IO;
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
using WizardTea.Core;

namespace WizardTea.Visualizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private bool isPanning = false;
    private Point lastMousePosition;
    private Thickness currentMargin = new(0, 0, 0,0);

    public MainWindow() {
        InitializeComponent();

        RootCanvas.MouseMove += Canvas_MouseMove;
        RootCanvas.MouseRightButtonDown += Canvas_MouseRightButtonDown;
        RootCanvas.MouseRightButtonUp += Canvas_MouseRightButtonUp;
    }
    
    private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
        isPanning = true;
        lastMousePosition = e.GetPosition(this);
        RootCanvas.CaptureMouse();
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e) {
        if (!isPanning && e.RightButton != MouseButtonState.Pressed) return;

        var currentPosition = e.GetPosition(this);
        var offsetX = currentPosition.X - lastMousePosition.X;
        var offsetY = currentPosition.Y - lastMousePosition.Y;

        currentMargin.Left += offsetX;
        currentMargin.Top += offsetY;

        NodeContainer.Margin = currentMargin;
        
        lastMousePosition = currentPosition;
    }
    
    private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
        isPanning = false;
        RootCanvas.ReleaseMouseCapture();
    }

    private void FileOpen_Click(object sender, RoutedEventArgs e) {
        var ofd = new OpenFileDialog {
            Filter = "NIF Files (*.nif)|*.nif"
        };

        if (ofd.ShowDialog() != true) return;
        var fileStream = File.OpenRead(ofd.FileName);
        var nifStream = new NifStream(fileStream);
        var file = new NifFile(nifStream);

        // do stuff
    }

    private void FileExit_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}