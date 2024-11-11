
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ChangeImage
{
    internal static class Services
    {
        public static async Task<WriteableBitmap> DivideImagePixelService(BitmapImage image, double percents)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(image);
            int totalPixels = writeableBitmap.PixelWidth * writeableBitmap.PixelHeight;
            int pixelsToDelete = (int)(totalPixels * percents / 100);

            Random rand = new Random();

            writeableBitmap.Lock();

            try
            {
                for (uint i = 0; i < pixelsToDelete; i++)
                {

                    int x = rand.Next(writeableBitmap.PixelWidth);
                    int y = rand.Next(writeableBitmap.PixelHeight);

                    IntPtr buffer = writeableBitmap.BackBuffer + y * writeableBitmap.BackBufferStride + x * 4;

                    int pixelValue = System.Runtime.InteropServices.Marshal.ReadInt32(buffer);

                    if ((pixelValue & 0xFF000000) == 0)
                    {
                        i--;
                        continue;
                    }

                    System.Runtime.InteropServices.Marshal.WriteInt32(buffer, 0x00000000);
                }
            }
            finally
            {
                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight));
                writeableBitmap.Unlock();
            }

            return writeableBitmap;
        }

        public static async Task SaveImageWithDialog(BitmapSource image)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|BMP Image|*.bmp",
                Title = "Save Image As",
                FileName = "MyImage"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                string extension = Path.GetExtension(filePath).ToLower();

                BitmapEncoder encoder;

                switch (extension)
                {
                    case ".jpg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    default:
                        encoder = new PngBitmapEncoder();
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(image));

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(stream);
                }

                MessageBox.Show("The file was created!", "Saving file", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
