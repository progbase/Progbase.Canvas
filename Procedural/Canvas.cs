using System;
using System.Runtime.InteropServices;

namespace Progbase.Procedural
{
    public static class Canvas
    {
        #region OS-specific
        private static class WindowsEnvironment
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool GetConsoleMode(IntPtr handle, out int mode);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetStdHandle(int handle);

            public static void Init()
            {
                IntPtr handle = GetStdHandle(-11);
                if (handle == IntPtr.Zero) throw new Exception("Can't get Windows std handle");
                if (!GetConsoleMode(handle, out int mode)) throw new Exception("Can't get Windows console mode");
                if (!SetConsoleMode(handle, mode | 0x4)) throw new Exception("Can't set Windows console mode");
            }
        }
        static Canvas()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsEnvironment.Init();
            }
        }
        #endregion

        private static Progbase.Canvas canvas = new Progbase.Canvas();

        #region API
        public static void SetOrigin(int conRow, int conColumn)
        {
            canvas.SetOrigin(conRow, conColumn);
        }

        public static void SetSize(int widthPixels, int heightPixels)
        {
            canvas.SetSize(widthPixels, heightPixels);
        }

        public static void InvertYOrientation()
        {
            canvas.InvertYOrientation();
        }

        public static void SetColor(byte value)
        {
            canvas.SetColor(value);
        }

        public static void SetColor(byte red, byte green, byte blue)
        {
            canvas.SetColor(red, green, blue);
        }

        public static void SetColor(double red, double green, double blue)
        {
            canvas.SetColor(red, green, blue);
        }

        public static void SetColor(string hexColor)
        {
            canvas.SetColor(hexColor);
        }

        public static void BeginDraw()
        {
            canvas.BeginDraw();
        }

        public static void EndDraw()
        {
            canvas.EndDraw();
        }

        public static void PutPixel(int x, int y)
        {
            canvas.PutPixel(x, y);
        }

        public static void StrokeLine(int x0, int y0, int x1, int y1)
        {
            canvas.StrokeLine(x0, y0, x1, y1);
        }

        public static void StrokeRect(int x, int y, int width, int height)
        {
            canvas.StrokeRect(x, y, width, height);
        }

        public static void FillRect(int x, int y, int width, int height)
        {
            canvas.FillRect(x, y, width, height);
        }

        public static void StrokeCircle(int x0, int y0, int radius)
        {
            canvas.StrokeCircle(x0, y0, radius);
        }

        public static void FillCircle(int x0, int y0, int radius)
        {
            canvas.FillCircle(x0, y0, radius);
        }

        public static void StrokeEllipse(int cx, int cy, int a, int b)
        {
            canvas.StrokeEllipse(cx, cy, a, b);
        }

        public static void FillEllipse(int cx, int cy, int a, int b)
        {
            canvas.FillEllipse(cx, cy, a, b);
        }
        #endregion
    }
}