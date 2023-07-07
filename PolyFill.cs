using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GrayBMP {
   class PolyFillWin : Window {
      public PolyFillWin (string file) {
         Width = 900; Height = 600;
         Left = 200; Top = 50;
         WindowStyle = WindowStyle.None;
         mBmp = new GrayBMP (Width, Height);

         Image image = new () {
            Stretch = Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Source = mBmp.Bitmap
         };
         RenderOptions.SetBitmapScalingMode (image, BitmapScalingMode.NearestNeighbor);
         RenderOptions.SetEdgeMode (image, EdgeMode.Aliased);
         Content = image;
         Stopwatch sw = Stopwatch.StartNew ();
         Fill (file);
         sw.Stop ();
         Console.WriteLine ($"Total time : {sw.Elapsed.TotalMilliseconds} ms");
      }
      readonly GrayBMP mBmp;

      void Fill (string file) {
         var edges = File.ReadAllLines (file)
                       .Select (a => a.Split (' ').Select (int.Parse).ToArray ()).ToList ();

         List<double> slopes = new ();
         for (int n = 0; n < edges.Count; n++) {
            var a = edges[n];
            var dy = a[3] - a[1];
            var dx = a[2] - a[0];

            var slope = 0.0;
            if (dy == 0) slope = 1.0;
            if (dx == 0) slope = 0.0;
            if ((dy != 0) && (dx != 0)) slope = (float)dx / dy;
            slopes.Add (slope);
         }

         int h = (int)Height;
         List<int> intersections = new ();
         for (int y = 0; y < h; y++) {
            intersections.Clear ();
            for (int i = 0; i < edges.Count; i++) {
               var a = edges[i];
               int v1 = a[1], v3 = a[3];
               if ((v1 <= y && v3 > y) || (v1 > y && v3 <= y))
                  intersections.Add ((int)(a[0] + slopes[i] * (y - v1)));
            }

            intersections = intersections.OrderBy (a => a).ToList ();
            if (intersections.Count % 2 != 0) throw new Exception ();
            var yLine = h - y; // Need this otherwise content will be rendered flipped
            for (int n = 0; n < intersections.Count; n += 2)
               mBmp.DrawLine (intersections[n], yLine, intersections[n + 1], yLine, 255);
         }
      }
   }
}