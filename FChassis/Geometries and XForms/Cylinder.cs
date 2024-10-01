using Flux.API;
using System.Windows.Threading;
using FChassis.Core;

namespace FChassis;

public class Cylinder {
   readonly double mDiameter = 10, mHeight = 40;
   readonly int mSegments = 100;
   public List<Point3> Points { get; set; } = [];
   public List<Geom.Triangle3D> Triangles { get; set; } = [];
   public Cylinder (double diameter, double height, int segments) {
      mDiameter = diameter; mHeight = height; mSegments = segments;
      double radius = mDiameter / 2.0;
      double angleStep = 2 * Math.PI / mSegments;
      double startZ = 0.0; // Starting X coordinate

      // Add points for the bottom base
      for (int i = 0; i <= mSegments; i++) {
         double angle = i * angleStep;
         Points.Add (new Point3 (radius * Math.Cos (angle), radius * Math.Sin (angle), startZ));
      }
      
      // Add points for the top base
      for (int i = 0; i < mSegments; i++) {
         double angle = i * angleStep;
         Points.Add (new Point3 (radius * Math.Cos (angle), radius * Math.Sin (angle), startZ + mHeight));
      }
      
      // Create triangles for the bottom base
      int bottomCenterIndex = Points.Count;
      Points.Add (new Point3 (0, 0, startZ)); // Center of the bottom base
      for (int i = 0; i < mSegments; i++) {
         Triangles.Add (new Geom.Triangle3D (bottomCenterIndex, i, (i + 1) % mSegments));
      }
      
      // Create triangles for the top base
      int topCenterIndex = Points.Count;
      Points.Add (new Point3 (0, 0, startZ + mHeight)); // Center of the top base
      for (int i = 0; i < mSegments; i++) {
         Triangles.Add (new Geom.Triangle3D (topCenterIndex, mSegments + ((i + 1) % mSegments), mSegments + i));
      }

      // Create triangles for the lateral surface
      for (int i = 0; i < mSegments; i++) {
         int nextIndex = (i + 1) % mSegments;
         Triangles.Add (new Geom.Triangle3D (i, mSegments + i, nextIndex));
         Triangles.Add (new Geom.Triangle3D (nextIndex, mSegments + i, mSegments + nextIndex));
      }
   }

   public void Draw (XForm4 LHCompTransform, Color32 LHToolColor, 
                     XForm4 RHCompTransform, Color32 RHToolColor) {
      foreach (var trg in Triangles) {
         var p1 = Points[trg.A]; var p2 = Points[trg.B];
         var p3 = Points[trg.C];

         if (LHCompTransform != null)
            _drawTriangle (LHCompTransform, LHToolColor);

         if (RHCompTransform != null)
            _drawTriangle (RHCompTransform, RHToolColor);

         void _drawTriangle(XForm4 compTransform, Color32 color) {
            var xFormP1 = Geom.V2P (compTransform * p1);
            var xFormP2 = Geom.V2P (compTransform * p2);
            var xFormP3 = Geom.V2P (compTransform * p3);
            AppUI.ThreadDispatcher.Invoke (() => {
               Lux.HLR = true;
               Lux.Color = color;
               Lux.Draw (EDraw.Triangle, [xFormP1, xFormP2, xFormP3]);
            });
         }
      }
   }
}