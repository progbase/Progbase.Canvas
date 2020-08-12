using System;
using System.Globalization;

namespace Progbase
{
    public class Canvas
    {
        #region Constants
        private const byte BlackColor = 0;
        private const string SetBackgroundColorFormat = "\x1b[48;5;{0}m";
        private const string SetForegroundColorFormat = "\x1b[38;5;{0}m";
        private const char UpperHalfBlock = '▀';
        #endregion

        #region Fields
        private byte[,] canvas = null;
        private byte currentColor = 0;
        private int originRow = 1;
        private int originColumn = 1;
        private int width = 1;
        private int height = 1;
        private bool isDrawing = false;
        private bool isYOrientationInverted = false;
        #endregion

        #region Constructors
        #endregion

        #region Methods
        public void SetOrigin(int conRow, int conColumn)
        {
            if (isDrawing) throw new InvalidOperationException("Can't set origin when drawing");

            if (conRow < 0) throw new ArgumentOutOfRangeException(nameof(conRow));
            if (conColumn < 0) throw new ArgumentOutOfRangeException(nameof(conColumn));
            originRow = conRow;
            originColumn = conColumn;
        }

        public void SetSize(int widthPixels, int heightPixels)
        {
            if (isDrawing) throw new InvalidOperationException("Can't set size when drawing");

            if (widthPixels < 1) throw new ArgumentOutOfRangeException(nameof(widthPixels));
            if (heightPixels < 1) throw new ArgumentOutOfRangeException(nameof(heightPixels));

            width = widthPixels;
            height = heightPixels;
            canvas = new byte[height, width];
        }

        public void InvertYOrientation()
        {
            if (isDrawing) throw new InvalidOperationException("Can't invert Y orientation when drawing");

            isYOrientationInverted = !isYOrientationInverted;
        }

        public void SetColor(byte value)
        {
            if (!isDrawing) throw new InvalidOperationException("Can set color only when drawing");

            currentColor = value;
        }

        public void SetColor(byte red, byte green, byte blue)
        {
            SetColor(EncodeColor(red, green, blue));
        }

        public void SetColor(double red, double green, double blue)
        {
            SetColor((byte)(red * 255), (byte)(green * 255), (byte)(blue * 255));
        }

        public void SetColor(string hexColor)
        {
            hexColor = hexColor.TrimStart('#');
            if (hexColor.Length == 6)
                SetColor(
                    byte.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber),
                    byte.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber),
                    byte.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber));
            else // assuming length of 8
                SetColor(
                    byte.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber),
                    byte.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber),
                    byte.Parse(hexColor.Substring(6, 2), NumberStyles.HexNumber));
        }

        public void BeginDraw()
        {
            if (isDrawing) throw new InvalidOperationException("Can't begin draw. Already drawing");

            Console.CursorVisible = false;

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    canvas[i, j] = BlackColor;
            currentColor = BlackColor;

            isDrawing = true;
        }

        public void EndDraw()
        {
            if (!isDrawing) throw new InvalidOperationException("Can't end draw. Not drawing");

            int rColorUpper = -1;
            int rColorLower = -1;

            bool setPosManually = originRow > 0 || originColumn > 0;

            int y;
            for (int yi = 0; yi < height / 2; yi++)
            {
                y = yi;
                if (isYOrientationInverted)
                {
                    y = (height / 2) - yi - 1;
                }

                if (setPosManually)
                {
                    Console.SetCursorPosition(originColumn, originRow + yi);
                }

                for (int x = 0; x < width; x++)
                {
                    int newCol_UP = GetColorAt(x, y * 2);
                    if (newCol_UP != rColorUpper)
                    {
                        Console.Write(isYOrientationInverted ? SetBackgroundColorFormat : SetForegroundColorFormat, newCol_UP);
                        rColorUpper = newCol_UP;
                    }

                    int newCol_DOWN = GetColorAt(x, y * 2 + 1);
                    if (newCol_DOWN != rColorLower)
                    {
                        Console.Write(isYOrientationInverted ? SetForegroundColorFormat : SetBackgroundColorFormat, newCol_DOWN);
                        rColorLower = newCol_DOWN;
                    }

                    Console.Write(UpperHalfBlock);
                }

                if (!setPosManually)
                {
                    Console.WriteLine();
                }
            }
            Console.ResetColor();
            Console.CursorVisible = true;

            isDrawing = false;
        }

        public void PutPixel(int x, int y)
        {
            if (!isDrawing) throw new InvalidOperationException("Not drawing");

            if (InBounds(x, 0, width - 1) && InBounds(y, 0, height - 1))
            {
                canvas[y, x] = currentColor;
            }
        }
        #endregion

        #region Extended Draw
        public void StrokeLine(int x0, int y0, int x1, int y1)
        {
            if (!isDrawing) throw new InvalidOperationException("Not drawing");

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x1 >= x0 ? 1 : -1;
            int sy = y1 >= y0 ? 1 : -1;

            PutPixel(x0, y0);

            if (dy <= dx)
            {
                int d = (dy << 1) - dx;
                int d1 = dy << 1;
                int d2 = (dy - dx) << 1;
                for (int x = x0 + sx, y = y0, i = 1; i <= dx; i++, x += sx)
                {
                    if (d > 0)
                    {
                        d += d2;
                        y += sy;
                    }
                    else
                    {
                        d += d1;
                    }
                    PutPixel(x, y);
                }
            }
            else
            {
                int d = (dx << 1) - dy;
                int d1 = dx << 1;
                int d2 = (dx - dy) << 1;
                for (int y = y0 + sy, x = x0, i = 1; i <= dy; i++, y += sy)
                {
                    if (d > 0)
                    {
                        d += d2;
                        x += sx;
                    }
                    else
                    {
                        d += d1;
                    }
                    PutPixel(x, y);
                }
            }
        }
        #endregion

        #region Extensions
        public void StrokeRect(int x, int y, int width, int height)
        {
            if (!isDrawing) throw new InvalidOperationException("Not drawing");

            for (int i = y; i < y + height; i++)
            {
                PutPixel(x, i);
                PutPixel(x + width - 1, i);
            }
            for (int j = x; j < x + width; j++)
            {
                PutPixel(j, y);
                PutPixel(j, y + height - 1);
            }
        }

        public void FillRect(int x, int y, int width, int height)
        {
            if (!isDrawing) throw new InvalidOperationException("Not drawing");

            for (int i = y; i < y + height; i++)
            {
                for (int j = x; j < x + width; j++)
                {
                    PutPixel(j, i);
                }
            }
        }

        public void StrokeCircle(int x0, int y0, int radius)
        {
            if (!isDrawing) throw new InvalidOperationException("Not drawing");

            StrokeEllipse(x0, y0, radius, radius);
        }

        public void FillCircle(int x0, int y0, int radius)
        {
            if (!isDrawing) throw new InvalidOperationException("Not drawing");

            if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius));

            FillEllipse(x0, y0, radius, radius);
        }

        public void StrokeEllipse(int cx, int cy, int a, int b)
        {
            if (!isDrawing) throw new InvalidOperationException("Not drawing");

            if (a <= 0) throw new ArgumentOutOfRangeException(nameof(a));
            if (b <= 0) throw new ArgumentOutOfRangeException(nameof(b));

            double len_ellipse = Math.PI * (a + b);
            double additional_accuracy = 1.5;
            double angle = 2 * Math.PI / (len_ellipse * additional_accuracy);
            for (double tmp_angle = 0; tmp_angle <= 2 * Math.PI; tmp_angle += angle)
            {
                double x = a * Math.Cos(tmp_angle);
                double y = b * Math.Sin(tmp_angle);
                PutPixel((int)(x + cx), (int)(y + cy));
            }
        }

        public void FillEllipse(int cx, int cy, int a, int b)
        {
            if (!isDrawing) throw new InvalidOperationException("Not drawing");

            if (a <= 0) throw new ArgumentOutOfRangeException(nameof(a));
            if (b <= 0) throw new ArgumentOutOfRangeException(nameof(b));

            for (float x = -a; x < a; x++)
            {
                for (float y = -b; y < b; y++)
                {
                    if (x * x / (a * a) + y * y / (b * b) <= 1)
                    {
                        PutPixel((int)(x + cx), (int)(y + cy));
                    }
                }
            }
        }
        #endregion

        #region Utilities
        private static byte EncodeColor(byte r, byte g, byte b)
        {
            r /= 51;
            g /= 51;
            b /= 51;

            return (byte)(16 + 36 * r + 6 * g + b);
        }

        private static bool InBounds(int x, int a, int b)
        {
            return x >= a && x <= b;
        }

        private byte GetColorAt(int x, int y)
        {
            return canvas[y, x];
        }
        #endregion
    }
}
