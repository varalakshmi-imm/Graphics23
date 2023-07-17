// Geometry.cs - Contains some basic Geometry structs (Complex numbers, Points, Vectors)
// ---------------------------------------------------------------------------------------
namespace GrayBMP;
using static Math;

/// <summary>A number in the complex plane of the form (X + iY)</summary>
readonly struct Complex {
   public Complex (double x, double y) => (X, Y) = (x, y);
   public override string ToString () => $"{X} + i{Y}";

   public double Norm => Sqrt (X * X + Y * Y);
   public double NormSq => X * X + Y * Y;

   public static readonly Complex Zero = new (0, 0);

   public static Complex operator + (Complex a, Complex b)
      => new (a.X + b.X, a.Y + b.Y);
   public static Complex operator * (Complex a, Complex b)
      => new (a.X * b.X - a.Y * b.Y, a.X * b.Y + a.Y * b.X);

   public readonly double X, Y;
}

/// <summary>A point in 2D space, with double-precision coordinates (X, Y)</summary>
readonly record struct Point2 (double X, double Y) {
   public (int X, int Y) Round () => ((int)(X + 0.5), (int)(Y + 0.5));
   public Point2 RadialMove (double r, double theta) 
      => new (X + r * Cos (theta), Y + r * Sin (theta));

   /// <summary>Slope of line connecting from this Point2 to another Point2</summary>
   public readonly double AngleTo (Point2 b) {
      double dx = b.X - X, dy = b.Y - Y;
      if (dx != 0 || dy != 0) {
         double fAngle = Atan2 (dy, dx);
         return fAngle;
      }
      return 0;
   }
}

/// <summary>A Line in 2 dimensions (A -> B)</summary>
readonly record struct Line (Point2 A, Point2 B);

/// <summary>A drawing is a collection of lines</summary>
class Drawing {
   public void AddLine (Line line) => mLines.Add (line);

   public IReadOnlyList<Line> Lines => mLines;
   List<Line> mLines = new ();
}
