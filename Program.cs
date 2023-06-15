using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static System.Math;

namespace A25;

class MyWindow : Window {
   public MyWindow () {
      Width = 800; Height = 600;
      Left = 50; Top = 50;
      WindowStyle = WindowStyle.None;
      Image image = new Image () {
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Top,
      };
      RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
      RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);

      mBmp = new WriteableBitmap ((int)Width, (int)Height,
         96, 96, PixelFormats.Gray8, null);
      mStride = mBmp.BackBufferStride;
      image.Source = mBmp;
      Content = image;

      MouseDown += OnMouseDown;
      DrawMandelbrot (-0.5, 0, 1);
      //// Horizontal and vertical lines
      DrawLine (10, 50, 10, 100); // x1 == x2 and y1 < y2
      DrawLine (100, 100, 10, 100); // y1 == y2 and x1 > x2
      DrawLine (100, 100, 100, 50); // x1 == x2 and y1 > y2
      DrawLine (100, 50, 10, 50); // y1 == y2 and x1 > x2
      // Slanting lines
      DrawLine (10, 75, 45, 100); // x1 < x2 and y1 < y2
      DrawLine (45, 100, 100, 75); // x1 < x2 and y1 > y2;
      DrawLine (100, 75, 45, 50); // x1 > x2 and y1 > y2;
      DrawLine (45, 50, 10, 75); // x1 > x2 and y1 < y2;
   }

   private void OnMouseDown (object sender, MouseButtonEventArgs e) {
      var pt = e.GetPosition (this);
      int x = (int)pt.X, y = (int)pt.Y;
      if (x1 == -1) { x1 = x; y1 = y; }
      else {
         int x2 = x, y2 = y;
         DrawLine (x1, y1, x2, y2);
         x1 = y1 = -1;
      }
   }
   int x1 = -1, y1 = -1;

   void DrawMandelbrot (double xc, double yc, double zoom) {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         int dx = mBmp.PixelWidth, dy = mBmp.PixelHeight;
         double step = 2.0 / dy / zoom;
         double x1 = xc - step * dx / 2, y1 = yc + step * dy / 2;
         for (int x = 0; x < dx; x++) {
            for (int y = 0; y < dy; y++) {
               Complex c = new Complex (x1 + x * step, y1 - y * step);
               SetPixel (x, y, Escape (c));
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, dx, dy));
      } finally {
         mBmp.Unlock ();
      }
   }

   // Draw line using Bresenham's line algorithm
   void DrawLine (int x1, int y1, int x2, int y2) {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         // Swap points
         if (x1 > x2) Swap ();
         int dx = x2 - x1, dy = y2 - y1;
         bool yOK = y1 <= y2;
         if (Abs (dy) < Abs (dx)) {
            dx = x2 - x1; dy = y2 - y1;
            int yStep = 1;
            if (dy < 0) { yStep = -1; dy = -dy; }
            int c1 = 2 * dy, c2 = 2 * (dy - dx);
            int decision = 2 * dy - dx;
            while (x1 <= x2) {
               SetPixel (x1, y1, 255);
               mBmp.AddDirtyRect (new Int32Rect (x1, y1, 1, 1));
               x1++;
               if (decision < 0) decision += c1;
               else { decision += c2; y1 += yStep; }
            }
         } else {
            if (!yOK) Swap ();
            dx = x2 - x1; dy = y2 - y1;
            int xStep = 1;
            if (dx < 0) { xStep = -1; dx = -dx; }
            (dx, dy) = (dy, dx);
            int c1 = 2 * dy, c2 = 2 * (dy - dx);
            int decision = 2 * dy - dx;
            while (y1 <= y2) {
               SetPixel (x1, y1, 255);
               mBmp.AddDirtyRect (new Int32Rect (x1, y1, 1, 1));
               y1++;
               if (decision < 0) decision += c1;
               else { decision += c2; x1 += xStep; }
            }
         }
      } finally {
         mBmp.Unlock ();
      }
      void Swap () { (x1, x2) = (x2, x1); (y1, y2) = (y2, y1); }
   }
   byte Escape (Complex c) {
      Complex z = Complex.Zero;
      for (int i = 1; i < 32; i++) {
         if (z.NormSq > 4) return (byte)(i * 8);
         z = z * z + c;
      }
      return 0;
   }

   void OnMouseMove (object sender, MouseEventArgs e) {
      if (e.LeftButton == MouseButtonState.Pressed) {
         try {
            mBmp.Lock ();
            mBase = mBmp.BackBuffer;
            var pt = e.GetPosition (this);
            int x = (int)pt.X, y = (int)pt.Y;
            SetPixel (x, y, 255);
            mBmp.AddDirtyRect (new Int32Rect (x, y, 1, 1));
         } finally {
            mBmp.Unlock ();
         }
      }
   }

   void DrawGraySquare () {
      try {
         mBmp.Lock ();
         mBase = mBmp.BackBuffer;
         for (int x = 0; x <= 255; x++) {
            for (int y = 0; y <= 255; y++) {
               SetPixel (x, y, (byte)x);
            }
         }
         mBmp.AddDirtyRect (new Int32Rect (0, 0, 256, 256));
      } finally {
         mBmp.Unlock ();
      }
   }

   void SetPixel (int x, int y, byte gray) {
      unsafe {
         var ptr = (byte*)(mBase + y * mStride + x);
         *ptr = gray;
      }
   }

   WriteableBitmap mBmp;
   int mStride;
   nint mBase;
}

internal class Program {
   [STAThread]
   static void Main (string[] args) {
      Window w = new MyWindow ();
      w.Show ();
      Application app = new Application ();
      app.Run ();
   }
}
