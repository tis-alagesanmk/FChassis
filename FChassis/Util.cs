using FChassis.GCodeGen;
using Flux.API;
using System.IO;
using static System.Math;
namespace FChassis;

using NotchAttribute = Tuple<
        Curve3, // Split curve, whose end point is the notch point
        Vector3, // Start Normal
        Vector3, // End Normal
        Vector3, // Outward Normal along flange
        Vector3, // Vector Outward to nearest boundary
        XForm4.EAxis, // Proximal boundary direction
        bool // Some boolean value
    >;
using ToolingSegment = ValueTuple<Curve3, Vector3, Vector3>;

internal static class Extensions {
   public static double LieOn (this double f, double a, double b) => (f - a) / (b - a);
   public static bool EQ (this double a, double b) => Abs (a - b) < 1e-6;
   public static bool EQ (this double a, double b, double err) => Abs (a - b) < err;
   public static bool EQ (this float a, float b) => Abs (a - b) < 1e-6;
   public static bool EQ (this float a, float b, double err) => Abs (a - b) < err;
   public static double D2R (this int a) => a * PI / 180;

   public static bool LieWithin (this double val, double leftLimit, 
                                 double rightLimit, double epsilon = 1e-6)
      => (leftLimit - epsilon < val && val < rightLimit + epsilon);
}

public class NegZException : Exception {
   // Parameterless constructor
   public NegZException () {}

   // Constructor with a message
   public NegZException (string message)
       : base (message) {}

   // Constructor with a message and an inner exception
   public NegZException (string message, Exception innerException)
       : base (message, innerException) {}
}


public enum EGCode {
   G0, G1, G2, G3, None
}
public enum EMove {
   Retract,
   Retract2SafeZ,
   SafeZ2SafeZ,
   SafeZ2Retract,
   Retract2Machining,
   Machining,
   RapidPosition
}

/// <summary>
/// Represents the drawable information of a G-Code segment, 
/// which is used for simulation purposes.
/// </summary>
/// <remarks>
/// The <see cref="GCodeGenerator"/> class populates a <see 
/// cref="List{GCodeDrawableSegment}"/> for each tool head.
/// This list can also be populated by parsing the G-Code directly.
/// </remarks>
public class GCodeSeg {
   EGCode mGCode;
   public EGCode GCode => mGCode;
   public GCodeSeg (Point3 stPoint, Point3 endPoint, Vector3 StNormal, Vector3 EndNormal,
                    EGCode gcmd, EMove moveType, string toolingName) 
      => SetGCLine (stPoint, endPoint, StNormal, EndNormal, gcmd, moveType, toolingName);
   public GCodeSeg (Arc3 arc, Point3 stPoint, Point3 endPoint, Point3 center, double radius,
                    Vector3 StNormal, EGCode gcmd, EMove moveType, string toolingName) 
      => SetGCArc (arc, stPoint, endPoint, center, radius, StNormal, gcmd, moveType, toolingName);
   public GCodeSeg (GCodeSeg rhs) {
      mStartPoint = rhs.mStartPoint; 
      mEndPoint = rhs.mEndPoint; 
      mRadius = rhs.mRadius;
      mCenter = rhs.mCenter; 
      mStartNormal = rhs.mStartNormal; 
      mEndNormal = rhs.mEndNormal;
      mMoveType = rhs.mMoveType; 
      mArc = rhs.mArc; 
      mToolingName = rhs.mToolingName; 
      mGCode = rhs.mGCode;
   }

   Point3 mStartPoint, mEndPoint, mCenter;
   Vector3 mStartNormal, mEndNormal;
   double mRadius;
   Arc3 mArc;
   public Arc3 Arc => mArc;
   EMove mMoveType;
   public EMove MoveType => mMoveType;
   string mToolingName;
   public string ToolingName { get => mToolingName; }
   public Point3 StartPoint => mStartPoint;
   public Point3 EndPoint => mEndPoint;
   public Point3 ArcCenter => mCenter;
   public double Radius => mRadius;
   public Vector3 StartNormal => mStartNormal;
   public Vector3 EndNormal => mEndNormal;

   public double Length {
      get {
         if (mGCode is EGCode.G0 or EGCode.G1) 
            return StartPoint.DistTo (EndPoint);
         else if (mGCode is EGCode.G2 or EGCode.G3) 
            return mArc.Length;

         throw new NotSupportedException ("Unknown G Entity while computing length");
      }
   }

   public void SetGCLine (Point3 stPoint, Point3 endPoint, Vector3 stNormal, 
                          Vector3 endNormal, EGCode gcmd, 
                          EMove moveType, string toolingName) {
      if (gcmd is not EGCode.G0 and not EGCode.G1)
         throw new InvalidDataException ("The GCode cmd for line is wrong");

      mStartPoint = stPoint; mEndPoint = endPoint; mGCode = gcmd;
      mStartNormal = stNormal; mEndNormal = endNormal;
      mMoveType = moveType;
      mToolingName = toolingName;
   }

   public void SetGCArc (Arc3 arc, Point3 stPoint, Point3 endPoint, 
                         Point3 center, double radius, Vector3 stNormal, 
                         EGCode gcmd, EMove moveType, string toolingName) {
      if (gcmd is not EGCode.G2 and not EGCode.G3)
         throw new InvalidDataException ("The GCode cmd for Arc is wrong");

      mStartPoint = stPoint; 
      mEndPoint = endPoint; 
      mCenter = center; 
      mRadius = radius; mGCode = gcmd;
      mStartNormal = stNormal;
      mArc = arc;
      mMoveType = moveType;
      mToolingName = toolingName;
   }

   public void XfmToMachine (GCodeGenerator codeGen) {
      mStartPoint = GCodeGenerator.XfmToMachine (codeGen, mStartPoint);
      mEndPoint = GCodeGenerator.XfmToMachine (codeGen, mEndPoint);
      mStartNormal = GCodeGenerator.XfmToMachineVec (codeGen, mStartNormal);
      mEndNormal = GCodeGenerator.XfmToMachineVec (codeGen, mEndNormal);
      if (IsArc ()) 
         mCenter = GCodeGenerator.XfmToMachine (codeGen, mCenter);
   }

   public GCodeSeg XfmToMachineNew (GCodeGenerator codeGGen) {
      GCodeSeg seg = new (this);
      seg.XfmToMachine (codeGGen);
      return seg;
   }
   public bool IsLine () => mGCode is EGCode.G0 or EGCode.G1;
   public bool IsArc () => mGCode is EGCode.G2 or EGCode.G3;
}
public static class Utils {
   public enum EPlane {
      /// <summary>YNeg is the 'front' plane, where Y less than 0</summary>
      YNeg,
      /// <summary>YPos is the 'back' plane, where Y greater than 0</summary>
      YPos,
      /// <summary>Top is the horizontal plane</summary>
      Top,
      Flex,
      None,
   }
   public enum EFlange {
      Bottom,
      Top,
      Web,
      Flex
   }
   public enum EArcSense {
      CW, CCW
   }

   public const double EpsilonVal = 1e-6;

   /// <summary>
   /// This method computes the angle between two vectors about X axis.
   /// The angle is made negative if the cross product of the input vectors
   /// oppose the global X axis.
   /// </summary>
   /// <param name="fromPointPV">The from vector</param>
   /// <param name="toPointPV">The to vector</param>
   /// <returns>The angle between the two vectors specified</returns>
   public static double GetAngleAboutXAxis (Vector3 fromPointPV, Vector3 toPointPV) {
      var theta = fromPointPV.AngleTo (toPointPV);
      var crossVec = Geom.Cross (fromPointPV, toPointPV);
      if (!crossVec.IsZero && crossVec.Opposing (XForm4.mXAxis)) 
         theta *= -1;

      return theta;
   }

   /// <summary>
   /// This method returns the angle between the global Z axis and the normal 
   /// to either of the planes Top, or YPos, or YNeg. 
   /// </summary>
   /// <param name="planeType">One of the EPlane types.</param>
   /// <returns>EPlane Top returns 0.0, EPlane YPos returns -pi/2
   /// and EPlane YNeg returns pi/2 radians</returns>
   /// <exception cref="NotSupportedException">If EPlane is other than YPos, or YNeg or
   /// Top, a NotSupportedException is thrown</exception>
   public static double GetAngle4PlaneTypeAboutXAxis (EPlane planeType) {
      double angleBetweenStartAndEndPoints;
      if (planeType == Utils.EPlane.Top) 
         angleBetweenStartAndEndPoints = 0.0;
      else if (planeType == Utils.EPlane.YPos) 
         angleBetweenStartAndEndPoints = -Math.PI / 2.0;
      else if (planeType == Utils.EPlane.YNeg) 
         angleBetweenStartAndEndPoints = Math.PI / 2.0;
      else 
         throw new NotSupportedException ("Unsupported plane type encountered");

      return angleBetweenStartAndEndPoints;
   }

   /// <summary>
   /// This method returns the plane type on which the arc is described.
   /// </summary>
   /// <param name="vec">The vector normal to the arc from the tooling.</param>
   /// <caveat>The vector normal should not be any vector other than the one
   /// obtained from the tooling</caveat>
   /// <returns>Returns one of Eplane.Top, Eplane.YPos, Eplane.YNeg or
   /// Eplane.Top.None</returns>
   public static EPlane GetArcPlaneType (Vector3 vec) {
      Workpiece.EType eType = Workpiece.Classify (vec);
      Utils.EPlane ePlane = eType switch {
         Workpiece.EType.YPos => Utils.EPlane.YPos,
         Workpiece.EType.YNeg => Utils.EPlane.YNeg,
         Workpiece.EType.Top  => Utils.EPlane.Top,
                            _ => EPlane.None
      };

      return ePlane;

      //   == Workpiece.EType.Top) return Utils.EPlane.Top;
      //if (Workpiece.Classify (vec) == Workpiece.EType.YPos) return Utils.EPlane.YPos;
      //if (Workpiece.Classify (vec) == Workpiece.EType.YNeg) return Utils.EPlane.YNeg;

      //if (Workpiece.Classify (vec) == Workpiece.EType.Top) return Utils.EPlane.Top;
      //if (Workpiece.Classify (vec) == Workpiece.EType.YPos) return Utils.EPlane.YPos;
      //if (Workpiece.Classify (vec) == Workpiece.EType.YNeg) return Utils.EPlane.YNeg;
      //return EPlane.None;
   }

   /// <summary>
   /// Given a normal vector, this method finds the flange type.
   /// </summary>
   /// <param name="vec"></param>
   /// <returns></returns>
   /// <exception cref="NotSupportedException"></exception>
   public static Utils.EFlange GetArcPlaneFlangeType (Vector3 vec) {
      // [Alag:Review] need to use "as switch"
      if (Workpiece.Classify (vec) == Workpiece.EType.Top) 
         return Utils.EFlange.Web;
      if (Workpiece.Classify (vec) == Workpiece.EType.YPos) 
         return Utils.EFlange.Top;
      if (Workpiece.Classify (vec) == Workpiece.EType.YNeg) 
         return Utils.EFlange.Bottom;

      throw new NotSupportedException (" Flange type could not be assessed");
   }

   /// <summary>
   /// This method is a handy one to project a 3d point onto XY or XZ plane
   /// </summary>
   /// <param name="pt">The 3d point</param>
   /// <param name="ep">One of the E3Planes, YNeg, YPos or Top planes</param>
   /// <returns>The 2d point, which is the projection of 3d point on the plane</returns>
   public static Point2 ToPlane (Point3 pt, EPlane ep) => ep switch {
      EPlane.YNeg => new Point2 (pt.X, pt.Z),
      EPlane.YPos => new Point2 (pt.X, pt.Z),
                _ => new Point2 (pt.X, pt.Y),
   };

   /// <summary>
   /// This method returns the EFlange type for the tooling. 
   /// </summary>
   /// <param name="toolingItem">The tooling</param>
   /// <returns>The EFlange type for the tooling, considering if the workpiece
   /// is LH or RH component, set.</returns>
   /// <exception cref="NotSupportedException">This exception is thrown if the plane type 
   /// could not be deciphered</exception>
   public static EFlange GetFlangeType (Tooling toolingItem) {
      if (toolingItem.IsPlaneFeature ()) {
         // [Alag:Review] need to use "as switch"
         if (Workpiece.Classify (toolingItem.Start.Vec) == Workpiece.EType.Top)
            return EFlange.Web;

         // TODO IMPORTANT clarify with Dinesh top or bottom
         if (Workpiece.Classify (toolingItem.Start.Vec) == Workpiece.EType.YPos)
            return MCSettings.It.PartConfig == MCSettings.PartConfigType.LHComponent 
               ? EFlange.Top : EFlange.Bottom;

         if (Workpiece.Classify (toolingItem.Start.Vec) == Workpiece.EType.YNeg)
            return MCSettings.It.PartConfig == MCSettings.PartConfigType.LHComponent 
               ? EFlange.Bottom : EFlange.Top;
      }

      if (toolingItem.IsFlexFeature ()) 
         return Utils.EFlange.Flex;

      throw new NotSupportedException (" Flange type could not be assessed");
   }

   /// <summary>
   /// THis methos returns the plane type given the tooling
   /// </summary>
   /// <param name="toolingItem">The input tooling item.</param>
   /// <returns>Returns one of the EPlane types such as EPlane.Top, YPos or YNeg</returns>
   /// <exception cref="InvalidOperationException">If the plane type could not be deciphered.</exception>
   public static EPlane GetPlaneType (Tooling toolingItem) {
      if (toolingItem.IsPlaneFeature ()) {
         // [Alag:Review] need to use "as switch"
         if (Workpiece.Classify (toolingItem.Start.Vec) == Workpiece.EType.Top)
            return EPlane.Top;

         if (Workpiece.Classify (toolingItem.Start.Vec) == Workpiece.EType.YPos)
            return EPlane.YPos;

         if (Workpiece.Classify (toolingItem.Start.Vec) == Workpiece.EType.YNeg)
            return EPlane.YNeg;
      }

      if (toolingItem.IsFlexFeature ()) 
         return Utils.EPlane.Flex;

      throw new InvalidOperationException (" The feature is neither plane nor flex");
   }

   /// <summary>
   /// This method returns the Vector3 normal emanating from the E3Plane
   /// given the tooling.
   /// </summary>
   /// <param name="toolingItem">The input tooling</param>
   /// <returns>The normal to the plane in the direction emanating 
   /// from the plane</returns>
   /// <exception cref="InvalidOperationException"></exception>
   public static Vector3 GetEPlaneNormal (Tooling toolingItem) {
      if (toolingItem.IsPlaneFeature ()) {
         // [Alag:Review] need to use "as switch"
         if (Workpiece.Classify (toolingItem.Start.Vec) == Workpiece.EType.Top)
            return XForm4.mZAxis;

         if (Workpiece.Classify (toolingItem.Start.Vec) == Workpiece.EType.YPos)
            return XForm4.mYAxis;

         if (Workpiece.Classify (toolingItem.Start.Vec) == Workpiece.EType.YNeg)
            return -XForm4.mYAxis;
      }

      if (toolingItem.IsFlexFeature ()) {
         var segs = toolingItem.Segs;
         Vector3 n = new (0.0, Math.Sqrt (2.0), Math.Sqrt (2.0));
         var yNegFlex = segs.Any (cutSeg => cutSeg.Vec0.Normalized ().Y < -0.1);
         if (yNegFlex) 
            n = new (0.0, -Math.Sqrt (2.0), Math.Sqrt (2.0));

         return n;
      }
      throw new InvalidOperationException (" The feature is neither plane nor flex");
   }

   /// <summary>This method finds the ordinate direction of the given vector featureNormal.</summary>
   /// <returns>
   /// One of the ordinate direction (FChassisUtils.EPlane.Top or FChassisUtils.EPlane.YNeg or FChassisUtils.EPlane.YPos )
   /// which strictly aligns [ Abs(dotproduct) = 1.0 ] with the featureNormal. 
   /// Returns FChassisUtils.EPlane.Flex for other cases
   /// </returns>
   public static EPlane GetFeatureNormalPlaneType (Vector3 featureNormal) {
      featureNormal = featureNormal.Normalized ();
      var zAxis = new Vector3 (0, 0, 1);
      var yAxis = new Vector3 (0, 1, 0);
      var featureNormalDotZAxis = featureNormal.Dot (zAxis);
      var featureNormalDotYAxis = featureNormal.Dot (yAxis);

      // [Alag:Review] need to use "as switch"
      if (Math.Abs (featureNormalDotZAxis - 1.0) < EpsilonVal) 
         return EPlane.Top;

      if (Math.Abs (featureNormalDotZAxis + 1.0) < EpsilonVal) 
         throw new NegZException ("Negative Z axis feature normal encountered");

      if (Math.Abs (featureNormalDotYAxis - 1.0) < EpsilonVal) 
         return EPlane.YPos;
      if (Math.Abs (featureNormalDotYAxis + 1.0) < EpsilonVal) 
         return EPlane.YNeg;

      return EPlane.Flex;
   }

   /// <summary>
   /// This method finds the cross product between two vectors (a cross b)
   /// </summary>
   /// <param name="a">First vector</param>
   /// <param name="b">Seond vector</param>
   /// <returns></returns>
   public static Vector3 CrossProduct (Vector3 a, Vector3 b) {
      return new Vector3 (
          a.Y * b.Z - a.Z * b.Y,
          a.Z * b.X - a.X * b.Z,
          a.X * b.Y - a.Y * b.X);
   }

   public static bool IsCircle (Curve3 curve) 
      => curve != null && curve is Arc3 && curve.Start.EQ (curve.End);

   public static bool IsArc (Curve3 curve) 
      => curve != null && curve is Arc3 && !curve.Start.EQ (curve.End);

   /// <summary>
   /// This method creates and returns a point, which is moved from the 
   /// input point, alone the direction specified upto a specific distance
   /// along that vector.
   /// </summary>
   /// <param name="pt">The input ref point</param>
   /// <param name="dir">The direction along which the new point has to be computed</param>
   /// <param name="moveLength">The distance by which the new point has to be moved 
   /// along the direction</param>
   /// <returns>The new point from a ref point, along a direction, by a specific distance.</returns>
   public static Point3 MovePoint (Point3 pt, Vector3 dir, double moveLength) {
      var normDir = dir.Normalized ();
      return (pt + normDir * moveLength);
   }

   /// <summary>
   /// THis method discretizes a given arc with no of steps input
   /// </summary>
   /// <param name="seg">The given arc segment</param>
   /// <param name="steps">No of steps</param>
   /// <returns>List of Tuples of the intermediate points with a linearly interpolated normals</returns>
   public static List<Tuple<Point3, Vector3>> DiscretizeArc (GCodeSeg seg, int steps) {
      List<Tuple<Point3, Vector3>> res = null;
      if (!seg.IsArc () || (seg.GCode != EGCode.G2 && seg.GCode != EGCode.G3)) 
         return res;
      
      res = [];
      double theta = 2 * Math.PI;
      Vector3 center2Start = seg.StartPoint - seg.ArcCenter;
      Vector3 center2End = seg.EndPoint - seg.ArcCenter;
      Vector3 crossVec = Geom.Cross (center2Start, center2End);
      if (crossVec.Length.EQ (0)) {
         var otherPt = Geom.P2V (Geom.Evaluate (seg.Arc, 0.5, seg.StartNormal));
         crossVec = Geom.Cross (center2Start, otherPt);
      }

      if (!seg.StartPoint.DistTo (seg.EndPoint).EQ (0)) {
         var val = center2Start.Dot (center2End) / (center2Start.Length * center2End.Length);
         if (val < -1) val += 1e-6;
         else if (val > 1) val -= 1e-6;
         val = val.Clamp (-1.0, 1.0);
         theta = Math.Acos (val);
         if (seg.GCode == EGCode.G3 && crossVec.Dot (seg.StartNormal) < 0.0)
            theta = 2 * Math.PI - theta;
         else if (seg.GCode == EGCode.G2 && crossVec.Dot (seg.StartNormal) > 0.0)
            theta = 2 * Math.PI - theta;
         if (seg.GCode == EGCode.G2 && theta > 0.0) 
            theta = -theta;
      }

      double delAlpha = theta / steps;
      for (int k = 0; k <= steps; k++) {
         double alphaK = k * delAlpha;
         Vector3 comp1 = center2Start * Math.Cos (alphaK);
         Vector3 comp2 = Geom.Cross (seg.StartNormal, center2Start) * Math.Sin (alphaK);
         Vector3 comp3 = seg.StartNormal * seg.StartNormal.Dot (center2Start) * (1.0 - Math.Cos (alphaK));
         Vector3 vRot = comp1 + comp2 + comp3;
         res.Add (new Tuple<Point3, Vector3> (seg.ArcCenter + Geom.V2P (vRot), seg.StartNormal));
      }

      return res;
   }

   /// <summary>
   /// This method discretizes a given line segment
   /// </summary>
   /// <param name="seg">Segment to be discretized</param>
   /// <param name="steps">No of each discretized line segments</param>
   /// <returns>A list of tuples of intermediate points, interpolated with normals</returns>
   public static List<Tuple<Point3, Vector3>> DiscretizeLine (GCodeSeg seg, int steps) {
      List<Tuple<Point3, Vector3>> res = null;
      if (seg.IsArc () || (seg.GCode != EGCode.G0 && seg.GCode != EGCode.G1)) 
         return res;

      res = [];
      double stepLength = seg.StartPoint.DistTo (seg.EndPoint) / steps;
      var prevNormal = seg.StartNormal.Normalized ();

      // For smooth transitioning of the tool, the normal's change from previous seg's last point to the 
      // current seg's last point should be gradual. This requires linear interpolation.
      res.Add (new Tuple<Point3, Vector3> (seg.StartPoint, seg.StartNormal.Normalized ()));
      for (int k = 1; k < steps - 1; k++) {
         var pt1 = Utils.MovePoint (seg.StartPoint, 
                                    Geom.P2V (seg.EndPoint) - Geom.P2V (seg.StartPoint), k * stepLength);
         var angleBetweenStNormalEndNormal = Utils.GetAngleAboutXAxis (seg.StartNormal, seg.EndNormal);
         if (angleBetweenStNormalEndNormal.EQ (0.0))
            res.Add (new Tuple<Point3, Vector3> (pt1, seg.StartNormal.Normalized ()));
         else if ((Math.Abs ((seg.EndPoint - seg.StartPoint).Normalized ().Dot (XForm4.mZAxis)) - 1.0).EQ (0.0)) {
            var t = (double)k / (double)(steps - 1);
            var newNormal = seg.StartNormal.Normalized () * (1 - t) + seg.EndNormal.Normalized () * t; 
            newNormal =newNormal.Normalized ();
            res.Add (new Tuple<Point3, Vector3> (pt1, newNormal));
         } else {
            var pt0 = Utils.MovePoint (seg.StartPoint, 
                                       Geom.P2V (seg.EndPoint) - Geom.P2V (seg.StartPoint), (k - 1) * stepLength);
            var pt2 = Utils.MovePoint (seg.StartPoint, 
                                       Geom.P2V (seg.EndPoint) - Geom.P2V (seg.StartPoint), (k + 1) * stepLength);
            var norm1 = Geom.Cross ((pt1 - pt0), XForm4.mXAxis);
            if (norm1.Opposing (prevNormal)) 
               norm1 = -norm1;

            prevNormal = norm1;

            var norm2 = Geom.Cross ((pt2 - pt1), XForm4.mXAxis);
            if (norm2.Opposing (prevNormal)) 
               norm2 = -norm2;

            var norm = ((norm2 + norm1) * 0.5).Normalized ();
            prevNormal = norm;
            res.Add (new Tuple<Point3, Vector3> (pt1, norm));
         }
      }
      res.Add (new Tuple<Point3, Vector3> (seg.EndPoint, seg.EndNormal.Normalized ()));
      return res;
   }
   
   /// <summary>
   /// This is an utility method to return the ordinate vector from
   /// input aaxis
   /// </summary>
   /// <param name="axis">The axis</param>
   /// <returns>The vector that the input axis' points to</returns>
   /// <exception cref="NotSupportedException">If an axis is non-ordinate, an exception is
   /// thrown</exception>
   public static Vector3 GetUnitVector (XForm4.EAxis axis) {
      Vector3 res = axis switch {
         XForm4.EAxis.NegZ    => -XForm4.mZAxis,
         XForm4.EAxis.Z       => XForm4.mZAxis,
         XForm4.EAxis.NegX    => -XForm4.mXAxis,
         XForm4.EAxis.X       => XForm4.mXAxis,
         XForm4.EAxis.NegY    => -XForm4.mYAxis,
         XForm4.EAxis.Y       => XForm4.mYAxis,
                            _ => throw new NotSupportedException ("Unsupported XForm.EAxis type")
      };

      return res;
   }
   
   /// <summary>
   /// This method returns the scrap side or material removal side direction w.r.t the tooling start segment
   /// w.r.t to the mid point of the first segment
   /// </summary>
   /// <param name="tooling"></param>
   /// <returns>If Line or Arc Thsi returns the mid point of the tooling segment along with the material 
   /// removal side direction evaluated at the mid point.
   /// If Circle, this returns the start point of the circle and the material removal direction 
   /// evaluated at the starting point.</returns>
   /// </exception>
   public static Tuple<Point3, Vector3> GetMaterialRemovalSideDirection (Tooling tooling) {
      var segmentsList = tooling.Segs.ToList ();
      var toolingPlaneNormal = segmentsList[0].Vec0;

      // Tooling direction as the direction of the st to end point in the case of line OR
      // tangent int he direction of start to end of the arc in the case of an arc
      Vector3 toolingDir;
      Point3 newToolingEntryPoint;
      if (Utils.IsCircle (segmentsList[0].Curve)) 
         newToolingEntryPoint = segmentsList[0].Curve.Start;
      else 
         newToolingEntryPoint = Geom.GetMidPoint (segmentsList[0].Curve, toolingPlaneNormal);
      if (segmentsList[0].Curve is Arc3 arc)
         (toolingDir, _) = Geom.EvaluateTangentAndNormalAtPoint (arc, newToolingEntryPoint, toolingPlaneNormal);
      else 
         toolingDir = (segmentsList[0].Curve.End - segmentsList[0].Curve.Start).Normalized ();

      // Ref points along the direction of the binormal,
      // which is along or opposing the direction
      // in which the material removal side exists.
      var biNormal = Geom.Cross (toolingDir, toolingPlaneNormal).Normalized ();
      Vector3 scrapSideDirection = biNormal.Normalized ();
      if (Geom.Cross (toolingDir, biNormal).Opposing (toolingPlaneNormal)) 
         scrapSideDirection = -biNormal;

      return new (newToolingEntryPoint, scrapSideDirection);
   }
   /// <summary>
   /// This method returns the vector from a point on a contour towards nearest 
   /// proximal boundary that is happening on -X or -Z axis. The magnitude of this 
   /// vector is the distance to boundary from the given point
   /// </summary>
   /// <param name="pt">The input point</param>
   /// <param name="model">The model, which is used for bounds. This will be replaced by bounds 
   /// in the next iteration</param>
   /// <param name="toolingItem">The tooling</param>
   /// <param name="proxBdy">The ordinate Axis of the proximal boundary vector. This is significant 
   /// if the given point is itself is at X=0.</param>
   /// <returns>The vector from the given point to the point on the nearest boundary along -X or X or -Z</returns>
   /// <exception cref="Exception">If the notch type is of type unknown, an exception is thrown</exception>
   static Vector3 GetVectorToProximalBoundary (Point3 pt, Model3 model, Tooling toolingItem, out XForm4.EAxis proxBdy) {
      Vector3 res;
      Point3 bdyPtXMin, bdyPtXMax, bdyPtZMin;
      switch (toolingItem.NotchKind) {
         case ENotchKind.Top:
         case ENotchKind.TopToYPos:
         case ENotchKind.TopToYNeg:
         case ENotchKind.Flex:
            if (pt.DistTo (bdyPtXMin = new Point3 (model.Bound.XMin, pt.Y, pt.Z)) 
                  < pt.DistTo (bdyPtXMax = new Point3 (model.Bound.XMax, pt.Y, pt.Z))) {
               res = bdyPtXMin - pt;
               proxBdy = XForm4.EAxis.NegX;
            } else {
               res = bdyPtXMax - pt;
               proxBdy = XForm4.EAxis.X;
            }
            break;

         case ENotchKind.YPos:
         case ENotchKind.YNeg:
            bdyPtZMin = new Point3 (pt.X, pt.Y, model.Bound.ZMin);
            res = bdyPtZMin - pt;
            proxBdy = XForm4.EAxis.NegZ;
            break;

         default:
            throw new Exception ("Unknown notch type encountered");
      }

      return res;
   }

   /// <summary>
   /// This is the primary method to evaluate the notch point on a tooling item. The tooling item contains
   /// the segments, which are a list of Larcs (Line and Arcs), Line3 in 2d and 3d and planar arcs.
   /// </summary>
   /// <param name="segments">The list of tooling segments on which the notch points need to be evaluated</param>
   /// <param name="percentage">This is the ratio of the length from start of the tooling segments' total length.
   /// <param name="leastCurveLength">This least possible length of the curve, 
   /// below which it is assumed a curve of zero length</param>
   /// <returns>A tuple of the index of the occurance of the point and the point itself, at the percentage of 
   /// the total length of the entire tooling</returns>
   /// <exception cref="Exception">An exception is thrown if the percentage is < 0 or more than 100</exception>
   public static Tuple<int, Point3> GetNotchPointsOccuranceParams (List<ToolingSegment> segments, 
                                                                   double percentage, double leastCurveLength = 0.5) {
      if (percentage < 1e-6 || percentage > 1.0 - 1e-6) 
         throw new Exception ("Notch entry points can not be lesser 0% or more than 100%");

      var totalSegsLength = segments.Sum (seg => seg.Item1.Length);
      double percentLength = percentage * totalSegsLength;
      double segmentLengthAtNotch = 0;
      int jj = 0;
      while (segmentLengthAtNotch < percentLength) {
         segmentLengthAtNotch += segments[jj].Item1.Length;
         jj++;
      }

      var segmentLength = percentLength;
      int occuranceIndex = jj - 1;
      double previousCurveLengths = 0.0;
      for (int kk = occuranceIndex - 1; kk >= 0; kk--) 
         previousCurveLengths += segments[kk].Item1.Length;

      segmentLength -= previousCurveLengths;

      // 25% of length can not happen to be almost close to the first segment's start point
      // but shall happen for the second segment onwards
      Point3 notchPoint;
      Tuple<int, Point3> notchPointOccuranceParams;
      var distToPrevSegsEndPoint = leastCurveLength;
      // in case of segmentLength is less than threshold, the notch attr is set as the 
      // index of the previous segments index and the point to be the end point of the 
      // previous segment's index.
      if (segmentLength < distToPrevSegsEndPoint)         
         notchPointOccuranceParams = new (occuranceIndex - 1, segments[occuranceIndex - 1].Item1.End);
      else if (segments[occuranceIndex].Item1.Length - segmentLength < distToPrevSegsEndPoint)
         notchPointOccuranceParams = new (occuranceIndex, segments[occuranceIndex].Item1.End);
      else {
         notchPoint = Geom.GetPointAtLengthFromStart (segments[occuranceIndex].Item1, 
                                                      segments[occuranceIndex].Item2.Normalized (), 
                                                      segmentLength);
         notchPointOccuranceParams = new (occuranceIndex, notchPoint);
      }

      return notchPointOccuranceParams;
   }

   /// <summary>
   /// This method computes the notch attribute at a given notch point and the index of the tooling segments.
   /// This notch attribute contains the curve segment. The end point of the curve segment will be the notch point
   /// 
   /// TODO: The notch attribute is yet to be optimized. It will be optimized in the subsequent iterations.
   /// </summary>
   /// <param name="model">The input model. This is used to find the bounds</param>
   /// <param name="toolingItem">The tooling item, which has the list of Larcs.</param>
   /// <param name="segments">The list of Larcs</param>
   /// <param name="segIndex">The index of the list of Larcs at which the notch point exists</param>
   /// <param name="notchPoint">The notch point, in the given context, is an unique point on the curve at which 
   /// it is desired to approach the cutting tool to start cutting the segments or leave gap with a small cut 
   /// so as to make it easy to remove the possibly heavy scrap material after cut,</param>
   /// <returns>The notch atrribute, which is a tuple of the following. The Curve, Start normal, The end Normal
   /// Normal along the flange of the curve segment, The outward vector to the nearest proximal boundary 
   /// The EAxis to the proximal boundary ( in the case of the previous vector being 0) and a flag</returns>
   /// <exception cref="NotSupportedException">An exception is thrown if the outward vector does not point 
   /// to the NegX, or X or Neg Z</exception>
   public static NotchAttribute ComputeNotchAttribute (Model3 model, Tooling toolingItem, 
                                                       List<ToolingSegment> segments,
                                                       int segIndex, Point3 notchPoint) {
      if ( segIndex == -1 )
         return new NotchAttribute (null, new Vector3(), new Vector3 (),
                                    new Vector3 (), new Vector3 (), 
                                    XForm4.EAxis.Z, false);

      XForm4.EAxis proxBdyStart;
      Vector3 outwardNormalAlongFlange;
      Vector3 vectorOutwardAtStart, vectorOutwardAtEnd, vectorOutwardAtSpecPoint;
      if (segments[segIndex].Item1 is Arc3 arc) {
         (var center, _) = Geom.EvaluateCenterAndRadius (arc);
         var vectorOutwardAtSpecPt = GetVectorToProximalBoundary (notchPoint, model, 
                                                                  toolingItem, out proxBdyStart);
         var flangeNormalVecAtSpecPt = (notchPoint - center).Normalized ();
         if (GetUnitVector (proxBdyStart).Opposing (flangeNormalVecAtSpecPt)) 
            flangeNormalVecAtSpecPt *= -1.0;

         return new NotchAttribute (segments[segIndex].Item1, 
                                    segments[segIndex].Item2.Normalized (), 
                                    segments[segIndex].Item3.Normalized (),
                                    flangeNormalVecAtSpecPt.Normalized (), 
                                    vectorOutwardAtSpecPt, proxBdyStart, true);
      } else {
         var line = segments[segIndex].Item1 as Line3;
         var p1p2 = line.End - line.Start;
         vectorOutwardAtStart = GetVectorToProximalBoundary (line.Start, model, toolingItem, out proxBdyStart);
         vectorOutwardAtEnd = GetVectorToProximalBoundary (line.End,  model, toolingItem, out _);
         vectorOutwardAtSpecPoint = GetVectorToProximalBoundary (notchPoint, model,toolingItem, out _);
         Vector3 bdyVec = proxBdyStart switch {
            XForm4.EAxis.NegX    => -XForm4.mXAxis,
            XForm4.EAxis.X       => XForm4.mXAxis,
            XForm4.EAxis.NegZ    => -XForm4.mZAxis,
                               _ => throw new NotSupportedException ("Outward vector can not be other than NegX, X, and NegZ")
         };

         outwardNormalAlongFlange = new Vector3 ();
         if (vectorOutwardAtStart.Length.EQ (0) || vectorOutwardAtEnd.Length.EQ (0))
            outwardNormalAlongFlange = bdyVec;
         else {
            int nc = 0;
            do {
               if (!Geom.Cross (p1p2, bdyVec).Length.EQ (0)) {
                  outwardNormalAlongFlange = Geom.Cross (segments[segIndex].Item2.Normalized (), 
                                                         p1p2).Normalized ();
                  if (outwardNormalAlongFlange.Opposing (bdyVec)) 
                     outwardNormalAlongFlange *= -1.0;
                  break;

               } else 
                  p1p2 = Geom.Perturb (p1p2);

               ++nc;
               if (nc > 10) 
                  break;
            } while (true);
         }
      }

      return new NotchAttribute (segments[segIndex].Item1, 
                                 segments[segIndex].Item2.Normalized (), 
                                 segments[segIndex].Item3.Normalized (),
                                 outwardNormalAlongFlange.Normalized (), 
                                 vectorOutwardAtSpecPoint, proxBdyStart, true);
   }

   /// <summary>
   /// This method is used to check the sanity of the tooling segments by checking the 
   /// G0 continuity
   /// </summary>
   /// <param name="segs">The input list of tooling segments</param>
   /// <exception cref="Exception">An exception is thrown if any segmnt misses 
   /// G0 continuity with its neighbor (with in a general tolerance of 1e-6)</exception>
   public static void CheckSanityOfToolingSegments (List<ToolingSegment> segs) {
      for (int ii = 1; ii < segs.Count; ii++) {
         if (!segs[ii - 1].Item1.End.DistTo (segs[ii].Item1.Start).EQ (0))
            throw new Exception ("There is a discontinuity in tooling segments");
      }
   }

   /// <summary>
   /// This method is used to split the given tooling segments as defined by the points
   /// prescribed in the notchPointsInfo list
   /// </summary>
   /// <param name="segments">The input segments and also the output</param>
   /// <param name="notchPtsInfo">The input notchPointsInfo list and also the output</param>
   public static void SplitToolingSegmentsAtPoints (ref List<ToolingSegment> segments, 
                                                    ref List<NotchPointInfo> notchPtsInfo) {
      for (int ii = 0; ii < notchPtsInfo.Count; ii++) {
         if (notchPtsInfo[ii].mSegIndex == -1) continue;
         var crvs = Geom.SplitCurve (segments[notchPtsInfo[ii].mSegIndex].Item1, 
                                     notchPtsInfo[ii].mPoints, 
                                     segments[notchPtsInfo[ii].mSegIndex].Item2.Normalized ());
         int stIndex = notchPtsInfo[ii].mSegIndex;
         List<NotchPointInfo> newNPInfo = [];
         if (crvs.Count > 1) {
            var toolSegsForCrvs = Geom.CreateToolingSegmentForCurves (segments[notchPtsInfo[ii].mSegIndex], crvs);
            segments.RemoveAt (notchPtsInfo[ii].mSegIndex);
            segments.InsertRange (notchPtsInfo[ii].mSegIndex, toolSegsForCrvs);

            // Create new entries for notchPointsInfo for segindex 
            for (int jj = 0; jj < crvs.Count; jj++) {
               int nptIdx = notchPtsInfo[ii].mPoints
                              .FindIndex (pt => pt.DistTo (crvs[jj].End).EQ (0));
               if (nptIdx != -1) {
                  NotchPointInfo nInfo = new () {
                     mSegIndex = stIndex++,
                     mPercentage = notchPtsInfo[ii].mPercentage,
                     mPoints = []
                  };

                  nInfo.mPoints.Add (crvs[jj].End);
                  newNPInfo.Add (nInfo);
               }
            }

            notchPtsInfo.RemoveAt (ii);
            notchPtsInfo.InsertRange (ii, newNPInfo);

            // Update SegIndex in indexObjects
            for (int jj = ii + newNPInfo.Count; jj < notchPtsInfo.Count; jj++) {
               var npInfoObj = notchPtsInfo[jj];
               npInfoObj.mSegIndex += crvs.Count - 1;
               if (notchPtsInfo[jj].mSegIndex != -1) 
                  notchPtsInfo[jj] = npInfoObj;
            }
         }
      }
   }

   /// <summary>
   /// This mwthod is a wrapper to Geom.SplitCurve, which splits the list of toolingSegments input 
   /// at the given point.
   /// </summary>
   /// <param name="segments">The list of tooling segments</param>
   /// <param name="segIndex">The segment's index in the list of tooling segment</param>
   /// <param name="point">The point at which the split is needed</param>
   /// <param name="fpn">The Feature Plane Normal, which should be the local normal to the segment</param>
   /// <returns>The list of tooling segments that got created. If no curves were split, it returns an 
   /// empty list</returns>
   public static List<ToolingSegment> SplitToolingSegmentsAtPoint(List<ToolingSegment> segments, int segIndex, Point3 point, Vector3 fpn) {
      List<ToolingSegment> toolSegs = [];
      List<Point3> intPoints = [point];
      // Consistency check
      if (segments.Count == 0 || segIndex < 0 || segIndex >= segments.Count ||
         !Geom.IsPointOnCurve (segments[segIndex].Item1, point, fpn)) 
         return toolSegs;

      var crvs = Geom.SplitCurve (segments[segIndex].Item1, 
                                  intPoints, fpn);
      var toolSegsForCrvs = Geom.CreateToolingSegmentForCurves (segments[segIndex], crvs);

      return toolSegsForCrvs;
   }

   /// <summary>
   /// This method is a predicate that tells if a segment is on the E3Flex. 
   /// TODO: In the subsequent iterations, an elegant way will be found to check 
   /// if the segment is on the E3Flex using projection/unprojection
   /// </summary>
   /// <param name="stNormal">The start normal of the segment</param>
   /// <param name="endNormal">The end normal of the segment.</param>
   /// <returns></returns>
   /// <exception cref="Exception"></exception>
   public static bool IsToolingOnFlex (Vector3 stNormal, Vector3 endNormal) {
      stNormal = stNormal.Normalized (); endNormal = endNormal.Normalized ();
      if ((stNormal.Dot (XForm4.mZAxis).EQ (1) && endNormal.Dot (XForm4.mZAxis).EQ (1)) 
         || (stNormal.Dot (XForm4.mYAxis).EQ (1) && endNormal.Dot (XForm4.mYAxis).EQ (1)) 
         || (stNormal.Dot (-XForm4.mYAxis).EQ (1) && endNormal.Dot (-XForm4.mYAxis).EQ (1))) 
         return false;
      else if (stNormal.Dot (-XForm4.mZAxis).EQ (1) ||
         endNormal.Dot (-XForm4.mZAxis).EQ (1)) throw new Exception ("Negative Z axis normal encountered");

      return true;
   }

   /// <summary>
   /// This is a utility method that computes the distance between the the segments, including the 
   /// start and end segment.
   /// </summary>
   /// <param name="segs">The list of tooling segments</param>
   /// <param name="stIndex">The start index of the tooling segment</param>
   /// <param name="endIndex">The end index of the tooling segment</param>
   /// <returns>The length of all the curves from start segment to end segment, including the start 
   /// and the end segment</returns>
   public static double GetDistanceBetween (List<ToolingSegment> segs, int stIndex, int endIndex) {
      double length = 0.0;
      for (int ii = stIndex; ii < endIndex; ii++)
         length += segs[ii].Item1.Length;

      return length;
   }

   /// <summary>
   /// This method is used to find that segment on which a given tooling length occurs. 
   /// </summary>
   /// <param name="segments">The input tooling segments</param>
   /// <param name="toolingLength">The tooling length from start of the segment</param>
   /// <returns>A tuple of the index of the segment on which the input tooling length happens and the
   /// incremental length of the index-th segment from its own start</returns>
   public static Tuple<int, double> GetSegmentLengthAndIndexForToolingLength (
                                       List<ToolingSegment> segments, double toolingLength) {
                                       double segmentLengthAtNotch = 0;
      int jj = 0;
      while (segmentLengthAtNotch < toolingLength) {
         segmentLengthAtNotch += segments[jj].Item1.Length;
         jj++;
      }

      var lengthInLastSegment = toolingLength;
      int occuranceIndex = jj - 1;
      double previousCurveLengths = 0.0;
      for (int kk = occuranceIndex - 1; kk >= 0; kk--) 
         previousCurveLengths += segments[kk].Item1.Length;

      lengthInLastSegment -= previousCurveLengths;
      return new Tuple<int, double> (occuranceIndex, lengthInLastSegment);
   }

   /// <summary>
   /// This method is used to find the length of the tooling segments from start index to end index 
   /// of the list of tooling segments, INCLUDING the start and the end segment
   /// </summary>
   /// <param name="segments">The input tooling segments</param>
   /// <param name="fromIdx">The index of the start segment</param>
   /// <param name="toIdx">The index of the end segment</param>
   /// <returns>The length of the tooling segments from start to end index including the start and 
   /// end the segments</returns>
   public static double GetLengthBetweenTooling (List<ToolingSegment> segments, 
                                                 int fromIdx, int toIdx) {
      if (fromIdx > toIdx) 
         (fromIdx, toIdx) = (toIdx, fromIdx);
      if (fromIdx == toIdx) 
         return segments[fromIdx].Item1.Length;
      double lengthBetween = segments
            .Skip (fromIdx + 1)
            .Take (toIdx - fromIdx + 1)
            .Sum (segment => segment.Item1.Length);

      return lengthBetween;
   }

   /// <summary>
   /// This method is used to find the length of the tooling segments from start point and the 
   /// end point on list of tooling segments. 
   /// </summary>
   /// <param name="segments">The input tooling segments</param>
   /// <param name="fromIdx">The from point on one of the segments</param>
   /// <param name="toIdx">The to point on one of the segments</param>
   /// <returns>The length of the tooling segments from start to end points 
   /// which is the sum of the start point to end of the start segment, 
   /// the lengths of all the tooling segments inbetween the start and end segments
   /// and the length of the last segment from start point of that segment To the given
   /// To Point</returns>
   public static double GetLengthBetweenTooling (List<ToolingSegment> segments, 
                                                 Point3 fromPt, Point3 toPt) {
      var fromPtOnSegment = segments.Select ((segment, index) => new { Segment = segment, Index = index })
                              .FirstOrDefault (x => Geom.IsPointOnCurve (x.Segment.Item1, fromPt, 
                                                                         x.Segment.Item2.Normalized ()));
      bool fromPtOnSegmentExists = fromPtOnSegment != null;
      if (!fromPtOnSegmentExists) 
         throw new Exception ("GetLengthBetweenTooling: From pt is not on segment");

      var toPtOnSegment = segments.Select ((segment, index) => new { Segment = segment, Index = index })
                              .FirstOrDefault (x => Geom.IsPointOnCurve (x.Segment.Item1, toPt, 
                                                                         x.Segment.Item2.Normalized ()));
      bool toPtOnSegmentExists = toPtOnSegment != null;
      if (!toPtOnSegmentExists) 
         throw new Exception ("GetLengthBetweenTooling: To pt is not on segment");

      // Swap the objects if from index is > to Index
      if (fromPtOnSegment.Index > toPtOnSegment.Index) {
         (fromPtOnSegment, toPtOnSegment) = (toPtOnSegment, fromPtOnSegment);
         (fromPt, toPt) = (toPt, fromPt);
      }

      double fromPtSegLength = Geom.GetLengthBetween (fromPtOnSegment.Segment.Item1, fromPt, 
                                                      fromPtOnSegment.Segment.Item1.End, 
                                                      fromPtOnSegment.Segment.Item2.Normalized ());
      double toPtSegLength = Geom.GetLengthBetween (toPtOnSegment.Segment.Item1, toPt, 
                                                    toPtOnSegment.Segment.Item1.End, 
                                                    toPtOnSegment.Segment.Item2.Normalized ());
      double intermediateLength = 0;
      if (fromPtOnSegment.Index != toPtOnSegment.Index 
         && fromPtOnSegment.Index + 1 < segments.Count 
         &&toPtOnSegment.Index - 1 >= 0)
         intermediateLength = GetLengthBetweenTooling (segments, fromPtOnSegment.Index + 1, 
                                                       toPtOnSegment.Index - 1);
      double length = intermediateLength + (fromPtSegLength + toPtSegLength);
      return length;
   }

   /// <summary>
   /// This method is used to find the length of the segments between the given point
   /// occuring on a tooling segment AND the lengths of all segments upto the last segment
   /// </summary>
   /// <param name="segments">The input segments list</param>
   /// <param name="pt">The given point</param>
   /// <returns>the length of the segments between the given point occuring on a tooling segment 
   /// AND the lengths of all segments upto the last segment</returns>
   /// <exception cref="Exception">An exception is thrown if the given point is not on any of the 
   /// input list of tooling segments</exception>
   public static double GetLengthFromEndToolingToPosition (List<ToolingSegment> segments, Point3 pt) {
      var result = segments
                     .Select ((segment, index) => new { Segment = segment, 
                                                        Index = index })
                     .FirstOrDefault (x => Geom.IsPointOnCurve (x.Segment.Item1, pt, 
                                                                x.Segment.Item2.Normalized ()));
      bool ptOnSegment = result != null;
      int idx = ptOnSegment ? result.Index : -1;
      if (!ptOnSegment) 
         throw new Exception ("GetLengthFromEndToolingToPosition: Given pt is not on any segment");

      double length = segments.Skip (idx + 1).Sum (segment => segment.Item1.Length);
      double idxthSegLengthFromPt = Geom.GetLengthBetween (segments[idx].Item1, pt, 
                                                           segments[idx].Item1.End, 
                                                           segments[idx].Item2.Normalized ());
      length += idxthSegLengthFromPt;
      return length;
   }

   /// <summary>
   /// This method computes the bounds in 3D of a set of points
   /// </summary>
   /// <param name="points">The input set of 3d points</param>
   /// <returns>The bounds in 3d</returns>
   public static Bound3 GetPointsBounds(List<Point3> points) {
      // Calculate max and min values for X, Y, Z
      var (maxX, minX, maxY, minY, maxZ, minZ) = (
          points.Max (p => p.X), points.Min (p => p.X),
          points.Max (p => p.Y), points.Min (p => p.Y),
          points.Max (p => p.Z), points.Min (p => p.Z)
      );

      Bound3 bounds = new (minX, minY, minZ, maxX, maxY, maxZ);
      return bounds;
   }

   /// <summary>
   /// This method returns the bounds 3d of a list of tooling segments from
   /// the starting index to the end index. If startIndex is -1, all the items 
   /// in the list are considered
   /// </summary>
   /// <param name="toolingSegs">The list of tooling segments</param>
   /// <param name="startIndex">The start index </param>
   /// <param name="endIndex">The end index</param>
   /// <returns></returns>
   public static Bound3 GetToolingSegmentsBounds(List<ToolingSegment> toolingSegs, 
                                                int startIndex=-1, int endIndex=-1) {
      List<ToolingSegment> toolingSegsSub = [];
      if (startIndex != -1) {
         int increment = startIndex <= endIndex ? 1 : -1;
         for (int ii = startIndex; (startIndex<=endIndex?ii<=endIndex:ii>=endIndex); ii += increment)
            toolingSegsSub.Add (toolingSegs[ii]);
      } else 
         toolingSegsSub = toolingSegs;
      
      // Extract all Point3 instances from Start and End properties of Curve3
      var points = toolingSegsSub.SelectMany (seg => new[] { seg.Item1.Start, seg.Item1.End });
      return GetPointsBounds (points.ToList());
   }

   /// <summary>
   /// This method is used to find the length of the segments between the given point
   /// occuring on a tooling segment AND the lengths of all segments upto the first segment
   /// </summary>
   /// <param name="segments">The input segments list</param>
   /// <param name="pt">The given point</param>
   /// <returns>the length of the segments between the given point occuring on a tooling segment 
   /// AND the lengths of all segments upto the first segment</returns>
   /// <exception cref="Exception">An exception is thrown if the given point is not on any of the 
   /// input list of tooling segments</exception>
   public static double GetLengthFromStartToolingToPosition (List<ToolingSegment> segments, Point3 pt) {
      double length = 0.0;
      var result = segments.Select ((segment, index) => new { Segment = segment, Index = index })
                           .FirstOrDefault (x => Geom.IsPointOnCurve (x.Segment.Item1, pt, 
                                                                      x.Segment.Item2.Normalized ()));

      bool ptOnSegment = result != null;
      int idx = ptOnSegment ? result.Index : -1;

      if (!ptOnSegment) 
         throw new Exception ("GetLengthFromStartToolingToPosition: Given pt is not on any segment");

      length = segments.Take (idx - 1).Sum (segment => segment.Item1.Length);
      length += Geom.GetLengthBetween (segments[idx].Item1, 
                                       segments[idx].Item1.Start, 
                                       pt, segments[idx].Item2.Normalized ());
      return length;
   }
}