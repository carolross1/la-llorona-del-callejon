using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

class ChromaCleaner
{
    static void Main(string[] args)
    {
        string dir = @"Assets\Sprites\Environment\Nivel 3";
        if (args.Length > 0) dir = args[0];

        Console.WriteLine($"Procesando imagenes en {dir}...");
        string[] files = Directory.GetFiles(dir, "*.png");

        foreach (var file in files)
        {
            string name = Path.GetFileName(file);
            if (name.Equals("rejasTile.png", StringComparison.OrdinalIgnoreCase))
            {
                // rejasTile ya tiene transparencia nativa
                Console.WriteLine($"Saltando {name} (ya cuenta con canal alfa nativo)");
                continue;
            }

            CleanMagenta(file);
        }
        Console.WriteLine("¡Limpieza de fondo magenta completada!");
    }

    static void CleanMagenta(string filePath)
    {
        string name = Path.GetFileName(filePath);
        using (var srcBmp = new Bitmap(filePath))
        {
            int w = srcBmp.Width;
            int h = srcBmp.Height;

            using (var dstBmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                Rectangle rect = new Rectangle(0, 0, w, h);
                BitmapData srcData = srcBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData dstData = dstBmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                int bytes = Math.Abs(srcData.Stride) * h;
                byte[] rgbValues = new byte[bytes];
                byte[] dstValues = new byte[bytes];

                Marshal.Copy(srcData.Scan0, rgbValues, 0, bytes);

                int clearedPixels = 0;

                for (int i = 0; i < bytes; i += 4)
                {
                    byte b = rgbValues[i];
                    byte g = rgbValues[i + 1];
                    byte r = rgbValues[i + 2];
                    byte a = rgbValues[i + 3];

                    // Detección de fondo magenta (#FF00FF con variaciones de compresión)
                    // Fondo típico: R >= 170, B >= 170, G <= 60
                    // Evitar falsos positivos en sombras moradas (donde R y B son bajos, o G está balanceado)
                    bool isMagenta = (r >= 165 && b >= 165 && g <= 60 && Math.Abs(r - b) <= 65);

                    if (isMagenta)
                    {
                        dstValues[i] = 0;     // B
                        dstValues[i + 1] = 0; // G
                        dstValues[i + 2] = 0; // R
                        dstValues[i + 3] = 0; // A (transparente)
                        clearedPixels++;
                    }
                    else
                    {
                        // Despill suave para bordes que tienen derrame de magenta
                        // Si hay un tono magenta suave en el borde (g bajo y r,b dominantes)
                        if (r > 140 && b > 140 && g < 100 && (r - g) > 70 && (b - g) > 70)
                        {
                            // Atenuar el exceso de rojo y azul para neutralizar el halo magenta
                            byte avg = (byte)((r + b) / 2);
                            byte excess = (byte)(avg - Math.Max((byte)80, g));
                            byte newR = (byte)Math.Max(0, r - excess / 2);
                            byte newB = (byte)Math.Max(0, b - excess / 2);

                            dstValues[i] = newB;
                            dstValues[i + 1] = g;
                            dstValues[i + 2] = newR;
                            dstValues[i + 3] = a;
                        }
                        else
                        {
                            dstValues[i] = b;
                            dstValues[i + 1] = g;
                            dstValues[i + 2] = r;
                            dstValues[i + 3] = a;
                        }
                    }
                }

                Marshal.Copy(dstValues, 0, dstData.Scan0, bytes);

                srcBmp.UnlockBits(srcData);
                dstBmp.UnlockBits(dstData);

                // Guardar sobrescribiendo de forma segura
                string tempPath = filePath + ".tmp.png";
                dstBmp.Save(tempPath, ImageFormat.Png);
                srcBmp.Dispose();

                File.Delete(filePath);
                File.Move(tempPath, filePath);

                Console.WriteLine($"  {name}: {clearedPixels:N0} píxeles de fondo magenta limpiados.");
            }
        }
    }
}
