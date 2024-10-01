using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Flux.API;
using static FChassis.Utils;

namespace FChassis;
using ToolingSegment = ValueTuple<Curve3, Vector3, Vector3>;
public static class DoubleExtensions {
   public static double Clamp (this double a, double min, double max) {
      if (a < min) 
         return min;

      if (a > max) 
         return max;

      return a;
   }
   public static double D2R (this double degrees) => degrees * (Math.PI / 180);
   public static double R2D (this double radians) => radians * (180.0 / Math.PI);
   public static double Round (this double input, int digits) => Math.Round (input, digits);
}
public class Geom {
   #region Enums
   public enum PairOfLineSegmentsType {
      Collinear,
      Skew,
      Parallel,
      SinglePointIntersection,
      SegmentsNotIntersectingWithinLimits
   }
   public enum PMC {
      Outside,
      Inside,
      On,
      CanNotEvaluate
   }
   public enum ToolingWinding {
      CW, CCW
   }
   #endregion

   #region DataTypes
   public struct Triangle3D (int a, int b, int c) {
      public int A { get; set; } = a;
      public int B { get; set; } = b;
      public int C { get; set; } = c;
   }
   #endregion

   public static Vector3 P2V (Point3 position) 
      => new (position.X, position.Y, position.Z);

   public static Point3 V2P (Vector3 vec) 
      => new (vec.X, vec.Y, vec.Z);

   public static Vector3 Cross (Vector3 left, Vector3 right) {
      return new Vector3 (
      left.Y * right.Z - left.Z * right.Y,
      left.Z * right.X - left.X * right.Z,
      left.X * right.Y - left.Y * right.X);
   }

   /// <summary>
   /// This method is used to compute a random number between lower and 
   /// upper limits double precision numbers
   /// </summary>
   /// <param name="min">Lower limit</param>
   /// <param name="max">Upper limit</param>
   /// <returns>The random number between the lower and upper limit double precision numbers</returns>
   public static double RandomNumberWithin (double min, double max) {
      var random = new Random ();
      if (max < min) 
         (max, min) = (min, max);

      double minVal = min + 1e-4; 
      double maxVal = max - 1e-4;
      var result = minVal + (random.NextDouble () * (maxVal - minVal));
      return result;
   }

   /// <summary>
   /// This method is to change a given vector a little in its direction with a magnitude 1e-3.
   /// This shall be used where a geometric degeneracy or a lock can be undone with perturbing 
   /// the vector a little so that it escapes the degeneracy
   /// </summary>
   /// <param name="vec">The Input vector to be perturbed a little</param>
   /// <returns>Returns a vector that is perturbed a little W.R.T the input vector</returns>
   public static Vector3 Perturb (Vector3 vec) {
      Vector3 res = new (vec.X * RandomNumberWithin (0, 1.0) * 1e-3, 
                         vec.Y * RandomNumberWithin (0, 1.0) * 1e-4,
                         vec.Z * RandomNumberWithin (0.0, 1.0) * 1e-4);
      return res;
   }

   /// <summary>
   /// This method is used to compute the local coordinate system of the 3d arc
   /// in such a way that the center to start direction becomes the local X Axis
   /// The normal to the plane of the arc, which MUST be obtained from Tooling plane
   /// normal becomes the Z axis and the Cross product of Z with X becomes Y axis.
   /// </summary>
   /// <param name="arc">The input arc</param>
   /// <param name="apn">The arc plane normal, which must be obtained from the tooling
   /// plane normal.</param>
   /// <returns>The Local coordinate system of the arc</returns>
   public static XForm4 GetArcCS (Arc3 arc, Vector3 apn) {
      (var center, _) = EvaluateCenterAndRadius (arc);
      var normal = apn.Normalized ();
      Vector3 xVec = (arc.Start - center).Normalized ();
      Vector3 yVec = Geom.Cross (normal, xVec).Normalized ();
      XForm4 transform = new (xVec, yVec, normal.Normalized (), Geom.P2V (center));
      return transform;
   }

   /// <summary>
   /// This method returns the angle subtended by the arc from start point to the end point
   /// about the arc plane normal that is emanating towards the observer. 
   /// </summary>
   /// <param name="arc">The input arc</param>
   /// <param name="apNormal">The arc plane normal, that shall be obtained from the tooling</param>
   /// <returns>A tuple of angle in radians, along with the sense of arc WRT the arc plane normal
   /// emanating towards the observer</returns>
   /// <caveat>We were using the curve.Evaluate() method of the Flux.API. It was observed that the 
   /// results were inaccurate. So the arc plane normal is used as a reference. However, 
   /// only in the case of semi-circular arcs, we still depend on evaluate 
   /// method of the arc in Flux.API. The parameter evaluation for arcs seems
   /// to be inaccurate.arcs, </caveat>
   public static Tuple<double, EArcSense> GetArcAngle (Arc3 arc, Vector3 apn) {
      if (arc.Start.DistTo (arc.End) < 1e-5) {
         // This is considered to be a circle
         return new Tuple<double, EArcSense> (2 * Math.PI, EArcSense.CCW);
      }

      (var center, var radius) = EvaluateCenterAndRadius (arc);
      var p2 = arc.Evaluate (0.2);
      var sp2Vec = (p2 - arc.Start).Normalized ();
      if (Utils.IsCircle (arc)) 
         return new Tuple<double, EArcSense> (Math.PI * 2, EArcSense.CCW);
      
      return GetArcAngle (arc.Start, arc.End, center, radius, arc.Length, sp2Vec, apn);
   }

   /// <summary>
   /// This method evaluates the point on the arc for a given parameter. Point to note here
   /// is that this method is defined for a parameter range from 0 to 1.
   /// </summary>
   /// <param name="arc">The Input arc</param>
   /// <param name="param">Legit value is from 0 to 1</param>
   /// <param name="apn">The arcplane normal, that should be provided by the tooling</param>
   /// <returns>Point3 point at the parameter</returns>
   public static Point3 EvaluateArc (Arc3 arc, double param, Vector3 apn) {
      var angleData = GetArcAngle (arc, apn);
      return GetArcPointAtAngle (arc, angleData.Item1 * param, apn);
   }

   /// <summary>
   /// This method evaluates the point on the line for a given parameter.
   /// </summary>
   /// <param name="ln">The given line segment</param>
   /// <param name="param">For a line segment, it is 0 to 1. 0 being the start,
   /// 1.0 being the end</param>
   /// <returns>Point3 point at the parameter</returns>
   public static Point3 EvaluateLine (Line3 ln, double param) 
                           => ln.Start * (1 - param) + ln.End * (param);

   /// <summary>
   /// This method is a wrapper to the Evaluate() of Arc and line. 
   /// </summary>
   /// <param name="crv">The curve which shall be Arc3 or Line3 type</param>
   /// <param name="param">A parameter from 0 to 1</param>
   /// <param name="apn">Arc plane normal that shall be provided by the tooling</param>
   /// <returns>The evaluated point of type Point3</returns>
   /// <exception cref="Exception">An exception is thrown if arc plane normal is not
   /// provided</exception>
   public static Point3 Evaluate (Curve3 crv, double param, Vector3? apn) {
      if (crv is Arc3 arc) {
         if (apn == null) 
            throw new Exception ("Arc plane normal cant be null");

         return EvaluateArc (arc, param, apn.Value);
      } 
      
      return EvaluateLine (crv as Line3, param);
   }

   /// <summary>
   /// This method returns the parameter ( 0 to 1 ) at a point on the curve 
   /// between start and end.
   /// </summary>
   /// <param name="crv">The input curve, which shall be Line3 or Arc3</param>
   /// <param name="pt">The input point at which the parameter should be evaluated</param>
   /// <param name="apn">The arc plane normal which should be provided by the tooling</param>
   /// <returns>The parameter for the point pt [0,1]</returns>
   /// <exception cref="ArgumentNullException">This exception is thrown if input curve is null</exception>
   /// <exception cref="Exception">This exception occurs if either the given point is not on the curve OR
   /// if there is an inconsistency in the parameter computed for the line</exception>
   public static double GetParamAtPoint (Curve3 crv, Point3 pt, Vector3? apn) {
      if (crv == null || apn == null) 
         throw new Exception ("Curve/arc plane normal is null");

      if (!IsPointOnCurve (crv, pt, apn.Value)) 
         throw new Exception ("The Point is not on the curve");

      if (crv is Arc3) {
         if (apn == null) 
            throw new Exception ("Arc Plane Normal needed");
         
         var (arcAngle, _) = GetArcAngle (crv as Arc3, apn.Value);
         var (arcAngleAtPt, _) = GetArcAngleAtPoint (crv as Arc3, pt, apn.Value);
         return arcAngleAtPt / arcAngle;
      } else {
         double denomX = (crv.End.X - crv.Start.X);
         double denomY = (crv.End.Y - crv.Start.Y);
         double denomZ = (crv.End.Z - crv.Start.Z);
         double? t1 = null, t2 = null, t3 = null;
         if (Math.Abs (denomX) > 1e-6) t1 = (pt.X - crv.Start.X) / denomX;
         if (Math.Abs (denomY) > 1e-6) t2 = (pt.Y - crv.Start.Y) / denomY;
         if (Math.Abs (denomZ) > 1e-6) t3 = (pt.Z - crv.Start.Z) / denomZ;
         // Handle all possible cases
         if (t1.HasValue && t2.HasValue && t3.HasValue) 
            return (t1.Value + t2.Value + t3.Value) / 3.0;
         else if (t1.HasValue && t2.HasValue) 
            return (t1.Value + t2.Value) / 2.0;
         else if (t1.HasValue && t3.HasValue) 
            return (t1.Value + t3.Value) / 2.0;
         else if (t2.HasValue && t3.HasValue) 
            return (t2.Value + t3.Value) / 2.0;
         else if (t1.HasValue) 
            return t1.Value;
         else if (t2.HasValue) 
            return t2.Value;
         else if (t3.HasValue) 
            return t3.Value;
         else 
            throw new InvalidOperationException ("The given point does not lie on the curve.");
      }
      throw new Exception ("Geometric inconsistency error in parameter computation");
   }

   /// <summary>
   /// This method computes the tangent (vector3) and normal (vector3) at any point on the arc.
   /// </summary>
   /// <param name="arc">The input arcThe </param>
   /// <param name="pt">The point on the arc</param>
   /// <param name="apn">The arc plane normal that should be provided by the tooling</param>
   /// <param name="constrainedWithinArc">An optional boolean flag if the computation point be constrained
   /// to be strictly between start and end of the curve segment. This is used here to check if the point
   /// on the curve is strictly between the start and end points</param>
   /// <returns>A tuple of Tangent and Normal vector at the given point</returns>
   public static Tuple<Vector3, Vector3> EvaluateTangentAndNormalAtPoint (Arc3 arc, Point3 pt, Vector3 apn,
                                                                          bool constrainedWithinArc = true) {
      if (arc == null || !IsPointOnCurve (arc as Curve3, pt, apn, constrainedWithinArc))
         throw new Exception ("Arc is null or point is not on the curve");
      
      var param = GetParamAtPoint (arc as Curve3, pt, apn);
      var (center, _) = EvaluateCenterAndRadius (arc);
      var pointAtParam1 = Geom.Evaluate (arc, param - 0.1, apn);
      var pointAtParam3 = Geom.Evaluate (arc, param + 0.1, apn);
      var pointAtParam2 = pt;
      var refVectorAlongTgt = pointAtParam2 - pointAtParam1;
      var normal = (pt - center).Normalized ();
      var planeNormal = Geom.Cross (pointAtParam3 - pointAtParam2, pointAtParam2 - pointAtParam1).Normalized ();
      var tangent = Geom.Cross (normal, planeNormal).Normalized ();
      if (tangent.Opposing (refVectorAlongTgt)) 
         tangent *= -1;

      tangent = tangent.Normalized ();
      return new Tuple<Vector3, Vector3> (tangent, normal);
   }

   /// <summary>
   /// This method is used to get the angle between the start to any given point on the arc, considering 
   /// the Arc sense also, WRT the arc plane normal. TODO: Merge thsi method with GetArcAngle()
   /// </summary>
   /// <param name="arc">The input arc</param>
   /// <param name="pt">The given point on the arc</param>
   /// <param name="apn">The arc plane normal that should be provided by the tooling</param>
   /// <returns>A tuple of arc angle in radians together with EArcSense, which could be CW or CCW. 
   /// The angle returned is in a sense CW or CCW WRT the arc plane normal amanating towards
   /// the observer</returns>
   /// <exception cref="Exception">An exception is thrown if the given point is not on the arc</exception>
   public static Tuple<double, EArcSense> GetArcAngleAtPoint (Arc3 arc, Point3 pt, Vector3 apn) {
      (var center, var radius) = EvaluateCenterAndRadius (arc);
      if (!Math.Abs (pt.DistTo (center) - radius).EQ (0.0) || !Math.Abs (apn.Dot (pt - center)).EQ (0.0))
         throw new Exception ("Given point is not on the 3d circle");

      var p2 = Geom.Evaluate (arc, 0.2, apn);
      var sp2Vec = (p2 - arc.Start).Normalized ();
      if (Utils.IsCircle (arc)) {
         if ((arc.Start - pt).Length.EQ (0.0)) return new Tuple<double, EArcSense> (0, EArcSense.CCW);
         else if ((arc.End - pt).Length.EQ (0.0)) 
            return new Tuple<double, EArcSense> (2 * Math.PI, EArcSense.CCW);

         return GetArcAngle (arc.Start, pt, center, radius, arc.Length, sp2Vec, apn);
      }

      return GetArcAngle (arc.Start, pt, center, radius, arc.Length, sp2Vec, apn);
   }

   /// <summary>
   /// This is the core method to find the angle subtended by the 3d arc, which other methods call. 
   /// A rference plane normal should be provided to determine the sense of the arc and the value 
   /// the angle.
   /// </summary>
   /// <param name="arcStPt">Start point of the arc</param>
   /// <param name="arcEndPt">End point of the arc</param>
   /// <param name="center">Center of the arc</param>
   /// <param name="radius">Radius of the arc</param>
   /// <param name="arcLength">The length of the arc. This is made use of to 
   /// decipher the sense of the arc</param>
   /// <param name="arcDirFromStartPt">This extra reference direction is used
   /// to find the sense of the arc when the arc is semicircular</param>
   /// <param name="apn">The normal of the plane on which the arc exists</param>
   /// <returns>The tuple of the angle and arc sense (Clockwise or Counter Clockwise)</returns>
   public static Tuple<double, EArcSense> GetArcAngle (Point3 arcStPt, Point3 arcEndPt, Point3 center,
                                                       double radius, double arcLength, 
                                                       Vector3 arcDirFromStartPt, Vector3 apn) {
      // compute center to start vector
      var cs = (arcStPt - center).Normalized (); var cp = (arcEndPt - center).Normalized ();
      
      // Compute the angle between the vectors in first or 4th quadrant.
      var dotp = cs.Dot (cp).Clamp (-1.0, 1.0);
      var angleUptoPt = Math.Acos (dotp);
      
      // In the case of a perfect semi-circle, since we do not have the data if the arc is 
      // clockwise or counter-clockwise, we go by the direction from start to 0.2th parameter
      // ** Warning ** : Only in the case of semi-circular arcs, we still depend on evaluate 
      //                 method of the arc in Flux.API. The parameter evaluation for arcs seems
      //                 to be inaccurate.
      // This direction gives us the hint of the arc is clockwise or counter-clockwise, WRT the 
      // apn vector ( arc plane normal, which shall be provided by the tooling )
      if (Math.Abs (Math.Abs (angleUptoPt) - Math.PI) < 1e-5) {
         var scVec = (center - arcStPt).Normalized ();
         // There is a finite difference in the arc length if the arc is semicircular. Instead of
         // the length being exactly PI*radius, in many cases, it is little lesser. In those cases,
         // we directly conclude that the arc is counter-clockwise if the apn is directed towards us
         if (arcLength < 2 * Math.PI) 
            return new (Math.Abs (angleUptoPt), EArcSense.CCW);

         if (!Geom.Cross (arcDirFromStartPt, scVec).Opposing (apn)) 
            return new (Math.Abs (angleUptoPt), EArcSense.CCW);

         return new (-Math.Abs (angleUptoPt), EArcSense.CW);
      } else {
         var s = (arcStPt - center).Normalized (); 
         var e = (arcEndPt - center).Normalized ();
         var sxe = Geom.Cross (s, e);
         if (!sxe.Opposing (apn)) {
            if (arcLength < Math.PI * radius) 
               return new (angleUptoPt, EArcSense.CCW);

            return new Tuple<double, EArcSense> (-(2 * Math.PI - angleUptoPt), EArcSense.CW);
         } else {
            if (arcLength < Math.PI * radius) 
               return new (-angleUptoPt, EArcSense.CW);

            return new (2 * Math.PI - angleUptoPt, EArcSense.CCW);
         }
      }
   }
   /// <summary>
   /// This method returns the evaluated point on the arc at an angle FROM the start point
   /// of the arc. 
   /// </summary>
   /// <param name="arc">The input arc</param>
   /// <param name="angleFromStPt">The angle from the start point of the arc</param>
   /// <param name="apn">The arc plane normal that should be obtained from the tooling</param>
   /// <returns>The point (type Point3) on the Arc that is "angleFromStPt" from the 
   /// start point of the arc</returns>
   public static Point3 GetArcPointAtAngle (Arc3 arc, double angleFromStPt, Vector3 apn) {
      (_, var radius) = EvaluateCenterAndRadius (arc);
      XForm4 transform = GetArcCS (arc, apn);
      var ptAtAngle = new Point3 (radius * Math.Cos (angleFromStPt), 
          radius * Math.Sin (angleFromStPt), 0.0);
      
      ptAtAngle = Geom.V2P (transform * ptAtAngle);
      return ptAtAngle;
   }

   /// <summary>
   /// This method returns two intermediate arc points on an "arc" 
   /// between fromPt to toPoint WRT the arc plane normal
   /// </summary>
   /// <param name="arc">The input arc</param>
   /// <param name="fromPt">The from point on the arc after which the intermediate
   /// points should be computed</param>
   /// <param name="toPoint">The to point before which the intermediate points should
   /// be computed</param>
   /// <param name="planeNormal">The plane normal which should be obtained from the tooling
   /// Important note: The plane normal is a very local phenomenon and shall only be obtained
   /// from the segment level and not from the tooling level.</param>
   /// <returns>Returns two intermediate points from "fromPt" and "toPoint" WRT 
   /// Arc Plane Normal in the direction of the sense of the arc</returns>
   public static List<Point3> GetTwoIntermediatePoints (Arc3 arc, Point3 fromPt, Point3 toPoint, Vector3 planeNormal) {
      if (!IsPointOnCurve (arc, fromPt, planeNormal) || !IsPointOnCurve (arc, toPoint, planeNormal))
         throw new InvalidOperationException ("The point is not on the arc");

      var angDataFromPt = GetArcAngleAtPoint (arc, fromPt, planeNormal);
      var angDataToPt = GetArcAngleAtPoint (arc, toPoint, planeNormal);
      var deltaAngle = angDataToPt.Item1 - angDataFromPt.Item1;
      
      List<Point3> points = [];
      points.Add (GetArcPointAtAngle (arc, angDataFromPt.Item1 + deltaAngle / 4.0, planeNormal));
      points.Add (GetArcPointAtAngle (arc, angDataFromPt.Item1 + deltaAngle * (3.0 / 4.0), planeNormal));
      return points;
   }

   /// <summary>
   /// This method returns the mid point of the curve segment. This method is a 
   /// wrapper to the specific midPoint method of Arc3 or Line3
   /// </summary>
   /// <param name="curve">The input curve segment, which shall be Arc3 or Line3</param>
   /// <param name="apn">ARc plane normal which shall be provided by the tooling</param>
   /// <returns>The mid point of the curve segment</returns>
   public static Point3 GetMidPoint (Curve3 curve, Vector3? apn) {
      if (curve is Line3 ln) 
         return (ln.Start + ln.End) * 0.5;

      return GetMidPoint (curve as Arc3, apn);
   }

   /// <summary>
   /// This method computes the mid point of the Arc segment
   /// </summary>
   /// <param name="arc">The input arc segment</param>
   /// <param name="apn">The arc plane normal that should be obtained from tooling</param>
   /// <returns>The mid point of the arc segment</returns>
   /// <exception cref="Exception">If the arc or arc plane normal is null or if the arc is actually a circle</exception>
   public static Point3 GetMidPoint (Arc3 arc, Vector3? apn) {
      if (apn == null) 
         throw new Exception ("Arc plane normal is null");
      
      if (arc == null) 
         throw new Exception ("Arc is null ");
      
      var arcAngData = GetArcAngle (arc, apn.Value);
      var mpAngle = arcAngData.Item1 / 2.0;
      return GetArcPointAtAngle (arc, mpAngle, apn.Value);
   }

   /// <summary>
   /// Computes a new point after the end point on the arc at a distance specified
   /// </summary>
   /// <param name="arc">The input arc</param>
   /// <param name="incrementDist">An incremental distance after the end point of the arc</param>
   /// <returns>Point3 which is at a distance "incrementDist" from the end point of the arc</returns>
   /// /// <exception cref="Exception">If the arc is null or if the arc is actually a circle</exception>
   public static Point3 GetNewEndPointOnArcAtIncrement (Arc3 arc, double incrementDist, Vector3 apn) {
      if (arc == null) 
         throw new Exception ("Arc plane normal is null");

      XForm4 transform = GetArcCS (arc, apn);
      var arcAngle = GetArcAngle (arc, apn);
      (_, var radius) = EvaluateCenterAndRadius (arc);
      var newAngle = (incrementDist + radius * arcAngle.Item1) / radius;
      var newEndPointOnArcAtIncrement = new Point3 (radius * Math.Cos (newAngle), radius * Math.Sin (newAngle), 0.0);
      newEndPointOnArcAtIncrement = Geom.V2P (transform * newEndPointOnArcAtIncrement);
      return newEndPointOnArcAtIncrement;
   }

   /// <summary>
   /// Computes a new point after the end point on the line at a distance specified
   /// </summary>
   /// <param name="arc">The input line segment</param>
   /// <param name="incrementDist">The delta distance after the end of the line segment.</param>
   /// <returns>Point3 which is at a distance "incrementDist" from the end point of the line</returns>
   public static Point3 GetNewEndPointOnLineAtIncrement (Line3 line, double incrementDist) {
      var newEndPointOnLineAtIncrement = line.End + (line.End - line.Start).Normalized () * incrementDist;
      return newEndPointOnLineAtIncrement;
   }

   /// <summary>
   /// This method computes the shortest (perpendicular) distance between two 3d lines
   /// </summary>
   /// <param name="p11">Start point of Line 1</param>
   /// <param name="p12">End point of the line 1</param>
   /// <param name="p21">Start point of Line 2</param>
   /// <param name="p22">End point of the line 2</param>
   /// <param name="type">Out parameter: Gives additional info if the lines are either
   /// COLLINEAR or SKEW or PARALLEL or SINGLEINTERSECTION</param>
   /// <returns></returns>
   public static double GetShortestDistBetweenLines (Point3 p11, Point3 p12, Point3 p21, Point3 p22,
                                                     out PairOfLineSegmentsType type) {
      var d1 = p12 - p11; var d2 = p22 - p21;
      double shortestDist;
      if (Geom.Cross (d1, d2).Length.EQ (0.0)) { // Parallel lines
         type = PairOfLineSegmentsType.Parallel;
         var r = p21 - p11;
         var perp2d1d2 = r - d1 * (r.Dot (d1) / (d1.Length * d1.Length));
         shortestDist = perp2d1d2.Length;
         if (shortestDist.EQ (0)) 
            type = PairOfLineSegmentsType.Collinear;
      } else {
         shortestDist = Math.Abs ((p11 - p21).Dot (Geom.Cross (d1, d2))) / Geom.Cross (d1, d2).Length;
         type = PairOfLineSegmentsType.Skew;
         if (shortestDist.EQ (0)) 
            type = PairOfLineSegmentsType.SinglePointIntersection;
      }

      return shortestDist;
   }

   /// <summary>
   /// This method returns the point of intersection between a pair of 3d coplanar line segments.
   /// </summary>
   /// <param name="p11">Start point of Line 1</param>
   /// <param name="p12">End point of Line 1</param>
   /// <param name="p21">Start point of Line 2</param>
   /// <param name="p22">End point of Line 2</param>
   /// <returns>A tuple of Point3 type with the status of intersection PairOfLineSegmentsType. 
   /// The intersection type should be checked before using the ixn point.</returns>
   /// <exception cref="InvalidOperationException"></exception>
   public static Tuple<Point3, Geom.PairOfLineSegmentsType> GetIntersectingPointBetLines (Point3 p11, Point3 p12,
                                                                                          Point3 p21, Point3 p22, 
                                                                                          bool constrainedWithinSegment = true) {
      Point3 res = new ();
      GetShortestDistBetweenLines (p11, p12, p21, p22, out Geom.PairOfLineSegmentsType linesType);
      if (linesType != PairOfLineSegmentsType.SinglePointIntersection)
         return new Tuple<Point3, Geom.PairOfLineSegmentsType> (res, linesType);

      var u = p12 - p11; var v = p22 - p21; var w = p21 - p11;
      Matrix<double> A = DenseMatrix.OfArray (new double[,] { { u.X, -v.X }, { u.Y, -v.Y }, { u.Z, -v.Z } });
      Matrix<double> B = DenseMatrix.OfArray (new double[,] { { w.X }, { w.Y }, { w.Z } });
      
      // Least square solution for the overdetermined system.
      Matrix<double> X = (A.Transpose () * A).Inverse () * A.Transpose () * B;
      res = new Point3 (p11.X + X[0, 0] * (p12.X - p11.X), 
                        p11.Y + X[0, 0] * (p12.Y - p11.Y), 
                        p11.Z + X[0, 0] * (p12.Z - p11.Z));

      if (constrainedWithinSegment) {
         if (!X[0, 0].LieWithin (0, 1) || !X[1, 0].LieWithin (0, 1))
            return new Tuple<Point3, Geom.PairOfLineSegmentsType> (res,
               Geom.PairOfLineSegmentsType.SegmentsNotIntersectingWithinLimits);
      }

      return new Tuple<Point3, Geom.PairOfLineSegmentsType> (res, Geom.PairOfLineSegmentsType.SinglePointIntersection);
   }

   /// <summary>
   /// This method returns an arbitrary normal to the plane containing the 
   /// arc, considering at random two points other than start point. 
   /// </summary>
   /// <param name="arc"></param>
   /// <returns>A vector3 which is an arbitrary normal</returns>
   /// <caveat>
   /// This normal need not conform to the E3Flex and E3Plane
   /// normals' expectations. This normal is only to be used for computation. Once the Flux.API 
   /// issues are resolved, this method shall be used. Currently thsi method is used where the 
   /// direction of the normal does not matter</caveat>
   public static Vector3 GetArcPlaneNormal (Arc3 arc) {
      Point3 P1 = (arc as Curve3).Start, 
             P2 = (arc as Curve3).Evaluate (0.3), 
             P3 = (arc as Curve3).Evaluate (0.7);
      Vector3 P1P2 = P2 - P1; 
      Vector3 P2P3 = P3 - P2;
      Vector3 arcNormal = Geom.Cross (P1P2, P2P3).Normalized ();
      return arcNormal;
   }

   /// <summary>
   /// This method computes the center and radius of the arc in 3d.
   /// </summary>
   /// <param name="arc"></param>
   /// <returns>Tuple<Center(Point3),Radius(double)></Center></returns>
   /// <exception cref="InvalidCastException"></exception>
   public static Tuple<Point3, double> EvaluateCenterAndRadius (Arc3 arc) {
      Tuple<Point3, double> res;
      // It is assumed that the Arc is the only curve, only then a circle could be a 
      // feature
      if (arc == null)
         throw new InvalidCastException ("The curve is not an Arc3");

      Point3 P1 = (arc as Curve3).Start, 
             P2 = (arc as Curve3).Evaluate (0.3), 
             P3 = (arc as Curve3).Evaluate (0.7);
      Vector3 P1P2 = P2 - P1; Vector3 P2P3 = P3 - P2;
      var apn = GetArcPlaneNormal (arc);
      Point3 M12 = (P1 + P2) * 0.5; 
      Point3 M23 = (P2 + P3) * 0.5;
      Vector3 R12 = Geom.Cross (P1P2, apn).Normalized (); 
      Vector3 R23 = Geom.Cross (P2P3, apn).Normalized ();
      Point3 M12Delta = M12 + R12 * 20.0; 
      Point3 M23Delta = M23 + R23 * 20.0;
      (var center, _) = GetIntersectingPointBetLines (M12, M12Delta, M23, M23Delta, false);
      var radius = center.DistTo (P1);
      res = new Tuple<Point3, double> (center, radius);
      return res;
   }

   /// <summary>
   /// This method checks if any given 3D point lies on the parametric curve segment including 
   /// a tolerance. The supported curves are Line3 and Arc3</summary>
   /// <param name="curve">The input arc or line</param>
   /// <param name="pt">The given point</param>
   /// <param name="constrainedWithinSegment">Flag to check within the segment of the curve. True by default</param>
   /// <param name="apn">The arc plane normal that should be obtained from the tooling</param>
   /// <returns>Boolean if the given point lies on the curve</returns>
   /// <exception cref="Exception">If the input curve is null or if the arc plane normal
   /// is not provided</exception>
   public static bool IsPointOnCurve (Curve3 curve, Point3 pt, Vector3? apn, 
                                      bool constrainedWithinSegment = true) {
      if (curve == null) 
         throw new Exception ("The curve passed is null");
      
      if (curve is Arc3) {
         if (apn == null) 
            throw new Exception ("Arc plane normal is null");
         
         var arc = curve as Arc3;
         (var center, var radius) = Geom.EvaluateCenterAndRadius (arc);
         
         // The given point pt should be having radius distance from the center
         var dist = center.DistTo (pt);
         
         // The given point should be on the same plane as that of the arc
         var ptToCenVec = center - pt; var dotp = apn.Value.Dot (ptToCenVec);
         
         //If either one of the above condition is false, return false
         if (!Math.Abs (dist - radius).EQ (0.0) || !Math.Abs (dotp).EQ (0.0)) 
            return false;

         if (constrainedWithinSegment) {
            var arcAngle = GetArcAngle (arc, apn.Value);
            var arcAngleFromStToPt = GetArcAngleAtPoint (arc, pt, apn.Value);
            var param = arcAngleFromStToPt.Item1 / arcAngle.Item1;
            if (param.LieWithin (0, 1, 1e-5)) 
               return true;

            return false;
         }

         return true;
      } else {
         var line = curve as Line3;
         var stToEndVec = line.End - line.Start; 
         var stToPtVec = pt - line.Start;
         if (!Geom.Cross (stToPtVec, stToEndVec).Length.EQ (0.0)) 
            return false;

         var param = stToPtVec.Dot (stToEndVec) / (stToEndVec.Dot (stToEndVec));
         if (constrainedWithinSegment) {
            if (param.LieWithin (0, 1)) 
               return true;
         } else 
            return true;
      }

      return false;
   }

   /// <summary>
   /// This method splits the given arc in to N+1 arcs, given "N" points
   /// in between the start and end point of the arc.
   /// Caveat: The "N" intermediate points should be in the same order 
   /// from start point to the end point of the arc
   /// </summary>
   /// <param name="arc">Arc tos split</param>
   /// <param name="interPointsList"> N intermediate points on the given arc</param>
   /// <param name="deltaBetweenArcs">The distance along the arc from an arc's end point 
   /// to the next arc's start point of the split arcs</param>
   /// <returns>List of split Arcs (Arc3)</returns>
   public static List<Arc3> SplitArc (Arc3 arc, List<Point3> interPointsList,
                                      double deltaBetweenArcs, Vector3 apn) {
      List<Arc3> splitArcs = [];
      List<Point3> points = [];
      points.Add (arc.Start); points.AddRange (interPointsList);
      points.Add (arc.End);
      if (points.Count > 2) {
         Point3 newIncrStPt = points[0];
         for (int ii = 0; ii < points.Count - 1; ii++) {
            List<Point3> twoIntermediatePoints = GetTwoIntermediatePoints (arc, newIncrStPt, points[ii + 1], apn);
            splitArcs.Add (new Arc3 (newIncrStPt, twoIntermediatePoints[0], twoIntermediatePoints[1], points[ii + 1]));
            newIncrStPt = GetNewEndPointOnArcAtIncrement (splitArcs[^1], deltaBetweenArcs, apn);
         }
      } else 
         splitArcs.Add (arc);

      return splitArcs;
   }

   /// <summary>
   /// This method splits the Line3 into N+1 line segments, given "N" points in between 
   /// the line's start and segment.
   /// Caveat: The "N" intermediate points should be in the same order 
   /// from start point to the end point of the line
   /// </summary>
   /// <param name="line">The line to split</param>
   /// <param name="interPointsList">N intermediate points on the given line</param>
   /// <param name="deltaBetweenArcs">The distance along the arc from an arc's end point 
   /// to the next arc's start point of the split arcs</param>
   /// <returns>List of split Arcs (Arc3)</returns>
   public static List<Line3> SplitLine (Line3 line, List<Point3> interPointsList, double deltaBetweenLines) {
      List<Line3> splitLines = [];
      List<Point3> points = [];
      points.Add (line.Start); points.AddRange (interPointsList);
      points.Add (line.End);
      if (points.Count > 2) {
         Point3 newIncrStPt = points[0];
         for (int ii = 0; ii < points.Count - 1; ii++) {
            splitLines.Add (new Line3 (newIncrStPt, points[ii + 1]));
            newIncrStPt = GetNewEndPointOnLineAtIncrement (splitLines[^1], deltaBetweenLines);
         }
      } else 
         splitLines.Add (line);

      return splitLines;
   }

   /// <summary>
   /// This method is a wrapper to SplitLine and SplitArc methods
   /// </summary>
   /// <param name="curve">Curve to be split</param>
   /// <param name="interPointsList">Intermediate points prescription in the same order 
   /// from start to end of the curve. Any duplicate point(s) or points that are equal to start 
   /// and end of the curve will be removed</param>
   /// <param name="deltaBetween">This is a optional distance between two curves after split. The curve
   /// is split between start to end (int point). Next curve starts from (int point + DELTA)</param>
   /// <param name="fpn">This is the abbreviation for "feature plane normal", which means, the normal
   /// at this segment's locality</param>
   /// <returns>Returns the list of Curve3. If there is no need to split the curves, this returns
   /// the original curve itself</returns>
   public static List<Curve3> SplitCurve (Curve3 curve, List<Point3> interPointsList, Vector3 fpn,
                                          double deltaBetween = 0.0) {
      var distinctInterPoints = interPointsList.Where ((p, index) => interPointsList
                                               .Take (index)
                                               .All (p2 => p2.EQ (p) != true))
                                               .ToList ();
      distinctInterPoints.RemoveAll (p => p.EQ (curve.Start) == true || p.EQ (curve.End) == true);
      if (curve is Arc3) 
         return SplitArc (curve as Arc3, distinctInterPoints, deltaBetween, fpn)
                   .Select (cr => (cr as Curve3)).ToList ();

      return SplitLine (curve as Line3, distinctInterPoints, deltaBetween)
                  .Select (cr => (cr as Curve3)).ToList ();
   }

   /// <summary>
   /// This method is used to split the given input curves
   /// </summary>
   /// <param name="curve">The input curve</param>
   /// <param name="interPointsDistances">The intermediate segment distances at which 
   /// the input curve shall be split. The last delta distance should not be provided.</param>
   /// <param name="fpn">The Feature Plane nOrmal, a local normal on which the curve exists (in the case of Arc3)</param>
   /// <param name="deltaBetween">This delta is used to give a gap between a split curve's end
   /// and the next split curve's start</param>
   /// <returns></returns>
   public static List<Curve3> SplitCurve (Curve3 curve, List<double> interPointsDistances, Vector3 fpn,
                                         double deltaBetween = 0.0) {
      List<Curve3> crvs = [];
      double totalGivenLengths = 0;
      interPointsDistances.Sum (item => totalGivenLengths += (item + deltaBetween));
      if (curve.Length < totalGivenLengths)
         return crvs;

      List<Point3> interPointsList = [];
      double cumulativeDist = 0;
      foreach (var dist in interPointsDistances) {
         cumulativeDist += dist;
         var pt = GetPointAtLengthFromStart (curve, fpn, cumulativeDist);
         interPointsList.Add (pt);
      }

      var distinctInterPoints = interPointsList.Where ((p, index) => interPointsList.Take (index)
                                               .All (p2 => p2.EQ (p) != true))
                                               .ToList ();
      distinctInterPoints.RemoveAll (p => p.EQ (curve.Start) == true || p.EQ (curve.End) == true);

      if (curve is Arc3) 
         return SplitArc (curve as Arc3, distinctInterPoints, deltaBetween, fpn)
                  .Select (cr => (cr as Curve3)).ToList ();

      return SplitLine (curve as Line3, distinctInterPoints, deltaBetween)
                  .Select (cr => (cr as Curve3)).ToList ();
   }

   /// <summary>
   /// This method is used to get the reversed curve Curve3 (Line3 or Arc3)
   /// </summary>
   /// <param name="curve">The input Curve3 (Line3 or Arc3)</param>
   /// <param name="planeNormal">The plane normal that contains the curve. This is for Arc3</param>
   /// <returns>The curve that is a reverse of the input curve</returns>
   public static Curve3 GetReversedCurve (Curve3 curve, Vector3 planeNormal) {
      if (curve is Arc3) {
         var arc = curve as Arc3;
         var intPoints = GetTwoIntermediatePoints (arc, arc.Start, arc.End, planeNormal);
         Arc3 reversedArc = new (curve.End, intPoints[1], intPoints[0], curve.Start);
         //var lenRevCrv = reversedArc.Length;
         return reversedArc as Curve3;
      } else if (curve is Line3) {
         Line3 ln = new (curve.End, curve.Start);
         return ln as Curve3;
      }

      return null;
   }

   /// <summary>
   /// This method returns the parameter at the given point of the line. Even if the point 
   /// is not within the line segment, the parameter lying outside o to 1 is returned
   /// </summary>
   /// <param name="line">Input Line segment</param>
   /// <param name="somePt">Some point</param>
   /// <returns></returns>
   public static double GetParamAtPoint (Line3 line, Point3 somePt) {
      var AP = somePt - line.Start; 
      var AB = line.End - line.Start;
      return (AP.Dot (AB) / AB.Dot (AB));
   }

   /// <summary>
   /// This method creates a new 3d arc given the following parameters.
   /// The algorithm: A 2d arc with center mapped to origin with radius
   /// as dist from center to start point is created. A local coordinate
   /// system is computed with X direction as the center to start, Z direction
   /// as the arc plane normal, and Y axis as the Cross product between Z and X.
   /// A sequence of two points on the arc is created at random locations. 
   /// These sequence of random points between the arc angle are then transformed 
   /// to the current 3d coordinates. The Arc3 is subsequently created
   /// </summary>
   /// <param name="stPoint">The start point on the arc</param>
   /// <param name="endPoint">The end point on the arc</param>
   /// <param name="center">The center of the arc</param>
   /// <param name="arcPlaneNormal">The arc plane normal that should be obtained from
   /// the tooling</param>
   /// <param name="sense">The sense of the arc CW or CCW WRT the arc plane normal
   /// emanating from, towards the observer</param>
   /// <returns>The created arc of type Arc3</returns>
   public static Arc3 CreateArc (Point3 stPoint, Point3 endPoint, Point3 center, 
                                 Vector3 arcPlaneNormal, EArcSense sense) {
      var radius = (stPoint - center).Length;
      
      // Set the local coordinate system of the arc
      var xAxis = (stPoint - center).Normalized ();
      var zAxis = arcPlaneNormal.Normalized ();
      var yAxis = Geom.Cross (zAxis, xAxis).Normalized ();
      XForm4 arcTransform = new ();
      arcTransform.SetRotationComponents (xAxis, yAxis, zAxis);
      arcTransform.SetTranslationComponent (Geom.P2V (center));
      
      // Find the angle at which the end point of the arc exists
      var endPtLocalCS = arcTransform.InvertNew () * endPoint;
      double arcAngle = Math.Atan2 (endPtLocalCS.Y, endPtLocalCS.X);
      
      if (arcAngle > 0 && sense == EArcSense.CW) 
         arcAngle = Math.PI * 2.0 - arcAngle;
      
      if (arcAngle < 0 && sense == EArcSense.CCW) 
         arcAngle = Math.PI * 2.0 + arcAngle;
      
      // Generate two extra points in between the start and end point of the arc 
      // to be created, sequentially.
      var angle2ndRandom = RandomNumberWithin (0, arcAngle);
      Point3 some2ndPoint = new (radius * Math.Cos (angle2ndRandom), radius * Math.Sign (angle2ndRandom), 0.0);
      some2ndPoint = Geom.V2P (arcTransform * some2ndPoint);
      var angle3rdRandom = RandomNumberWithin (angle2ndRandom, arcAngle);
      Point3 some3rdPoint = new (radius * Math.Cos (angle3rdRandom), radius * Math.Sign (angle3rdRandom), 0.0);
      some3rdPoint = Geom.V2P (arcTransform * some3rdPoint);
      
      // Create the arc
      Arc3 arc = new (stPoint, some2ndPoint, some3rdPoint, endPoint);
      return arc;
   }

   /// <summary>
   /// This method returns the linear interpolated normal for the given line at the specific inout point.
   /// </summary>
   /// <param name="line">Input line</param>
   /// <param name="stNormal">Normal at the start point of the line</param>
   /// <param name="endNormal">normal at the end point of the line</param>
   /// <param name="somePt">Point at which the interpolated normal has to be computed</param>
   /// <returns></returns>
   public static Vector3 GetLinearlyInterpolatedNormalAtPoint (Line3 line, Vector3 stNormal, 
                                                               Vector3 endNormal, Point3 somePt) {
      var param = GetParamAtPoint (line, somePt);
      return stNormal * (1 - param) + endNormal * param;
   }

   /// <summary>
   /// This method is used to linearly interpolate the normals for a sub-set child line
   /// of a parent. For toolings on flex, the normals at start and end of the lines are different,
   /// ( they are average of the normals between segments)
   /// </summary>
   /// <param name="parentLine">The reference line</param>
   /// <param name="stNormal">The start normal of the parent line or the reference line at start point</param>
   /// <param name="endNormal">The end normal of the parent line or the reference line at end point</param>
   /// <param name="childLine">The line for which the start and end normals at start and end points need
   /// to be evaluated</param>
   /// <returns></returns>
   public static Tuple<Vector3, Vector3> GetLinearlyInterpolatedNormalsForLine (Line3 parentLine, Vector3 stNormal, 
                                                                                Vector3 endNormal, Line3 childLine) {
      var startNormalChild = GetLinearlyInterpolatedNormalAtPoint (parentLine, stNormal, endNormal, childLine.Start);
      var endNormalChild = GetLinearlyInterpolatedNormalAtPoint (parentLine, stNormal, endNormal, childLine.End);
      return Tuple.Create (startNormalChild, endNormalChild);
   }

   /// <summary>
   /// This method is used to directionally reverse the given tooling segment,
   /// where the Start points and normals become the end points and normals and vice versa
   /// </summary>
   /// <param name="ts">The input Tooling Segment</param>
   /// <returns>This returns the reversed tooling segment</returns>
   public static ToolingSegment GetReversedToolingSegment (ToolingSegment ts) {
      var crv = GetReversedCurve (ts.Item1, ts.Item2.Normalized ());
      return new ToolingSegment (crv, ts.Item3, ts.Item2);
   }

   /// <summary>
   /// This method is a wrapper for GetReversedToolingSegment(), to reverse a list of 
   /// tooling segments</summary>
   /// <param name="toolSegs"></param>
   /// <returns>This returns the reversed tooling segments in the reverse order</returns>
   public static List<ToolingSegment> GetReversedToolingSegments (List<ToolingSegment> toolSegs) {
      List<ToolingSegment> resSegs = [];
      for (int ii = toolSegs.Count - 1; ii >= 0; ii--)
         resSegs.Add (GetReversedToolingSegment (toolSegs[ii]));

      return resSegs;
   }

   /// <summary>
   /// This method is used to linearly interpolate the normal at pointon the tooling segment
   /// </summary>
   /// <param name="parentSegment">The tooling segment that is used as a reference</param>
   /// <param name="atPoint">The point at which the the normal needs to be interpolated W.R.T 
   /// the reference tooling segment</param>
   /// <returns>This returns the normal vector at the given point</returns>
   public static Vector3 GetLinearlyInterpolatedNormalAtPoint (ToolingSegment parentSegment, Point3 atPoint) {
      if ((parentSegment.Item1.Start - atPoint).IsZero) 
         return parentSegment.Item2;
      else if ((parentSegment.Item1.End - atPoint).IsZero) 
         return parentSegment.Item3;
      if (parentSegment.Item1 is Arc3) 
         return parentSegment.Item2;
      else 
         return GetLinearlyInterpolatedNormalAtPoint (parentSegment.Item1 as Line3, 
                                                      parentSegment.Item2, 
                                                      parentSegment.Item3, atPoint);
   }

   /// <summary>
   /// This is a utility method that creates a tooling segment (ValueTuple (Curve3, Vector3,Vector3) )
   /// </summary>
   /// <param name="parentSegment">The reference segment to use its start and end normals.</param>
   /// <param name="crv">The curve which will be wrapped by the tooling segment</param>
   /// <returns>The newly created tooling segment</returns>
   public static ToolingSegment CreateToolingSegmentForCurve (ToolingSegment parentSegment, Curve3 crv) {
      return new ToolingSegment (crv, parentSegment.Item2, parentSegment.Item3);
   }

   /// <summary>
   /// This method is a creates a list of tooling segments for a list of curves
   /// </summary>
   /// <param name="parentSegment">The reference tooling segment</param>
   /// <param name="curves">The input set of curves</param>
   /// <returns>List of newly created tooling segments</returns>
   public static List<ToolingSegment> CreateToolingSegmentForCurves (ToolingSegment parentSegment, List<Curve3> curves) {
      List<ToolingSegment> res = [];
      if (curves == null || curves.Count == 0) 
         return res;

      foreach (var crv in curves) {
         var ts = new ToolingSegment (crv, parentSegment.Item2, parentSegment.Item3);
         res.Add (ts);
      }

      return res;
   }

   /// <summary>
   /// This is a utility method that returns a point from the start of the curve
   /// at a specific length of the curve. 
   /// </summary>
   /// <param name="curve">The supported curves are Arc3 and Line3</param>
   /// <param name="planeNormal">The plane normal is needed in the case of Arc3</param>
   /// <param name="length">The input length of the curve from the start of the curve.</param>
   /// <returns>The Point3 if the point exists on the curve and within the start and end of the curve</returns>
   /// <exception cref="Exception">If the degeneracies happen such as Length is negative, or 
   /// if the length is more than the length of the curve itself, exception is thrown.
   /// </exception>
   public static Point3 GetPointAtLengthFromStart (Curve3 curve, Vector3 planeNormal, double length) {
      Point3 pointAtLengthFromStart;
      if (length < -1e-6) 
         throw new Exception ("GetPointAtLengthFromStart: Length can not be less than zero");
      
      if (length > curve.Length + 1e-6) 
         throw new Exception ("GetPointAtLengthFromStart: Length can not be more than curve's length");

      if (length.LieWithin (-1e-6, 1e-6)) 
         return curve.Start;

      if (Math.Abs (curve.Length - length).LieWithin (-1e-6, 1e-6)) 
         return curve.End;
      
      if (curve is Arc3) {
         Arc3 arc = curve as Arc3;
         (_, var radius) = Geom.EvaluateCenterAndRadius (arc);
         double thetaAtPoint;
         double arcAngle;

         (arcAngle, _) = Geom.GetArcAngle (arc, planeNormal);
         var transform = Geom.GetArcCS (arc, planeNormal);

         double lengthRatio = (length) / arc.Length;
         thetaAtPoint = arcAngle * lengthRatio;
         pointAtLengthFromStart = Geom.V2P (transform * new Point3 (radius * Math.Cos (thetaAtPoint), radius * Math.Sin (thetaAtPoint), 0.0));
      } else {
         pointAtLengthFromStart = curve.Start + (curve.End - curve.Start) * (length / (curve.End - curve.Start).Length);
         double t = length / curve.Length;
         pointAtLengthFromStart = curve.Start * (1 - t) + curve.End * t;
      }

      return pointAtLengthFromStart;
   }

   /// <summary>
   /// This method returns the length of the curve between two points on the curve
   /// </summary>
   /// <param name="curve">The input curve</param>
   /// <param name="start">The start point after which the length shall be computed. This need not be
   /// the start point of the curve.</param>
   /// <param name="end">The end point upto which the length of the curve is computed. 
   /// This need not be the end point of the curve.</param>
   /// <param name="planeNormal">The locality plane normal used in the case of Arc3.</param>
   /// <returns>The length between the two given points AND on the arcs</returns>
   /// <exception cref="Exception">If the given points are not on the curve OR if they are 
   /// not in between the curve's start and end point</exception>
   public static double GetLengthBetween (Curve3 curve, Point3 start, Point3 end, Vector3 planeNormal) {
      if (((curve.Start - start).Length.EQ (0) && (curve.End - end).Length.EQ (0)) 
         || ((curve.Start - end).Length.EQ (0) && (curve.End - start).Length.EQ (0)))
         return curve.Length;
      
      if (!Geom.IsPointOnCurve (curve, start, planeNormal) || !Geom.IsPointOnCurve (curve, end, planeNormal))
         throw new Exception ("The point is not on the curve");
      
      var t1 = Geom.GetParamAtPoint (curve, start, planeNormal); 
      var t2 = Geom.GetParamAtPoint (curve, end, planeNormal);
      
      if (t1 > t2) 
         (t1, t2) = (t2, t1);

      return (t2 - t1) * curve.Length;
   }

   /// <summary>
   /// This method returns the length between two points ordered by its parameters
   /// </summary>
   /// <param name="curve">The input curve</param>
   /// <param name="t1">Start parameter</param>
   /// <param name="t2">End Parameter</param>
   /// <returns>The length of the curve in between the given parameters</returns>
   /// <exception cref="Exception"></exception>
   public static double GetLengthBetween (Curve3 curve, double t1, double t2) {
      if (!t1.LieWithin (0, 1) || !t2.LieWithin (0, 1)) 
         throw new Exception ("Parameters are not within 0.0 and 1.0");

      if (t1 > t2) 
         (t1, t2) = (t2, t1);

      return (t1 - t2) * curve.Length;
   }

   /// <summary>
   /// This method creates a list of tooling segments from the given input tooling segments, after segIndex-th item.
   /// upto the currIndex and upto a point in the currIndex-th item in input tooling segments, at which the sum of the 
   /// lengths ( from end of segIndex-th item to the point on the currIndex-th item) is "uptoLength".
   /// </summary>
   /// <param name="toolingItem">The parent tooling item</param>
   /// <param name="wireJointDistance">The distance by which the new tooling segment's start point should start</param>
   /// <param name="segIndex">The index of the segments of the tooling item, after which the new tooling segment is sought</param>
   /// <param name="segs">The segments of the tooling item. Note: This segs might not be the original segs of the tooling item,
   /// as a provision to consider the modified segments is provided
   /// </param>
   /// <param name="reverseTrace">Boolean flag if the curves are sought in the reverse treading</param>
   /// <returns>List<ToolingSegment>, which are the intermediate sequential tooling segments from the end of the segIndex-th
   /// tooling segment to (forward or reverse order) the split tooling segments. The split tooling segments can be either 1 or 2
   /// based on the point at which the split is made on the "currIndex"-th item, where the total lengths of all the tooling segments
   /// excluding the last segment is "uptoLength". The last tooling segment after the split is also added. 
   /// In case the last split element is the initial segment itself, (when the split point is at the end of the initial element)
   /// the segment at next to currIndex ( +1 if not reversed, -1 if reversed), is added. 
   /// </returns>
   public static Tuple<Point3, int> GetToolingPointAndIndexAtLength (List<ToolingSegment> segs, int segStartIndex,
                                                                     double uptoLength, Vector3 fpn, 
                                                                     bool reverseTrace = false, 
                                                                     double minimumCurveLength = 0.5) {
      Tuple<Point3, int> res = new (new Point3 (), -1);
      double crvLengths = 0.0;
      var currIndex = segStartIndex;
      bool first = true;
      while (crvLengths < uptoLength && Math.Abs (crvLengths - uptoLength) > 1e-3) {
         if (reverseTrace ? currIndex - 1 >= 0 
                          : currIndex + 1 < segs.Count) {
            if (reverseTrace) {
               if (!first) currIndex--;
            } else 
               currIndex++;
            
            if (first) 
               first = !first;

            crvLengths += segs[currIndex].Item1.Length;
         } else 
            break;
      }

      // What if the crvLengths is almost the "uptoLength"?
      // In that case, no curve shall be split. The next seg item shall be added 
      // from segs to have an uniform output that the last segment shall be 
      // machinable.
      double prevCurveLengths = 0;
      if (reverseTrace) {
         for (int ii = currIndex + 1; ii < segStartIndex; ii++)
            prevCurveLengths += segs[ii].Item1.Length;
      } else {
         for (int ii = currIndex - 1; ii > segStartIndex; ii--)
            prevCurveLengths += segs[ii].Item1.Length;
      }

      double deltaDist = uptoLength - prevCurveLengths;

      if (deltaDist < minimumCurveLength) {
         if (reverseTrace) 
            --currIndex; 
         else 
            ++currIndex;

         res = new (segs[currIndex].Item1.End, currIndex);
      } else if (segs[currIndex].Item1.Length - deltaDist < minimumCurveLength) {
         res = new (segs[currIndex].Item1.End, currIndex);
      } else {
         // Reverse the parameter if needed
         double t = deltaDist / segs[currIndex].Item1.Length;
         if (reverseTrace) 
            t = 1 - t;
         
         var pt = Geom.Evaluate (segs[currIndex].Item1, t, fpn);
         res = new (pt, currIndex);
      }

      return res;
   }

   /// <summary>
   /// This method is used to find the segments' winding sense if it is Clockwise or Counter-ClockWise WRT
   /// the normal emanating towards the observer.
   /// Algorithm: The start point of the segment is projected onto the plane define by the normal and point
   /// of the plane. The point (q) is guaranteed outside. The normal is either chosen to be (0,sqrt(1/2),sqrt(1/2))
   /// if one of the normals of the segment's normal is yPos or (0,-sqrt(1/2),sqrt(1/2)) if one of the segment's 
   /// normal is yNeg. A farthest point from the start point of the segments is evaluated. The farthest point should 
   /// be on the convex part of the polygon on the plane.The cross product between 
   /// (Start->One-Point-before-Farthest-Point) and (Start->Farthest-Point) is evaluated. If this cross product 
   /// bears the same sense with plane normal, the windings are counter-clockwise, otherwise clockwise
   /// </summary>
   /// <param name="planeNormal">The normal of the plane on which the segments are to be projected. The normal is 
   /// either chosen to be (0,sqrt(1/2),sqrt(1/2)) if one of the normals of the segment's normal is yPos or 
   /// (0,-sqrt(1/2),sqrt(1/2)) if one of the segment's normal is yNeg</param>
   /// <param name="pointOnPlane">A reference point on the plane to be projected that defines the plane</param>
   /// <param name="cutSegs">Segments of lines/arcs [ ValueTuple<Point3,Vector3,Vector3>]</param>
   /// <returns>ToolingWinding, which is either Clockwise(CW) or counter-clockwise(CCW)</returns>
   public static ToolingWinding GetToolingWinding (Vector3 planeNormal, Point3 pointOnPlane,
                                                   List<ValueTuple<Curve3, Vector3, Vector3>> cutSegs) {
      List<Point3> pointsOnPLane = [];
      foreach (var cutSeg in cutSegs) {
         var p = cutSeg.Item1.Start;
         var pq = pointOnPlane - p;
         var dot = pq.Dot (planeNormal);
         var projP = p + planeNormal * dot;
         if (!pointsOnPLane.Any (p => p.DistTo (projP) < 1e-3)) 
            pointsOnPLane.Add (projP);
      }

      var refPointOnPlane = pointsOnPLane[0];
      var pointsOnPLaneCopy = pointsOnPLane.ToList ();
      var farthestPointAndIndex = pointsOnPLaneCopy.Select ((p, index) => new { Point = p, Index = index })
                                                   .Skip (1) // Skip the 0th point
                                                   .OrderByDescending (p => refPointOnPlane.DistTo (p.Point))
                                                   .First ();
      var farthestIndex = farthestPointAndIndex.Index;
      var previousIndex = farthestPointAndIndex.Index - 1;
      var vi_1 = pointsOnPLane[previousIndex] - refPointOnPlane;
      var vi = pointsOnPLane[farthestIndex] - refPointOnPlane;
      var cross = Geom.Cross (vi_1, vi);
      if (cross.Opposing (planeNormal)) 
         return ToolingWinding.CW;

      return ToolingWinding.CCW;
   }
}

/// <summary>
/// The class XForm4 encapsulates the 3D homogenous transformation construction of 
/// size 4X4 matrix and transformation implementations for a rigid body. (There is no 
/// scaling or shear). This is an affine transformation and the determinant of this 
/// transform is always +1. The leadng 3X3 matrix represents an orthonormal unit vectors
/// The first 3 elments of the last column represents the translation component.
/// </summary>
public class XForm4 {
   public static readonly Vector3 mZAxis = new (0, 0, 1);
   public static readonly Vector3 mYAxis = new (0, 1, 0);
   public static readonly Vector3 mXAxis = new (1, 0, 0);

   public static bool IsXAxis (Vector3 vec)
      => (vec.Normalized ().Dot (mXAxis) - 1.0).EQ (0);

   public static bool IsYAxis (Vector3 vec)
      => (vec.Normalized ().Dot (mYAxis) - 1.0).EQ (0);

   public static bool IsZAxis (Vector3 vec)
      => (vec.Normalized ().Dot (mZAxis) - 1.0).EQ (0);

   public static bool IsNegXAxis (Vector3 vec) 
      => (vec.Normalized ().Dot (-mXAxis) - 1.0).EQ (0);

   public static bool IsNegYAxis (Vector3 vec) 
      => (vec.Normalized ().Dot (-mYAxis) - 1.0).EQ (0);
   

   public static bool IsNegZAxis (Vector3 vec)
      => (vec.Normalized ().Dot (-mZAxis) - 1.0).EQ (0);

   public enum EAxis { X, Y, Z, NegX, NegY, NegZ }

   public enum EXFormType { Identity, Zero }

   double[,] matrix = new double[4, 4];

   public XForm4 (EXFormType type = EXFormType.Identity) {
      if (type == EXFormType.Zero) 
         Zero ();
      else 
         Identify ();
   }

   public XForm4 Translate (Vector3 trans) {
      matrix[0, 3] = trans.X; 
      matrix[1, 3] = trans.Y; 
      matrix[2, 3] = trans.Z; 
      matrix[3, 3] = 1.0;
      return this;
   }

   public XForm4 (Vector3 col1, Vector3 col2, Vector3 col3, Vector3 col4) {
      Zero ();
      SetRotationComponents (col1, col2, col3);
      Translate (col4);
   }

   public void SetRotationComponents (Vector3 col1, Vector3 col2, Vector3 col3) {
      col1 = col1.Normalized (); 
      col2 = col2.Normalized (); 
      col3 = col3.Normalized ();
      matrix[0, 0] = col1.X; 
      matrix[1, 0] = col1.Y; 
      matrix[2, 0] = col1.Z;
      matrix[0, 1] = col2.X; 
      matrix[1, 1] = col2.Y; 
      matrix[2, 1] = col2.Z;
      matrix[0, 2] = col3.X; 
      matrix[1, 2] = col3.Y; 
      matrix[2, 2] = col3.Z;
   }

   public void SetTranslationComponent (Vector3 col4) {
      matrix[0, 3] = col4.X; 
      matrix[1, 3] = col4.Y; 
      matrix[2, 3] = col4.Z;
   }

   public double this[int i, int j] {
      get => matrix[i, j];
      set => matrix[i, j] = value;
   }

   public XForm4 MultiplyNew (XForm4 right) {
      double[,] result = new double[4, 4];
      for (int i = 0; i < 4; i++)
         for (int j = 0; j < 4; j++)
            for (int k = 0; k < 4; k++)
               result[i, j] += this[i, k] * right[k, j];

      return new XForm4 (result);
   }

   public XForm4 Multiply (XForm4 right) {
      double[,] result = new double[4, 4];
      for (int i = 0; i < 4; i++)
         for (int j = 0; j < 4; j++)
            for (int k = 0; k < 4; k++)
               result[i, j] += this[i, k] * right[k, j];

      matrix = result;
      return this;
   }

   public Vector3 Multiply (Vector3 vec) {
      double[,] result = new double[4, 1];
      double[,] rhs = new double[4, 1]; 
      rhs[0, 0] = vec.X; 
      rhs[1, 0] = vec.Y; 
      rhs[2, 0] = vec.Z; 
      rhs[3, 0] = 0.0;
      double mult;
      for (int i = 0; i < 4; i++) {
         for (int j = 0; j < 1; j++) {
            for (int k = 0; k < 4; k++) {
               mult = this[i, k] * rhs[k, j];
               result[i, j] += mult;
            }
         }
      }

      return new Vector3 (result[0, 0], result[1, 0], result[2, 0]);
   }

   public Vector3 Multiply (Point3 point) {
      var vec = Geom.P2V (point);
      double[,] result = new double[4, 1];
      double[,] rhs = new double[4, 1]; rhs[0, 0] = vec.X; rhs[1, 0] = vec.Y; rhs[2, 0] = vec.Z; rhs[3, 0] = 1.0;
      for (int i = 0; i < 4; i++) {
         for (int j = 0; j < 1; j++) {
            for (int k = 0; k < 4; k++)
               result[i, j] += this[i, k] * rhs[k, j];
         }
      }
      return new Vector3 (result[0, 0], result[1, 0], result[2, 0]);
   }

   void Identify () {
      for (int i = 0; i < 4; i++)
         for (int j = 0; j < 4; j++)
            matrix[i, j] = i == j ? 1.0 : 0.0;
   }

   void Zero () {
      for (int i = 0; i < 4; i++)
         for (int j = 0; j < 4; j++)
            matrix[i, j] = 0.0;
   }

   public XForm4 (XForm4 rhs) {
      for (int i = 0; i < 4; i++)
         for (int j = 0; j < 4; j++)
            matrix[i, j] = rhs.matrix[i, j];
   }

   public XForm4 (double[,] coords) {
      for (int i = 0; i < 4; i++)
         for (int j = 0; j < 4; j++)
            matrix[i, j] = coords[i, j];
   }

   void RotationalTranspose () {
      for (int i = 0; i < 3; i++)
         for (int j = i + 1; j < 3; j++)
            matrix[j, i] = matrix[i, j];
   }

   public XForm4 Invert () {
      RotationalTranspose ();
      double[] p = [0, 0, 0];
      for (int i = 0; i < 3; i++)
         for (int j = 0; j < 3; j++)
            p[i] += matrix[i, j] * matrix[j, 3];

      for (int i = 0; i < 3; i++)
         matrix[i, 3] = -p[i];

      return this;
   }

   public XForm4 InvertNew () {
      XForm4 resMat = new (this);
      resMat.Invert ();
      return resMat;
   }

   public static XForm4 GetRotationXForm (EAxis ax, double angle /*Degrees*/) {
      XForm4 rot = new ();
      switch (ax) {
         case EAxis.X:
            rot[1, 1] = Math.Cos (angle.D2R ());
            rot[1, 2] = -Math.Sin (angle.D2R ());
            rot[2, 1] = Math.Sin (angle.D2R ());
            rot[2, 2] = Math.Cos (angle.D2R ());
            break;

         case EAxis.Y:
            rot[0, 0] = Math.Cos (angle.D2R ());
            rot[0, 2] = Math.Sin (angle.D2R ());
            rot[2, 0] = -Math.Sin (angle.D2R ());
            rot[2, 2] = Math.Cos (angle.D2R ());
            break;

         case EAxis.Z:
            rot[0, 0] = Math.Cos (angle.D2R ());
            rot[0, 1] = -Math.Sin (angle.D2R ());
            rot[1, 0] = Math.Sin (angle.D2R ());
            rot[1, 1] = Math.Cos (angle.D2R ());
            break;

         default:
            break;
      }

      return rot;
   }

   public XForm4 Rotate (EAxis ax, double angle /*Degrees*/) {
      XForm4 rotateXForm = GetRotationXForm (ax, angle);
      this.matrix = (this * rotateXForm).matrix;
      return this;
   }

   // Overload the "*" operator
   public static XForm4 operator * (XForm4 a, XForm4 b) => a.MultiplyNew (b);
   public static Vector3 operator * (XForm4 xf, Point3 pt) => xf.Multiply (pt);
   public static Vector3 operator * (XForm4 xf, Vector3 v3) => xf.Multiply (v3);
}