using Flux.API;
namespace FChassis.GCodeGen;

using static FChassis.Utils;
using NotchAttribute = Tuple<
        Curve3, // Split curve, whose end point is the notch point
        Vector3, // Start Normal
        Vector3, // End Normal
        Vector3, // Outward Normal along flange
        Vector3, // Vector Outward to nearest boundary
        XForm4.EAxis, // Proximal boundary direction
        bool>; // Some boolean value 
using ToolingSegment = ValueTuple<Curve3, Vector3, Vector3>;

#region Data structures and Enums used Notch Computation
/// <summary>
/// The NotchPointInfo structure holds a list of notch specific points list
/// against the index of the occuring in the List of Tooling Segments and
/// the percentage of the length (by prescription).
/// </summary>
public struct NotchPointInfo {
   public NotchPointInfo (int sgIndx, Point3 pt, double percent) {
      mSegIndex = sgIndx; mPoints.Add (pt); mPercentage = percent;
   }
   public int mSegIndex;
   public List<Point3> mPoints = [];
   public double mPercentage = -1;
}

/// <summary>
/// The following enums signify the various cutting or rapid positioning strokes used during notch cutting.
/// <list type="number">
/// <item>
///     <description>WireJointApproach: This approach is mandatory to start the non-edge notch tooling. 
///     The tool describes a set of cutting strokes as follows:</description>
///     <list type="bullet">
///         <item>Approach to the midpoint of the line segment from the notch point at 50% of the length to the 
///         part boundary (in the scrap side direction).</item>
///         <item>Cutting stroke from the above midpoint to the part boundary in the scrap side direction.</item>
///         <item>Rapid positioning to the midpoint.</item>
///         <item>Cutting stroke from the midpoint to the 50% point on the tooling segment.</item>
///     </list>
/// </item>
/// <item>
///     <description>DirectApproach: This approach involves moving from one end of the non-edge notch tooling
///     to the midpoint of the WireJointApproach midpoint and making a cutting stroke from that midpoint
///     to the 50% notch point on the tooling.</description>
/// </item>
/// <item>
///     <description>GambitMachiningAt50Reverse: This operation machines a distance of wire joint length 
///     from the 50% notch point in the reverse direction of the notch tooling.</description>
/// </item>
/// <item>
///     <description>GambitMachiningAt50Forward: This operation machines a distance of wire joint length 
///     from the 50% notch point in the forward direction of the notch tooling.</description>
/// </item>
/// <item>
///     <description>MachineToolingForward: This operation machines the notch tooling profile that occurs
///     on the Web, Bottom, or Top Flanges in the forward direction of the notch tooling.</description>
/// </item>
/// <item>
///     <description>MachineToolingReverse: This operation machines the notch tooling profile that occurs
///     on the Web, Bottom, or Top Flanges in the reverse direction of the notch tooling.</description>
/// </item>
/// <item>
///     <description>MachineFlexToolingReverse: This operation machines the notch tooling profile that occurs
///     on the Flex section in the reverse direction of the notch tooling.</description>
/// </item>
/// <item>
///     <description>MachineFlexToolingForward: This operation machines the notch tooling profile that occurs
///     on the Flex section in the forward direction of the notch tooling.</description>
/// </item>
/// </list>
/// </summary>
public enum NotchSectionType {
   /// <summary>
   /// This is mandatory to start the non-edge notch tooling. 
   /// The tool describes a set of the following cutting strokes
   /// -> Approach to the mid point of the line segment from notch point at 50% of the length to the 
   /// part boundary (in scrap side direction)
   /// -> Cutting stroke from the above mid point to the part boundary in the scrap side direction
   /// -> Rapid position to the mid point
   /// -> Cutting stroke from mid point to the 50% point on the tooling segment
   /// </summary>
   WireJointApproach,

   /// <summary>
   /// This is the approach from one end of the non-edge notch tooling
   /// to the mid point of the WireJointApproach mid point and a cutting stroke from that mid point
   /// to the 50% notch point on the tooling
   /// </summary>
   DirectApproach,

   /// <summary>
   /// This is to machine a distance of wire joint distance 
   /// from 50% notch point in the reverse order of the notch tooling
   /// </summary>
   GambitMachiningAt50Reverse,

   /// <summary>
   /// This is to machine a distance of wire joint distance 
   /// from 50% notch point in the forward order of the notch tooling
   /// </summary>
   GambitMachiningAt50Forward,

   /// <summary>
   /// This is to machine the notch tooling profile that occurs
   /// on Web or Bottom or Top Flanges in the forward direction of the notch tooling
   /// </summary>
   MachineToolingForward,

   /// <summary>
   /// This is to machine the notch tooling profile that occurs
   /// on Web or Bottom or Top Flanges in the reverse direction of the notch tooling
   /// </summary>
   MachineToolingReverse,

   /// <summary>
   /// This is to machine the notch tooling profile that occurs
   /// on the Flex section in the reverse direction of the notch tooling
   /// </summary>
   MachineFlexToolingReverse,

   /// <summary>
   /// This is to machine the notch tooling profile that occurs
   /// on the Flex section in the forward direction of the notch tooling
   /// </summary>
   MachineFlexToolingForward,

   /// <summary>
   /// This is to introduce a partly joined arrangement with a distance of 
   /// wire joint distance specified in the settings in the reverse direction 
   /// of the notch tooling
   /// </summary>
   WireJointTraceJumpReverse,

   /// <summary>
   /// This is to introduce a partly joined arrangement with a distance of 
   /// wire joint distance specified in the settings in the forward direction 
   /// of the notch tooling
   /// </summary>
   WireJointTraceJumpForward,

   /// <summary>
   /// This is a directive to move to the mid point of the intitial segment defined 
   /// in "WireJointApproach", to reposition the tooling head
   /// </summary>
   MoveToMidApproach
}

/// <summary>
/// The following structure holds notch specific ( prescribed by process team) points
/// along the notch. The integers are the indices of the list of tooling segments, whose 
/// end points are the notch specific points at which either the entry to the cutting 
/// profile happens or a wire joint is introduced, or an initial gambit action to move
/// in the forward or reverse direction of wire joint distance happens.
/// </summary>
public struct NotchSegmentIndices {
   public NotchSegmentIndices () { }
   public int segIndexAt25pc = -1, segIndexAt50pc = -1, segIndexAt75pc = -1,
      segIndexAtWJTPost25pc = -1, segIndexAtWJTPost50pc = -1, segIndexAtWJTPre50pc = -1,
      segIndexAtWJTPost75pc = -1;
   public List<Tuple<int, int, int, int>> flexIndices = [];
}

/// <summary>
/// The entire notch tooling is subdivided into multiple tooling blocks.
/// The following structure holds the sub section of the notch tooling
/// </summary>
public struct NotchSequenceSection {
   public NotchSequenceSection () { }
   public int mStartIndex = -1;
   public int mEndIndex = -1;
   public NotchSectionType mSectionType;
}
#endregion

/// <summary>
/// The class Notch holds all the sequences of actions from creating notch specific points
/// through writing the notch. The sequence is modularized as much as it is needed. Once the 
/// the final prescription is made from the process team, more optimizations will be done
/// </summary>
public class Notch {
   #region Constructors
   public Notch (Tooling toolingItem, Model3 model, GCodeGenerator gcodeGen, EPlane prevPlaneType,
                 double notchWireJointDistance, double notchApproachLength, double[] percentlength, 
                 double totalPrevCutToolingsLength, double totalToolingsCutLength, 
                 double curveLeastLength = 0.5) {
      mToolingItem = toolingItem;
      mModel = model;
      mNotchApproachLength = notchApproachLength;
      mNotchWireJointDistance = notchWireJointDistance;
      mCurveLeastLength = curveLeastLength;
      mPercentLength = percentlength;
      mGCodeGen = gcodeGen;
      mPrevPlane = prevPlaneType;
      mSegments.AddRange (mToolingItem.Segs.ToList ());
      mTotalToolingsCutLength = totalToolingsCutLength;
      mCutLengthTillPrevTooling = totalPrevCutToolingsLength;

      // Compute the Notch parameters
      ComputeNotchParameters ();
   }
   #endregion

   #region Caching tool position
   ToolingSegment mExitTooling;
   Point3 mLastPosition;
   public ToolingSegment Exit { get => mExitTooling; }
   #endregion

   #region External references
   GCodeGenerator mGCodeGen;
   static Model3 mModel;
   Tooling mToolingItem;
   EPlane mPrevPlane = EPlane.None;
   public EPlane PrevPlane { 
      get => mPrevPlane; 
      set => mPrevPlane = value; }
   #endregion

   #region Tunable Parameters / Setting Prescriptions
   double mCurveLeastLength;
   
   // As desired by the machine team
   double[] mPercentLength = [0.25, 0.5, 0.75];
   double mNotchWireJointDistance = 2.0;
   public double NotchWireJointDistance { 
                     get => mNotchWireJointDistance; 
                     set => mNotchWireJointDistance = value; }
   double mNotchApproachLength = 5.0;
   public double NotchApproachLength { 
                     get => mNotchApproachLength; 
                     set => mNotchApproachLength = value; }
   #endregion

   #region Intermediate Data Structures
   NotchSegmentIndices mNotchIndices;
   List<NotchAttribute> mNotchAttrs = [];
   List<NotchSequenceSection> mNotchSequences = [];
   Point3?[] mWireJointPts = [null, null, null, null];
   List<Point3> mFlexWireJointPts = [];
   double mBlockCutLength = 0;
   double mTotalToolingsCutLength = 0;
   double mCutLengthTillPrevTooling = 0;
   
   // Find the flex segment indices
   List<Tuple<int, int>> mFlexIndices = [];

   // Each List<Curve3> shall hold minimum 1 curve or maximum 2 curves
   //List<ToolingSegment>[] mSplitCurveSegs = [[], [], []];

   // The indices of segs on whose segment the 25%, 50% and 75% of the length occurs
   int?[] mSegIndices = [null, null, null]; 
   int mSegsCount = 0;

   // The point on the segment which shall participate in notch tooling
   Point3?[] mNotchPoints = new Point3?[3];
   List<NotchPointInfo> mNotchPointsInfo = [];
   #endregion

   #region Public Properties
   public List<NotchAttribute> NotchAttributes { get => mNotchAttrs; }
   List<ToolingSegment> mSegments = [];
   #endregion

   #region Notch parameters computing methods
   /// <summary>
   /// A predicate method that returns if the given "notchPoint" is within the 
   /// flex section of tooling, considering a minimum thershold "minThresholdLenFromNPToFlexPt"
   /// outside of flex also as inside
   /// </summary>
   /// <param name="flexIndices">The list of flexe indices where each item is a tuple 
   /// of start and end index in the tooling segments</param>
   /// <param name="segs">The input tooling segments</param>
   /// <param name="notchPoint">The input notch point</param>
   /// <param name="minThresholdLenFromNPToFlexPt">The minimum threshold distance of the 
   /// notch point from the nearest flex start/end point, even if outside, is considered
   /// to be inside.</param>
   /// <returns>A tuple of bool: if the notch point is within the flex, 
   /// Start Index and End Index</returns>
   Tuple<bool, int, int> IsPointWithinFlex (List<Tuple<int, int>> flexIndices, List<ToolingSegment> segs, 
                                            Point3 notchPoint, double minThresholdLenFromNPToFlexPt) {
      bool isWithinAnyFlex = false;
      int stIndex = -1, endIndex = -1;
      foreach (var flexIdx in flexIndices) {
         var flexToolingLen = Utils.GetLengthBetweenTooling (segs, flexIdx.Item1, flexIdx.Item2);
         var lenNPToFlexStPt = Utils.GetLengthBetweenTooling (segs, notchPoint, segs[flexIdx.Item1].Item1.Start);
         var lenNPToFlexEndPt = Utils.GetLengthBetweenTooling (segs, notchPoint, segs[flexIdx.Item2].Item1.End);
         var residue = lenNPToFlexStPt + lenNPToFlexEndPt - flexToolingLen;
         if (lenNPToFlexStPt < minThresholdLenFromNPToFlexPt 
                  || lenNPToFlexEndPt < minThresholdLenFromNPToFlexPt 
                  || Math.Abs (residue).EQ (0, 1e-2)) {
            isWithinAnyFlex = true;
            stIndex = flexIdx.Item1; endIndex = flexIdx.Item2;
            break;
         }
      }

      return new Tuple<bool, int, int> (isWithinAnyFlex, stIndex, endIndex);
   }

   /// <summary>
   /// This method computes the 25%, 50% and 75% of the notch points AGAIN, if the previous 
   /// computation finds the locations existing within the flexes. A heuristic is used, where
   /// 25% and 75% of the notch points are recomputed ONLY IF the notch point to the start (for 25%-th
   /// notch point) Or to the end ( for 75%-th notch point) is more than 200 units (mm).
   /// This is as per the requirement. If the length is < 200 units, the corresponding notch point 
   /// is refused. ( by making the index -1).
   /// </summary>
   /// <param name="segs">The list of tooling segments</param>
   /// <param name="notchPtCountIndex">Index to mean if it is 25/50/75%-th (0,1,2) respectively.</param>
   /// <param name="notchPt">The given notch point</param>
   /// <param name="thresholdNotchLenForNotchApproach">This is the length threshold for decising 
   /// if the noych point needs to be recomputed. The value used is 200 units</param>
   /// <param name="segIndices">The indices of the 25/50/75%-th notch point occurances
   /// on the list of tooling segments</param>
   /// <param name="notchPoints">The array of the notch points at 25/50/75%-th lengths</param>
   void RecomputeNotchPointsWithinFlex (List<ToolingSegment> segs, int notchPtCountIndex, Point3 notchPt,
                                        double thresholdNotchLenForNotchApproach, 
                                        ref int?[] segIndices, ref Point3?[] notchPoints) {
      double? lenToToolingEnd = null;
      if (notchPtCountIndex == 2) // Notch point at 75% of the tooling length is within a flex section
         lenToToolingEnd = Utils.GetLengthFromEndToolingToPosition (segs, notchPt);
      else if (notchPtCountIndex == 0) // Notch point at 25% of the tooling length is within a flex section
         lenToToolingEnd = Utils.GetLengthFromStartToolingToPosition (segs, notchPt);

      if (lenToToolingEnd != null) {
         if (lenToToolingEnd.Value > thresholdNotchLenForNotchApproach) {
            // Add new notch point at approx mid
            double percent;
            if (notchPtCountIndex == 2) 
               percent = mPercentLength[2] = 0.875;
            else 
               percent = mPercentLength[0] = 0.125;

            var (sgIdx, npt) = Utils.GetNotchPointsOccuranceParams (segs, percent, mCurveLeastLength);
            segIndices[notchPtCountIndex] = sgIdx; notchPoints[notchPtCountIndex] = npt;
         } else {
            // Mark this notch as false or delete
            segIndices[notchPtCountIndex] = null; 
            notchPoints[notchPtCountIndex] = null;
         }
      } else {
         // Handle 50% pc case here
         var (sgIdx, npt) = 
               Utils.GetNotchPointsOccuranceParams (segs, 0.4, mCurveLeastLength);
                                                    mPercentLength[1] = 0.4;
         segIndices[notchPtCountIndex] = sgIdx; notchPoints[notchPtCountIndex] = npt;
      }
   }

   /// <summary>
   /// This method recomputes the notch points for length ratios of 25% and 75% if
   /// these points exist within the flex section of the tooling. The following actions will be taken:
   /// <list type="number">
   /// <item>
   ///     <description>If the notch points are within "minThresholdLenFromNPToFlexPt" units 
   ///     from the nearest flex and outside the flex, these notch points are removed.</description>
   /// </item>
   /// <item>
   ///     <description>If the notch points occur outside the flex section and if the distance from
   ///     the extreme section (start for 25% and end for 75%) is more than "thresholdNotchLenForNotchApproach",
   ///     the 25%-th notch point is recomputed at 0.125-th length and the 75%-th notch point is recomputed
   ///     at 0.875-th length of the notch tooling.</description>
   /// </item>
   /// <item>
   ///     <description>If the notch point at 50%-th length lies within the flex, another notch point
   ///     is computed at 40% of the total tooling length from the start.</description>
   /// </item>
   /// </list>
   /// </summary>
   /// <param name="segs">The input tooling segments list, which should be further segmented.</param>
   /// <param name="flexIndices">The list of tuples containing the start and end indices of the flex.</param>
   /// <param name="notchPoints">The existing notch points at 25%, 50%, and 75% of the total tooling length.</param>
   /// <param name="segIndices">The array of indices of the list of tooling segments where the 
   /// notch points occur.</param>
   /// <param name="mPercentLength">The input specification of the percentage length (25%, 50%, 75%).</param>
   /// <param name="minThresholdLenFromNPToFlexPt">The minimum length of the tooling segment below which
   /// a notch point is considered invalid and removed if it occurs.</param>
   /// <param name="thresholdNotchLenForNotchApproach">The threshold length of the tooling segments that
   /// allows for recomputing notch points at 25% and 75%. If the length from this notch point to the nearest
   /// end is less than this threshold, it is not required to create a new notch point as removing the scrap
   /// part is manageable.</param>
   void RecomputeNotchPointsAgainstFlexNotch (List<ToolingSegment> segs, List<Tuple<int, int>> flexIndices,
                                              ref Point3?[] notchPoints, ref int?[] segIndices, 
                                              double[] mPercentLength, double minThresholdLenFromNPToFlexPt,
                                              double thresholdNotchLenForNotchApproach) {
      //bool recomputeNeeded = false;
      //do {
      int index = 0;
      while (index < mPercentLength.Length) {
         if (notchPoints[index] == null) { index++; continue; }
         var (isWithinAnyFlex, flexStartIndex, flexEndIndex) = 
                     IsPointWithinFlex (flexIndices, segs, notchPoints[index].Value, 
                                        minThresholdLenFromNPToFlexPt);

         if (isWithinAnyFlex) 
            RecomputeNotchPointsWithinFlex (segs, index, notchPoints[index].Value, thresholdNotchLenForNotchApproach, 
                                            ref segIndices, ref notchPoints);
         else if (segIndices[index] != -1) {
            if (flexStartIndex != -1) {
               var fromNPTToFlexStart = 
                        Utils.GetLengthBetweenTooling (segs, notchPoints[index].Value, segs[flexStartIndex].Item1.Start);
               if (fromNPTToFlexStart < 10.0) {
                  var (newNPTAtIndex, idx) = 
                        Geom.GetToolingPointAndIndexAtLength (segs, segIndices[index].Value, 11.0/*length offset for 50% pt*/,
                                                              segs[segIndices[index].Value].Item2, reverseTrace: true);
                  segIndices[index] = idx; notchPoints[index] = newNPTAtIndex;
               }
            }
            if (flexEndIndex != -1) {
               var fromNPTToFlexEnd = Utils.GetLengthBetweenTooling (segs, notchPoints[index].Value, segs[flexEndIndex].Item1.End);
               if (fromNPTToFlexEnd < 10.0) {
                  var (newNPTAtIndex, idx) = 
                        Geom.GetToolingPointAndIndexAtLength (segs, segIndices[index].Value, 11.0/*length offset for 50% pt*/,
                                                           segs[segIndices[index].Value].Item2, reverseTrace: false);
                  segIndices[index] = idx; notchPoints[index] = newNPTAtIndex;
               }
            }
         }

         index++;
      }
   }

   /// <summary>
   /// This method computes a list of tuples representing the start and end indices of the tooling
   /// segments that occur on the Flex.
   /// </summary>
   /// <param name="segs">The input list of tooling segments.</param>
   /// <returns>A list of tuples, where each tuple contains the start and end indices of the tooling
   /// segments that occur on the Flex. The method assumes that there are two flex toolings on the notch tooling.</returns>
   public static List<Tuple<int, int>> GetFlexSegmentIndices (List<ToolingSegment> segs) {
      // Find the flex segment indices
      List<Tuple<int, int>> flexIndices = [];
      int flexEndIndex, flexStartIndex = -1;
      for (int ii = 0; ii < segs.Count; ii++) {
         var (_, stNormal, endNormal) = segs[ii];
         if (Utils.IsToolingOnFlex (stNormal, endNormal)) {
            if (flexStartIndex == -1) 
               flexStartIndex = ii;
         } else if (flexStartIndex != -1) {
            flexEndIndex = ii - 1;
            var indxes = new Tuple<int, int> (flexStartIndex, flexEndIndex);
            flexIndices.Add (indxes);
            flexStartIndex = -1;
         }
      }

      return flexIndices;
   }

   /// <summary>
   /// The following method computes the wire joint positions
   /// at 25%, 50% and 75% of the lengths and splits the tooling segments in such a way that 
   /// the end point of the segment is the notch or wire joint point
   /// </summary>
   /// <param name="segs">The input tooling segments list</param>
   /// <param name="notchPoints">The input prescribed notch points</param>
   /// <param name="notchPointsInfo">A data structure that stores one notch or wire joint
   /// point per unique index of the list of tooling items after splitting. The end point is
   /// the notch or wire joint distance point</param>
   /// <param name="atLength">A variable that holds the wire joint length</param>
   public void ComputeWireJointPositionsOnFlanges (List<ToolingSegment> segs, Point3?[] notchPoints,
      ref List<NotchPointInfo> notchPointsInfo, double atLength) {

      // Split the tooling segments at wire joint length from notch points 
      mWireJointPts = [null, null, null, null];
      int ptCount = 0;
      for (int ii = 0; ii < notchPoints.Length; ii++) {
         if (notchPoints[ii] == null) { 
            ptCount++; 
            continue; 
         }

         // Find the index of the occurrence of the point where Curve3.End matches the given point
         var notchPointIndex = segs.Select ((segment, idx) => new { segment, idx })
                                   .Where (x => x.segment.Item1.End.DistTo (notchPoints[ii].Value).EQ (0))
                                   .Select (x => x.idx)
                                   .FirstOrDefault ();
         
         // If the wire Joint Distance is close to 0.0, this should not affect
         // the parameters of the notch at 50% of the length (pre, @50 and post)
         if (atLength < 0.5 && ii == 1) 
            atLength = 2.0;

         (mWireJointPts[ptCount], var segIndex) = 
               Geom.GetToolingPointAndIndexAtLength (segs, notchPointIndex,
                                                     atLength, segs[notchPointIndex].Item2.Normalized ());
         var splitToolSegs = Utils.SplitToolingSegmentsAtPoint (segs, segIndex, mWireJointPts[ptCount].Value,
                                                                segs[notchPointIndex].Item2.Normalized ());

         // Make the NotchPointsInfo to contain unique entries by having unique index of the
         // tooling segments list per point (notch or wire joint)
         UpdateNotchPointsInfo (ref splitToolSegs, ref segs, ref notchPointsInfo, segIndex);

         // At 50%..
         if (ii == 1) {
            ptCount++;
            (mWireJointPts[ptCount], segIndex) = 
                  Geom.GetToolingPointAndIndexAtLength (segs, notchPointIndex, atLength,
                                                        segs[notchPointIndex].Item2.Normalized (), 
                                                                                       reverseTrace: true);
            splitToolSegs = Utils.SplitToolingSegmentsAtPoint (segs, segIndex, mWireJointPts[ptCount].Value,
                                                               segs[notchPointIndex].Item2.Normalized ());
            UpdateNotchPointsInfo (ref splitToolSegs, ref segs, ref notchPointsInfo, segIndex);
            (mWireJointPts[ptCount], mWireJointPts[ptCount - 1]) = (mWireJointPts[ptCount - 1], mWireJointPts[ptCount]);
         }
         ptCount++;
      }
   }

   /// <summary>
   /// The following method computes the indices of all the notch points and wire joint points that are occurring
   /// on the list of segmented tooling segments for each of the above points. 
   /// Note: The notch point or the wire joint lengthed points occur as the end point of the tooling segment
   /// </summary>
   /// <param name="segs">The segmented tooling segments list</param>
   /// <param name="notchPoints">The points at the prescribed lengths of tooling (25%, 50% and 75%)</param>
   /// <param name="wjtPoints">The points on the list of tooling segments where wire joint jump trace is desired</param>
   /// <param name="flexWjtPoints">The start and the end points of the flex tooling which is also treated 
   /// as wire joint jump trace</param>
   public void ComputeNotchToolingIndices (List<ToolingSegment> segs, Point3?[] notchPoints,
                                           Point3?[] wjtPoints, List<Point3> flexWjtPoints) {
      mNotchIndices = new NotchSegmentIndices ();
      int ptCount = 0;
      for (int ii = 0; ii < notchPoints.Length; ii++) {
         if (notchPoints[ii] == null) {
            ptCount++;
            continue;
         }

         var notchPointIndexPostSplit = segs.Select ((segment, idx) => new { segment, idx })
                                            .Where (x => x.segment.Item1.End.DistTo (notchPoints[ii].Value).EQ (0))
                                            .Select (x => x.idx)
                                            .FirstOrDefault ();
         var wjtPointIndexPostSplit = segs.Select ((segment, idx) => new { segment, idx })
                                          .Where (x => x.segment.Item1.End.DistTo (wjtPoints[ptCount].Value).EQ (0))
                                          .Select (x => x.idx)
                                          .FirstOrDefault ();
         if (ii == 0) {
            mNotchIndices.segIndexAt25pc = notchPointIndexPostSplit;
            mNotchIndices.segIndexAtWJTPost25pc = wjtPointIndexPostSplit;
         } else if (ii == 1) {
            mNotchIndices.segIndexAt50pc = notchPointIndexPostSplit;
            mNotchIndices.segIndexAtWJTPre50pc = wjtPointIndexPostSplit;
         } else {
            mNotchIndices.segIndexAt75pc = notchPointIndexPostSplit;
            mNotchIndices.segIndexAtWJTPost75pc = wjtPointIndexPostSplit;
         }

         if (ii == 1) {
            ptCount++;
            if (wjtPoints[ptCount] == null) 
               continue;

            wjtPointIndexPostSplit = segs.Select ((segment, idx) => new { segment, idx })
                                         .Where (x => x.segment.Item1.End.DistTo (wjtPoints[ptCount].Value).EQ (0))
                                         .Select (x => x.idx)
                                         .FirstOrDefault ();
            mNotchIndices.segIndexAtWJTPost50pc = wjtPointIndexPostSplit;
         }

         ptCount++;
      }

      for (int ii = 0; ii < flexWjtPoints.Count; ii += 4) {
         var preSegFlexStPointIndex = segs.Select ((segment, idx) => new { segment, idx })
                                          .Where (x => x.segment.Item1.End.DistTo (flexWjtPoints[ii]).EQ (0))
                                          .Select (x => x.idx)
                                          .FirstOrDefault ();
         var flexStPointIndex = segs.Select ((segment, idx) => new { segment, idx })
                                    .Where (x => x.segment.Item1.End.DistTo (flexWjtPoints[ii + 1]).EQ (0))
                                    .Select (x => x.idx)
                                    .FirstOrDefault ();
         var flexEndPointIndex = segs.Select ((segment, idx) => new { segment, idx })
                                     .Where (x => x.segment.Item1.End.DistTo (flexWjtPoints[ii + 2]).EQ (0))
                                     .Select (x => x.idx)
                                     .FirstOrDefault ();
         var postFlexEndPointIndex = segs.Select ((segment, idx) => new { segment, idx })
                                         .Where (x => x.segment.Item1.End.DistTo (flexWjtPoints[ii + 3]).EQ (0))
                                         .Select (x => x.idx)
                                         .FirstOrDefault ();
         Tuple<int, int, int, int> flexIndices = new (preSegFlexStPointIndex, flexStPointIndex, 
                                                      flexEndPointIndex, postFlexEndPointIndex);
         mNotchIndices.flexIndices.Add (flexIndices);
      }
   }

   /// <summary>
   /// The following method creates a Notch Sequence Section with user inputs
   /// </summary>
   /// <param name="startIndex">The start index of the list of tooling segments</param>
   /// <param name="endIndex">The end index of the list of tooling segments</param>
   /// <param name="notchSectionType">The type of tooling block that is desired</param>
   /// <returns></returns>
   /// <exception cref="Exception">The method expects that the start and end end indices are in non 
   /// decreasing order. Unless, this throws an exception. In the case of reversed tooling segment
   /// too, the start and end index should be prescribed in the same non-decreasing order</exception>
   NotchSequenceSection CreateNotchSequence (int startIndex, int endIndex, NotchSectionType notchSectionType) {
      if (notchSectionType == NotchSectionType.MachineToolingForward) {
         if (startIndex > endIndex) throw new Exception ("StartIndex < endIndex for forward machiniing");
      }

      var nsq = new NotchSequenceSection () {
         mStartIndex = startIndex,
         mEndIndex = endIndex,
         mSectionType = notchSectionType
      };

      return nsq;
   }

   /// <summary>
   /// Creates notch sequences in the reverse direction of the list of tooling,
   /// taking into account the notch and wirejoint points and flex sections 
   /// for various possible occurrences of notch points and flex sections.
   /// </summary>
   /// <param name="mNotchIndices">The indices of the notch and wirejoint points.</param>
   /// <returns>A list of assembled notch sequence sections to be used for generating G Code.</returns>
   /// <exception cref="Exception">Thrown when the notch sequences do not follow the correct directional
   /// or sequential order. If the order is incorrect, an exception will be thrown.</exception>
   List<NotchSequenceSection> CreateNotchReverseSequences (NotchSegmentIndices mNotchIndices) {
      List<NotchSequenceSection> reverseSequences = [];
      int revStartIndex = mNotchIndices.segIndexAtWJTPost50pc;
      if (mNotchIndices.flexIndices.Count > 0) {
         if (revStartIndex <= mNotchIndices.flexIndices[0].Item4 + 1 && revStartIndex >= mNotchIndices.flexIndices[0].Item1 - 1)
            throw new Exception ("Flex tooling index conflicts with positions at post50pc or post25pc+1");
      }

      int ix25 = mNotchIndices.segIndexAt25pc;
      int ixPost25 = mNotchIndices.segIndexAtWJTPost25pc;
      int ix50 = mNotchIndices.segIndexAt50pc;
      int ixPost50 = mNotchIndices.segIndexAtWJTPost50pc;

      if (mNotchIndices.flexIndices.Count == 1) {
         int f0i1 = mNotchIndices.flexIndices[0].Item1; 
         int f0i2 = mNotchIndices.flexIndices[0].Item2;
         int f0i3 = mNotchIndices.flexIndices[0].Item3; 
         int f0i4 = mNotchIndices.flexIndices[0].Item4;

         if ((ixPost25 != -1 && revStartIndex > ixPost25 && ixPost25 > f0i4 && f0i4 > f0i1) 
                     || revStartIndex > f0i4 && f0i4 > f0i1) {
            // @50 > @25 > f0
            if (ixPost25 != -1) {
               reverseSequences.Add (CreateNotchSequence (revStartIndex, ixPost25 + 1, NotchSectionType.MachineToolingReverse));
               reverseSequences.Add (CreateNotchSequence (ixPost25, ixPost25, NotchSectionType.WireJointTraceJumpReverse));
               reverseSequences.Add (CreateNotchSequence (ix25, f0i4 + 1, NotchSectionType.MachineToolingReverse));
            } else 
               reverseSequences.Add (CreateNotchSequence (revStartIndex, f0i4 + 1, NotchSectionType.MachineToolingReverse));

            reverseSequences.Add (CreateNotchSequence (f0i4, f0i4, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f0i3, f0i2, NotchSectionType.MachineFlexToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f0i1, f0i1, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f0i1 - 1, 0, NotchSectionType.MachineToolingReverse));
         } else if ((ixPost25 != -1 && revStartIndex > f0i4 && f0i4 > f0i1 && f0i1 > ixPost25) 
                     || revStartIndex > f0i4 && f0i4 > f0i1 && f0i1 > 0) {
            // 50 > f0 > 25 > 0
            reverseSequences.Add (CreateNotchSequence (revStartIndex, f0i4 + 1, NotchSectionType.MachineToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f0i4, f0i4, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f0i3, f0i2, NotchSectionType.MachineFlexToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f0i1, f0i1, NotchSectionType.WireJointTraceJumpReverse));
            if (ixPost25 != -1) {
               reverseSequences.Add (CreateNotchSequence (f0i1 - 1, ixPost25 + 1, NotchSectionType.MachineToolingReverse));
               reverseSequences.Add (CreateNotchSequence (ixPost25, ixPost25, NotchSectionType.WireJointTraceJumpReverse));
               reverseSequences.Add (CreateNotchSequence (ix25, 0, NotchSectionType.MachineToolingReverse));
            } else 
               reverseSequences.Add (CreateNotchSequence (f0i1 - 1, 0, NotchSectionType.MachineToolingReverse));
         } else if ((ixPost25 != -1 && f0i1 > ixPost50 && ixPost50 > ixPost25) 
                        || f0i1 > ixPost50 && ixPost50 > 0) {
            // f0 > 50 > 25
            if (ixPost25 != -1) {
               reverseSequences.Add (CreateNotchSequence (revStartIndex, ixPost25 + 1, NotchSectionType.MachineToolingReverse));
               reverseSequences.Add (CreateNotchSequence (ixPost25, ixPost25, NotchSectionType.WireJointTraceJumpReverse));
               reverseSequences.Add (CreateNotchSequence (ix25, 0, NotchSectionType.MachineToolingReverse));
            } else reverseSequences.Add (CreateNotchSequence (revStartIndex, 0, NotchSectionType.MachineToolingReverse));
         } else 
            throw new Exception ("Conflicting indices from Post50 through @25 with flex indices");
      } else if (mNotchIndices.flexIndices.Count == 2) {
         int f0i1 = mNotchIndices.flexIndices[0].Item1; int f0i2 = mNotchIndices.flexIndices[0].Item2;
         int f0i3 = mNotchIndices.flexIndices[0].Item3; int f0i4 = mNotchIndices.flexIndices[0].Item4;
         int f1i1 = mNotchIndices.flexIndices[1].Item1; int f1i2 = mNotchIndices.flexIndices[1].Item2;
         int f1i3 = mNotchIndices.flexIndices[1].Item3; int f1i4 = mNotchIndices.flexIndices[1].Item4;

         if ((ixPost25 != -1 && ix50 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > ix25) 
                        || ix50 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > 0) {
            // 50 > f1 > f0 > 25 > 0
            reverseSequences.Add (CreateNotchSequence (revStartIndex, f1i4 + 1, NotchSectionType.MachineToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f1i4, f1i4, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f1i3, f1i2, NotchSectionType.MachineFlexToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f1i1, f1i1, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f1i1 - 1, f0i4, NotchSectionType.MachineToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f0i4, f0i4, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f0i3, f0i2, NotchSectionType.MachineFlexToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f0i1, f0i1, NotchSectionType.WireJointTraceJumpReverse));
            if (ixPost25 != -1) {
               reverseSequences.Add (CreateNotchSequence (f0i1 - 1, ixPost25, NotchSectionType.MachineToolingReverse));
               reverseSequences.Add (CreateNotchSequence (ixPost25, ixPost25, NotchSectionType.WireJointTraceJumpReverse));
               reverseSequences.Add (CreateNotchSequence (ix25, 0, NotchSectionType.MachineToolingReverse));
            } else 
               reverseSequences.Add (CreateNotchSequence (f0i1 - 1, 0, NotchSectionType.MachineToolingReverse));
         } else if ((ixPost25 != -1 && ix50 > f1i4 && f1i4 > f1i1 && f1i1 > ix25 && ix25 > f0i4 && f0i4 > f0i1 && f0i1 > 0) 
                        || (ix50 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > 0)) {
            // 50 > f1 > 25 > f0 > 0 
            reverseSequences.Add (CreateNotchSequence (revStartIndex, f1i4 + 1, NotchSectionType.MachineFlexToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f1i4, f1i4, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f1i3, f1i2, NotchSectionType.MachineFlexToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f1i1, f1i1, NotchSectionType.WireJointTraceJumpReverse));
            if (ixPost25 != -1) {
               reverseSequences.Add (CreateNotchSequence (f1i1 - 1, ixPost25, NotchSectionType.MachineToolingReverse));
               reverseSequences.Add (CreateNotchSequence (ixPost25, ixPost25, NotchSectionType.WireJointTraceJumpReverse));
               reverseSequences.Add (CreateNotchSequence (ix25, f0i4 + 1, NotchSectionType.MachineToolingReverse));
            } 
            else 
               reverseSequences.Add (CreateNotchSequence (f1i1 - 1, f0i4 + 1, NotchSectionType.MachineToolingReverse));

            reverseSequences.Add (CreateNotchSequence (f0i4, f0i4, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f0i3, f0i2, NotchSectionType.MachineFlexToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f0i1, f0i1, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f0i1 - 1, 0, NotchSectionType.MachineToolingReverse));
         } else if ((ixPost25 != -1 && ix50 > ix25 && ix25 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > 0) 
                        || ix50 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > 0) {
            // 50 > 25 > f1 > f0 > 0 
            if (ixPost25 != -1) {
               reverseSequences.Add (CreateNotchSequence (revStartIndex, ixPost25, NotchSectionType.MachineToolingReverse));
               reverseSequences.Add (CreateNotchSequence (ixPost25, ixPost25, NotchSectionType.WireJointTraceJumpReverse));
               reverseSequences.Add (CreateNotchSequence (ixPost25 - 1, f1i4 + 1, NotchSectionType.MachineToolingReverse));
            } 
            else reverseSequences.Add (CreateNotchSequence (revStartIndex, f1i4 + 1, NotchSectionType.MachineToolingReverse));

            reverseSequences.Add (CreateNotchSequence (f1i4, f1i4, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f1i3, f1i2, NotchSectionType.MachineFlexToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f1i1, f1i1, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f1i1 - 1, f0i4 + 1, NotchSectionType.MachineToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f0i4, f0i4, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f0i3, f0i2, NotchSectionType.MachineFlexToolingReverse));
            reverseSequences.Add (CreateNotchSequence (f0i1, f0i1, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (f0i1 - 1, 0, NotchSectionType.MachineToolingReverse));
         } else 
            throw new Exception ("Invalid sequence of notch points encountered");
      } else if (mNotchIndices.flexIndices.Count == 0) {
         // 50 > 25 > 0
         if (ix25 != -1) {
            reverseSequences.Add (CreateNotchSequence (revStartIndex, ixPost25 + 1, NotchSectionType.MachineToolingReverse));
            reverseSequences.Add (CreateNotchSequence (ixPost25, ixPost25, NotchSectionType.WireJointTraceJumpReverse));
            reverseSequences.Add (CreateNotchSequence (ix25, 0, NotchSectionType.MachineToolingReverse));
         } else 
            reverseSequences.Add (CreateNotchSequence (revStartIndex, 0, NotchSectionType.MachineToolingReverse));
         
      } else throw new Exception ("Invalid sequence of notch points encountered");
      return reverseSequences;
   }

   /// <summary>
   /// The following method creates Notch sequences in the forward direction of the 
   /// list of tooling, considering the notch and wirejoint points and flex sections 
   /// for various possible occurances of notch points and flex sections.
   /// </summary>
   /// <param name="mNotchIndices">The indices of the notch, wire joint points</param>
   /// <returns>A list of assembled notch sequence sections to be used for writing G Code</returns>
   /// <exception cref="Exception">The notch sequences happen in a strong directional
   /// order. If this directional/sequencial order is wrong, exception will be thrown</exception>
   List<NotchSequenceSection> CreateNotchForwardSequences (List<ToolingSegment> segs, NotchSegmentIndices mNotchIndices) {
      int forwardStartIndex = mNotchIndices.segIndexAt50pc;
      int forwardEndEndex = segs.Count - 1;
      int ix50 = mNotchIndices.segIndexAt50pc;
      //int ixPost50 = mNotchIndices.segIndexAtWJTPost50pc;
      int ix75 = mNotchIndices.segIndexAt75pc;
      int ixPost75 = mNotchIndices.segIndexAtWJTPost75pc;
      List<NotchSequenceSection> forwardSequences = [];
      int nFlexes = mNotchIndices.flexIndices.Count;
      if (nFlexes == 2) {
         int f0i1 = mNotchIndices.flexIndices[0].Item1; 
         int f0i2 = mNotchIndices.flexIndices[0].Item2;
         int f0i3 = mNotchIndices.flexIndices[0].Item3; 
         int f0i4 = mNotchIndices.flexIndices[0].Item4;
         int f1i1 = mNotchIndices.flexIndices[1].Item1; 
         int f1i2 = mNotchIndices.flexIndices[1].Item2;
         int f1i3 = mNotchIndices.flexIndices[1].Item3; 
         int f1i4 = mNotchIndices.flexIndices[1].Item4;

         // e > @75 > f1 > f0 > @50
         if ((ix75 != -1 && ix75 > f1i4 && f1i4 > f1i1  && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > forwardStartIndex) 
                        || (f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > forwardStartIndex)) {
            forwardSequences.Add (CreateNotchSequence (forwardStartIndex, f0i1 - 1, NotchSectionType.MachineToolingForward));
            forwardSequences.Add (CreateNotchSequence (f0i1, f0i1, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f0i2, f0i3, NotchSectionType.MachineFlexToolingForward));
            forwardSequences.Add (CreateNotchSequence (f0i4, f0i4, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f0i4 + 1, f1i1 - 1, NotchSectionType.MachineToolingForward));
            forwardSequences.Add (CreateNotchSequence (f1i1, f1i1, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f1i2, f1i3, NotchSectionType.MachineFlexToolingForward));
            forwardSequences.Add (CreateNotchSequence (f1i4, f1i4, NotchSectionType.WireJointTraceJumpForward));
            if (ix75 != -1) {
               forwardSequences.Add (CreateNotchSequence (f1i4 + 1, ix75, NotchSectionType.MachineToolingForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75, ixPost75, NotchSectionType.WireJointTraceJumpForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
            } else
               forwardSequences.Add (CreateNotchSequence (f1i4 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
         } else if ((ix75 != -1 && ix75 > ix50 && ix50 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1) 
                        || (ix50 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1)) {
            // e > @75 > @50 > f1 > f0
            if (ix75 != -1) {
               forwardSequences.Add (CreateNotchSequence (forwardStartIndex, ix75, NotchSectionType.MachineToolingForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75, ixPost75, NotchSectionType.WireJointTraceJumpForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
            } else
               forwardSequences.Add (CreateNotchSequence (forwardStartIndex, forwardEndEndex, NotchSectionType.MachineToolingForward));
         } else if ((ix75 != -1 && ix75 > f1i4 && f1i4 > f1i1 && f1i1 > forwardStartIndex) 
                        || (f1i4 > f1i1 && f1i1 > forwardStartIndex)) {
            // e > @75 > f1 > @50 > f0
            forwardSequences.Add (CreateNotchSequence (forwardStartIndex, f1i1 - 1, NotchSectionType.MachineToolingForward));
            forwardSequences.Add (CreateNotchSequence (f1i1, f1i1, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f1i2, f1i3, NotchSectionType.MachineFlexToolingForward));
            forwardSequences.Add (CreateNotchSequence (f1i4, f1i4, NotchSectionType.WireJointTraceJumpForward));
            if (ix75 != -1) {
               forwardSequences.Add (CreateNotchSequence (f1i4 + 1, ix75, NotchSectionType.MachineToolingForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75, ixPost75, NotchSectionType.WireJointTraceJumpForward));
               forwardSequences.Add (CreateNotchSequence (ix75 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
            } else
               forwardSequences.Add (CreateNotchSequence (f1i4 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
         } else if ((ix75 != -1 && f1i4 > f1i1 && f1i1 > ix75 && ix75 > f0i4 && f0i4 > f0i1 && f0i1 > ix50) 
                        || (f1i4 > f1i1 && f0i4 > f0i1 && f0i1 > ix50)) {
            // e > f1 > @75 > @f0 > @50 
            forwardSequences.Add (CreateNotchSequence (forwardStartIndex, f0i1 - 1, NotchSectionType.MachineToolingForward));
            forwardSequences.Add (CreateNotchSequence (f0i1, f0i1, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f0i2, f0i3, NotchSectionType.MachineFlexToolingForward));
            forwardSequences.Add (CreateNotchSequence (f0i4, f0i4, NotchSectionType.WireJointTraceJumpForward));
            if (ix75 != -1) {
               forwardSequences.Add (CreateNotchSequence (f0i4 + 1, ix75, NotchSectionType.MachineToolingForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75, ixPost75, NotchSectionType.WireJointTraceJumpForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
            } else
               forwardSequences.Add (CreateNotchSequence (f0i4 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
         } else if ((ix75 != -1 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > ix75 && ix75 > ix50) 
                        || (f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > ix50)) {
            // e > f1 > f0 > @75 > @50
            if (ix75 != -1) {
               forwardSequences.Add (CreateNotchSequence (forwardStartIndex, ix75, NotchSectionType.MachineToolingForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75, ixPost75, NotchSectionType.WireJointTraceJumpForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75 + 1, f0i1 - 1, NotchSectionType.MachineToolingForward));
            } else
               forwardSequences.Add (CreateNotchSequence (forwardStartIndex, f0i1 - 1, NotchSectionType.MachineToolingForward));

            forwardSequences.Add (CreateNotchSequence (f0i1, f0i1, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f0i2, f0i3, NotchSectionType.MachineFlexToolingForward));
            forwardSequences.Add (CreateNotchSequence (f0i4, f0i4, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f0i4 + 1, f1i1 - 1, NotchSectionType.MachineToolingForward));
            forwardSequences.Add (CreateNotchSequence (f1i1, f1i1, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f1i2, f1i3, NotchSectionType.MachineFlexToolingForward));
            forwardSequences.Add (CreateNotchSequence (f1i4, f1i4, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f1i4 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
         } else if ((ix75 != -1 && f1i4 > f1i1 && f1i1 > ix75 && ix75 > ix50 && ix50 > f0i4 && f0i4 > f0i1) 
                        || (f1i4 > f1i1 && ix50 > f0i4 && f0i4 > f0i1)) {
            // e > f1 > @75 > @50 > f0
            if (ix75 != -1) {
               forwardSequences.Add (CreateNotchSequence (forwardStartIndex, ix75, NotchSectionType.MachineToolingForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75, ixPost75, NotchSectionType.WireJointTraceJumpForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75 + 1, f1i1 - 1, NotchSectionType.MachineToolingForward));
            } else
               forwardSequences.Add (CreateNotchSequence (forwardStartIndex, f1i1 - 1, NotchSectionType.MachineToolingForward));

            forwardSequences.Add (CreateNotchSequence (f1i1, f1i1, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f1i2, f1i3, NotchSectionType.MachineFlexToolingForward));
            forwardSequences.Add (CreateNotchSequence (f1i4, f1i4, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f1i4 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
         } else throw new Exception ("Unknown sequence encountered");
      } else if (nFlexes == 1) {
         int f0i1 = mNotchIndices.flexIndices[0].Item1; int f0i2 = mNotchIndices.flexIndices[0].Item2;
         int f0i3 = mNotchIndices.flexIndices[0].Item3; int f0i4 = mNotchIndices.flexIndices[0].Item4;
         if ((ix75 != -1 && f0i1 > ix75 && ix75 > forwardStartIndex) 
                        || (f0i1 > forwardStartIndex)) {
            // e > f0i4 > f0i1 > @75 > @50
            if (ix75 != -1) {
               forwardSequences.Add (CreateNotchSequence (forwardStartIndex, ix75, NotchSectionType.MachineToolingForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75, ixPost75, NotchSectionType.WireJointTraceJumpForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75 + 1, f0i1 - 1, NotchSectionType.MachineToolingForward));
            } else
               forwardSequences.Add (CreateNotchSequence (forwardStartIndex, f0i1 - 1, NotchSectionType.MachineToolingForward));

            forwardSequences.Add (CreateNotchSequence (f0i1, f0i1, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f0i2, f0i3, NotchSectionType.MachineFlexToolingForward));
            forwardSequences.Add (CreateNotchSequence (f0i4, f0i4, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f0i4 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
         } else if ((ix75 != -1 && ix75 > forwardStartIndex && forwardStartIndex > f0i4) 
                        || (forwardStartIndex > f0i4)) {
            // e > @75 > @50 > foi4 > foi1 
            if (ix75 != -1) {
               forwardSequences.Add (CreateNotchSequence (forwardStartIndex, ix75, NotchSectionType.MachineToolingForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75, ixPost75, NotchSectionType.WireJointTraceJumpForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
            } else
               forwardSequences.Add (CreateNotchSequence (forwardStartIndex, forwardEndEndex, NotchSectionType.MachineToolingForward));
         } else if ((ix75 != -1 && ix75 > f0i4 && f0i4 > f0i1 && f0i1 > ix50) 
                        || (f0i4 > f0i1 && f0i1 > ix50)) {
            // e > @75 > f0i4 > f0i1 > @50
            forwardSequences.Add (CreateNotchSequence (forwardStartIndex, f0i1 - 1, NotchSectionType.MachineToolingForward));
            forwardSequences.Add (CreateNotchSequence (f0i1, f0i1, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (f0i2, f0i3, NotchSectionType.MachineFlexToolingForward));
            forwardSequences.Add (CreateNotchSequence (f0i4, f0i4, NotchSectionType.WireJointTraceJumpForward));
            if (ix75 != -1) {
               forwardSequences.Add (CreateNotchSequence (f0i4 + 1, ix75, NotchSectionType.MachineToolingForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75, ixPost75, NotchSectionType.WireJointTraceJumpForward));
               forwardSequences.Add (CreateNotchSequence (ixPost75 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
            } else
               forwardSequences.Add (CreateNotchSequence (f0i4 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
         } else 
            throw new Exception ("Invalid sequence of notch points encountered");
      } else if (nFlexes == 0) {
         if (ix75 != -1) {
            forwardSequences.Add (CreateNotchSequence (forwardStartIndex, ix75, NotchSectionType.MachineToolingForward));
            forwardSequences.Add (CreateNotchSequence (ixPost75, ixPost75, NotchSectionType.WireJointTraceJumpForward));
            forwardSequences.Add (CreateNotchSequence (ixPost75 + 1, forwardEndEndex, NotchSectionType.MachineToolingForward));
         } else 
            forwardSequences.Add (CreateNotchSequence (forwardStartIndex, forwardEndEndex, NotchSectionType.MachineToolingForward));
         
      } else 
         throw new Exception ("No of flex notch sections invalid");

      return forwardSequences;
   }

   /// <summary>
   /// Creates a sequence section to enter or re-enter the notch tooling 
   /// before or after machining the first part. The following steps are involved:
   /// <list type="number">
   /// <item>
   ///     <description>The notch tooling is machined, allowing the scrap-side material 
   ///     to connect with the required side at smaller lengths (wire joint distances). 
   ///     A direct approach to the tooling segment is completely avoided.</description>
   /// </item>
   /// <item>
   ///     <description>A cutting stroke is made from the approximate midpoint of the 
   ///     scrap-side material to the nearest edge.</description>
   /// </item>
   /// <item>
   ///     <description>Another cutting stroke is made from the midpoint to the 50%-th notch point.</description>
   /// </item>
   /// <item>
   ///     <description>Machining of the notch starts in the direction that reaches the 
   ///     0 of X the fastest.</description>
   /// </item>
   /// <item>
   ///     <description>Further cutting occurs from the midpoint to the 50%-th notch point, 
   ///     with machining in the direction opposite to the previously traced path.</description>
   /// </item>
   /// </list>
   /// </summary>
   /// <param name="reEntry">Indicates whether this is a re-entry (True) or a first-time entry (False).</param>
   /// <returns>A structure that holds the type of the sequence section.</returns>
   NotchSequenceSection CreateApproachToNotchSequence (bool reEntry = false) {
      // Create first notch sequence
      NotchSequenceSection nsq = new () { mSectionType = NotchSectionType.WireJointApproach };
      if (reEntry) 
         nsq.mSectionType = NotchSectionType.DirectApproach;
      
      return nsq;
   }

   /// <summary>
   /// This method is an internal utility to store unique set of 
   /// the index of the tooling segment against the notch point (the end
   /// point of that index-th segment). 
   /// Reason: There are cases where more than 1 notch points occur in the 
   /// same index-th tooling segment. This case is split so that one 
   /// index-th tooling segment does not contain more than 1 notch point
   /// </summary>
   /// <param name="splitToolSegs">The tooling segments that are already split at
   /// the points of interest</param>
   /// <param name="segs">The parent segments list of the tooling, which needs to be 
   /// corrected with the split segments</param>
   /// <param name="notchPointsInfo">The structure that holds the index and the single 
   /// notch point</param>
   /// <param name="index">The index at which the split has happened</param>
   void UpdateNotchPointsInfo (ref List<ToolingSegment> splitToolSegs, 
                               ref List<ToolingSegment> segs,
                               ref List<NotchPointInfo> notchPointsInfo, 
                               int index) {
      if (splitToolSegs.Count > 1) {
         segs.RemoveAt (index);
         segs.InsertRange (index, splitToolSegs);

         // Modify notchPointsInfo too
         int ptIdx = index;
         List<NotchPointInfo> ptsInfo = [];
         for (int jj = 0; jj < splitToolSegs.Count; jj++) {
            NotchPointInfo ptInfo = new (ptIdx++, splitToolSegs[jj].Item1.End, 0);
            ptsInfo.Add (ptInfo);
         }

         for (int jj = 0; jj < notchPointsInfo.Count; jj++) {
            if (notchPointsInfo[jj].mSegIndex == index) {
               notchPointsInfo.RemoveAt (jj);
               notchPointsInfo.AddRange (ptsInfo);
               break;
            }
         }

         for (int jj = ptIdx; jj < notchPointsInfo.Count; jj++) {
            var npsInfo = notchPointsInfo[jj];
            npsInfo.mSegIndex = jj;
            notchPointsInfo[jj] = npsInfo;
         }
      }
   }

   /// <summary>
   /// This method the first in the series of notch points computations. Given 
   /// the interested locations (at 25%, 50% and 75%), this method computes 
   /// the list of 3 tuples, where the first item stores the indices of the 
   /// tooling segments on which the notch point occurs and the second item in the
   /// tuple is the notch point itself on the tooling.
   /// </summary>
   void ComputeNotchPointOccurances () {
      mSegsCount = 0;
      while (mSegsCount < mPercentLength.Length) {
         if ((mSegsCount == 0 || mSegsCount == 2) && mNotchWireJointDistance < 0.5) {
            NotchPointInfo np = new (-1, new Point3 (), toPercentage(mSegsCount));
            mNotchPointsInfo.Add (np);
         } else {
            var (segIndex, npt) = Utils.GetNotchPointsOccuranceParams (mSegments, mPercentLength[mSegsCount], mCurveLeastLength);
            mSegIndices[mSegsCount] = segIndex; mNotchPoints[mSegsCount] = npt;
            mNotchPointsInfo.FindIndex (np => np.mSegIndex == segIndex);

            // Find the notch point with the specified segIndex
            var index = mNotchPointsInfo.FindIndex (np => np.mSegIndex == segIndex);
            if (index != -1) 
               mNotchPointsInfo[index].mPoints.Add (npt);
            else {
               NotchPointInfo np = new (segIndex, npt, toPercentage(mSegsCount));
               mNotchPointsInfo.Add (np);
            }
         }

         mSegsCount++;
      }
   }

   /// <summary>
   /// This method collects the tooling segments' indices for the flex section AND
   /// also collects the wire joint points which are at wireJointDistance to the start
   /// of the flex(es) and wireJointDistance from the end of the flex(es).
   /// </summary>
   /// <remarks>
   /// <note>
   /// If the wire joint distance is prescribed as zero in the settings, the wire joint
   /// points computation is not affected. A value of 2.0 units is assumed. This is because, as per the 
   /// requirement, wire joint distance prescription affects only the points at 25% and 75% of 
   /// the lengthened points on the notch.
   /// </note>
   /// </remarks>
   void ComputeWireJointPositionsOnFlexes () {
      for (int ii = 0; ii < mFlexIndices.Count; ii++) {
         mFlexIndices = GetFlexSegmentIndices (mSegments);
         Point3 preFlexSegStPt; int preFlexSegIndex;
         int segIndexPrevFlexSegStart = mFlexIndices[ii].Item1 - 1; // Index of the segment which is fully tooled
         double wireJointDist = mNotchWireJointDistance;
         if (wireJointDist < 0.5) wireJointDist = 2.0;
         (preFlexSegStPt, preFlexSegIndex) 
            = Geom.GetToolingPointAndIndexAtLength (mSegments, segIndexPrevFlexSegStart,
                     wireJointDist, mSegments[segIndexPrevFlexSegStart].Item2.Normalized (), reverseTrace: true);
         var splitToolSegs = Utils.SplitToolingSegmentsAtPoint (mSegments, preFlexSegIndex, preFlexSegStPt,
                                                                mSegments[segIndexPrevFlexSegStart].Item2.Normalized ());

         UpdateNotchPointsInfo (ref splitToolSegs, ref mSegments, ref mNotchPointsInfo, preFlexSegIndex);
         Utils.CheckSanityOfToolingSegments (mSegments);
         mFlexIndices = GetFlexSegmentIndices (mSegments);

         var (flexStPtIndex, _) = mFlexIndices[ii];
         mFlexWireJointPts.Add (mSegments[flexStPtIndex - 1].Item1.End);
         mFlexWireJointPts.Add (mSegments[flexStPtIndex].Item1.End);
         Point3 postFlexSegEndPt; int postFlexSegEndIndex;
         int segIndexFlexEnd = mFlexIndices[ii].Item2;
         (postFlexSegEndPt, postFlexSegEndIndex) = 
                  Geom.GetToolingPointAndIndexAtLength (mSegments, segIndexFlexEnd, wireJointDist,
                                                        mSegments[segIndexFlexEnd].Item2.Normalized ());
         splitToolSegs = Utils.SplitToolingSegmentsAtPoint (mSegments, postFlexSegEndIndex, postFlexSegEndPt,
                                                            mSegments[segIndexFlexEnd].Item2.Normalized ());
         UpdateNotchPointsInfo (ref splitToolSegs, ref mSegments, ref mNotchPointsInfo, postFlexSegEndIndex);
         Utils.CheckSanityOfToolingSegments (mSegments);
         mFlexIndices = GetFlexSegmentIndices (mSegments);
         mFlexWireJointPts.Add (mSegments[mFlexIndices[ii].Item2].Item1.End);
         mFlexWireJointPts.Add (mSegments[mFlexIndices[ii].Item2 + 1].Item1.End);
      }

      for (int ii = 0; ii < mNotchPointsInfo.Count; ii++) {
         if (mNotchWireJointDistance < 0.5 && ii != 1) {
            var npInfo = mNotchPointsInfo[ii];
            npInfo.mSegIndex = -1;
            mNotchPointsInfo[ii] = npInfo;
         }
      }
   }

   /// <summary>
   /// This method creates and populates various notch parameters and data structures required 
   /// for tooling operations. It performs the following actions:
   /// <list type="number">
   /// <item>
   ///     <description>Splits the list of tooling segments at all notch points, wire joint points, 
   ///     and at the start and end of flex sections.</description>
   /// </item>
   /// <item>
   ///     <description>Records indices and coordinates of the notch points where occurrences happen.</description>
   /// </item>
   /// <item>
   ///     <description>Identifies wire joint points, their lengths, and their respective indices.</description>
   /// </item>
   /// <item>
   ///     <description>Determines the indices of points at the start and end of flex sections.
   ///     <list type="bullet">
   ///         <item>
   ///             <description>The initial segment whose end marks the starting point of the flex section.
   ///             This segment's length is equivalent to the wire joint distance.</description>
   ///         </item>
   ///         <item>
   ///             <description>The segment that represents the first segment of the flex section.</description>
   ///         </item>
   ///         <item>
   ///             <description>The index of the segment that marks the end of the flex section.</description>
   ///         </item>
   ///         <item>
   ///             <description>The segment whose starting point is the ending point of the flex section.
   ///             Its length is also the wire joint distance.</description>
   ///         </item>
   ///     </list>
   ///     </description>
   /// </item>
   /// <item>
   ///     <description>Stores sequentially ordered indices for the segments and points.</description>
   /// </item>
   /// </list>
   /// </summary>
   void ComputeNotchParameters () {
      Utils.CheckSanityOfToolingSegments (mSegments);
      var fpn = Utils.GetEPlaneNormal (mToolingItem);
      if (!mToolingItem.IsNotch ()) 
         return;

      // Find the flex segment indices
      mFlexIndices = GetFlexSegmentIndices (mSegments);

      // The indices of mSegments on whose segment the 25%, 50% and 75% of the length occurs
      mSegIndices = [null, null, null]; 
      mSegsCount = 0;

      // The point on the segment which shall participate in notch tooling
      mNotchPoints = new Point3?[3];
      mNotchPointsInfo = [];

      // Compute the occurances of the notch points
      // at 25%, 50% and 75% of the total tooling lengths
      ComputeNotchPointOccurances ();

      // Find if any of the notch point is with in the flex indices
      double minThresholdLenFromNPToFlexPt = 15;
      double thresholdNotchLenForNotchApproach = 200.0;

      // Recompute or refuse the notch points if they occur within flex sections. The way to refuse the notch point
      // its participation is by setting its index = -1
      RecomputeNotchPointsAgainstFlexNotch (mSegments, mFlexIndices, ref mNotchPoints, ref mSegIndices, mPercentLength,
                                            minThresholdLenFromNPToFlexPt, thresholdNotchLenForNotchApproach);

      // Re-Populate NotchPointsInfo List 
      mNotchPointsInfo = [];
      for (int ii = 0; ii < mPercentLength.Length; ii++) {
         mNotchPointsInfo.FindIndex (np => np.mSegIndex == mSegIndices[ii]);

         // Find the notch point with the specified segIndex
         var index = mNotchPointsInfo.FindIndex (np => np.mSegIndex == mSegIndices[ii]);
         if (index != -1 && mNotchPoints[ii] != null) 
            mNotchPointsInfo[index].mPoints.Add (mNotchPoints[ii].Value);
         else {
            if (mSegIndices[ii] != null) {
               NotchPointInfo np = new (mSegIndices[ii].Value, mNotchPoints[ii].Value, toPercentage (ii));
               mNotchPointsInfo.Add (np);
            } else {
               NotchPointInfo np = new (-1, new Point3 (), toPercentage(ii));
               mNotchPointsInfo.Add (np);
            }
         }
      }

      // Split the curves and modify the indices and segments in segments and
      // in mNotchPointsInfo
      SplitToolingSegmentsAtPoints (ref mSegments, ref mNotchPointsInfo);
      
      // Run the sanity test on the segments after split
      Utils.CheckSanityOfToolingSegments (mSegments);

      // Reassign percentages 
      for (int ii = 0; ii < mNotchPointsInfo.Count; ii++) {
         var nptobj = mNotchPointsInfo[ii];
         nptobj.mPercentage = mPercentLength[ii];
         mNotchPointsInfo[ii] = nptobj;
      }

      // Compute the notch attributes
      mNotchAttrs = GetNotchAttributes (ref mSegments, ref mNotchPointsInfo, mModel, mToolingItem);

      // Check the sanity of the segments of the notch.
      Utils.CheckSanityOfToolingSegments (mSegments);

      // Compute the wire joint positions on the flanges, which are intentionally created discontinuities to allow for
      // a small strip (wire notch distance) to hold on to the otherwise cut parts, which require a minimal
      // physical force to cut away the scrap side material
      ComputeWireJointPositionsOnFlanges (mSegments, mNotchPoints, ref mNotchPointsInfo, mNotchWireJointDistance);

      // Compute the wire joint positions on the flexes. The start and end positions of the 
      // flexes are created with this wire joints
      ComputeWireJointPositionsOnFlexes ();

      // Compute the indices of notch points and wire joint skip(jump) trace points
      ComputeNotchToolingIndices (mSegments, mNotchPoints, mWireJointPts, mFlexWireJointPts);

      // Catch errors if any
      CatchErrors ();

      // Create the list of notch sequence sections. Each section is a local action directive to
      // cut with a specific category. This is also the location where the sequences shall be modified
      // based on the need.
      mNotchSequences.Clear ();
      mNotchSequences.Add (CreateApproachToNotchSequence ());

      // Assemble the tooling sequence sections
      int forwardStartIndex = mNotchIndices.segIndexAtWJTPre50pc;
      if (IsForwardFirstNotchTooling (mSegments)) {
         mNotchSequences.Add (CreateNotchSequence (mNotchIndices.segIndexAt50pc, 
                                                   mNotchIndices.segIndexAt50pc, 
                                                   NotchSectionType.GambitMachiningAt50Reverse));
         mNotchSequences.AddRange (CreateNotchForwardSequences (mSegments, mNotchIndices));
         mNotchSequences.Add (CreateNotchSequence (mSegments.Count - 1, -1, NotchSectionType.MoveToMidApproach));
         mNotchSequences.Add (CreateApproachToNotchSequence (reEntry: true));
         mNotchSequences.Add (CreateNotchSequence (mNotchIndices.segIndexAtWJTPost50pc, 
                                                   mNotchIndices.segIndexAtWJTPost50pc, 
                                                   NotchSectionType.GambitMachiningAt50Forward));
         mNotchSequences.AddRange (CreateNotchReverseSequences (mNotchIndices));

      } else {
         mNotchSequences.Add (CreateNotchSequence (mNotchIndices.segIndexAtWJTPost50pc, 
                                                   mNotchIndices.segIndexAtWJTPost50pc, 
                                                   NotchSectionType.GambitMachiningAt50Forward));
         mNotchSequences.AddRange (CreateNotchReverseSequences (mNotchIndices));
         mNotchSequences.Add (CreateNotchSequence (0, -1, NotchSectionType.MoveToMidApproach));
         mNotchSequences.Add (CreateApproachToNotchSequence (reEntry: true));
         mNotchSequences.Add (CreateNotchSequence (mNotchIndices.segIndexAt50pc, 
                                                   mNotchIndices.segIndexAt50pc, 
                                                   NotchSectionType.GambitMachiningAt50Reverse));
         mNotchSequences.AddRange (CreateNotchForwardSequences (mSegments, mNotchIndices));
      }
   }

   static double toPercentage (int count) {
      return count switch {
         0 => 25,
         1 => 50,
         _ => 75,
      };
   }

   /// <summary>
   /// This following method is used to quickly compute notch data to decide if the notch is 
   /// valid.
   /// </summary>
   /// <param name="segs">The input tooling segments</param>
   /// <param name="percentPos">The positions of the points occuring in the interested order</param>
   /// <param name="curveLeastLength">The least count length of the curve</param>
   /// <returns></returns>
   public static Tuple<int[], Point3[]> ComputeNotchPointOccuranceParams (List<ToolingSegment> segs, 
                                                                          double[] percentPos, double curveLeastLength) {
      int count = 0;
      Point3[] notchPoints = new Point3[3];
      int[] segIndices = [-1, -1, -1];
      List<NotchPointInfo> notchPointsInfo = [];
      while (count < percentPos.Length) {
         List<ToolingSegment> splitCurves = [];
         (segIndices[count], notchPoints[count]) = 
               Utils.GetNotchPointsOccuranceParams (segs, percentPos[count], curveLeastLength);
         notchPointsInfo.FindIndex (np => np.mSegIndex == segIndices[count]);

         // Find the notch point with the specified segIndex
         var index = notchPointsInfo.FindIndex (np => np.mSegIndex == segIndices[count]);
         if (index != -1) 
            notchPointsInfo[index].mPoints.Add (notchPoints[count]);
         else {
            // [Alag:Review] confirm for percentage
            NotchPointInfo np = new (segIndices[count], notchPoints[count], toPercentage (count)); 
            notchPointsInfo.Add (np);
         }
         count++;
      }

      return new Tuple<int[], Point3[]> (segIndices, notchPoints);
   }

   /// <summary>
   /// This method is an utility to check for the sanity of the notch tooling
   /// after the segments are split at points of interest and if the indices are sane
   /// and orderly. This mwethod is a temporary one. Once the notch creation expectation is
   /// stabilized and frozen, this method will be removed
   /// </summary>
   /// <exception cref="Exception">Exceptions are thrown if the conditions of the 
   /// tooling segments at their interested points (notch, wire joint, flex) are not 
   /// as expected.</exception>
   void CatchErrors () {
      int ix25 = mNotchIndices.segIndexAt25pc;
      int ixPost25 = mNotchIndices.segIndexAtWJTPost25pc;
      int ix50 = mNotchIndices.segIndexAt50pc;
      int ixPost50 = mNotchIndices.segIndexAtWJTPost50pc;
      int ixPre50 = mNotchIndices.segIndexAtWJTPre50pc;
      int ix75 = mNotchIndices.segIndexAt75pc;
      int ixPost75 = mNotchIndices.segIndexAtWJTPost75pc;
      int nSegs = mSegments.Count;
      int nFlexes = mNotchIndices.flexIndices.Count;

      if (mSegIndices[0] != null && !mSegments[ix25].Item1.End.DistTo (mNotchPoints[0].Value).EQ (0))
         throw new Exception ("Index, point variation at 25%");
      
      if (mSegIndices[1] != null && !mSegments[ix50].Item1.End.DistTo (mNotchPoints[1].Value).EQ (0))
         throw new Exception ("Index, point variation at 50%");
      
      if (mSegIndices[2] != null && !mSegments[ix75].Item1.End.DistTo (mNotchPoints[2].Value).EQ (0))
         throw new Exception ("Index, point variation at 75%");
      
      if (mWireJointPts[0] != null && !mSegments[ixPost25].Item1.End.DistTo (mWireJointPts[0].Value).EQ (0))
         throw new Exception ("Index, point variation at post 25%");
      
      if (mWireJointPts[1] != null && !mSegments[ixPre50].Item1.End.DistTo (mWireJointPts[1].Value).EQ (0))
         throw new Exception ("Index, point variation at pre 50%");
      
      if (mWireJointPts[2] != null && !mSegments[ixPost50].Item1.End.DistTo (mWireJointPts[2].Value).EQ (0))
         throw new Exception ("Index, point variation at post 50%");
      
      if (mWireJointPts[3] != null && !mSegments[ixPost75].Item1.End.DistTo (mWireJointPts[3].Value).EQ (0))
         throw new Exception ("Index, point variation at post 75%");

      if (mFlexIndices.Count == 1) {
         if (!mSegments[mNotchIndices.flexIndices[0].Item1].Item1.End.DistTo (mFlexWireJointPts[0]).EQ (0) 
               || !mSegments[mNotchIndices.flexIndices[0].Item2].Item1.End.DistTo (mFlexWireJointPts[1]).EQ (0) 
               || !mSegments[mNotchIndices.flexIndices[0].Item3].Item1.End.DistTo (mFlexWireJointPts[2]).EQ (0) 
               || !mSegments[mNotchIndices.flexIndices[0].Item4].Item1.End.DistTo (mFlexWireJointPts[3]).EQ (0))
            throw new Exception ("First Flex Index point and segment's point are different");
      } else if (mFlexIndices.Count == 2) {
         if (!mSegments[mNotchIndices.flexIndices[1].Item1].Item1.End.DistTo (mFlexWireJointPts[4]).EQ (0) 
               || !mSegments[mNotchIndices.flexIndices[1].Item2].Item1.End.DistTo (mFlexWireJointPts[5]).EQ (0) 
               || !mSegments[mNotchIndices.flexIndices[1].Item3].Item1.End.DistTo (mFlexWireJointPts[6]).EQ (0) 
               || !mSegments[mNotchIndices.flexIndices[1].Item4].Item1.End.DistTo (mFlexWireJointPts[7]).EQ (0))
            throw new Exception ("Second Flex Index point and segment's point are different");
      }
      if (mNotchIndices.segIndexAtWJTPost50pc == -1 
               || mNotchIndices.segIndexAt50pc == -1 
               || mNotchIndices.segIndexAtWJTPost50pc == -1)
         throw new Exception (" Indices of mSegments at pre/post/@50 is/are -1 ");

      if (ixPost25 != -1 && ixPost25 < ix25) 
         throw new Exception ("Index of post 25 < index at 25");

      if (ixPost50 < ix50) 
         throw new Exception ("Index of post 50 < index at 50");

      if (ix50 < ixPre50) 
         throw new Exception ("Index of post 50 < index at 50");

      if (ixPost75 != -1 && ixPost75 < ix75) 
         throw new Exception ("Index of post 75 < index at 75");

      // Check the full sequence
      if (ix25 != -1 && ix75 != -1) {
         if (mSegments.Count - 1 >= ixPost75 && ixPost75 > ix75 && ix75 > ixPost50 
               && ixPost50 > ix50 && ix50 > ixPre50 
               && ixPre50 > ixPost25 && ixPost25 > ix25 
               && ix25 >= 0) {
            ;
         } else 
            throw new Exception ("Invalid sequence of notch points");
      } else if (ix25 == -1 && ix75 == -1 && mSegments.Count - 1 >= ixPost50 
               && ixPost50 > ix50 && ix50 > ixPre50) {
         ;
      } else if (ix25 == -1) {
         if (mSegments.Count - 1 >= ixPost75 && ixPost75 > ix75 && ix75 > ixPost50 
               && ixPost50 > ix50 && ix50 > ixPre50 
               && ixPre50 >= 0) {
            ;
         }  else 
            throw new Exception ("Invalid sequence of notch points");
      } else if (ix75 == -1) {
         if (mSegments.Count - 1 >= ixPost50 
               && ixPost50 > ix50 && ix50 > ixPre50 
               && ixPre50 > ixPost25 && ixPost25 > ix25 
               && ix25 >= 0) {
            ;
         } else 
            throw new Exception ("Invalid sequence of notch points");
      }

      bool conditionMet;
      if (IsForwardFirstNotchTooling (mSegments)) {
         if (nFlexes == 2) {
            int f0i1 = mNotchIndices.flexIndices[0].Item1;
            int f0i4 = mNotchIndices.flexIndices[0].Item4;
            int f1i1 = mNotchIndices.flexIndices[1].Item1;
            int f1i4 = mNotchIndices.flexIndices[1].Item4;
            if (ix75 != -1) {
               conditionMet = ((nSegs > ixPost75 && ixPost75 > ix75 && ix75 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50) 
                                    || (nSegs > ixPost75 && ixPost75 > ix75 && ix75 > f1i4 && f1i4 > f1i1 && f1i1 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f0i4 && f0i4 > f0i1 && f0i1 >= 0) 
                                    || (nSegs > f1i4 && f1i4 > f1i1 && f1i1 > ixPost75 && ixPost75 > ix75 && ix75 > f0i4 && f0i4 > f0i1 && f0i1 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0) 
                                    || (nSegs > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > ixPost75 && ixPost75 > ix75 && ix75 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0) 
                                    || (nSegs > f1i4 && f1i4 > f1i1 && f1i1 > ixPost75 && ixPost75 > ix75 && ix75 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f0i4 && f0i4 > f0i1 && f0i1 >= 0));
               if (!conditionMet) 
                  throw new Exception ("Sequence problem");
            } else {
               conditionMet = ((nSegs > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50) 
                                    || (nSegs > f1i4 && f1i4 > f1i1 && f1i1 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f0i4 && f0i4 > f0i1 && f0i1 > 0) 
                                    || (nSegs > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0) 
                                    || (nSegs > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0) 
                                    || (nSegs > f1i4 && f1i4 > f1i1 && f1i1 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f0i4 && f0i4 > f0i1 && f0i1 > 0));
               
               if (!conditionMet) 
                  throw new Exception ("Sequence problem");
            }
         } else if (nFlexes == 1) {
            int f0i1 = mNotchIndices.flexIndices[0].Item1; 
            int f0i4 = mNotchIndices.flexIndices[0].Item4;
            if (ix75 != -1) {
               conditionMet = ((nSegs > f0i4 && f0i4 > f0i1 && f0i1 > ixPost75 && ixPost75 > ix75 && ix75 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0) 
                                    || (nSegs > ixPost75 && ixPost75 > ix75 && ix75 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f0i4 && f0i4 > f0i1 && f0i1 > 0) 
                                    || (nSegs > ixPost75 && ixPost75 > ix75 && ix75 > f0i4 && f0i4 > f0i1 && f0i1 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0));
               if (!conditionMet) 
                  throw new Exception ("Sequence problem");
            } else {
               conditionMet = ((nSegs > f0i4 && f0i4 > f0i1 && f0i1 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0) 
                                    || (nSegs > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f0i4 && f0i4 > f0i1 && f0i1 > 0) 
                                    || (nSegs > f0i4 && f0i4 > f0i1 && f0i1 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0));
               if (!conditionMet) 
                  throw new Exception ("Sequence problem");
            }
         } else if (nFlexes == 0) {
            if (ix75 != -1) 
               conditionMet = (nSegs > ixPost75 && ixPost75 > ix75 && ix75 > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0);
            else 
               conditionMet = (nSegs > ixPost50 && ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0);
            
            if (!conditionMet) 
               throw new Exception ("Sequence problem");
         }
      } else { // reverse direction notch tooling
         if (nFlexes == 1) {
            int f0i1 = mNotchIndices.flexIndices[0].Item1; 
            int f0i4 = mNotchIndices.flexIndices[0].Item4;
            if (ix25 != -1) {
               conditionMet = ((ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > ixPost25 && ixPost25 > ix25 && ix25 > f0i4 && f0i4 > f0i1 && f0i1 > 0) 
                                    || (ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f0i4 && f0i4 > f0i1 && f0i1 > ixPost25 && ixPost25 > ix25 && ix25 >= 0) 
                                    || (ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > ixPost25 && ixPost25 > ix25 && ix25 >= 0));
               if (!conditionMet) 
                  throw new Exception ("Sequence problem");
            } else {
               conditionMet = ((ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f0i4 && f0i4 > f0i1 && f0i1 > 0) 
                                    || (ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f0i4 && f0i4 > f0i1 && f0i1 > 0) 
                                    || (ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0));
               if (!conditionMet) 
                  throw new Exception ("Invalid sequence of notch points");
            }
         } else if (nFlexes == 2) {
            int f0i1 = mNotchIndices.flexIndices[0].Item1; 
            int f0i4 = mNotchIndices.flexIndices[0].Item4;
            int f1i1 = mNotchIndices.flexIndices[1].Item1; 
            int f1i4 = mNotchIndices.flexIndices[1].Item4;

            if (ix25 != -1) {
               conditionMet = ((ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > ixPost25 && ixPost25 > ix25 && ix25 >= 0) 
                                 || (ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f1i4 && f1i4 > f1i1 && f1i1 > ixPost25 && ixPost25 > ix25 && ix25 > f0i4 && f0i4 > f0i1 && f0i1 > 0) 
                                 || (ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > ixPost25 && ixPost25 > ix25 && ix25 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > 0));
               if (!conditionMet) 
                  throw new Exception ("Sequence problem");
            } else {
               conditionMet = ((ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > 0) 
                                 || (ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > 0) 
                                 || (ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > f1i4 && f1i4 > f1i1 && f1i1 > f0i4 && f0i4 > f0i1 && f0i1 > 0));
               if (!conditionMet) 
                  throw new Exception ("Sequence problem");
            }
         } else if (nFlexes == 0) {
            if (ix25 != -1) 
               conditionMet = (ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 > ixPost25 && ixPost25 > ix25 && ix25 >= 0);
            else 
               conditionMet = (ixPost50 > ix50 && ix50 > ixPre50 && ixPre50 >= 0);

            if (!conditionMet) 
               throw new Exception ("Sequence problem");
         }
      }

      if (mNotchWireJointDistance > 0.5) {
         if (ixPost25 != -1 && !mSegments[ixPost25].Item1.Length.LieWithin (mNotchWireJointDistance, mNotchWireJointDistance + 0.5)) 
            throw new Exception ("Wire Joint segment length @post25!= 2 )");

         if (ixPost75 != -1 && !mSegments[ixPost75].Item1.Length.LieWithin (mNotchWireJointDistance, mNotchWireJointDistance + 0.5)) 
            throw new Exception ("Wire Joint segment length @post75!= 2 )");

         if (!mSegments[ixPost50].Item1.Length.LieWithin (mNotchWireJointDistance, mNotchWireJointDistance + 0.5)) 
            throw new Exception ("Wire Joint segment length @post50!= 2 )");

         if (!mSegments[ix50].Item1.Length.LieWithin (mNotchWireJointDistance, mNotchWireJointDistance + 0.5)) 
            throw new Exception ("Wire Joint segment length @post50!= 2 )");
      }
   }
   #endregion

   #region G Code writer
   /// <summary>
   /// This method writes the G Code for the notch tooling. 
   /// Prerequisite: The method ComputeNotchParameters() should be
   /// called before calling this method. 
   /// </summary>
   /// <exception cref="Exception">Exception will be thrown if the indices do not conform to
   /// the order.</exception>
   public void WriteNotch () {
      var outwardVec = mNotchAttrs[1].Item5;
      var outwardVecDir = outwardVec.Normalized ();

      // @Notchpoint 50
      Point3 notchPointAt50pc = mSegments[mNotchIndices.segIndexAt50pc].Item1.End;
      Point3 nMid1 = notchPointAt50pc + outwardVec * 0.5;
      double gap = NotchWireJointDistance > 0.5 ? NotchWireJointDistance : 2.0;
      Point3 nMid2 = nMid1 - outwardVecDir * gap;
      var n1 = nMid1 + XForm4.mYAxis * NotchApproachLength;
      var n2 = nMid2 + XForm4.mYAxis * NotchApproachLength;
      var (_, notchApproachStNormal, notchApproachEndNormal, _, _, _, _) = mNotchAttrs[1];
      mBlockCutLength = mCutLengthTillPrevTooling;
      foreach (var notchSequence in mNotchSequences) {
         switch (notchSequence.mSectionType) {
            case NotchSectionType.WireJointApproach: {
                  Utils.EPlane currPlaneType = Utils.GetFeatureNormalPlaneType (notchApproachEndNormal);
                  List<Point3> pts = [];
                  pts.Add (nMid2); 
                  pts.Add (n2);
                  pts.Add (notchPointAt50pc + outwardVecDir);
                  pts.Add (notchPointAt50pc);
                  mGCodeGen.InitializeNotchToolingBlock (mToolingItem, pts, notchApproachStNormal,
                                                         "Notch: Wire Joint Approach to the Tooling");
                                                         mGCodeGen.EnableMachiningDirective ();

                  // *** Moving to the mid point wire joint distance ***
                  mGCodeGen.WriteLine (nMid1, notchApproachStNormal, notchApproachEndNormal, currPlaneType,
                                       mPrevPlane, Utils.GetArcPlaneFlangeType (notchApproachEndNormal.Normalized ()),
                                       mToolingItem.Name);

                  mGCodeGen.WriteLine (notchPointAt50pc + outwardVec, notchApproachStNormal,
                                       notchApproachEndNormal, currPlaneType, mPrevPlane,
                                       Utils.GetArcPlaneFlangeType (notchApproachEndNormal.Normalized ()), 
                                       mToolingItem.Name);
                  mGCodeGen.DisableMachiningDirective ();
                  mBlockCutLength += n1.DistTo (nMid1);
                  mBlockCutLength += nMid1.DistTo (notchPointAt50pc + outwardVec);
                  mGCodeGen.FinalizeNotchToolingBlock (mToolingItem, mBlockCutLength, mTotalToolingsCutLength);

                  // *** Retract and move to next machining start point n2
                  mGCodeGen.MoveToRetract (n1, notchApproachEndNormal, mToolingItem.Name);
                  mGCodeGen.MoveToMachiningStartPosition (n2, notchApproachStNormal, mToolingItem.Name);

                  pts.Clear ();
                  pts.Add (nMid1); pts.Add (n1); pts.Add (notchPointAt50pc);
                  mGCodeGen.InitializeNotchToolingBlock (mToolingItem, pts, notchApproachStNormal,
                                                         "Notch: Wire Joint Approach to the Tooling");
                                                         mGCodeGen.EnableMachiningDirective ();

                  // *** Start machining from n2 -> nMid2 -> 50% dist end point ***
                  mGCodeGen.WriteLine (nMid2, notchApproachStNormal, notchApproachEndNormal, currPlaneType,
                                       mPrevPlane, Utils.GetArcPlaneFlangeType (notchApproachEndNormal.Normalized ()),
                                       mToolingItem.Name);

                  // @Notchpoint 50
                  mGCodeGen.WriteLine (mSegments[mNotchIndices.segIndexAt50pc].Item1.End, notchApproachStNormal,
                                       notchApproachEndNormal, currPlaneType, mPrevPlane,
                                       Utils.GetArcPlaneFlangeType (notchApproachEndNormal.Normalized ()), mToolingItem.Name);
                  mGCodeGen.DisableMachiningDirective ();
                  mBlockCutLength += n2.DistTo (nMid2);
                  mBlockCutLength += nMid2.DistTo (mSegments[mNotchIndices.segIndexAt50pc].Item1.End);
                  mGCodeGen.FinalizeNotchToolingBlock (mToolingItem, mBlockCutLength, mTotalToolingsCutLength);
                  mLastPosition = mGCodeGen.GetLastToolHeadPosition ().Item1;
               }
               break;

            case NotchSectionType.DirectApproach: {
                  outwardVec = mNotchAttrs[1].Item5;
                  outwardVecDir = outwardVec.Normalized ();

                  Utils.EPlane currPlaneType = Utils.GetFeatureNormalPlaneType (notchApproachEndNormal);
                  
                  // @Notchpoint 50
                  notchPointAt50pc = mSegments[mNotchIndices.segIndexAt50pc].Item1.End;

                  List<Point3> pts = [];
                  pts.Add (notchPointAt50pc); pts.Add (mLastPosition);
                  pts.Add (n1); pts.Add (nMid1);
                  mGCodeGen.InitializeNotchToolingBlock (mToolingItem, pts, notchApproachStNormal,
                                                         "Notch: Direct Approach to the Tooling");
                  mGCodeGen.EnableMachiningDirective ();
                  mGCodeGen.WriteLine (mSegments[mNotchIndices.segIndexAt50pc].Item1.End, 
                                       notchApproachStNormal, notchApproachEndNormal, currPlaneType, mPrevPlane,
                                       Utils.GetArcPlaneFlangeType (notchApproachEndNormal.Normalized ()), mToolingItem.Name);

                  mGCodeGen.DisableMachiningDirective ();
                  mBlockCutLength += mLastPosition.DistTo (mSegments[mNotchIndices.segIndexAt50pc].Item1.End);
                  mGCodeGen.FinalizeNotchToolingBlock (mToolingItem, mBlockCutLength, mTotalToolingsCutLength);
                  mLastPosition = mGCodeGen.GetLastToolHeadPosition ().Item1;
               }
               break;

            case NotchSectionType.GambitMachiningAt50Forward:
            case NotchSectionType.GambitMachiningAt50Reverse: {
                  ToolingSegment segment = mSegments[notchSequence.mStartIndex];
                  if (notchSequence.mSectionType == NotchSectionType.GambitMachiningAt50Reverse)
                     segment = Geom.GetReversedToolingSegment (mSegments[notchSequence.mStartIndex]);

                  mGCodeGen.WriteCurve (segment, mToolingItem.Name);
                  mBlockCutLength += segment.Item1.Length;
                  mGCodeGen.FinalizeNotchToolingBlock (mToolingItem, mBlockCutLength, mTotalToolingsCutLength);
                  mLastPosition = mGCodeGen.GetLastToolHeadPosition ().Item1;
                  break;
               }

            case NotchSectionType.WireJointTraceJumpForward:
            case NotchSectionType.WireJointTraceJumpReverse: {
                  NotchAttribute notchAttr;
                  if (notchSequence.mSectionType == NotchSectionType.WireJointTraceJumpForward)
                     notchAttr = ComputeNotchAttribute (mModel, mToolingItem, mSegments, notchSequence.mStartIndex,
                                                        mSegments[notchSequence.mStartIndex].Item1.End);
                  else
                     notchAttr = ComputeNotchAttribute (mModel, mToolingItem, mSegments, notchSequence.mStartIndex,
                                                        mSegments[notchSequence.mStartIndex].Item1.Start);
                  Vector3 scrapSideNormal;
                  if (Math.Abs (mSegments[notchSequence.mStartIndex].Item2.Normalized ().Z - 1.0).EQ (0) 
                        || Math.Abs (-mSegments[notchSequence.mStartIndex].Item2.Normalized ().Y + 1.0).EQ (0) 
                        || Math.Abs (mSegments[notchSequence.mStartIndex].Item2.Normalized ().Y - 1.0).EQ (0))
                     scrapSideNormal = notchAttr.Item4;
                  else 
                     scrapSideNormal = notchAttr.Item5;

                  bool zeroVec = scrapSideNormal.IsZero;
                  Point3 pt = mSegments[notchSequence.mStartIndex].Item1.End;
                  Vector3 stNormal = mSegments[notchSequence.mStartIndex].Item2.Normalized ();
                  Vector3 endNormal = mSegments[notchSequence.mStartIndex].Item3.Normalized ();
                  string comment = "(( ** Notch: Wire Joint Jump Trace Forward Direction ** ))";
                  if (notchSequence.mSectionType == NotchSectionType.WireJointTraceJumpReverse) {
                     pt = mSegments[notchSequence.mStartIndex].Item1.Start;
                     stNormal = mSegments[notchSequence.mStartIndex].Item3.Normalized ();
                     endNormal = mSegments[notchSequence.mStartIndex].Item2.Normalized ();
                     comment = "((** Notch: Wire Joint Jump Trace Reverse Direction ** ))";
                  }

                  EFlange flangeType = Utils.GetArcPlaneFlangeType (endNormal);
                  mGCodeGen.WriteWireJointTraceForNotch (pt, stNormal, endNormal, scrapSideNormal,
                     mLastPosition, NotchApproachLength, ref mPrevPlane, flangeType, mToolingItem,
                     ref mBlockCutLength, mTotalToolingsCutLength, comment);
                  mLastPosition = mGCodeGen.GetLastToolHeadPosition ().Item1;
               }
               break;

            case NotchSectionType.MachineToolingForward: {
                  if (notchSequence.mStartIndex > notchSequence.mEndIndex)
                     throw new Exception ("In WriteNotch: MachineToolingForward : startIndex > endIndex");
                  mGCodeGen.InitializeNotchToolingBlock (mToolingItem, mSegments, mSegments[notchSequence.mStartIndex].Item2, 
                                                         notchSequence.mStartIndex, notchSequence.mEndIndex,
                                                         comment: "Notch: Machining Forward Direction");
                  mGCodeGen.EnableMachiningDirective ();
                  for (int ii = notchSequence.mStartIndex; ii <= notchSequence.mEndIndex; ii++) {
                     mExitTooling = mSegments[ii];
                     mGCodeGen.WriteCurve (mSegments[ii], mToolingItem.Name);
                     mBlockCutLength += mSegments[ii].Item1.Length;
                  }

                  mGCodeGen.DisableMachiningDirective ();
                  mLastPosition = mGCodeGen.GetLastToolHeadPosition ().Item1;
                  mGCodeGen.FinalizeNotchToolingBlock (mToolingItem, mBlockCutLength, mTotalToolingsCutLength);
               }
               break;

            case NotchSectionType.MachineToolingReverse: {
                  if (notchSequence.mStartIndex < notchSequence.mEndIndex)
                     throw new Exception ("In WriteNotch: MachineToolingReverse : startIndex < endIndex");
                  mGCodeGen.InitializeNotchToolingBlock (mToolingItem, mSegments, mSegments[notchSequence.mStartIndex].Item2,
                                                         notchSequence.mStartIndex, notchSequence.mEndIndex, 
                                                         comment: "Notch: Machining Reverse Direction");
                  mGCodeGen.EnableMachiningDirective ();
                  for (int ii = notchSequence.mStartIndex; ii >= notchSequence.mEndIndex; ii--) {
                     mExitTooling = Geom.GetReversedToolingSegment (mSegments[ii]);
                     mGCodeGen.WriteCurve (mExitTooling, mToolingItem.Name);
                     mBlockCutLength += mExitTooling.Item1.Length;
                  }

                  mGCodeGen.DisableMachiningDirective ();
                  mLastPosition = mGCodeGen.GetLastToolHeadPosition ().Item1;
                  mGCodeGen.FinalizeNotchToolingBlock (mToolingItem, mBlockCutLength, mTotalToolingsCutLength);
               }
               break;

            case NotchSectionType.MachineFlexToolingReverse: {
                  if (notchSequence.mStartIndex < notchSequence.mEndIndex) 
                     throw new Exception ("In WriteNotchGCode: MachineFlexToolingReverse : startIndex < endIndex");

                  mGCodeGen.InitializeNotchToolingBlock (mToolingItem, mSegments, mSegments[notchSequence.mStartIndex].Item2,
                                                         notchSequence.mStartIndex, notchSequence.mEndIndex, 
                                                         circularMotionCmd: false, "Notch: Flex machining Reverse Direction");
                  mGCodeGen.EnableMachiningDirective ();
                  for (int ii = notchSequence.mStartIndex; ii >= notchSequence.mEndIndex; ii--) {
                     var segment = Geom.GetReversedToolingSegment (mSegments[ii]);
                     mGCodeGen.WriteCurve (segment, mToolingItem.Name);
                     mBlockCutLength += segment.Item1.Length;
                  }

                  mGCodeGen.DisableMachiningDirective ();
                  mLastPosition = mGCodeGen.GetLastToolHeadPosition ().Item1;
                  mGCodeGen.FinalizeNotchToolingBlock (mToolingItem, mBlockCutLength, mTotalToolingsCutLength);
               }
               break;

            case NotchSectionType.MachineFlexToolingForward: {
                  if (notchSequence.mStartIndex > notchSequence.mEndIndex)
                     throw new Exception ("In WriteNotch: MachineFlexToolingForward : startIndex > endIndex");
                  mGCodeGen.InitializeNotchToolingBlock (mToolingItem, mSegments, mSegments[notchSequence.mStartIndex].Item2,
                                                         notchSequence.mStartIndex, notchSequence.mEndIndex, 
                                                         circularMotionCmd: false, "Notch: Flex machining Forward Direction");
                  mGCodeGen.EnableMachiningDirective ();
                  for (int ii = notchSequence.mStartIndex; ii <= notchSequence.mEndIndex; ii++) {
                     mGCodeGen.WriteCurve (mSegments[ii], mToolingItem.Name);
                     mBlockCutLength += mSegments[ii].Item1.Length;
                  }

                  mGCodeGen.DisableMachiningDirective ();
                  mLastPosition = mGCodeGen.GetLastToolHeadPosition ().Item1;
                  mGCodeGen.FinalizeNotchToolingBlock (mToolingItem, mBlockCutLength, mTotalToolingsCutLength);
               }
               break;

            case NotchSectionType.MoveToMidApproach: {
                  Point3 prevEndPoint = mExitTooling.Item1.End;
                  Vector3 PrevEndNormal = mExitTooling.Item3.Normalized ();
                  mGCodeGen.MoveToRetract (prevEndPoint, PrevEndNormal, mToolingItem.Name);
                  mGCodeGen.MoveToNextTooling (PrevEndNormal, mExitTooling, nMid2, notchApproachStNormal,
                                              Utils.GetArcPlaneFlangeType (PrevEndNormal), 
                                              "Moving from one end of tooling to mid of tooling", "", false);
                  mGCodeGen.MoveToMachiningStartPosition (nMid2, notchApproachStNormal, mToolingItem.Name);
                  mLastPosition = nMid2;
               }
               break;

            default:
               throw new Exception ("Undefined notch sequence");
         }
      }
   }
   #endregion

   #region Getters / Predicates
   /// <summary>
   /// This method is used to find of the notch occurs only on the endge of the 
   /// part. This case is mostly for testing purpose.
   /// </summary>
   /// <param name="model">The model to get the bounds of the tooling</param>
   /// <param name="toolingItem">The tooling item</param>
   /// <param name="percentPos">The positions of the points occuring in the interested order</param>
   /// <param name="notchWireJointDistance">The wire joint distance</param>
   /// <param name="notchApproachLength">The notch approach length</param>
   /// <returns></returns>
   public static bool IsEdgeNotch (Model3 model, Tooling toolingItem,
      double[] percentPos, double notchApproachLength, double leastCurveLength) {
      var attrs = GetNotchApproachParams (model, toolingItem, percentPos, notchApproachLength, leastCurveLength);
      if (toolingItem.IsNotch () && attrs.Count == 0) return true;
      return false;
   }

   /// <summary>
   /// This method computes the total machinable length of a notch with approach.
   /// Note: A notch with approach is that notch that does not occur on the part's 
   /// edge.
   /// </summary>
   /// <param name="model">The model, for the sake of getting the bounds</param>
   /// <param name="toolingItem">The notch tooling item</param>
   /// <param name="percentPos">The array of percentages at which notch points are desired</param>
   /// <param name="notchWireJointDistance">The gap that is intended to make the sheet metal hold up
   /// even after the cut, which shall require a little physical force to break away the scrap side</param>
   /// <param name="notchApproachLength">The length of the laser cutting line length that is desired to
   /// tread before the tooling segment is reached to cut</param>
   /// <param name="leastCurveLength">The least length of the curve (0.5 ideally) below which it is 
   /// assumed that there is no curve</param>
   /// <returns>The overall length of the cut (this includes tooling and other cutting strokes for approach etc.)</returns>
   public static double GetTotalNotchToolingLength (Model3 model, Tooling toolingItem,
      double[] percentPos, double notchWireJointDistance, double notchApproachLength, double leastCurveLength) {
      var attrs = GetNotchApproachParams (model, toolingItem, percentPos, notchApproachLength, leastCurveLength);
      double totalMachiningLength = 0;
      // Computation of total machining length
      var outwardVec = attrs[1].Item3;
      var outwardVecDir = outwardVec.Normalized ();

      // For gambit move from @50
      totalMachiningLength += 2*2; // Two times 2.0 length

      // For notch approach dist 
      int notchApproachDistCount = 0;

      // For notch approach cut ( entry)
      notchApproachDistCount += 2;

      int wireJointDistCount = 0;
      // For flexes: Subtract wirejointDist count one for each flex if wireJointDist > 0.5
      // Each wire joint trace at flex has one notchApproachDistCount added
      var segs = toolingItem.Segs.ToList ();
      var flexIndices = GetFlexSegmentIndices (segs);
      if (flexIndices.Count > 0) {
         if (notchWireJointDistance > 0.5) wireJointDistCount -= 2;
         notchApproachDistCount +=2;
         if (flexIndices.Count > 1) {
            notchApproachDistCount += 2;
            if (notchWireJointDistance > 0.5) wireJointDistCount -= 2;
         }
      }

      // Assuming that 25% and 75% cut length wire joint traces
      // exist ( wireJointDistance is not zero) and are outside 
      // the flexes.
      wireJointDistCount -= 2;
      notchApproachDistCount += 2;

      // Account for totalCutLength from above counts
      totalMachiningLength += (notchApproachDistCount*notchApproachLength);
      totalMachiningLength += (wireJointDistCount * notchWireJointDistance);

      // To account for notch approach
      Point3 notchPointAt50pc = attrs[1].Item1;
      Point3 nMid1 = notchPointAt50pc + outwardVec * 0.5;
      Point3 nMid2 = notchWireJointDistance > 0.5 ? nMid1 - outwardVecDir * notchWireJointDistance : nMid1 - outwardVecDir * 2.0;

      // nMid1 to end of the part along the outward vector
      totalMachiningLength += nMid1.DistTo (notchPointAt50pc + outwardVec);

      // Two times tracing from nMid2 (inside) to the 50% lengthed segment's end, one for 
      // initial approach and the other for re-entry
      totalMachiningLength += 2 * nMid2.DistTo (attrs[1].Item1);

      // Add the length of all the tooling segment of the notch
      foreach (var (crv,_,_) in segs) totalMachiningLength += crv.Length;
      return totalMachiningLength;
   }
   /// <summary>
   /// This method is used to compute the entry point to the notch tooling.
   /// Unlike the other features such as holes etc., where the entry is the 
   /// first segment's starting point in the list of tooling segments, Notch
   /// is handled differently, where the entry is not on the tooling or on any edge.
   /// It is an approximate midpoint from the point at 50% of the length of the tooling
   /// to the end point on the nearest boundary direction.
   /// </summary>
   /// <param name="model">Model is to get the bounds</param>
   /// <param name="toolingItem">The input tooling item of this notch</param>
   /// <param name="percentPos">The positions of the points occuring in the interested order</param>
   /// <param name="notchWireJointDistance">The wire joint distance</param>
   /// <param name="notchApproachLength">Notch Approach length</param>
   /// <param name="curveLeastLength">The least count of the curve length</param>
   /// <returns></returns>
   public static ValueTuple<Point3, Vector3> GetNotchEntry (Model3 model, Tooling toolingItem,
      double[] percentPos, double notchApproachLength, double curveLeastLength = 0.5) {
      List<Tuple<Point3, Vector3, Vector3>> attrs = [];
      var segs = toolingItem.Segs.ToList ();
      if (!toolingItem.IsNotch ()) return new ValueTuple<Point3, Vector3> (segs[0].Curve.Start, segs[0].Vec0);
      Point3[] notchPoints;
      int[] segIndices;
      (segIndices, notchPoints) = ComputeNotchPointOccuranceParams (segs, percentPos, curveLeastLength);
      var notchPointsInfo = GetNotchPointsInfo (segIndices, notchPoints, percentPos.Length);

      // Split the curves and modify the indices and segments in segments and
      // in notchPointsInfo
      SplitToolingSegmentsAtPoints (ref segs, ref notchPointsInfo);
      var notchAttrs = GetNotchAttributes (ref segs, ref notchPointsInfo, model, toolingItem);
      foreach (var notchAttr in notchAttrs) {
         var (_, _, endNormal, _, ToNearestBdyVec, _, _) = notchAttr;
         var approachEndPoint = notchAttr.Item1.End;
         if (ToNearestBdyVec.Length > notchApproachLength - Utils.EpsilonVal) {
            var res = new Tuple<Point3, Vector3, Vector3> (approachEndPoint + ToNearestBdyVec, endNormal,
               ToNearestBdyVec);
            attrs.Add (res);
         } else {
            attrs.Clear ();
            break;
         }
      }
      if (attrs.Count > 0) {
         var outwardVec = notchAttrs[1].Item5;

         // @Notchpoint 50
         Point3 notchPointAt50pc = notchAttrs[1].Item1.End;
         Point3 nMid1 = notchPointAt50pc + outwardVec * 0.5;
         var n1 = nMid1 + XForm4.mYAxis * notchApproachLength;
         return new ValueTuple<Point3, Vector3> (n1, notchAttrs[1].Item2.Normalized ());
      } else return new ValueTuple<Point3, Vector3> (segs[0].Curve.Start, segs[0].Vec0);
   }

   /// <summary>
   /// This is an utility method that creates from the indices of segments and notch points array to 
   /// NotchPointsInfo data strcuture to ascertain the uniqueness of the segment's index against a (only one)
   /// point after the input tooling segment is split.
   /// </summary>
   /// <param name="segIndices">An array of indices at which the notch points occur</param>
   /// <param name="notchPoints">The array of notch points</param>
   /// <param name="count">The counter: 0 means 25%, 1 means 50% and 2 means 75%</param>
   /// <returns>A list of the NotchPointsInfo that contains the unique set of index against
   /// the notch point</returns>
   public static List<NotchPointInfo> GetNotchPointsInfo (int[] segIndices, Point3[] notchPoints, int count) {
      List<NotchPointInfo> notchPointsInfo = [];
      for (int ii = 0; ii < count; ii++) {
         notchPointsInfo.FindIndex (np => np.mSegIndex == segIndices[ii]);

         // Find the notch point with the specified segIndex
         var index = notchPointsInfo.FindIndex (np => np.mSegIndex == segIndices[ii]);
         if (index != -1) notchPointsInfo[index].mPoints.Add (notchPoints[ii]);
         else {
            NotchPointInfo np = new (segIndices[ii], notchPoints[ii], ii == 0 ? 25 : (ii == 1 ? 50 : 75));
            notchPointsInfo.Add (np);
         }
      }
      return notchPointsInfo;
   }

   /// <summary>
   /// This method computes the vital notch parameters such as the points, normals and the
   /// direction to the nearest boundary at all the interested positions in the notch
   /// </summary>
   /// <param name="model">Model is used to get the bounds of the tooling</param>
   /// <param name="toolingItem">The input tooling item</param>
   /// <param name="percentPos">The positions of the interested points at lengths</param>
   /// <param name="notchApproachDistance">The approach distance for the notch</param>
   /// <param name="curveLeastLength">The least length of the curve, below which the curve data is 
   /// removed</param>
   /// <returns>A list of tuples that contain the notch point, normal at the point
   /// and the direction to the nearest boundary</returns>
   public static List<Tuple<Point3, Vector3, Vector3>> GetNotchApproachParams (Model3 model, Tooling toolingItem,
      double[] percentPos, double notchApproachDistance, double curveLeastLength) {
      List<Tuple<Point3, Vector3, Vector3>> attrs = [];
      var segs = toolingItem.Segs.ToList ();
      if (!toolingItem.IsNotch ()) return attrs;
      Point3[] notchPoints;
      int[] segIndices;
      (segIndices, notchPoints) = ComputeNotchPointOccuranceParams (segs, percentPos, curveLeastLength);
      var notchPointsInfo = GetNotchPointsInfo (segIndices, notchPoints, percentPos.Length);

      // Split the curves and modify the indices and segments in segments and
      // in notchPointsInfo
      SplitToolingSegmentsAtPoints (ref segs, ref notchPointsInfo);
      var notchAttrs = GetNotchAttributes (ref segs, ref notchPointsInfo, model, toolingItem);
      foreach (var notchAttr in notchAttrs) {
         var (_, _, endNormal, _, ToNearestBdyVec, _, _) = notchAttr;
         //var approachEndPoint = splitCurves[0].Item1.End;
         var approachEndPoint = notchAttr.Item1.End;
         if (ToNearestBdyVec.Length > notchApproachDistance - Utils.EpsilonVal) {
            var res = new Tuple<Point3, Vector3, Vector3> (approachEndPoint + ToNearestBdyVec, endNormal, ToNearestBdyVec);
            attrs.Add (res);
         } else {
            attrs.Clear ();
            break;
         }
      }
      return attrs;
   }

   /// <summary>
   /// This method is used to obtain the direction in which the notch
   /// shall be machined upon the first entry. 
   /// </summary>
   /// <param name="segs">The input segments of the tooling</param>
   /// <returns>True if machining be in the forward direction. False otherwise.</returns>
   bool IsForwardFirstNotchTooling (List<ToolingSegment> segs) {
      bool forwardNotchTooling;
      if (segs[0].Item1.Start.X - mModel.Bound.XMin < mModel.Bound.XMax - segs[0].Item1.Start.X) {
         if (segs[^1].Item1.End.X < segs[0].Item1.Start.X) forwardNotchTooling = true;
         else forwardNotchTooling = false;
      } else {
         if (segs[^1].Item1.End.X > segs[0].Item1.Start.X) forwardNotchTooling = true;
         else forwardNotchTooling = false;
      }
      return forwardNotchTooling;
   }

   /// <summary>
   /// This method is used to compute all the notch attributes given the 
   /// the input List of NotchPointInfo. 
   /// Important note: Before calling this method, the input tooling segments are to be split
   /// at the occurance of the notch points. The expected output of this pre-step is to have a data strcuture 
   /// (NotchPOintInfo) where each one has the segmet index and only one point at that index. 
   /// The tooling segments are split in such a way that the notch points or any other characteristic points
   /// are at the end of the index-th segment.
   /// </summary>
   /// <param name="segments">The input list of tooling segments</param>
   /// <param name="notchPointsInfo">The List of NotchPointInfo where each item as exactly one
   /// index of the segment in the list and only one point, which should be the end point of 
   /// the index-th segment in the tooling segments list</param>
   /// <param name="model">The model is used to get the bounds</param>
   /// <param name="toolingItem">The tooling item.</param>
   /// <returns></returns>
   /// <exception cref="Exception">An exception is thrown if the pre-step to split the tooling segments is not 
   /// made. This is checked if each of the NotchPointInfo has only one point for the index (of the segment)</exception>
   public static List<NotchAttribute> GetNotchAttributes (ref List<ToolingSegment> segments,
      ref List<NotchPointInfo> notchPointsInfo, Model3 model, Tooling toolingItem) {
      List<NotchAttribute> notchAttrs = [];

      // Assertion that each notch point info should have only one point after split
      // The notch point of the segIndex-th segment is the end point of the segment
      for (int ii = 0; ii < notchPointsInfo.Count; ii++) {
         if (notchPointsInfo[ii].mSegIndex == -1) continue;
         var pts = notchPointsInfo[ii].mPoints;
         if (pts.Count != 1) throw new Exception ($"GetNotchAttributes: List<NotchPointInfo> notchPointsInfo {ii}th indexth points size != 1");
      }

      // Compute the notch attributes
      for (int ii = 0; ii < notchPointsInfo.Count; ii++) {
         //if (notchPointsInfo[ii].mSegIndex == -1) continue;
         var newNotchAttr = ComputeNotchAttribute (model, toolingItem, segments, notchPointsInfo[ii].mSegIndex, notchPointsInfo[ii].mPoints[0]);
         notchAttrs.Add (newNotchAttr);
      }
      return notchAttrs;
   }
}
#endregion