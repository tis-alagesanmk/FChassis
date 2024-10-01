using Flux.API;
using System.IO;
using static System.Math;
using System.ComponentModel;
using System;
namespace FChassis;

public class Workpiece : INotifyPropertyChanged {
   #region Interface ---------------------------------------------------------------------
   public Model3 Model => mModel;
   readonly Model3 mModel;

   public List<Tooling> Cuts => mCuts;
   List<Tooling> mCuts = [];

   public Workpiece (Model3 model, Part part) {
      mBound = (mModel = model).Bound;
      mNCFileName = Path.GetFileNameWithoutExtension (part.Info.FileName);
      mNCFilePath = Path.GetDirectoryName (part.Info.FileName);
   }

   Bound3 mBound;
   readonly string mNCFileName;

   public string NCFileName => mNCFileName;

   readonly string mNCFilePath;
   public string NCFilePath => mNCFilePath;
   public Bound3 Bound => mBound;

   public event PropertyChangedEventHandler PropertyChanged;
   protected virtual void OnPropertyChanged (string propertyName) 
      => PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));

   public bool SortingComplete => sortingComplete;
   bool sortingComplete = false;

   public bool HoleCutsComplete { get; set; } = false;
   public bool NotchCutsComplete { get; set; } = false;
   public bool MarkingsComplete { get; set; } = false;
   /// <summary>Align the model for processing</summary>
   public void Align () {
      // First, align the baseplane so that it is aligned with the XY plane
      Apply (Matrix3.Between (mModel.Baseplane.Xfm.ToCS (), CoordSystem.World));

      // The baseplane should have the maximum extent in X - if that is in Y instead,
      // rotate it 90 degrees about the Z axis. Model extrusion is now in the X direction
      var size = mModel.Baseplane.Bound.Size;
      if (size.Y > size.X) 
         Apply (Matrix3.Rotation (EAxis.Z, Geo.HalfPI));

      // If the flanges are protruding 'downward', then rotate the model by 180 
      // degrees about the X axis
      if (-mModel.Bound.ZMin < mModel.Bound.ZMax) 
         Apply (Matrix3.Rotation (EAxis.X, Geo.PI));

      // Now shift the origin:
      var (mbound, pbound) = (mModel.Bound, mModel.Baseplane.Bound);
      double dx = -mbound.XMin;        // Model stretches from X=0 to X=Len
      double dy = -mbound.Midpoint.Y;  // Model is centered in Y (Y=0 is the centerline)
      double dz = -pbound.ZMin;        // Bottom of the baseplane is at Z=0,
                                       // and two flanges are down in -Z territory
      Apply (Matrix3.Translation (dx, dy, dz));
      mBound = mModel.Bound;

      // Helper ...........................
      void Apply (Matrix3 xfm) {
         foreach (var ent in mModel.Entities)
            ent.Xform (xfm);
      }
   }

   public void DoAddHoles () {
      if (HoleCutsComplete) 
         return;

      int cutIndex = Cuts.Count + 1;
      foreach (var ep in mModel.Entities.OfType<E3Plane> ()) {
         foreach (var con in ep.Contours.Skip (1)) {
            var shape = con.Clone ().Cleanup (threshold: 1e-3);
            EType type = Classify (ep);

            // If the contour appears to be CCW w.r.t a negative X
            // with positive Y or vice versa after projection for YNeg plane
            // OR converse of the above consition
            // if YPos plane, shape has to be reversed
            if ((type == EType.Top && ep.Xfm.M11 * ep.Xfm.M22 < 0.0) 
               || (type == EType.YNeg && ep.Xfm.M11 * ep.Xfm.M23 < 0.0) 
               || (type == EType.YPos && ep.Xfm.M11 * ep.Xfm.M23 > 0.0)) {
               if (shape.Winding == EWinding.CCW) 
                  shape.Reverse ();
            } else if (shape.Winding == EWinding.CW) 
               shape.Reverse ();

            Tooling cut = new (this, ep, shape, EKind.Hole);
            Cuts.Add (cut);
            var name = $"Tooling-{cutIndex++} - {Utils.GetFlangeType (Cuts[^1])} - {Cuts[^1].Kind}";
            Cuts[^1].Name = name;
         }
      }

      foreach (var ef in mModel.Entities.OfType<E3Flex> ()) {
         var bound = new Bound2 (ef.Trims.Select (a => a.Bound));
         foreach (var con in ef.Trims) {
            var b2 = con.Bound;
            // [Alag:Review] need to use OR and combinue "continue"
            if (b2.XMin.EQ (bound.XMin, 0.01) || b2.XMax.EQ (bound.XMax, 0.01)) 
               continue;

            if (b2.YMin.EQ (bound.YMin, 0.01) || b2.YMax.EQ (bound.YMax, 0.01)) 
               continue;

            var shape = con.Clone ().Cleanup (threshold: 1e-3);
            if (shape.Winding == EWinding.CW) shape.Reverse ();
            Cuts.Add (new Tooling (this, ef, shape, EKind.Hole));
            Cuts[^1].Name = $"Tooling-{cutIndex++} - {Utils.GetFlangeType (Cuts[^1])} - {Cuts[^1].Kind}";
         }
      }

      // In the case of FlexHoles, since the segments happen
      // on E3Plane and E3Flex and the resultantlist of segments
      // are not owned by any one E3Entity, the segments' start points are projected
      // onto the plane away from E3Flex either by 45 deg or by -45 deg.
      // This is decided by the mid normal to the E3Flex.
      // The windiwng of the polygon on the projected plane is used to check if the
      // Traces of the tooling has to be reversed.
      foreach (var cut in Cuts) {
         var cutSegs = Cuts[^1].Segs.ToList ();
         bool yNegFlex = cutSegs.Any (cutSeg => cutSeg.Vec0.Normalized ().Y < -0.1);
         if (cut.Kind == EKind.Hole && Utils.GetFlangeType (cut) == Utils.EFlange.Flex) {
            Vector3 n = new (0.0, Math.Sqrt (2.0), Math.Sqrt (2.0));
            Point3 q = new (0.0, mBound.YMax - 10.0, mBound.ZMax + 10.0);
            if (yNegFlex) {
               n = new Vector3 (0.0, -Math.Sqrt (2.0), Math.Sqrt (2.0));
               q = new Point3 (0.0, mBound.YMin - 10.0, mBound.ZMax + 10.0);
            }

            if (Geom.GetToolingWinding (n, q, cutSegs) == Geom.ToolingWinding.CW) 
               cut.Reverse ();
         }
      }

      HoleCutsComplete = true;
      Dirty ();
   }

   public void DoTextMarking () {
      if (MarkingsComplete) 
         return;

      int cutIndex = Cuts.Count + 1;
      var bp = mModel.Baseplane;
      var xfm = bp.Xfm.GetInverse () * Matrix3.Translation (0, 0, Offset);
      var textPt = new Point2 (400.0, -12.5);
      if (MCSettings.It.MarkTextPosX.LieWithin (mModel.Bound.XMin, mModel.Bound.XMax))
         textPt = new Point2 (MCSettings.It.MarkTextPosX, MCSettings.It.MarkTextPosY);

      var e2t = new E2Text (MCSettings.It.MarkText, textPt, 25, "SIMPLEX", 0);
      foreach (var pline in e2t.Plines) {
         Pline p2 = pline.Xformed (xfm);
         Cuts.Add (new Tooling (this, mModel.Baseplane, p2, EKind.Mark));
         Cuts[^1].Name = $"Tooling-{cutIndex++} - {Utils.GetFlangeType (Cuts[^1])} - {Cuts[^1].Kind}";
      }

      MarkingsComplete = true;
      Dirty ();
   }

   public void DoSorting () {
      double clearance = 25;
      mCuts = [.. mCuts.OrderBy (a => a.Start.Pt.X)];
      for (int i = 0; i < mCuts.Count; i++) 
         mCuts[i].SeqNo = i;

      var box = mBound;
      for (int i = 1; i < mCuts.Count; i++) {
         Tooling prevTooling = mCuts[i - 1], currTooling = mCuts[i];
         var pts = prevTooling.PostRoute; pts.Clear ();
         Point3 prevToolingEndPlusClearance = prevTooling.End.Lift (clearance),
                currToolingStartPlusClearance = currTooling.Start.Lift (clearance);
         double xmid = (prevToolingEndPlusClearance.X + currToolingStartPlusClearance.X) / 2,
                ymin = box.YMin - clearance, 
                ymax = box.YMax + clearance, 
                zmax = box.ZMax + clearance;
         pts.Add (prevTooling.End); pts.Add (new (prevToolingEndPlusClearance, prevTooling.End.Vec));

         Flux.API.Vector3 vecN = new Flux.API.Vector3 (0, -1, 1).Normalized (),
                vecP = new Flux.API.Vector3 (0, 1, 1).Normalized ();
         switch ((Classify (prevTooling.End.Vec), Classify (currTooling.End.Vec))) {
            case (EType.Top, EType.YNeg):
            case (EType.YNeg, EType.Top):
               pts.Add (new (new (xmid, ymin, zmax), vecN));
               break;

            case (EType.Top, EType.YPos):
            case (EType.YPos, EType.Top):
               pts.Add (new (new (xmid, ymax, zmax), vecP));
               break;

            case (EType.YNeg, EType.YPos):
               pts.Add (new (new (xmid, ymin, zmax), vecN));
               pts.Add (new (new (xmid, ymax, zmax), vecP));
               break;

            case (EType.YPos, EType.YNeg):
               pts.Add (new (new (xmid, ymax, zmax), vecP));
               pts.Add (new (new (xmid, ymin, zmax), vecN));
               break;
         }

         pts.Add (new (currToolingStartPlusClearance, currTooling.Start.Vec)); 
         pts.Add (currTooling.Start);
      }

      sortingComplete = true;
   }

   public void DoCutNotchesAndCutouts () {
      if (NotchCutsComplete) 
         return;

      int cutIndex = Cuts.Count + 1;
      var mb = mBound;
      List<Tooling> cuts = [];

      // The notches in the planes
      foreach (var ep in mModel.Entities.OfType<E3Plane> ()) {
         // First, compute the 'full rectangle bound' of this plane, and any segments
         // of this plane contour not lying on that bound need to be cut out
         var pb = ep.Bound;
         (Point3 p1, Point3 p2) = Classify (ep) switch {
            EType.Top => (new Point3 (mb.XMin, pb.YMin, mb.YMax), 
                          new Point3 (mb.XMax, pb.YMax, mb.YMax)),
                    _ => (new Point3 (mb.XMin, pb.YMin, pb.ZMin), 
                          new Point3 (mb.XMax, pb.YMin, pb.ZMax)),
         };

         (Point2 p3, Point2 p4) = (Unproject (p1), Unproject (p2));
         Bound2 rect = new (p3, p4);
         foreach (var notch in GetNotches (rect, ep.Contours[0])) {
            cuts.Add (new Tooling (this, ep, notch, EKind.Notch));
         }

         // Helper ...............................
         Point2 Unproject (Point3 pt) {
            var pt2 = pt * ep.Xfm.GetInverse ();
            return new (pt2.X, pt2.Y);
         }
      }

      foreach (var ef in mModel.Entities.OfType<E3Flex> ()) {
         // First compute the 'full rectangle bound' of this flex
         Point2 pt2 = ef.Trims.First ().P1; 
         Point3 pt3 = ef.Project (pt2).Pt;
         double dx = pt2.X - pt3.X;
         var rect = new Bound2 (ef.Trims.Select (a => a.Bound));
         rect = new Bound2 (mBound.XMin + dx, rect.YMin, mBound.XMax + dx, rect.YMax);
         foreach (var trim in ef.Trims)
            foreach (var notch in GetNotches (rect, trim))
               cuts.Add (new Tooling (this, ef, notch, EKind.Notch));
      }

      // Connect the notch toolings that are close to each other
      bool done = false;
      Tooling t0 = null, t1 = null;
      while (!done) {
         done = true;
         for (int i = 0; i < cuts.Count - 1; i++) {
            for (int j = i + 1; j < cuts.Count; j++) {
               t0 = cuts[i]; t1 = cuts[j];
               if (t0 == null) 
                  break;

               if (t1 == null) 
                  continue;

               Tooling tm = t0.JoinTo (t1, Tooling.mNotchJoinableLengthToClose)
                              ?? t1.JoinTo (t0, Tooling.mNotchJoinableLengthToClose);
               if (tm != null) {
                  cuts.Add (tm); cuts[i] = cuts[j] = null; 
                  done = false;
               }
            }

            if (t0 == null) 
               continue;
         }
      }

      foreach (var cut in cuts) {
         if (cut != null) {
            cut.IdentifyCutout ();
            cut.Name = $"Tooling-{cutIndex++} - {Utils.GetFlangeType (cut)} - {cut.Kind}";
            var cutSegs = cut.Segs.ToList ();
            bool YNegPlaneNotch = cutSegs.Any (cutSeg => Math.Abs (cutSeg.Vec0.Normalized ().Y + 1.0).EQ (0));
            bool YPosPlaneNotch = cutSegs.Any (cutSeg => Math.Abs (cutSeg.Vec0.Normalized ().Y - 1.0).EQ (0));
            bool TopPlaneNotch = cutSegs.Any (cutSeg => Math.Abs (cutSeg.Vec0.Normalized ().Z - 1.0).EQ (0));
            bool FlexPlaneNotch = !YNegPlaneNotch && !YPosPlaneNotch && !TopPlaneNotch;
            if (cut.Kind == EKind.Cutout) {
               // In the case of Cutouts, ( closed notches ) since the segments happen on 
               // E3Plane and E3Flex and the resultant list of segments are not owned by any one
               // E3Entity, the segments' start point are projected onto the plane away from E3Flex
               // in 45 deg or -45 deg. The windiwng of the polygon on the projected plane is used
               // to check if the Traces of the tooling has to be reversed.
               Vector3 n = Utils.GetEPlaneNormal (cut);
               Point3 q = new (0.0, mBound.YMax + 10.0, mBound.ZMax + 10.0);
               bool yNegFlexFeat = cutSegs.Any (cutSeg => cutSeg.Vec0.Normalized ().Y < -0.1);
               if (yNegFlexFeat) 
                  q = new Point3 (0.0, mBound.YMin - 10.0, mBound.ZMax + 10.0);
               if (Geom.GetToolingWinding (n, q, cutSegs) == Geom.ToolingWinding.CW) 
                  cut.Reverse ();
            } else {
               if (TopPlaneNotch && YNegPlaneNotch) 
                  cut.NotchKind = ENotchKind.TopToYNeg;
               else if (TopPlaneNotch && YPosPlaneNotch) 
                  cut.NotchKind = ENotchKind.TopToYPos;
               else if (TopPlaneNotch && YPosPlaneNotch && YNegPlaneNotch) 
                  cut.NotchKind = ENotchKind.YNegToYPos;
               else if (TopPlaneNotch) 
                  cut.NotchKind = ENotchKind.Top;
               else if (YNegPlaneNotch) 
                  cut.NotchKind = ENotchKind.YNeg;
               else if (YPosPlaneNotch) 
                  cut.NotchKind = ENotchKind.YPos;
               else if (FlexPlaneNotch) 
                  cut.NotchKind = ENotchKind.Flex;
               else {
                  // Unsupported type
                  throw new Exception ("Unsupported Notch Type");
               }
            }

            mCuts.Add (cut);
         }
      }

      var segs = mCuts[1].Segs.ToList ();
      int count = segs.Count;
      NotchCutsComplete = true;
      Dirty ();
   }
   #endregion

   #region Implementation ----------------------------------------------------------------
   void Dirty () {
      foreach (var cut in mCuts) {
         cut.PostRoute.Clear ();
         cut.SeqNo = -1;
      }
   }

   public static EType Classify (Flux.API.Vector3 vec) {
      if (Abs (vec.Z) > Abs (vec.Y)) 
         return EType.Top;

      return vec.Y < 0 ? EType.YNeg : EType.YPos;
   }

   public static EType Classify (E3Plane ep) {
      var vec = ep.ThickVector.Normalized ();
      if (Abs (vec.Z).EQ (1)) 
         return EType.Top;

      if (Abs (vec.Y).EQ (1)) {
         Point3 pt = Point2.Zero * ep.Xfm;
         return (pt.Y < -10) ? EType.YNeg : EType.YPos;
      }

      throw new Exception ("Unsupported plane");
   }

   public enum EType { Top, YNeg, YPos };
   const double Offset = 5;

   static List<Pline> GetNotches (Bound2 b, Pline p) {
      List<Pline> output = [];
      const double E = 0.01;
      foreach (var seg in p.Segs) {
         if (seg.IsCurved) { 
            output.Add (seg.ToPline ()); 
            continue; 
         }

         //[Alag:Review] need to use OR and combine "continue"
         if (seg.A.Y.EQ (b.YMin, E) && seg.B.Y.EQ (b.YMin, E)) // Bottom edge
            continue;      

         if (seg.A.Y.EQ (b.YMax, E) && seg.B.Y.EQ (b.YMax, E)) // Top edge
            continue;      

         if (seg.A.X.EQ (b.XMin, E) && seg.B.X.EQ (b.XMin, E)) // Left edge
            continue;      

         if (seg.A.X.EQ (b.XMax, E) && seg.B.X.EQ (b.XMax, E)) // Right edge
            continue;      

         output.Add (seg.ToPline ());
      }

      for (int i = output.Count - 1; i >= 0; i--) {
         Pline one = output[(i + output.Count - 1) % output.Count], 
               two = output[i];

         if (one.P2.EQ (two.P1)) { 
            one.Append (two); 
            output.RemoveAt (i); 
         }
      }

      return output;
   }
   #endregion
}