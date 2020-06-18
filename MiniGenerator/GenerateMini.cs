using MiniGenerator.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Encoder = System.Drawing.Imaging.Encoder;

namespace MiniGenerator
{
    public static class GenerateMini
    {
        public static void StartProcessing(string inputFile, string outputPath, ProcessingConfiguration configuration)
        {
            // Prepara caminho para salvar imagem
            string filename = Path.GetFileName(inputFile);
            string outputFilePath = outputPath + "\\" + filename;
            Directory.CreateDirectory(outputPath);

            // Abre imagem
            Bitmap image = new Bitmap(inputFile);

            // Gera mini
            int miniWidth = image.Width / configuration.ResizeFactor;
            int miniHeight = image.Height / configuration.ResizeFactor;

            Bitmap mini = new Bitmap(image, miniWidth, miniHeight);

            image.Dispose();

            if (configuration.BorderThickness != 0)
            {
                mini = drawBorder(mini, configuration.BorderThickness);
            }

            outputFilePath = Path.ChangeExtension(outputFilePath, "jpg");
            saveImage(outputFilePath, mini);

            mini.Dispose();
        }

        private static void saveImage(string path, Bitmap image, long quality = 90L)
        {
            using (EncoderParameters encoderParameters = new EncoderParameters(1))
            using (EncoderParameter encoderParameter = new EncoderParameter(Encoder.Quality, quality))
            {
                ImageCodecInfo codecInfo = ImageCodecInfo.GetImageDecoders().First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
                encoderParameters.Param[0] = encoderParameter;
                image.Save(path, codecInfo, encoderParameters);
            }
        }

        private unsafe static Bitmap drawBorder(Bitmap image, int thickness)
        {
            BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);

            byte bytesPerPixel = (byte)(Bitmap.GetPixelFormatSize(imageData.PixelFormat) / 8);

            // Ponteiro para o início do primeiro pixel da imagem
            byte* ptrFirstPixel = (byte*)imageData.Scan0.ToPointer();

            // Borda superior
            Parallel.For(0, thickness, y =>
            {
                Parallel.For(0, imageData.Width, x =>
                {
                    byte* data = ptrFirstPixel + y * imageData.Stride + x * bytesPerPixel;

                    // Pinta de preto
                    for (int i = 0; i < bytesPerPixel; i++)
                    {
                        data[i] = (byte)0;
                    }
                });
            });

            // Borda esquerda
            Parallel.For(0, imageData.Height, y =>
            {
                Parallel.For(0, thickness, x =>
                {
                    byte* data = ptrFirstPixel + y * imageData.Stride + x * bytesPerPixel;

                    // Pinta de preto
                    for (int i = 0; i < bytesPerPixel; i++)
                    {
                        data[i] = (byte)0;
                    }
                });
            });

            // Borda inferior
            Parallel.For(imageData.Height - thickness, imageData.Height, y =>
            {
                Parallel.For(0, imageData.Width, x =>
                {
                    byte* data = ptrFirstPixel + y * imageData.Stride + x * bytesPerPixel;

                    // Pinta de preto
                    for (int i = 0; i < bytesPerPixel; i++)
                    {
                        data[i] = (byte)0;
                    }
                });
            });

            // Borda direita
            Parallel.For(0, imageData.Height, y =>
            {
                Parallel.For(imageData.Width - thickness, imageData.Width, x =>
                {
                    byte* data = ptrFirstPixel + y * imageData.Stride + x * bytesPerPixel;

                    // Pinta de preto
                    for (int i = 0; i < bytesPerPixel; i++)
                    {
                        data[i] = (byte)0;
                    }
                });
            });

            image.UnlockBits(imageData);

            return image;
        }
    }
}
