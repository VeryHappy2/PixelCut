using Microsoft.Win32;
using System.Drawing;
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

namespace ChangeImage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static BitmapImage _image;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;)|*.png;*.jpg;";

            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(openFileDialog.FileName);
                bitmap.EndInit();
                _image = bitmap;
                LoadedImage.Source = bitmap;
            }
        }

        private async void ValueSlider_ValueChanged(object slender, RoutedEventArgs e)
        {
            await Task.Delay(1200);
            double percents = ValueSlider.Value;

            if (_image == null)
                return;
             
            LoadedImage.Source = await Services.DivideImagePixelService(_image, percents);
            SliderValueText.Text = $"Value: {ValueSlider.Value}";
        }

        private async void SaveImage_Click(object slender, RoutedEventArgs e)
        {
            if (LoadedImage.Source == null) 
                return;

            BitmapSource bitmapSource = LoadedImage.Source as BitmapSource;

            await Services.SaveImageWithDialog(bitmapSource);
        }
    }
}