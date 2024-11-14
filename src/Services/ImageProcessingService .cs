using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ChangeImage.Service
{
    internal class ImageProcessingService
    {
        internal async Task<WriteableBitmap> DivideImagePixelServiceAsync(BitmapImage image, double percents, string path)
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
        
        internal BitmapImage LoadImage(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memoryStream;
                        bitmap.EndInit();

                        return bitmap;
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null!;
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
