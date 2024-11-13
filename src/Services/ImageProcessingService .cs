using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ChangeImage.ImageProcessingService
{
    internal class ImageProcessingService
    {
        internal WriteableBitmap DivideImagePixelServiceAsync(BitmapImage image, double percents, string path)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(image);
            int totalPixels = writeableBitmap.PixelWidth * writeableBitmap.PixelHeight;
            int pixelsToDelete = (int)(totalPixels * percents / 100);

            List<(int x, int y)> pixels = new List<(int x, int y)>();

            for (int y = 0; y < writeableBitmap.PixelHeight; y++)
            {
                for (int x = 0; x < writeableBitmap.PixelWidth; x++)
                {
                    pixels.Add((x, y));
                }
            }

            Random rand = new Random();
            for (int i = pixels.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (pixels[i], pixels[j]) = (pixels[j], pixels[i]);
            }

            writeableBitmap = ChangePixelsToTransparent(bitmap: writeableBitmap, mixedPixels: pixels, pixelsToDelete);

            return writeableBitmap;
        }

        internal void SaveImageWithDialog(BitmapSource image)
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
        private WriteableBitmap ChangePixelsToTransparent(WriteableBitmap bitmap, List<(int, int)> mixedPixels, int pixelsToDelete)
        {
            bitmap.Lock();

            try
            {
                for (int i = 0; i < pixelsToDelete; i++)
                {
                    (int x, int y) = mixedPixels[i];
                    IntPtr buffer = bitmap.BackBuffer + y * bitmap.BackBufferStride + x * 4;
                    Marshal.WriteInt32(buffer, 0x00000000);
                }
            }
            finally
            {
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            }

            return bitmap;
        }
    }
}
