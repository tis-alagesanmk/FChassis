using FChassis.Processes;
using Flux.API;
using System.IO;
using System.Text.RegularExpressions;

namespace FChassis.GCodeGen;

using static FChassis.MCSettings;
using NotchAttribute = Tuple<
        Curve3, // Split curve, whose end point is the notch point
        Vector3, // Start Normal
        Vector3, // End Normal
        Vector3, // Outward Normal along flange
        Vector3, // Vector Outward to nearest boundary
        XForm4.EAxis, // Proximal boundary direction
        bool>; // Some boolean value 
using ToolingSegment = ValueTuple<Curve3, Vector3, Vector3>;

/// <summary>
/// The following class parses any G Code and caches the G0 and G1 segments. Work is 
/// still in progress to read G2 and G3 segments. The processor has to be set with 
/// the Traces to simulate. Currently, only one G Code (file) can be used for simulation.
/// </summary>
public class GCodeParser {
   readonly List<GCodeSeg>[] mTraces = [[], []];
   public List<GCodeSeg>[] Traces { get => mTraces; }
   XForm4 mXformLH, mXformRH;
   Point3? mLHOrigin, mRHOrigin;
   double? mJobLength;
   public double? JobLength { get => mJobLength; set => mJobLength = value; }
   double? mJobWidth;
   public double? JobWidth { get => mJobWidth; set => mJobWidth = value; }
   double? mJobThickness;
   public double? JobThickness { get => mJobThickness; set => mJobThickness = value; }
   public void ClearZombies () {
      mTraces[0].Clear ();
      mTraces[1].Clear ();
   }
   public GCodeParser () { }
   bool? mLHComponent = true;
   public bool? LHComponent { get => mLHComponent; set { mLHComponent = value; } }
   int? mHead;

   void EvaluateMachineXFm () {
      // For LH component
      mXformLH = new XForm4 ();
      mXformLH.Translate (new Vector3 (0.0, -JobWidth.Value / 2.0, 0.0));

      // For RH component
      mXformRH = new XForm4 ();
      mXformRH.Translate (new Vector3 (0.0, JobWidth.Value / 2.0, 0.0));
   }

   public void Parse (string filePath) {
      var fileLines = File.ReadAllLines (filePath);
      double lastX = 0, lastY = 0, lastZ = 0, lastAngle = 0;
      double iic = 0, jjc = 0, kkc = 0;
      Vector3 lastNormal = XForm4.mZAxis;
      bool firstEntry = true;
      string arcPlane = "XY";
      bool initTime = true;
      foreach (var line in fileLines) {
         if (line.StartsWith ("G18")) 
            arcPlane = "XZ";
         else if (line.StartsWith ("G17")) 
            arcPlane = "XY";

         // Variables init
         double x = lastX, y = lastY, z = lastZ, angle = lastAngle;
         Vector3 normal = lastNormal;

         string cncIdPattern = @"CNC_ID\s*=\s*(\d+)";
         Match cncIdMatch = Regex.Match (line, cncIdPattern, RegexOptions.IgnoreCase);
         if (cncIdMatch.Success) {
            mHead = int.Parse (cncIdMatch.Groups[1].Value) - 1;
            if (mHead != 0 && mHead != 1)
               throw new Exception ("Undefined head (tool)");
            mTraces[mHead.Value].Clear ();
         }
         {
            string jobLengthPattern = @"Job_Length\s*=\s*(\d+)";
            Match jobLengthMatch = Regex.Match (line, jobLengthPattern, RegexOptions.IgnoreCase);
            if (jobLengthMatch.Success) {
               JobLength = int.Parse (jobLengthMatch.Groups[1].Value);
               if (!JobLength.HasValue)
                  throw new Exception ("Job length can not be inferred from Din file");
            }
         }
         {
            string jobWidthPattern = @"Job_Width\s*=\s*(\d+)";
            Match jobWidthMatch = Regex.Match (line, jobWidthPattern, RegexOptions.IgnoreCase);
            if (jobWidthMatch.Success) {
               JobWidth = int.Parse (jobWidthMatch.Groups[1].Value);
               if (!JobWidth.HasValue)
                  throw new Exception ("Job Width can not be inferred from Din file");
            }
         }
         {
            string jobThicknessPattern = @"Job_Thickness\s*=\s*(\d+)";
            Match jobThicknessMatch = Regex.Match (line, jobThicknessPattern, RegexOptions.IgnoreCase);
            if (jobThicknessMatch.Success) {
               JobThickness = int.Parse (jobThicknessMatch.Groups[1].Value);
               if (!JobThickness.HasValue)
                  throw new Exception ("Job Thickness can not be inferred from Din file");
            }
         }

         if (initTime && JobLength.HasValue && JobWidth.HasValue && JobThickness.HasValue) {
            mLHOrigin = new Point3 (0.0, -JobWidth.Value / 2, JobThickness.Value);
            mRHOrigin = new Point3 (JobLength.Value, JobWidth.Value / 2, JobThickness.Value);
            EvaluateMachineXFm ();
            initTime = false;
         }

         string jobTypePattern = @"Job_Type\s*=\s*(\d+)";
         Match jobTypeMatch = Regex.Match (line, jobTypePattern, RegexOptions.IgnoreCase);
         if (jobTypeMatch.Success) {
            var job_type = int.Parse (jobTypeMatch.Groups[1].Value);
            if (job_type == 1) 
               LHComponent = true;
            else if (job_type == 2) 
               LHComponent = false;
            else 
               throw new Exception("Undefined Part Configuration [ Job_Type should either be 1 (LHComponent) or 2 (RHComponent)]");
         }

         // Regular expression to match G followed by 0, 1, 2, or 3 with optional whitespace (spaces, tabs)
         string gPattern = @"G\s*([0-9]+)";
         Match gMatch = Regex.Match (line, gPattern, RegexOptions.IgnoreCase);
         EGCode eGCodeVal;
         if (gMatch.Success) {
            //axisValues["G"] = double.Parse (gMatch.Groups[1].Value);
            var gval = int.Parse (gMatch.Groups[1].Value);
            eGCodeVal = gval switch {
               0 => EGCode.G0,
               1 => EGCode.G1,
               2 => EGCode.G2,
               3 => EGCode.G3,
               _ => EGCode.None
            };

            // Regular expression to match X, Y, Z, A, B, C, I, J, K, F followed by optional
            // whitespace (spaces, tabs) and then a number
            string axisPattern = @"([XYZABCIJKxyzabcijkfF])\s*([-+]?\d+(\.\d+)?)";
            MatchCollection axisMatches = Regex.Matches (line, axisPattern);

            // Loop through all matches and add them to the dictionary
            foreach (Match match in axisMatches) {
               string axis = match.Groups[1].Value.ToUpper ();
               double value = double.Parse (match.Groups[2].Value);
               
               //axisValues[axis] = value;
               switch (axis[0]) {
                  case 'X': 
                     x = value; 
                     continue;
                  case 'Y': 
                     y = value; 
                     continue;
                  case 'Z': z = 
                     value; 
                     continue;
                  case 'I': 
                     iic = value; 
                     continue;
                  case 'J': 
                     jjc = value; 
                     continue;
                  case 'K': 
                     kkc = value; 
                     continue;
                  case 'A':
                     normal = new Vector3 (0, -Math.Sin (value.D2R ()), Math.Cos (value.D2R ()));
                     continue;
                  default:
                     continue;
               }
            }
            
            string comment = string.Format ($"( Din file {0} )", filePath);
            if (!LHComponent.HasValue) 
               throw new Exception ("Unable to find the part configuration. LHComponent or RHComponent");

            if (!mHead.HasValue) 
               throw new Exception ("Unable to find the Tool '0' or '1'");

            if (eGCodeVal == EGCode.G0 || eGCodeVal == EGCode.G1) {
               var point = new Point3 (x, y, z);
               if (LHComponent.Value) 
                  point = Geom.V2P (mXformLH * point);
               else 
                  point = Geom.V2P (mXformRH * point);

               Point3 prevPoint;
               if (firstEntry) {
                  prevPoint = mLHComponent.Value 
                                    ? mLHOrigin.Value : mRHOrigin.Value;
                  firstEntry = false;
               } else 
                  prevPoint = mTraces[mHead.Value][^1].EndPoint;

               mTraces[mHead.Value].Add (new GCodeSeg (prevPoint, point, lastNormal, normal, 
                                                       eGCodeVal, EMove.Machining, comment));
            } else if (eGCodeVal == EGCode.G2 || eGCodeVal == EGCode.G3) {
               Point3 arcStartPoint = new (lastX, lastY, lastZ),
                      arcEndPoint = new (x, y, z), arcCenter;
               if (arcPlane == "XY") 
                  arcCenter = new Point3 (arcStartPoint.X + iic, arcStartPoint.Y + jjc, z);
               else 
                  arcCenter = new Point3 (arcStartPoint.X + iic, y, arcStartPoint.Z + kkc);

               if (mLHComponent.Value) {
                  arcStartPoint = Geom.V2P (mXformLH * arcStartPoint);
                  arcEndPoint = Geom.V2P (mXformLH * arcEndPoint);
                  arcCenter = Geom.V2P (mXformLH * arcCenter);
               } else {
                  arcStartPoint = Geom.V2P (mXformRH * arcStartPoint);
                  arcEndPoint = Geom.V2P (mXformRH * arcEndPoint);
                  arcCenter = Geom.V2P (mXformRH * arcCenter);
               }

               Arc3 arc = Geom.CreateArc (arcStartPoint, arcEndPoint, arcCenter, normal,
                                          eGCodeVal == EGCode.G2 
                                             ?Utils.EArcSense.CW
                                             :Utils.EArcSense.CCW);

               var radius = (arcStartPoint - arcCenter).Length;
               mTraces[mHead.Value].Add (new GCodeSeg (arc, arcStartPoint, arcEndPoint, 
                                                       arcCenter, radius, normal, eGCodeVal,
                                                       EMove.Machining, comment));
            }
         }

         lastX = x;
         lastY = y;
         lastZ = z;
         lastNormal = normal;
      }
   }
}
public class GCodeGenerator {

   #region Data members
   List<E3Flex> mFlexes;      // Flexes in the workpiece
   List<E3Plane> mPlanes;     // Planes in this workpiece
   double mThickness;         // Workpiece thickness
   readonly Point3[] mToolPos = new Point3[2];     // Tool position (for each head)
   readonly Vector3[] mToolVec = new Vector3[2];   // Tool orientaton (for each head)
   readonly Point3[] mSafePoint = new Point3[2];
   readonly bool mDebug = false;
   StreamWriter sw;
   bool mMachiningDirectiveSet = false;
   double mCurveLeastLength = 0.5;
   double[] mPercentLengths = [0.25, 0.5, 0.75];
   int mProgramNumber;
   string NCName => Path.GetFileNameWithoutExtension (Process.Workpiece.NCFileName);
   #endregion

   #region Properties
   readonly List<GCodeSeg>[] mTraces = [[], []];
   public List<GCodeSeg>[] Traces => mTraces;
   Processor mProcess;
   public Processor Process { get => mProcess; set => mProcess = value; }
   List<NotchAttribute> mNotchAttributes = [];
   public List<NotchAttribute> NotchAttributes { get { return mNotchAttributes; } }
   #endregion

   #region MCSettings Properties
   public double Standoff { get; set; }
   public double ApproachLength { get; set; }
   public bool UsePingPong { get; set; }
   public double MarkTextPosX { get; set; }
   public double MarkTextPosY { get; set; }
   public MCSettings.PartConfigType PartConfigType { get; set; }
   public string MarkText { get; set; }
   public uint SerialNumber { get; set; }
   public double PartitionRatio { get; set; }
   public MCSettings.EHeads Heads { get; set; }
   public int ProgNo { get; set; }
   public ERotate MarkAngle { get; set; }
   public bool OptimizeSequence { get; set; }
   public bool OptimizePartition { get; set; }
   public double SafetyZone { get; set; }
   public EKind[] ToolingPriority { get; set; }
   public double NotchWireJointDistance { get; set; } = 2.0;
   public double NotchApproachLength { get; set; } = 5.0;
   public bool Cutouts { get; set; } = true;
   public bool CutNotches { get; set; } = true;
   public bool CutMarks { get; set; } = true;
   public bool CutHoles { get; set; } = true;
   #endregion

   /// <summary>Resets the GCodeGenerator state to a known default, for testing</summary>
   /// There is a lot of state in the GCodeGenerator, like program numbers that
   /// keep incrementing forward. We need to reset all this state to some known defaults
   /// so that tests can be run. Otherwise, the tests become sequence dependent and if we 
   /// add or remove additional tests in the sequence, the program numbers for all subsequent
   /// parts will be incorrect, leading to spurious test failures
   public void ResetForTesting () {
      PartitionRatio = 0.5;
      Heads = MCSettings.EHeads.Both;
      ProgNo = 1;
      PartConfigType = PartConfigType.LHComponent;
      ResetBookKeepers ();
      Cutouts = CutNotches = CutMarks = CutHoles = true;
      NotchWireJointDistance = MCSettings.It.NotchWireJointDistance;
   }
   public void ResetBookKeepers () {
      mPgmNo[Utils.EFlange.Web] = 3000;
      mPgmNo[Utils.EFlange.Top] = 2000;
      mPgmNo[Utils.EFlange.Bottom] = 1000;
      mContourProgNo = ContourProgNo;
      mNotchProgNo = NotchProgNo;
      mMarkProgNo = MarkProgNo;
      mHashProgNo.Clear ();
   }

   #region GCode BookKeepers
   readonly HashSet<int> mHashProgNo = [];
   bool mWebFlangeOnly = false;
   const int WebCCNo = 3, FlangeCCNo = 2;
   int mContourProgNo = ContourProgNo;
   int mNotchProgNo = NotchProgNo;
   int mMarkProgNo = MarkProgNo;
   const int ContourProgNo = 5000;
   const int NotchProgNo = 4000;
   const int MarkProgNo = 8000;
   const int DigitProg = 6000, DigitConst = 1000, DigitPitch = 7;

   // As we are outputting two head and we need to maintain program number
   // save program number in a dictionary so that it can be used while writing
   // cutting head 2 program number
   readonly Dictionary<Utils.EFlange, int> mPgmNo = new () {
      [Utils.EFlange.Web] = 3000,
      [Utils.EFlange.Top] = 2000,
      [Utils.EFlange.Bottom] = 1000
   };

   int mNo = 0;
   int GetNotchProgNo () => mNotchProgNo;
   int GetStartMarkProgNo () => mMarkProgNo;
   int GetProgNo (Tooling item) {
      if (OptimizeSequence) return mNo++;
      if (item.IsNotch ()) return mNotchProgNo++;
      else if (item.IsCutout () || item.IsFlexHole ()) return mContourProgNo++;
      else if (item.IsMark ()) return mMarkProgNo++;
      else return mPgmNo[Utils.GetFlangeType (item)];
   }
   void OutN (StreamWriter sw, int progNo, string comment = "") {
      if (mHashProgNo.Add (progNo)) {
         sw.WriteLine ($"N{progNo}{(string.IsNullOrEmpty (comment) ? "" : $"({comment})")}");
         sw.WriteLine ($"BlockID={progNo}");
      } else throw new InvalidOperationException ($"Program number {progNo} is repeated");
   }
   #endregion

   #region GCode Options
   const int Rapid = 8000;

   bool IsSingleHead => !OptimizePartition && (PartitionRatio.EQ (0.0) 
                              || PartitionRatio.EQ (1.0));
   bool IsSingleHead1 => IsSingleHead && PartitionRatio.EQ (1.0);

   readonly double mSafeClearance = 28.0;
   readonly double mRetractClearance = 10.0;
   readonly double[] mControlDiameter = [14.7];
   #endregion

   #region Partition Data members
   /// <summary>The X-partition location</summary>
   double mXSplit;
   #endregion

   #region Tool Configuration data
   int mToolHead = 0;
   public int ToolHead { get => mToolHead; set => mToolHead = value; }
   static XForm4 mXformLHInv;
   static XForm4 mXformRHInv;
   #endregion

   #region Enums and Types
   enum EToolingShape {
      /// <summary>Circle hole</summary>
      Circle,
      HoleShape,
      NotchStart,
      NotchGiveWay,
      Notch,
      Cutout,
      
      /// <summary>Left segment of notch</summary>
      NotchL,
      NotchL2,
      
      /// <summary>Right segment of notch</summary>
      NotchR,
      NotchR2,
      Text,
      
      // If there is no hole in a flange, add this dummy
      HoleSubstituteLine,
      CutOutStart,
      CutOutStart2,
      CutOutYNeg,
      CutOutYPos,
      CutOutEnd,
      
      // Used to bring cutting head to top
      SerialStartNo,
      Arc,
      Others,
   }
   #endregion

   #region Constructors and constructing utilities
   public GCodeGenerator (Processor process) {
      mProcess = process;
      SetFromMCSettings ();
      MCSettings.It.OnSettingValuesChangedEvent += SetFromMCSettings;
   }

   /// <summary>
   /// The following method sets the properties that are local to the 
   /// GCodeGenerator. Note: Any changes to the global MCSettings properties
   /// will trigger the following method, as MCSettings.It.OnSettingValuesChangedEvent 
   /// is subscribed with SetFromMCSettings;
   /// </summary>
   public void SetFromMCSettings () {
      Standoff = MCSettings.It.Standoff;
      ApproachLength = MCSettings.It.ApproachLength;
      UsePingPong = MCSettings.It.UsePingPong;
      NotchApproachLength = MCSettings.It.NotchApproachLength;
      NotchWireJointDistance = MCSettings.It.NotchWireJointDistance;
      MarkText = MCSettings.It.MarkText;
      MarkTextPosX = MCSettings.It.MarkTextPosX;
      MarkTextPosY = MCSettings.It.MarkTextPosY;
      PartConfigType = MCSettings.It.PartConfig;
      SerialNumber = MCSettings.It.SerialNumber;
      PartitionRatio = MCSettings.It.PartitionRatio;
      Heads = MCSettings.It.Heads;
      ProgNo = MCSettings.It.ProgNo;
      MarkAngle = MCSettings.It.MarkAngle;
      ToolingPriority = MCSettings.It.ToolingPriority;
      OptimizePartition = MCSettings.It.OptimizePartition;
      OptimizeSequence = MCSettings.It.OptimizeSequence;
      SafetyZone = MCSettings.It.SafetyZone;
      ToolingPriority = MCSettings.It.ToolingPriority;
   }

   GCodeGenerator () { }

   public void OnNewWorkpiece () {
      mFlexes = Process.Workpiece.Model.Flexes.ToList ();
      mPlanes = Process.Workpiece.Model.Entities.OfType<E3Plane> ().ToList ();
      mThickness = mPlanes[0].ThickVector.Length;
      mWebFlangeOnly = mFlexes.Count == 0;
   }
   #endregion

   #region Utilies for Tool Transformations
   static void EvaluateToolConfigXForms (Processor process) {
      // Evaluate XForms wrt to the machine
      mXformLHInv = new XForm4 ();
      mXformLHInv.Translate (new Vector3 (0.0, process.Workpiece.Bound.YMin, 0.0));
      mXformLHInv.Invert ();

      // For RH component
      mXformRHInv = new XForm4 ();
      mXformRHInv.Translate (new Vector3 (0.0, process.Workpiece.Bound.YMax, 0.0));
      mXformRHInv.Invert ();
   }

   public static Point3 XfmToMachine (GCodeGenerator codeGen, Point3 ptWRTWCS) {
      Vector3 resVec;
      if (codeGen.PartConfigType == MCSettings.PartConfigType.LHComponent) resVec = mXformLHInv * ptWRTWCS;
      else resVec = mXformRHInv * ptWRTWCS;
      return Geom.V2P (resVec);
   }

   public Point3 XfmToMachine (Point3 ptWRTWCS) {
      Vector3 resVec;
      if (PartConfigType == MCSettings.PartConfigType.LHComponent) resVec = mXformLHInv * ptWRTWCS;
      else resVec = mXformRHInv * ptWRTWCS;
      return Geom.V2P (resVec);
   }

   public static XForm4 XfmToMachine (GCodeGenerator codeGen, XForm4 xFormWCS) {
      XForm4 mcXForm;
      if (codeGen.PartConfigType == MCSettings.PartConfigType.LHComponent) mcXForm = mXformLHInv * xFormWCS;
      else mcXForm = mXformRHInv * xFormWCS;
      return mcXForm;
   }

   public XForm4 XfmToMachine (XForm4 xFormWCS) {
      XForm4 mcXForm;
      if (PartConfigType == MCSettings.PartConfigType.LHComponent) mcXForm = mXformLHInv * xFormWCS;
      else mcXForm = mXformRHInv * xFormWCS;
      return mcXForm;
   }

   public static Vector3 XfmToMachineVec (GCodeGenerator codeGen, Vector3 vecWRTWCS) {
      Vector3 resVec;
      if (codeGen.PartConfigType == MCSettings.PartConfigType.LHComponent) resVec = mXformLHInv * vecWRTWCS;
      else resVec = mXformRHInv * vecWRTWCS;
      return resVec;
   }

   public Vector3 XfmToMachineVec (Vector3 vecWRTWCS) {
      Vector3 resVec;
      if (PartConfigType == MCSettings.PartConfigType.LHComponent) resVec = mXformLHInv * vecWRTWCS;
      else resVec = mXformRHInv * vecWRTWCS;
      return resVec;
   }
   #endregion

   #region Tooling filters (FChassisMachineSettings.Settings)
   Utils.EFlange[] flangeCutPriority = [Utils.EFlange.Bottom, Utils.EFlange.Top, 
                                        Utils.EFlange.Web, Utils.EFlange.Flex];
   public List<Tooling> GetToolings4Head0 () 
      => [..Process.Workpiece.Cuts.Where (cut => cut.Head == 0)
         .OrderBy (cut => Array.IndexOf(MCSettings.It.ToolingPriority, cut.Kind))
         .ThenBy (cut => cut.IsHole () ? Array.IndexOf (flangeCutPriority, Utils.GetFlangeType (cut)) : 0)
         .ThenBy (cut => cut.Start.Pt.X)];

   public List<Tooling> GetToolings4Head1 () 
      => [..Process.Workpiece.Cuts.Where (cut => cut.Head == 1)
         .OrderBy (cut => Array.IndexOf(MCSettings.It.ToolingPriority, cut.Kind))
         .ThenBy (cut => cut.IsHole () ? Array.IndexOf (flangeCutPriority, Utils.GetFlangeType (cut)) : 0)
         .ThenBy (cut => cut.Start.Pt.X)];
   #endregion

   #region Partition Implementation 
   /// <summary>This creates the optimal partition of holes so both heads are equally busy</summary>
   public void CreatePartition (bool optimize) {
      double min = 0.1, max = 0.9, mid = 0;
      int count = 15;
      if (!optimize) {
         count = 1;
         min = max = PartitionRatio;
      }

      for (int i = 0; i < count; i++) {
         mid = (min + max) / 2;
         Partition (mid);
         GetTimes (out double t0, out double t1);
         if (t0 > t1) 
            max = mid; else min = mid;
      }

      mXSplit = Math.Round (mid * Process.Workpiece.Bound.XMax, 0);
   }
   /// <summary>This partitions the cuts with a given ratio, and sorts them</summary>
   void Partition (double ratio) {
      double xf = Math.Round (Process.Workpiece.Bound.XMax * ratio, 1);
      foreach (Tooling tooling in Process.Workpiece.Cuts) {
         if (IsSingleHead) {
            if (IsSingleHead1) {
               if (Heads is MCSettings.EHeads.Left or MCSettings.EHeads.Both)
                  tooling.Head = 0;
               else if (Heads is MCSettings.EHeads.Right or MCSettings.EHeads.Both)
                  tooling.Head = 1;
               else throw new InvalidOperationException
                     ("Only single head detected while the tool configuration with Both tools prescribed!");
            }
         } else {
            if (tooling.End.Pt.X < xf)
               tooling.Head = 0;
            else
               tooling.Head = 1;
         }
      }
   }

   void GetTimes (out double t0, out double t1) {
      t0 = t1 = 0;
      double tpierce = mThickness < 7 ? 0.1 : 0.3;

      for (int i = 0; i <= 1; i++) {
         double t = 0, fr = 1.8;
         if (mThickness < 7) fr = i == 0 ? 1 : 1.3;
         else fr = i == 0 ? 1.3 : 1;
         fr *= 1000 / 60.0;    // Convert to mm/sec

         List<Tooling> cuts = Process.Workpiece.Cuts.Where (a => a.Head == i).ToList ();
         if (cuts.Count == 0) {
            if (i == 0) t0 = 0;
            else t1 = 0;
            continue;
         }
         var pt = cuts[0].Start.Pt;
         double rrx = 1000, rry = 500;
         foreach (var cut in cuts) {
            // First, find the traverse path to this location, and add the
            // traverse time (simplified)
            var vec = cut.Start.Pt - pt;
            double tx = Math.Abs (vec.X) / rrx, ty = Math.Abs (vec.Y) / rry;
            t += Math.Max (tx, ty);

            // Then, add the pierce time
            t += 0.5;

            // Then, add the cutting time (accurate)
            t += cut.Perimeter / fr;
            pt = cut.End.Pt;
         }
         if (i == 0) t0 = t;
         else t1 = t;
      }
   }
   #endregion

   #region GCode Generation Methods

   /// <summary>
   /// This method writes M14 directive in G Code if a previous
   /// immediate M14 is not found and/or M15 is found
   /// Note: This M14 directive intimates the machine controller
   /// to start the machining process
   /// </summary>
   public void EnableMachiningDirective () {
      if (!mMachiningDirectiveSet) {
         sw.WriteLine ("(( ** Machining Block **))");
         sw.WriteLine ("M14");
      }

      mMachiningDirectiveSet = true;
   }

   /// <summary>
   /// This method writes M15 directive in G Code if a 
   /// previous M14 is found.
   /// Note: The M15 directive intimates the machine controller
   /// to stop the machining process.
   /// </summary>
   public void DisableMachiningDirective () {
      if (mMachiningDirectiveSet)
         sw.WriteLine ("M15");

      mMachiningDirectiveSet = false;
   }

   /// <summary>
   /// The main entry point to generate the G Code. This encapsulates all the 
   /// cases of tooling, all the options, for the G Code. 
   /// </summary>
   /// <param name="head">The tooling head. 0 for left head(1), 1 for the right head (2)</param>
   /// <returns>Returns the no of toolings for which the GCode were generated</returns>
   public int GenerateGCode (int head) {
      ToolHead = head;
      mToolPos[0] = new Point3 (Process.Workpiece.Bound.XMin, Process.Workpiece.Bound.YMin, mSafeClearance);
      mToolPos[1] = new Point3 (Process.Workpiece.Bound.XMax, Process.Workpiece.Bound.YMax, mSafeClearance);
      mSafePoint[0] = new Point3 (Process.Workpiece.Bound.XMin, Process.Workpiece.Bound.YMin, 50);
      mSafePoint[1] = new Point3 (Process.Workpiece.Bound.XMax, Process.Workpiece.Bound.YMax, 50);
      EvaluateToolConfigXForms (Process);

      string headPos = mDebug ? $"{ToolHead + 1}" : "";
      string ncName = Process.Workpiece.NCFileName + "-" + headPos + $"({(PartConfigType == MCSettings.PartConfigType.LHComponent ? "LH" : "RH")}).din";
      string ncFolder;
      if (head == 0) {
         ncFolder = Path.Combine (Process.Workpiece.NCFilePath, "Head1");
         Directory.CreateDirectory (ncFolder);
      } else {
         ncFolder = Path.Combine (Process.Workpiece.NCFilePath, "Head2");
         Directory.CreateDirectory (ncFolder);
      }

      string fileName = Path.Combine (ncFolder, ncName);
      List<Tooling> cuts = null;
      using (sw = new StreamWriter (fileName)) {
         sw.WriteLine ("{1}({0})", ncName, ProgNo);
         sw.WriteLine ("N1");
         sw.WriteLine ($"CNC_ID={ToolHead + 1}");
         sw.WriteLine ($"Job_Length = {Math.Round (Process.Workpiece.Bound.XMax, 1)}");
         sw.WriteLine ($"Job_Width = {Math.Round (Process.Workpiece.Bound.YMax - Process.Workpiece.Bound.YMin, 1)}");
         sw.WriteLine ("Job_Height = {0}\r\nJob_Thickness = {1}", Math.Round (Process.Workpiece.Bound.ZMax - Process.Workpiece.Bound.ZMin, 1), Math.Round (mThickness, 1));
         sw.WriteLine ($"X_Partition = {mXSplit}");
         
         // Output the outer radius
         if (!mWebFlangeOnly) 
            sw.WriteLine ($"Job_O_Radius = {Math.Round (Process.Workpiece.Model.Flexes.First ().Radius + Process.Workpiece.Model.Flexes.First ().Thickness, 1)}");
         
         // Output the lh or rh component
         sw.WriteLine ($"Job_Type  = {(PartConfigType == MCSettings.PartConfigType.LHComponent ? 1 : 2)}");
         if (!string.IsNullOrEmpty (MarkText)) {
            sw.WriteLine ($"Marking_X_Pos = {Math.Round (MarkTextPosX, 1)}");
            sw.WriteLine ($"Marking_Y_Pos = {Math.Round (MarkTextPosY, 1)}");
            sw.WriteLine ($"Marking_Angle = {(int)MarkAngle}");
            sw.WriteLine ($"G253 F=\"ModelTag:{MarkText}\" E0");
         }

         sw.WriteLine ("(BF-Soffset:S1, TF-Soffset:S2, WEB_BF-Soffset:S3, WEB_TF-Soffset:S4, Marking:S3)\r\n(Block No - BF:N1001~N1999, TF:N2001~N2999, WEB:N3001~N3999, Notch:N4001~N4999, " +
                       "CutOut:N5001~N5999)" +
                       "\r\n(BlockType - 0:Flange Holes, 1:Web Block with BF reference, -1:Web Block with TF reference, 2:Notch, 3:Cutout, 4:Marking)" +
                        "\r\n(PM:Pierce Method, CM:Cutting Method, EM:End Method, ZRH: Z/Y Retract Height)\r\n(M50 - Sync On command, only in Tandem job Programs)\r\n(Job_TYPE - 1:LH JOB, 2:RH_JOB)\r\n" +
                        "(X_Correction & YZ_Correction Limit +/-5mm)");
         sw.WriteLine ("\r\n(---Don't alter above Parameters---)\r\n");

         if (head == 0) cuts = GetToolings4Head0 (); else cuts = GetToolings4Head1 ();
         bool iSingleHead = cuts.Count == Process.Workpiece.Cuts.Count;
         
         // Skip mark text - Filter out Marks ( but why??)
         if (!iSingleHead) sw.WriteLine ("M50");
         sw.WriteLine ("M15");
         sw.WriteLine ("H=LaserTableID");
         sw.WriteLine ("G61\r\nG40 E0");
         sw.WriteLine ();
         sw.WriteLine ($"(Cutting with head {ToolHead + 1})");
         int np = 0;

         foreach (var name in Enum.GetNames (typeof (Utils.EFlange))) {
            Utils.EFlange p = (Utils.EFlange)Enum.Parse (typeof (Utils.EFlange), name);
            int cnt = cuts.Count (a => Utils.GetFlangeType (a) == p && !a.IsCutout () && !a.IsNotch ());
            if (cnt == 0) 
               continue;

            if (p != Utils.EFlange.Flex)
               sw.WriteLine ($"(N{(!OptimizeSequence ? mPgmNo[p] : np) + 1} to N{cnt + (!OptimizeSequence ? mPgmNo[p] : np)} in {p} flange)");

            np += cnt;
         }

         // Now write notch information
         int cNotches = cuts.Count (a => a.Kind == EKind.Notch);
         if (cNotches != 0) sw.WriteLine ($"(N{GetNotchProgNo () + 1} to N{GetNotchProgNo () + cNotches} for notches)");
         
         // Now write Cutout part information
         int cMarks = cuts.Count (a => a.Kind == EKind.Mark);
      
         if (cMarks != 0)
            sw.WriteLine ($"(N{GetStartMarkProgNo () + 1} to N{GetStartMarkProgNo () + cMarks} for markings)");

         // GCode generation for all the eligible tooling starts here
         //----------------------------------------------------------
         WriteCuts (cuts, false);

         sw.WriteLine ("\r\nN10000");
         sw.WriteLine ("EndOfJob");
         sw.WriteLine ("G99");
      }

      string headInfo = $"for Head{ToolHead + 1}";
      return cuts.Count;
   }

   /// <summary>
   /// The following method writes the program number to intimate the 
   /// GCode controller to make appropriate actions before starting to
   /// machine the upcoming we, top or bottom flanges
   /// </summary>
   /// <param name="toolingItem">The input tooling item</param>
   /// <param name="number">Ther block number. </param>
   void SetProgNo (Tooling toolingItem, int number) {
      if (toolingItem.IsHole ()) 
         mPgmNo[Utils.GetFlangeType (toolingItem)] = number;
      else if (toolingItem.IsNotch ()) 
         mNotchProgNo = number;
      else if (toolingItem.IsCutout () || toolingItem.IsFlexHole ()) 
         mContourProgNo = number;
      else if (toolingItem.IsMark ()) 
         mMarkProgNo = number;
   }

   public bool IgnoreSafety => SafetyZone.EQ (0);

   public void WriteCurve (ToolingSegment segment, string toolingName) {
      WriteCurve (segment.Item1, segment.Item2, segment.Item3, toolingName);
   }

   //public void WriteCurve (ToolingSegment segment, string toolingName) {
   //   WriteCurve (segment.Item1, segment.Item2, segment.Item3, toolingName);
   //}

   /// <summary>
   /// This method generates G Code for machining a curve, which can either be a line or
   /// an arc.
   /// </summary>
   /// <param name="curve">Curve to machine</param>
   /// <param name="stNormal">Normal at the start of the curve</param>
   /// <param name="endNormal">Normal at the end of the curve</param>
   /// <param name="prevPlaneType">Utils.EPlane type of the previous segment</param>
   /// <param name="toolingName">Tooling name (for simulation data)</param>
   public void WriteCurve (Curve3 curve, Vector3 stNormal, Vector3 endNormal,
                           string toolingName) {
      
      /* current normal plane type (at the end) */
      var currPlaneType = Utils.GetArcPlaneType (endNormal);
      if (curve is Line3) {
         var endPoint = curve.End;/* end point*/
         WriteLine (endPoint, stNormal, endNormal, toolingName);
      } else if (curve is Arc3) {
         var (cen, _) = Geom.EvaluateCenterAndRadius (curve as Arc3);
         
         //currFlangeType = Utils.GetArcPlaneFlangeType (endNormal);
         WriteArc (curve as Arc3, currPlaneType,
                   cen, curve.Start, curve.End, 
                   stNormal, toolingName);
      }

      //prevPlaneType = currPlaneType;
   }

   /// <summary>
   /// This method writes G Code machining for Arcs/Circles.
   /// </summary>
   /// <param name="arc"> The arc in 3d, to be machined thrrough</param>
   /// <param name="arcPlaneType">Arc plane type [Utils.EPlane] to derive the if the arc is G2 or 
   /// G3</param>
   /// <param name="arcFlangeType">Arc flange type [Utils.EFlange] to omit/include the appropriate 
   /// coordinates</param>
   /// <param name="arcCenter">Center of the arc</param>
   /// <param name="arcStartPoint">Arc start point</param>
   /// <param name="arcEndPoint">End point of the arc</param>
   /// <param name="startNormal">Start normal at the beginning of the arc. The arc is considered 
   /// planar and so the end normal
   /// is same as start normal</param>
   /// <param name="toolingName">Name of the tooling for simulation and debug</param>
   /// <exception cref="ArgumentException"></exception>
   void WriteArc (Arc3 arc, Utils.EPlane arcPlaneType, Utils.EFlange arcFlangeType,
                  Point3 arcCenter, Point3 arcStartPoint, Point3 arcEndPoint, 
                  Vector3 startNormal, string toolingName) {
      Utils.EArcSense arcType;
      var apn = arcPlaneType switch {
         Utils.EPlane.Top  => XForm4.mZAxis,
         Utils.EPlane.YPos => XForm4.mYAxis,
         Utils.EPlane.YNeg => -XForm4.mYAxis,
                         _ => throw new Exception ("Arc can not be written onflex plane")
      };
      var (_, arcSense) = Geom.GetArcAngle (arc, apn);

      // Both in YNeg and YPos plane, PLC is taking a different reference
      // Z axis is decreasing while moving from top according to Eckelmann controller
      // So need to reverse clockwise and counter clockwise option
      /*^ (arcPlaneType == FChassisUtils.EPlane.Top && !Options.ReverseY)*/
      if (arcSense == Utils.EArcSense.CCW) 
         arcType = Utils.EArcSense.CCW;
      else 
         arcType = Utils.EArcSense.CW;

      arcStartPoint = Utils.MovePoint (arcStartPoint, startNormal, Standoff);
      arcEndPoint = Utils.MovePoint (arcEndPoint, startNormal, Standoff);
      arcCenter = Utils.MovePoint (arcCenter, startNormal, Standoff);
      
      // Transform the arc end point to machine coordinate system
      var mcCoordArcCenter = XfmToMachine (arcCenter);
      var mcCoordArcStPoint = XfmToMachine (arcStartPoint);
      var mcCoordArcEndPoint = XfmToMachine (arcEndPoint);
      var mcCoordArcCenter2D = Utils.ToPlane (mcCoordArcCenter, arcPlaneType);
      var mcCoordArcStPoint2D = Utils.ToPlane (mcCoordArcStPoint, arcPlaneType);
      var mcCoordArcEndPoint2D = Utils.ToPlane (mcCoordArcEndPoint, arcPlaneType);
      var arcSt2CenVec = mcCoordArcCenter2D - mcCoordArcStPoint2D; // This gives I and J
      var radius = arcSt2CenVec.Length;
      EGCode gCmd;

      if (arcType == Utils.EArcSense.CW) 
         gCmd = EGCode.G2; else gCmd = EGCode.G3;

      mTraces[ToolHead].Add (new GCodeSeg (arc, arcStartPoint, arcEndPoint, arcCenter, 
                                           radius, startNormal,
                                           gCmd, EMove.Machining, toolingName));
      mToolPos[ToolHead] = arcEndPoint;
      mToolVec[ToolHead] = startNormal;

      switch (arcFlangeType) {
         case Utils.EFlange.Web:
            sw.Write ("G{0} I{1} J{2}", 
                              arcType == Utils.EArcSense.CW ? 2 : 3, 
                              arcSt2CenVec.X.ToString ("F3"),
                              arcSt2CenVec.Y.ToString ("F3"));
            if (Utils.IsArc (arc)) 
               sw.Write (" X{0} Y{1}", 
                              mcCoordArcEndPoint2D.X.ToString ("F3"),
                              mcCoordArcEndPoint2D.Y.ToString ("F3"));
            else 
               sw.Write (" (( Circle ))");
            break;

         case Utils.EFlange.Bottom:
            sw.Write ("G{0} I{1} K{2}", 
                              arcType == Utils.EArcSense.CW ? 2 : 3, 
                              arcSt2CenVec.X.ToString ("F3"),
                              arcSt2CenVec.Y.ToString ("F3"));
            if (Utils.IsArc (arc)) 
               sw.Write (" X{0} Z{1}", 
                              mcCoordArcEndPoint2D.X.ToString ("F3"),
                              mcCoordArcEndPoint2D.Y.ToString ("F3"));
            else 
               sw.Write (" (( Circle ))");
            break;

         case Utils.EFlange.Top:
            sw.Write ("G{0} I{1} K{2}", 
                              arcType == Utils.EArcSense.CW ? 2 : 3, 
                              arcSt2CenVec.X.ToString ("F3"),
                              arcSt2CenVec.Y.ToString ("F3"));
            if (Utils.IsArc (arc)) 
               sw.Write (" X{0} Z{1}", 
                              mcCoordArcEndPoint2D.X.ToString ("F3"),
                              mcCoordArcEndPoint2D.Y.ToString ("F3"));
            else 
               sw.Write (" (( Circle ))");
            break;

         default:
            throw new ArgumentException ("Arc is ill-defined perhaps on the flex");
      }
      sw.WriteLine ();
   }

   void WriteArc (Arc3 arc, Utils.EPlane arcPlaneType, 
                  Point3 arcCenter, Point3 arcStartPoint, Point3 arcEndPoint, 
                  Vector3 startNormal, string toolingName) {
      Utils.EArcSense arcType;
      var apn = arcPlaneType switch {
         Utils.EPlane.Top  => XForm4.mZAxis,
         Utils.EPlane.YPos => XForm4.mYAxis,
         Utils.EPlane.YNeg => -XForm4.mYAxis,
                         _ => throw new Exception ("Arc can not be written onflex plane")
      };
      var (_, arcSense) = Geom.GetArcAngle (arc, apn);
      
      // Both in YNeg and YPos plane, PLC is taking a different reference
      // Z axis is decreasing while moving from top according to Eckelmann controller
      // So need to reverse clockwise and counter clockwise option
      /*^ (arcPlaneType == FChassisUtils.EPlane.Top && !Options.ReverseY)*/
      if (arcSense == Utils.EArcSense.CCW) 
         arcType = Utils.EArcSense.CCW;
      else 
         arcType = Utils.EArcSense.CW;

      arcStartPoint = Utils.MovePoint (arcStartPoint, startNormal, Standoff);
      arcEndPoint = Utils.MovePoint (arcEndPoint, startNormal, Standoff);
      arcCenter = Utils.MovePoint (arcCenter, startNormal, Standoff);
      
      // Transform the arc end point to machine coordinate system
      var mcCoordArcCenter = XfmToMachine (arcCenter);
      var mcCoordArcStPoint = XfmToMachine (arcStartPoint);
      var mcCoordArcEndPoint = XfmToMachine (arcEndPoint);
      var mcCoordArcCenter2D = Utils.ToPlane (mcCoordArcCenter, arcPlaneType);
      var mcCoordArcStPoint2D = Utils.ToPlane (mcCoordArcStPoint, arcPlaneType);
      var mcCoordArcEndPoint2D = Utils.ToPlane (mcCoordArcEndPoint, arcPlaneType);
      var arcSt2CenVec = mcCoordArcCenter2D - mcCoordArcStPoint2D; // This gives I and J
      var radius = arcSt2CenVec.Length;
      EGCode gCmd;
      if (arcType == Utils.EArcSense.CW) 
         gCmd = EGCode.G2; 
      else 
         gCmd = EGCode.G3;

      mTraces[ToolHead].Add (new GCodeSeg (arc, arcStartPoint, arcEndPoint, 
                                           arcCenter, radius, startNormal,
                                           gCmd, EMove.Machining, toolingName));
      mToolPos[ToolHead] = arcEndPoint;
      mToolVec[ToolHead] = startNormal;

      Utils.EFlange arcFlangeType;
      if (startNormal.Normalized ().Dot (XForm4.mYAxis).EQ (1)) 
         arcFlangeType = Utils.EFlange.Top;
      else if (startNormal.Normalized ().Dot (-XForm4.mYAxis).EQ (1)) 
         arcFlangeType = Utils.EFlange.Bottom;
      else if (startNormal.Normalized ().Dot (XForm4.mZAxis).EQ (1)) 
         arcFlangeType = Utils.EFlange.Web;
      else 
         throw new Exception ("Arc Plane can not be ascertained");

      switch (arcFlangeType) {
         case Utils.EFlange.Web:
            sw.Write ("G{0} I{1} J{2}", 
                           arcType == Utils.EArcSense.CW ? 2 : 3, arcSt2CenVec.X.ToString ("F3"),
                           arcSt2CenVec.Y.ToString ("F3"));
            if (Utils.IsArc (arc)) 
               sw.Write (" X{0} Y{1}", 
                           mcCoordArcEndPoint2D.X.ToString ("F3"),
                           mcCoordArcEndPoint2D.Y.ToString ("F3"));
            else 
               sw.Write (" (( Circle ))");
            break;

         case Utils.EFlange.Bottom:
            sw.Write ("G{0} I{1} K{2}", 
                           arcType == Utils.EArcSense.CW ? 2 : 3, arcSt2CenVec.X.ToString ("F3"),
                           arcSt2CenVec.Y.ToString ("F3"));
            if (Utils.IsArc (arc)) sw.Write (" X{0} Z{1}", 
                                                mcCoordArcEndPoint2D.X.ToString ("F3"),
                                                mcCoordArcEndPoint2D.Y.ToString ("F3"));
            else 
               sw.Write (" (( Circle ))");
            break;
         case Utils.EFlange.Top:
            sw.Write ("G{0} I{1} K{2}", 
                           arcType == Utils.EArcSense.CW ? 2 : 3, 
                           arcSt2CenVec.X.ToString ("F3"),
                           arcSt2CenVec.Y.ToString ("F3"));
            if (Utils.IsArc (arc)) sw.Write (" X{0} Z{1}", mcCoordArcEndPoint2D.X.ToString ("F3"),
               mcCoordArcEndPoint2D.Y.ToString ("F3"));
            else 
               sw.Write (" (( Circle ))");
            break;

         default:
            throw new ArgumentException ("Arc is ill-defined perhaps on the flex");
      }

      sw.WriteLine ();
   }

   /// <summary>
   /// This method writes GCode for the segment of motion from Retracted tool position 
   /// to the machining start tool position.</summary>
   /// <param name="toolingItem">The input tooling</param>
   /// <param name="toolingStartPosition">The new tooling start position</param>
   /// <param name="toolingStartNormal">The normal at the tooling start normal</param>
   public void MoveToMachiningStartPosition (Point3 toolingStartPosition, 
                                             Vector3 toolingStartNormal, 
                                             string toolingName) {
      // Linear Move to start machining tooling
      Point3 toolingStartPointWithMachineClearance = toolingStartPosition + toolingStartNormal * Standoff;
      string comment = "Linear move to the start of tooling";
      var mcCoordToolingStartPointWithMachineClearance =
               XfmToMachine (toolingStartPointWithMachineClearance);
      sw.WriteLine ("G1 X{0} Y{1} Z{2} F{3} (({4}))", 
                        mcCoordToolingStartPointWithMachineClearance.X.ToString ("F3"),
                        mcCoordToolingStartPointWithMachineClearance.Y.ToString ("F3"),
                        mcCoordToolingStartPointWithMachineClearance.Z.ToString ("F3"),
                        Rapid, comment);
      mTraces[ToolHead].Add (new (mToolPos[ToolHead], toolingStartPointWithMachineClearance,
                             mToolVec[ToolHead], toolingStartNormal, 
                             EGCode.G1, EMove.Retract2Machining, toolingName));
      mToolPos[ToolHead] = toolingStartPointWithMachineClearance;
      mToolVec[ToolHead] = toolingStartNormal;
   }

   /// <summary>
   /// This method specifically writes machining G Code (G1) for linear
   /// moves.
   /// </summary>
   /// <param name="endPoint">End point of the current segment.</param>
   /// <param name="startNormal">Start normal of the current linear segment, needed for simulation data</param>
   /// <param name="endNormal">End normal of the current linear segment, needed for simulation data</param>
   /// <param name="currPlaneType">Current plane type, needed if angle to be included in the G Code statement</param>
   /// <param name="previousPlaneType">Previous plane type, needed if angle to be included in the 
   /// G Code statement</param>
   /// <param name="currFlangeType">Current flange type, needed to include Y/Z coordinates in the G Code</param>
   /// <param name="toolingName">Name of the tooling for simulation purposes</param>
   public void WriteLine (Point3 endPoint, Vector3 startNormal, Vector3 endNormal,
                         Utils.EPlane currPlaneType, Utils.EPlane 
      previousPlaneType, Utils.EFlange currFlangeType, string toolingName) {
      var endPointWithMCClearance = endPoint + endNormal * Standoff;
      Point3 mcCoordEndPointWithMCClearance;
      string lineSegmentComment = "";
      double angleBetweenZAxisAndCurrToolingEndPoint;
      
      // This following check does not set angle every time for the same plane type.
      if (currPlaneType == Utils.EPlane.Flex || currPlaneType != previousPlaneType) {
         if (currPlaneType == Utils.EPlane.Flex)
            angleBetweenZAxisAndCurrToolingEndPoint 
                     = Utils.GetAngleAboutXAxis (XForm4.mZAxis, endNormal).R2D ();
         else
            angleBetweenZAxisAndCurrToolingEndPoint 
                     = Utils.GetAngle4PlaneTypeAboutXAxis (currPlaneType).R2D ();

         mcCoordEndPointWithMCClearance = XfmToMachine (endPointWithMCClearance);
         if (currFlangeType == Utils.EFlange.Bottom || currFlangeType == Utils.EFlange.Top)
            sw.WriteLine ("G1 X{0} Z{1} A{2} {3}", 
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Z.ToString ("F3"), 
                              angleBetweenZAxisAndCurrToolingEndPoint.ToString ("F3"),
                              lineSegmentComment);
         else if (currFlangeType == Utils.EFlange.Web)
            sw.WriteLine ("G1 X{0} Y{1} A{2} {3}", 
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Y.ToString ("F3"), 
                              angleBetweenZAxisAndCurrToolingEndPoint.ToString ("F3"),
                              lineSegmentComment);
         else
            sw.WriteLine ("G1 X{0} Y{1} Z{2} A{3} {4}", 
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Y.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Z.ToString ("F3"),
                              angleBetweenZAxisAndCurrToolingEndPoint.ToString ("F3"), 
                              lineSegmentComment);

         mTraces[ToolHead].Add (new GCodeSeg (mToolPos[ToolHead], endPointWithMCClearance,
                                              startNormal, endNormal, EGCode.G1, 
                                              EMove.Machining, toolingName));
         mToolPos[ToolHead] = endPointWithMCClearance;
         mToolVec[ToolHead] = endNormal;
      } else {
         mcCoordEndPointWithMCClearance = XfmToMachine (endPointWithMCClearance);
         if (currFlangeType == Utils.EFlange.Top || currFlangeType == Utils.EFlange.Bottom)
            sw.WriteLine ("G1 X{0} Z{1} {2}", 
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Z.ToString ("F3"),
                              lineSegmentComment);
         else if (currFlangeType == Utils.EFlange.Web)
            sw.WriteLine ("G1 X{0} Y{1} {2}",
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Y.ToString ("F3"), 
                              lineSegmentComment);
         else
            sw.WriteLine ("G1 X{0} Y{1} Z{2} {3}", 
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Y.ToString ("F3"), 
                              mcCoordEndPointWithMCClearance.Z.ToString ("F3"),
                                                   lineSegmentComment);
         mTraces[ToolHead].Add (new GCodeSeg (mToolPos[ToolHead], endPointWithMCClearance,
                                              startNormal, endNormal, EGCode.G1, 
                                              EMove.Machining, toolingName));
         mToolPos[ToolHead] = endPointWithMCClearance;
         mToolVec[ToolHead] = endNormal;
      }
   }

   public void WriteLine (Point3 endPoint, Vector3 startNormal, Vector3 endNormal,
                          string toolingName) {
      var endPointWithMCClearance = endPoint + endNormal * Standoff;
      Point3 mcCoordEndPointWithMCClearance;
      string lineSegmentComment = "";
      double angleBetweenZAxisAndCurrToolingEndPoint;

      bool planeChange = false;
      var angleBetweenPrevAndCurrNormal = endNormal.AngleTo (mToolVec[ToolHead]).R2D ();
      if (!angleBetweenPrevAndCurrNormal.EQ (0)) 
         planeChange = true;

      angleBetweenZAxisAndCurrToolingEndPoint = endNormal.AngleTo (XForm4.mZAxis).R2D ();
      mcCoordEndPointWithMCClearance = XfmToMachine (endPointWithMCClearance);

      //Utils.EPlane previousPlaneType = Utils.GetArcPlaneType (mToolVec[ToolHead]);
      Utils.EFlange currFlangeType = Utils.GetArcPlaneFlangeType (startNormal);
      //Utils.EPlane currPlaneType = Utils.GetArcPlaneType (startNormal);

      if (planeChange) {
         if (currFlangeType == Utils.EFlange.Bottom || currFlangeType == Utils.EFlange.Top)
            sw.WriteLine ("G1 X{0} Z{1} A{2} {3}", 
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Z.ToString ("F3"), 
                              angleBetweenZAxisAndCurrToolingEndPoint.ToString ("F3"),
                              lineSegmentComment);
         else if (currFlangeType == Utils.EFlange.Web)
            sw.WriteLine ("G1 X{0} Y{1} A{2} {3}", 
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Y.ToString ("F3"), 
                              angleBetweenZAxisAndCurrToolingEndPoint.ToString ("F3"),
                              lineSegmentComment);
         else
            sw.WriteLine ("G1 X{0} Y{1} Z{2} A{3} {4}", 
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Y.ToString ("F3"), 
                              mcCoordEndPointWithMCClearance.Z.ToString ("F3"),
                              angleBetweenZAxisAndCurrToolingEndPoint.ToString ("F3"), 
                              lineSegmentComment);
      } else {
         if (currFlangeType == Utils.EFlange.Top || currFlangeType == Utils.EFlange.Bottom)
            sw.WriteLine ("G1 X{0} Z{1} {2}", 
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Z.ToString ("F3"), 
                              lineSegmentComment);
         else if (currFlangeType == Utils.EFlange.Web)
            sw.WriteLine ("G1 X{0} Y{1} {2}", 
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Y.ToString ("F3"), 
                              lineSegmentComment);
         else
            sw.WriteLine ("G1 X{0} Y{1} Z{2} {3}", 
                              mcCoordEndPointWithMCClearance.X.ToString ("F3"),
                              mcCoordEndPointWithMCClearance.Y.ToString ("F3"), 
                              mcCoordEndPointWithMCClearance.Z.ToString ("F3"),
                              lineSegmentComment);
      }

      mTraces[ToolHead].Add (new GCodeSeg (mToolPos[ToolHead], endPointWithMCClearance,
                                           startNormal, endNormal, EGCode.G1, 
                                           EMove.Machining, toolingName));
      mToolPos[ToolHead] = endPointWithMCClearance;
      mToolVec[ToolHead] = endNormal;


      //if (currPlaneType != previousPlaneType) {
      //   if (currPlaneType == Utils.EPlane.Flex)
      //      angleBetweenZAxisAndCurrToolingEndPoint = Utils.GetAngleAboutXAxis (XForm4.mZAxis,
      //         endNormal).R2D ();
      //   else
      //      angleBetweenZAxisAndCurrToolingEndPoint = Utils.GetAngle4PlaneTypeAboutXAxis (currPlaneType).R2D ();
      //   mcCoordEndPointWithMCClearance = XfmToMachine (endPointWithMCClearance);
      //   if (currFlangeType == Utils.EFlange.Bottom || currFlangeType == Utils.EFlange.Top)
      //      sw.WriteLine ("G1 X{0} Z{1} A{2} {3}", mcCoordEndPointWithMCClearance.X.ToString("F3"),
      //      mcCoordEndPointWithMCClearance.Z.ToString("F3"), angleBetweenZAxisAndCurrToolingEndPoint.ToString("F3"),
      //      lineSegmentComment);
      //   else if (currFlangeType == Utils.EFlange.Web)
      //      sw.WriteLine ("G1 X{0} Y{1} A{2} {3}", mcCoordEndPointWithMCClearance.X.ToString("F3"),
      //      mcCoordEndPointWithMCClearance.Y.ToString("F3"), angleBetweenZAxisAndCurrToolingEndPoint.ToString("F3"),
      //      lineSegmentComment);
      //   else
      //      sw.WriteLine ("G1 X{0} Y{1} Z{2} A{3} {4}", mcCoordEndPointWithMCClearance.X.ToString("F3"),
      //         mcCoordEndPointWithMCClearance.Y.ToString("F3"), mcCoordEndPointWithMCClearance.Z.ToString("F3"),
      //          angleBetweenZAxisAndCurrToolingEndPoint.ToString("F3"), lineSegmentComment);

      //   mTraces[ToolHead].Add (new GCodeSeg (mToolPos[ToolHead], endPointWithMCClearance,
      //      startNormal, endNormal, EGCode.G1, EMove.Machining, toolingName));
      //   mToolPos[ToolHead] = endPointWithMCClearance;
      //   mToolVec[ToolHead] = endNormal;
      //} else {
      //   mcCoordEndPointWithMCClearance = XfmToMachine (endPointWithMCClearance);
      //   if (currFlangeType == Utils.EFlange.Top || currFlangeType == Utils.EFlange.Bottom)
      //      sw.WriteLine ("G1 X{0} Z{1} {2}", mcCoordEndPointWithMCClearance.X.ToString("F3"),
      //      mcCoordEndPointWithMCClearance.Z.ToString("F3"), lineSegmentComment);
      //   else if (currFlangeType == Utils.EFlange.Web)
      //      sw.WriteLine ("G1 X{0} Y{1} {2}", mcCoordEndPointWithMCClearance.X.ToString("F3"),
      //      mcCoordEndPointWithMCClearance.Y.ToString("F3"), lineSegmentComment);
      //   else
      //      sw.WriteLine ("G1 X{0} Y{1} Z{2} {3}", mcCoordEndPointWithMCClearance.X.ToString("F3"),
      //         mcCoordEndPointWithMCClearance.Y.ToString("F3"), mcCoordEndPointWithMCClearance.Z.ToString("F3"),
      //         lineSegmentComment);
      //   mTraces[ToolHead].Add (new GCodeSeg (mToolPos[ToolHead], endPointWithMCClearance,
      //      startNormal, endNormal, EGCode.G1, EMove.Machining, toolingName));
      //   mToolPos[ToolHead] = endPointWithMCClearance;
      //   mToolVec[ToolHead] = endNormal;
      //}
   }

   /// <summary>
   /// This method writes G Code for the plane of the arc to be machined
   /// </summary>
   /// <param name="arcFlangeType">Arc Flange Type is a type of Utils.EFlange</param>
   void WritePlaneForCircularMotionCommand (Utils.EFlange arcFlangeType) {
      switch (arcFlangeType) {
         case Utils.EFlange.Web: 
            sw.WriteLine ("G17"); 
            break;
         case Utils.EFlange.Top: 
            sw.WriteLine ("G18"); 
            break;
         case Utils.EFlange.Bottom: 
            sw.WriteLine ("G18"); 
            break;
      }
   }

   /// <summary>
   /// This method accounts for the notch approach length by adding a quartercircular arc
   /// lying on the material removal side or scrap side of the hole or cutout. 
   ///
   /// The start point of the arc will be on the material removal side, and the ending 
   /// point of the arc will be the mid point of the segment (arc or line). The ending point 
   /// of the arc is the start point of the arc if it is a circle. 
   /// 
   /// The tooling shall have to start from the approach arc start point and end at the 
   /// mid point of the original start segment if it is an arc or line segment, the end point 
   /// 
   /// If the segment is not a circle, in order to make this seamless, the start segment is 
   /// split into two. The modified first segment becomes the arc, second segment becomes 
   /// the second split segment of the original first curve, and finally,
   /// the last segment is the first split segment of the original first segment.
   /// 
   /// If the input segment is a circle, the modified segments list shall now have the approach arc
   /// as the first tooling segment, and the circle itself as the next tooling segment.
   /// 
   /// Algorithm: 
   /// The approach distance from the starting point in the case of circle, or the mid point
   /// in the case of line or arc, is set to 4 times the approach length set in the settings. 
   /// If this approach length is greater than the radius of the circle/arc itself, then the approach 
   /// distance is set to 2.0 mm. If it is still greater each halfed value is checked until the value is 
   /// more than 0.5mm. Otherwise, 0.5 mm is set.
   /// 
   /// The center of the approach arc is computed to be at radius distance ( approach distance of arc / 2)
   /// The first point on the arc is computed to be the above radius distance along the direction of 
   /// negative tooling
   /// 
   /// The last point of the arc is the new tooling entry point, which is the mid point of the tooling
   /// if the segment is not circle and is either arc or line OR the start point of the circle.
   /// Next, 2 points are found at the distances of 4 and then 2 mm from the new tooling entry point,
   /// (which is the end point of the arc), in the direction of negative tooling, say d1 and d2.
   ///
   /// The intermediate points are found by taking a point along the direction of  d1 and d2 from the 
   /// center of the arcs.
   /// </summary>
   /// <param name="toolingItem"></param>
   /// <returns>The modified list of the tooling segments</returns>
   List<ToolingSegment> GetSegmentsAccountedForApproachLength (Tooling toolingItem) {
      List<ToolingSegment> modifiedSegmentsList = [];
      var toolingSegmentsList = toolingItem.Segs.ToList ();
      Vector3 materialRemovalDirection; Point3 firstToolingEntryPt;
      if (!toolingItem.IsNotch () && !toolingItem.IsMark ()) {
         // E3Plane normal 
         var apn = Utils.GetEPlaneNormal (toolingItem);
         
         // Compute an appropriate approach length. From the engg team, 
         // it was asked to have a dia = approach length * 4, which is 
         // stored in approachDistOfArc. 
         // if the circle's dia is smaller than the above approachDistOfArc
         // then assign approach length from settings. 
         // Recursively find the approachDistOfArc by halving the previous value
         // until its not lesser than 0.5. 0.5 is the lower limit.
         var approachDistOfArc = ApproachLength * 4.0;
         double circleRad;
         if (toolingItem.Segs.ToList ()[0].Curve is Arc3 circle && Utils.IsCircle (circle)) {
            (_, circleRad) = Geom.EvaluateCenterAndRadius (circle);
            if (circleRad < approachDistOfArc) 
               approachDistOfArc = ApproachLength;
            while (circleRad < approachDistOfArc) {
               if (approachDistOfArc < mCurveLeastLength) 
                  break;

               approachDistOfArc *= mCurveLeastLength;
            }
         }
         
         // Compute the scrap side direction
         (firstToolingEntryPt, materialRemovalDirection) = Utils.GetMaterialRemovalSideDirection (toolingItem);
         
         // Compute the tooling direction.
         Vector3 toolingDir;
         if (toolingSegmentsList[0].Curve is Line3)
            toolingDir = toolingSegmentsList[0].Curve.End - toolingSegmentsList[0].Curve.Start;
         else
            (toolingDir, _) = Geom.EvaluateTangentAndNormalAtPoint (toolingSegmentsList[0].Curve as Arc3,
                                                                    firstToolingEntryPt, apn);
         toolingDir = toolingDir.Normalized ();
         
         // Compute new start point of the tooling, which is the start point of the quarter arc point on the 
         // scrap side of the material
         //var newToolingStPt = firstToolingEntryPt + materialRemovalDirection * approachDistOfArc;
         var approachArcRad = approachDistOfArc * 0.5;
         var arcCenter = firstToolingEntryPt + materialRemovalDirection * approachArcRad;
         var newToolingStPt = arcCenter - toolingDir * approachArcRad;
         
         // Find 2 points on the ray from newToolingStPt in the direction of -toolingDir, from the center of the arc
         var p1 = firstToolingEntryPt - toolingDir * 4.0; var p2 = firstToolingEntryPt - toolingDir * 2.0;
         
         // Compute the vectors from center of the arc to the above points
         var cp1 = (p1 - arcCenter).Normalized (); var cp2 = (p2 - arcCenter).Normalized ();
         
         // Compute the intersection of the vector cenetr to p1/p2 on the circle of the arc. These are 
         // intermediate points along the actual direction of the arc
         var ip1 = arcCenter + cp1 * approachArcRad; var ip2 = arcCenter + cp2 * approachArcRad;
         
         // Create arc, the fourth point being the midpoint of the arc or starting point
         // if its a circle.
         Arc3 arc = new (newToolingStPt, ip1, ip2, firstToolingEntryPt);
         if (Utils.IsCircle (toolingSegmentsList[0].Curve)) {
            modifiedSegmentsList.Add (new (arc, toolingSegmentsList[0].Vec0, toolingSegmentsList[0].Vec0));
            modifiedSegmentsList.Add (toolingSegmentsList[0]);
            return modifiedSegmentsList;
         } else {
            List<Point3> internalPoints = [];
            internalPoints.Add (Geom.GetMidPoint (toolingSegmentsList[0].Curve, apn));
            var splitCurves = Geom.SplitCurve (toolingSegmentsList[0].Curve, internalPoints, apn, 0.0);
            modifiedSegmentsList.Add (new (arc, toolingSegmentsList[0].Vec0, toolingSegmentsList[0].Vec0));
            modifiedSegmentsList.Add (new (splitCurves[1], toolingSegmentsList[0].Vec0, 
                                           toolingSegmentsList[0].Vec1));
            
            for (int ii = 1; ii < toolingSegmentsList.Count; ii++) 
               modifiedSegmentsList.Add (toolingSegmentsList[ii]);

            modifiedSegmentsList.Add (new (splitCurves[0], toolingSegmentsList[0].Vec0, 
                                      toolingSegmentsList[0].Vec1));
            return modifiedSegmentsList;
         }
      } else 
         return toolingSegmentsList;
   }

   /// <summary>
   /// This is the method to be called for actual machining. This takes care of 
   /// linear and circular machining.
   /// </summary>
   /// <param name="toolingSegmentsList">List of Tooling segments (of lines and arcs)</param>
   /// <param name="toolingItem">The actual tooling item. The tooling segments list might vary 
   /// from the tooling segments of the tooling item, when the segments are modified for approach 
   /// distance by adding a quarter circular arc</param>
   ToolingSegment WriteTooling (List<ToolingSegment> toolingSegmentsList, Tooling toolingItem,
                                double totalPrevCutToolingsLength, 
                                double totalToolingCutLength) {
      ToolingSegment ts;
      (var curve, var CurveStartNormal, _) = toolingSegmentsList[0];

      bool validNotch = toolingItem.IsNotch () 
                                 && !Notch.IsEdgeNotch (Process.Workpiece.Model, toolingItem,
                                                        mPercentLengths, NotchApproachLength, 
                                                        mCurveLeastLength);

      // Handle moving to machining start position for a non-edge notch
      if (validNotch) {
         var notchEntry = Notch.GetNotchEntry (Process.Workpiece.Model, toolingItem, mPercentLengths,
                                               NotchApproachLength, mCurveLeastLength);
         MoveToMachiningStartPosition (notchEntry.Item1, notchEntry.Item2, toolingItem.Name);
      } else 
         MoveToMachiningStartPosition (curve.Start, CurveStartNormal, toolingItem.Name);

      Utils.EPlane previousPlaneType = Utils.EPlane.None;
      Utils.EPlane currPlaneType;
      if (toolingItem.IsFlexFeature ()) 
         currPlaneType = Utils.EPlane.Flex;
      else 
         currPlaneType = Utils.GetFeatureNormalPlaneType (CurveStartNormal);

      if (validNotch) {
         // Create the notch data
         Notch notch = new (toolingItem, Process.Workpiece.Model, this, previousPlaneType, 
                            NotchWireJointDistance, NotchApproachLength, mPercentLengths, 
                            totalPrevCutToolingsLength, totalToolingCutLength, 
                            curveLeastLength: mCurveLeastLength);

         // Write the notch
         notch.WriteNotch ();
         mNotchAttributes.AddRange (NotchAttributes);
         SetProgNo (toolingItem, mProgramNumber);
         ts = notch.Exit;
      } else {
         // Write all other features such as Holes, Cutouts and edge notches
         EnableMachiningDirective ();
         for (int i = 0; i < toolingSegmentsList.Count; i++) {
            var (Curve, startNormal, endNormal) = toolingSegmentsList[i];
            startNormal = startNormal.Normalized ();
            endNormal = endNormal.Normalized ();
            var startPoint = Curve.Start;
            var endPoint = Curve.End;
            if (i > 0) 
               currPlaneType = Utils.GetFeatureNormalPlaneType (endNormal);

            if (Curve is Arc3) { // This is a 2d arc. 
               var arcPlaneType = Utils.GetArcPlaneType (startNormal);
               var arcFlangeType = Utils.GetArcPlaneFlangeType (startNormal);
               (var center, _) = Geom.EvaluateCenterAndRadius (Curve as Arc3);
               WriteArc (Curve as Arc3, arcPlaneType, arcFlangeType, center, 
                         startPoint, endPoint, startNormal,
                         toolingItem.Name);
            } else 
               WriteLine (endPoint, startNormal, endNormal, currPlaneType, previousPlaneType,
                          Utils.GetFlangeType (toolingItem), toolingItem.Name);
            previousPlaneType = currPlaneType;
         }

         DisableMachiningDirective ();
         ts = toolingSegmentsList[^1];
      }

      return ts;
   }

   /// <summary>
   /// This method gets the tooling shape kind of the tooling
   /// </summary>
   /// <param name="toolingItem">The input tooling</param>
   /// <returns>Returns one of Notch, HoleShape, Text, or Cutout</returns>
   /// <exception cref="NotSupportedException">This exception is thrown if 
   /// any other kind is encountered</exception>
   static EToolingShape GetToolingShapeKind (Tooling toolingItem) {
      EToolingShape shape = EToolingShape.HoleShape;
      Curve3 firstCurve = toolingItem.Segs.First ().Curve;
      if (firstCurve as Arc3 != null) {
         if (Utils.IsCircle (firstCurve))
            shape = EToolingShape.Circle;
      }  // [Alag:Review] need to use switch case
      else {
         shape = toolingItem.Kind switch {
            EKind.Notch    => EToolingShape.Notch,
            EKind.Hole     => EToolingShape.HoleShape,
            EKind.Mark     => EToolingShape.Text,
            EKind.Cutout   => EToolingShape.Cutout,
                         _ => throw new NotSupportedException ("Invalid tooling item kind encountered")
         };
      }

      return shape;
   }

   /// <summary>
   /// THis method moves the tool in rapid position to the safety position. 
   /// This the X and Y coordinate of the tool origin with Z value as 28 mm.
   /// This method registers this data only for the simulation and has no 
   /// bearing on the G Code that is being written. 
   /// </summary>
   void MoveToSafety () {
      mTraces[ToolHead].Add (new (mSafePoint[ToolHead], mToolPos[ToolHead], XForm4.mZAxis, XForm4.mZAxis,
                                  EGCode.G0, EMove.Retract2SafeZ, "No tooling"));
      mToolVec[ToolHead] = XForm4.mZAxis;
   }

   /// <summary>
   /// This method moves the tool from current machining position, end of tooling to 
   /// the retracted position only for the simulation. This has no bearing on the G Code 
   /// that is being written.
   /// </summary>
   /// <param name="endPt">Current tooling end point</param>
   /// <param name="endNormal">End normal at the tooling end point</param>
   /// <param name="toolingName">Tooling name</param>
   public void MoveToRetract (Point3 endPt, Vector3 endNormal, string toolingName) {
      var toolingEPRetracted = Utils.MovePoint (endPt, endNormal, mRetractClearance);
      mTraces[ToolHead].Add (new (mToolPos[ToolHead], toolingEPRetracted, endNormal, endNormal,
                                  EGCode.G0, EMove.Retract, toolingName));
      mToolPos[ToolHead] = toolingEPRetracted;
      mToolVec[ToolHead] = endNormal.Normalized ();
   }

   /// <summary>
   /// This method makes the tool move from previous tooling retract position, which is 
   /// previous tooling end position away from the position by end normal of the previous tooling
   /// TO the position, whose coordinates are X of the next tooling, Y of the next tooling and Z as safety
   /// value (28 mm).
   /// </summary>
   /// <param name="prevToolingSegs">Segments of the previous tooling</param>
   /// <param name="prevToolingName">Name of the previous tooling</param>
   /// <param name="currToolingSegs">Segments of the current tooling.</param>
   /// <param name="currentToolingName">Name of the current tooling</param>
   void MoveFromRetractToSafety (List<ToolingSegment> prevToolingSegs, string prevToolingName,
                                 List<ToolingSegment> currToolingSegs,
                                 string currentToolingName) {
      if (prevToolingSegs != null && prevToolingSegs.Count > 0) {
         (var prevSegEndCurve, _, var prevSegEndCurveEndNormal) = prevToolingSegs[^1];
         var prevToolingEPRetracted =
                Utils.MovePoint (prevSegEndCurve.End, prevSegEndCurveEndNormal, 
                                 mRetractClearance);
         Point3 prevToolingEPRetractedSafeZ = 
                  new (prevToolingEPRetracted.X, prevToolingEPRetracted.Y,
                                 mSafeClearance);
         var mcCoordsPrevToolingEPRetractedSafeZ = XfmToMachine (prevToolingEPRetractedSafeZ);
         sw.WriteLine ("G1 X{0} Y{1} Z{2} A0 F{3}", 
                                 mcCoordsPrevToolingEPRetractedSafeZ.X.ToString ("F3"),
                                 mcCoordsPrevToolingEPRetractedSafeZ.Y.ToString ("F3"),
                                 mcCoordsPrevToolingEPRetractedSafeZ.Z.ToString ("F3"), Rapid);
         mTraces[ToolHead].Add (new GCodeSeg (mToolPos[ToolHead], prevToolingEPRetractedSafeZ, mToolVec[ToolHead],
                                              XForm4.mZAxis, EGCode.G0, EMove.Retract2SafeZ, prevToolingName));
         mToolPos[ToolHead] = prevToolingEPRetractedSafeZ;
         mToolVec[ToolHead] = XForm4.mZAxis;
      }

      (var currSegStCurve, var currSegStCurveStNormal, _) = currToolingSegs[0];
      
      // Move to the current tooling item start posotion safeZ
      var currToolingSPRetracted =
             Utils.MovePoint (currSegStCurve.Start, currSegStCurveStNormal, mRetractClearance);
      Point3 currToolingSPRetractedSafeZ = new (currToolingSPRetracted.X, currToolingSPRetracted.Y,
                                                mSafeClearance);
      var mcCoordsCurrToolingSPRetractedSafeZ = XfmToMachine (currToolingSPRetractedSafeZ);
      sw.WriteLine ("G0 X{0} Y{1} Z{2} A0", 
                                 mcCoordsCurrToolingSPRetractedSafeZ.X.ToString ("F3"),
                                 mcCoordsCurrToolingSPRetractedSafeZ.Y.ToString ("F3"),
                                 mcCoordsCurrToolingSPRetractedSafeZ.Z.ToString ("F3"));
      mTraces[ToolHead].Add (new (mToolPos[ToolHead], currToolingSPRetractedSafeZ, mToolVec[ToolHead],
                             XForm4.mZAxis, EGCode.G0,
                             EMove.SafeZ2SafeZ, currentToolingName));
      mToolPos[ToolHead] = currToolingSPRetractedSafeZ;
      mToolVec[ToolHead] = XForm4.mZAxis;
   }

   /// <summary>
   /// This method moves the tool using G1, from safety Z position (28mm) to
   /// the retract position (for the next tooling). The retract position is the
   /// position from the next tooling start point, offset by retract clearance
   /// along the start normal vector
   /// </summary>
   /// <param name="toolingStartPt">Tooling start point of the next tooling</param>
   /// <param name="toolingStartNormalVec">Normal vector (outward) at the next tooling start point</param>
   /// <param name="toolingName">Name of the tooling : Can be used in simulation for debug purpose</param>
   void MoveFromSafetyToRetract (Point3 toolingStartPt, Vector3 toolingStartNormalVec, 
                                 string toolingName) {
      var currToolingStPtRetracted =
            Utils.MovePoint (toolingStartPt, toolingStartNormalVec, 
                             mRetractClearance);
      var angleBetweenZAxisNcurrToolingStPt =
            Utils.GetAngleAboutXAxis (XForm4.mZAxis, toolingStartNormalVec).R2D ();

      var mcCoordsCurrToolingStPtRetracted = XfmToMachine (currToolingStPtRetracted);
      sw.WriteLine ("G1 X{0} Y{1} Z{2} A{3} F{4}", 
                                 mcCoordsCurrToolingStPtRetracted.X.ToString ("F3"),
                                 mcCoordsCurrToolingStPtRetracted.Y.ToString ("F3"),
                                 mcCoordsCurrToolingStPtRetracted.Z.ToString ("F3"), 
                                 angleBetweenZAxisNcurrToolingStPt, Rapid);
      mTraces[ToolHead].Add (new (mToolPos[ToolHead], currToolingStPtRetracted,
                                  mToolVec[ToolHead], toolingStartNormalVec, 
                                  EGCode.G0, EMove.SafeZ2Retract, toolingName));
      mToolPos[ToolHead] = currToolingStPtRetracted;
      mToolVec[ToolHead] = toolingStartNormalVec.Normalized ();
   }

   /// <summary>
   /// This method writes the X bounds for the feature described by the list of 
   /// tooling items, to write START_X and END_X. 
   /// </summary>
   /// <param name="toolingItem">The inout tooling item</param>
   /// <param name="segments">The list of tooling segments</param>
   /// <param name="startIndex">The start index in the list of tooling items. If it is -1, all the segments in the tooling
   /// segments will be considered</param>
   /// <param name="endIndex">The end index of the list of tooling items.</param>
   public void WriteBounds (Tooling toolingItem, List<ToolingSegment> segments, 
                            int startIndex = -1, int endIndex = -1) {
      var toolingSegsBounds = Utils.GetToolingSegmentsBounds (segments, startIndex, endIndex);
      var xMin = toolingSegsBounds.XMin; var xMax = toolingSegsBounds.XMax;
      
      sw.WriteLine ($"START_X={xMin:F3} END_X={xMax:F3}");
      OutN (sw, mProgramNumber, toolingItem.IsNotch () 
                                    || toolingItem.IsCutout () 
                                       ? $"{GetToolingShapeKind (toolingItem)}" : "");
      sw.WriteLine ($"PathLength={toolingItem.Perimeter:F2}");
   }

   /// <summary>
   /// This method writes START_X and END_X values, that signify the X bounds of the feature.
   /// </summary>
   /// <param name="toolingItem">The input tooling item</param>
   /// <param name="pts">The input set of points for which the bounds need to be written</param>
   public void WriteBounds (Tooling toolingItem, List<Point3> pts) {
      var toolingSegsBounds = Utils.GetPointsBounds (pts);
      var xMin = toolingSegsBounds.XMin; 
      var xMax = toolingSegsBounds.XMax;

      sw.WriteLine ($"START_X={xMin:F3} END_X={xMax:F3}");
      OutN (sw, mProgramNumber, toolingItem.IsNotch () 
                  || toolingItem.IsCutout () 
                        ? $"{GetToolingShapeKind (toolingItem)}" : "");
      mProgramNumber++;
      sw.WriteLine ($"PathLength={toolingItem.Perimeter:F2}");
   }

   /// <summary>
   /// This method is used to initialize the tooling block for non-edge notches, holes,
   /// cutouts and marks.
   /// </summary>
   /// <param name="toolingItem">The input tooling item</param>
   /// <param name="segs">The list of tooling segments</param>
   /// <param name="startIndex">The start index in the list of tooling items</param>
   /// <param name="endIndex">The end endex in the list of tooling items.</param>
   public void InitializeToolingBlock (Tooling toolingItem, List<ToolingSegment> segs, 
                                       int startIndex = -1, int endIndex = -1) {
      // ** Tool block initialization **
      sw.WriteLine ();
      sw.WriteLine ("(( ** Tool Block Initialization ** ))");
      WriteBounds (toolingItem, segs, startIndex, endIndex);

      // Now compute the offset based on X
      int offset = 0;
      switch (Utils.GetFlangeType (toolingItem)) {
         case Utils.EFlange.Top: offset = 2; 
            break;
         case Utils.EFlange.Bottom: offset = 1; 
            break;
         case Utils.EFlange.Web: offset = 3; 
            break;// toolingItem.ShouldConsiderReverseRef ? 4 : 3; break;
      }

      sw.WriteLine ();
      string sComment = offset switch {
         1 => string.Format ("(( ** Machining on the Bottom Flange ** ))"),
         2 => string.Format ("(( ** Machining on the Top Flange ** ))"),
         3 => string.Format ("(( ** Machining on the Web Flange ** ))"),
         _ => ""
      };
      sw.WriteLine (sComment);
      sw.WriteLine ($"S{offset}");
      
      // Output X tool compensation
      if (Utils.GetPlaneType (toolingItem) == Utils.EPlane.Top) 
         sw.WriteLine ($"G93 Z0 T1");
      else 
         sw.WriteLine ($"G93 Z=-Head_Height T1");

      WritePlaneForCircularMotionCommand (Utils.GetFlangeType (toolingItem));
      sw.WriteLine ("G61");

      if (toolingItem.IsNotch () || toolingItem.IsCutout () && !toolingItem.IsFlexCutout ())
         sw.WriteLine ("PM=Notch_PM CM=Notch_CM EM=Notch_EM ZRH=Notch_YRH");

      // REVISIT for IsFlexCutout
      else if (toolingItem.IsFlexCutout ())
         sw.WriteLine ("PM=Profile_PM CM=Profile_CM EM=Profile_EM ZRH=Profile_YRH");
      else if (Utils.GetFlangeType (toolingItem) == Utils.EFlange.Web)
         sw.WriteLine ("PM=Web_PM CM=Web_CM EM=Web_EM ZRH=Web_ZRH");
      else 
         sw.WriteLine ("PM=Flange_PM CM=Flange_CM EM=Flange_EM ZRH=Flange_YRH");

      sw.WriteLine ("Update_Param");
      sw.WriteLine ($"Lead_In={(toolingItem.IsNotch () 
                           ? NotchApproachLength 
                           :ApproachLength):F3)}");
      sw.WriteLine ("(( ** End - Tool Block Initialization ** ))");
      sw.WriteLine ();
   }

   /// <summary>
   /// This method initializes the tooling block of an non-edge notch by specifying
   /// flange type, plane type for arcs, exact position mode, time fed rate
   /// and other parameters. This also writes X bounds for the specific section
   /// </summary>
   /// <param name="toolingItem">The tooling item input</param>
   /// <param name="segs">The segments participating in the specific notch section.
   /// Please refer to the parameters startIndex and endIndex</param>
   /// <param name="segmentNormal">The normal to the set tooling items.</param>
   /// /// <param name="startIndex">The start index of the tooling segments. If startIndex is -1,
   /// then the entire tooling segments will be considered</param>
   /// /// <param name="endIndex">The end index of the tooling segments</param>
   /// /// <param name="circularMotionCmd">If it is true, an appropriate G Code directive 
   /// between G17 or G18 will be written</param>
   /// <param name="comment">User's comment</param>
   public void InitializeNotchToolingBlock (Tooling toolingItem, List<ToolingSegment> segs, Vector3 segmentNormal,
                                            int startIndex = -1, int endIndex = -1, 
                                            bool circularMotionCmd = true, string comment = "") {
      // ** Tool block initialization **
      sw.WriteLine ();
      sw.WriteLine ("(( ** Notch: Tool Block Initialization ** ))");
      sw.WriteLine ($"(({comment}))");
      sw.WriteLine ();
      WriteBounds (toolingItem, segs, startIndex, endIndex);
      mProgramNumber++;

      // Now compute the offset based on X
      int offset = 0;
      switch (Utils.GetArcPlaneFlangeType (segmentNormal)) {
         case Utils.EFlange.Top: offset = 2; 
            break;
         case Utils.EFlange.Bottom: offset = 1; 
            break;
         case Utils.EFlange.Web: offset = 3; 
            break;// toolingItem.ShouldConsiderReverseRef ? 4 : 3; break;
      }

      sw.WriteLine ();
      string sComment = offset switch {
         1 => string.Format ("(( Machining on the Bottom Flange ))"),
         2 => string.Format ("(( Machining on the Top Flange ))"),
         3 => string.Format ("(( Machining on the Web Flange ))"),
         _ => ""
      };
      sw.WriteLine (sComment);
      sw.WriteLine ($"S{offset}");

      // Output X tool compensation
      if (Utils.GetArcPlaneType (segmentNormal) == Utils.EPlane.Top) 
         sw.WriteLine ($"G93 Z0 T1");
      else 
         sw.WriteLine ($"G93 Z=-Head_Height T1");
      if (circularMotionCmd) 
         WritePlaneForCircularMotionCommand (Utils.GetArcPlaneFlangeType (segmentNormal));

      sw.WriteLine ("G61");
      sw.WriteLine ("PM=Notch_PM CM=Notch_CM EM=Notch_EM ZRH=Notch_YRH");
      sw.WriteLine ("Update_Param");
      sw.WriteLine ($"Lead_In={NotchApproachLength:F3)}");
      sw.WriteLine ();
   }

   /// <summary>
   /// This method initializes the tooling block by specifying
   /// flange type, exact position mode, time fed rate
   /// and other parameters. This also writes X bounds for the specific section
   /// </summary>
   /// <param name="toolingItem">The tooling item input</param>
   /// <param name="points">THe set of points that participate in the tooling section. 
   /// This is so in the case of approach to the tooling considering approach length 
   /// and wire joint distance in the case of non-edge notches</param>
   /// <param name="segmentNormal">The normal to the set of points</param>
   /// <param name="comment">User's comment</param>
   public void InitializeNotchToolingBlock (Tooling toolingItem, List<Point3> points,
                                            Vector3 segmentNormal, string comment = "") {
      
      // ** Tool block initialization **
      sw.WriteLine ();
      sw.WriteLine ("(( ** Notch: Tool Block Initialization ** ))");
      sw.WriteLine ($"(({comment}))");
      WriteBounds (toolingItem, points);
      mProgramNumber++;

      // Now compute the offset based on X
      int offset = 0;
      switch (Utils.GetArcPlaneFlangeType (segmentNormal)) {
         case Utils.EFlange.Top: offset = 2; 
            break;
         case Utils.EFlange.Bottom: offset = 1; 
            break;
         case Utils.EFlange.Web: offset = 3; 
            break;// toolingItem.ShouldConsiderReverseRef ? 4 : 3; break;
      }

      sw.WriteLine ();
      string sComment = offset switch {
         1 => string.Format ("(( Machining on the Bottom Flange ))"),
         2 => string.Format ("(( Machining on the Top Flange ))"),
         3 => string.Format ("(( Machining on the Web Flange ))"),
         _ => ""
      };

      sw.WriteLine (sComment);
      sw.WriteLine ($"S{offset}");

      // Output X tool compensation
      if (Utils.GetArcPlaneType (segmentNormal) == Utils.EPlane.Top) 
         sw.WriteLine ($"G93 Z0 T1");
      else 
         sw.WriteLine ($"G93 Z=-Head_Height T1");

      // ** Note: This method will not write the G17 or G18 **
      sw.WriteLine ("G61");
      sw.WriteLine ("PM=Notch_PM CM=Notch_CM EM=Notch_EM ZRH=Notch_YRH");
      sw.WriteLine ("Update_Param");
      sw.WriteLine ($"Lead_In={NotchApproachLength:F3}");
      sw.WriteLine ();
   }

   /// <summary>
   /// FinalizeToolingBlock is to be called at the end of the tooling of types
   /// other than non-edge notches
   /// </summary>
   /// <param name="toolingItem">The input tooling item</param>
   /// <param name="markLength">Mark length (text)</param>
   /// <param name="totalMarkLength">Total Mark Length</param>
   /// <param name="cutLength">cut length of tooling other than </param>
   /// <param name="totalCutLength">Total cut length of the toolings</param>
   void FinalizeToolingBlock (Tooling toolingItem, double totalMarkLength,
                              double totalCutLength) {
      sw.WriteLine ();
      sw.WriteLine ("(( ** Tooling Block Finalization ** )");
      double percentage = 0;
      double markLength = 0, cutLength = 0;

      if (toolingItem.IsMark ()) {
         markLength += toolingItem.Perimeter;
         percentage = markLength / totalMarkLength * 100;
      } else if (toolingItem.IsHole () || toolingItem.IsCutout () || toolingItem.IsNotch ()) {
         cutLength += toolingItem.Perimeter;
         percentage = cutLength / totalCutLength * 100;
      }

      sw.WriteLine ($"G253 E0 F=\"{(toolingItem.IsMark () ? 1 : 2)}=1:1:{percentage.Round (0)}\"");
      sw.WriteLine ("G40 E1"); // Cancel tool diameter compensation
      sw.WriteLine ();
   }

   /// <summary>
   /// FinalizeNotchToolingBlock is exclusive to non-edge Notches. This writes G code 
   /// directive to write overall completion percentage
   /// </summary>
   /// <param name="toolingItem">The tooling Item</param>
   /// <param name="cutLength">The cut length of the notch tooling block</param>
   /// <param name="totalCutLength">Total cut length of the notch (including wire joint length
   /// and notch approach)</param>
   public void FinalizeNotchToolingBlock (Tooling toolingItem,
                                          double cutLength, 
                                          double totalCutLength) {
      sw.WriteLine ();
      sw.WriteLine ("(( ** Tooling Block Finalization ** ))");
      double percentage = (cutLength / totalCutLength) * 100;
      sw.WriteLine ($"G253 E0 F=\"{(toolingItem.IsMark () ? 1 : 2)}=1:1:{percentage:F0}\"");
      sw.WriteLine ("G40 E1"); // Cancel tool diameter compensation
   }

   /// <summary>
   /// This is the main method which prepares the machine with calling various pre-machining
   /// settings/macros, and then calls WriteTooling, which actually calls machining G Codes.
   /// This also adds post processing macros to complete
   /// </summary>
   /// <param name="toolingItems"></param>
   /// <param name="shouldOutputDigit"></param>
   void WriteCuts (List<Tooling> toolingItems, bool shouldOutputDigit) {
      Tooling prevToolingItem = null;
      List<ValueTuple<Curve3, Vector3, Vector3>> prevToolingSegs = null;
      bool first = true;
      string traverseM = UsePingPong ? "M1014" : "";
      mProgramNumber = mPgmNo[Utils.EFlange.Web];
      double totalToolingCutLength = Process.Workpiece.Cuts.Where (a => (a.IsCutout () 
                                                                     || a.IsHole ())).Sum (a => a.Perimeter);

      // For notches, compute the length
      foreach (var ti in toolingItems) {
         if (ti.IsNotch ()) {
            if (Notch.IsEdgeNotch (Process.Workpiece.Model, ti, 
                                   mPercentLengths, NotchApproachLength, 
                                   mCurveLeastLength))
               totalToolingCutLength += ti.Perimeter;
            else {
               totalToolingCutLength += 
                     Notch.GetTotalNotchToolingLength (Process.Workpiece.Model, 
                                                       ti, [0.25, 0.5, 0.75], NotchWireJointDistance,
                                                       NotchApproachLength, mCurveLeastLength);
            }
         }
      }

      double totalMarkLength = Process.Workpiece.Cuts.Where (a => a.IsMark ()).Sum (a => a.Perimeter);
      ToolingSegment? lastToolingSegment = null;
      double prevCutToolingsLength = 0;
      for (int i = 0; i < toolingItems.Count; i++) {
         Tooling toolingItem = toolingItems[i];
         var pr = PartitionRatio;
         var nwjDist = NotchWireJointDistance;
         var nApproachDist = NotchApproachLength;
         
         // The following switches are for tests.
         if (!Cutouts) 
            if (toolingItem.IsCutout ()) 
               continue;

         if (!CutNotches) 
            if (toolingItem.IsNotch ()) 
               continue;

         if (!CutMarks) 
            if (toolingItem.IsMark ()) 
               continue;

         if (!CutHoles) 
            if (toolingItem.IsHole ()) 
               continue;

         sw.WriteLine ("\n\n(( **Tooling Name : {0}** ))", toolingItem.Name);
         if (first) 
            prevToolingItem = null;

         mProgramNumber = GetProgNo (toolingItem);
         mProgramNumber++;
         
         // Open shutter and go to program number
         // Output the first program as probing function always
         if (first) {
            string ncname = NCName;
            if (ncname.Length > 20) 
               ncname = ncname[..20];

            sw.WriteLine ($"G253 E0 F=\"0=1:{ncname}:{Math.Round (totalToolingCutLength, 2)}," +
                          $"{Math.Round (totalMarkLength, 2)}\"");
            if (shouldOutputDigit)
               sw.WriteLine ("G253 E0 F=\"3=THL RF\"");

            sw.WriteLine ($"G20 X=BlockID\r\n");
         }

         // Output sync for reverse flange reference block
         if (!first 
            && Utils.GetPlaneType (prevToolingItem) !=
                  Utils.GetPlaneType (toolingItem))
            sw.WriteLine ("G4 X2");

         sw.WriteLine ();
         bool notchWithApproach = toolingItem.IsNotch () 
                                       && !Notch.IsEdgeNotch (Process.Workpiece.Model, toolingItem,
                                                              mPercentLengths, NotchApproachLength, 
                                                              mCurveLeastLength);
         if (!notchWithApproach) 
            InitializeToolingBlock (toolingItem, toolingItem.Segs.ToList ());

         var modifiedToolingSegs = GetSegmentsAccountedForApproachLength (toolingItem);
         if (modifiedToolingSegs == null || modifiedToolingSegs?.Count == 0) 
            continue;

         if (first) 
            MoveToSafety ();
         else 
            MoveToRetract (lastToolingSegment.Value.Item1.End, 
                           lastToolingSegment.Value.Item2, 
                           prevToolingItem.Name);

         if (notchWithApproach) {
            var notchEntry = Notch.GetNotchEntry (Process.Workpiece.Model, 
                                                  toolingItem, mPercentLengths,
                                                  NotchApproachLength, mCurveLeastLength);

            if (lastToolingSegment != null)
               MoveToNextTooling (lastToolingSegment.Value.Item2, lastToolingSegment,
                                  notchEntry.Item1, notchEntry.Item2.Normalized (), 
                                  Utils.GetFlangeType (toolingItem),
                                  prevToolingItem != null ? prevToolingItem.Name : "",
                                  toolingItem.Name, first);
            else
               MoveToNextTooling (prevToolingItem != null 
                                    ? prevToolingItem.End.Vec : new Vector3 (),
                                  (prevToolingSegs != null && prevToolingSegs.Count > 0) 
                                    ? prevToolingSegs[^1] : null,
                                  notchEntry.Item1, notchEntry.Item2.Normalized (), 
                                  Utils.GetFlangeType (toolingItem),
                                  prevToolingItem != null ? prevToolingItem.Name : "",
                                  toolingItem.Name, first);
         } else {
            if (lastToolingSegment != null)
               MoveToNextTooling (lastToolingSegment.Value.Item2, lastToolingSegment,
                                  modifiedToolingSegs[0].Item1.Start, 
                                  modifiedToolingSegs[0].Item2, Utils.GetFlangeType (toolingItem),
                                  prevToolingItem != null 
                                          ? prevToolingItem.Name : "",
                                   toolingItem.Name, first);
            else
               MoveToNextTooling (prevToolingItem != null 
                                          ? prevToolingItem.End.Vec : new Vector3 (),
                                  (prevToolingSegs != null && prevToolingSegs.Count > 0) ? prevToolingSegs[^1] : null,
                                  modifiedToolingSegs[0].Item1.Start, modifiedToolingSegs[0].Item2, Utils.GetFlangeType (toolingItem),
                                  prevToolingItem != null 
                                          ? prevToolingItem.Name : "",
                                  toolingItem.Name, first);
         }

         int CCNo = Utils.GetFlangeType (toolingItem) == Utils.EFlange.Web 
                                                   ? WebCCNo : FlangeCCNo;
         if (toolingItem.IsCircle ()) {
            var evalValue = Geom.EvaluateCenterAndRadius (toolingItem.Segs.ToList ()[0].Curve as Arc3);
            if (mControlDiameter.Any (a => a.EQ (2 * evalValue.Item2))) 
               CCNo = 4;
         } else if (toolingItem.IsNotch ()) 
            CCNo = 1;

         int outCCNO = CCNo;
         if (toolingItem.IsFlexCutout ()) outCCNO = 1;
         int blockType = 0;
         if (Utils.GetFlangeType (toolingItem) == Utils.EFlange.Web)
            blockType = toolingItem.ShouldConsiderReverseRef ? -1 : 1;

         if (toolingItem.IsNotch ()) 
            blockType = Utils.GetFlangeType (toolingItem) == Utils.EFlange.Top ? -2 : 2;

         if (toolingItem.IsCutout ())
            blockType = Utils.GetFlangeType (toolingItem) is Utils.EFlange.Top or Utils.EFlange.Top ? -3 : 3;

         if (toolingItem.IsMark ()) 
            blockType = 4;

         sw.WriteLine ($"BlockType={blockType}");
         sw.WriteLine ("X_Correction=0\r\nYZ_Correction=0");
         if (toolingItem.IsCircle ()) {
            var evalValue = Geom.EvaluateCenterAndRadius (toolingItem.Segs.ToList ()[0].Curve as Arc3);
            Point3 arcMcCoordsCenter;
            if (prevToolingItem != null) 
               arcMcCoordsCenter = XfmToMachine (evalValue.Item1);
            else 
               arcMcCoordsCenter = XfmToMachine (evalValue.Item1);

            var point2 = Utils.ToPlane (arcMcCoordsCenter, Utils.GetFeatureNormalPlaneType (toolingItem.Start.Vec));
            sw.WriteLine ($"X_Coordinate={point2.X}");
            sw.WriteLine ($"YZ_Coordinate={point2.Y}");
         }

         // Output the Cutting offset. Customer need to cut hole slightly larger than given in geometry
         // We are using G42 than G41 while cutting holes
         // If we are reversing y and not reversing x. We are in 4th quadrant. Flip 42 or 41
         // Tool diameter compensation
         sw.WriteLine ("ToolCorrection");
         if (toolingItem.IsCircle ()) 
            sw.WriteLine ($"G{(mXformRHInv[1, 3] < 0.0 ? 41 : 42)} D1 R=P1300 E0");
         sw.WriteLine ();

         // ** Machining **
         lastToolingSegment = WriteTooling (modifiedToolingSegs, toolingItem, 
                                            prevCutToolingsLength, totalToolingCutLength);

         // ** Tooling block finalization - Start**
         if (!notchWithApproach) 
            FinalizeToolingBlock (toolingItem, totalMarkLength, totalToolingCutLength);

         // Compute the cut tooling length
         if (!toolingItem.IsMark ()) {
            if (toolingItem.IsNotch () && notchWithApproach)
               prevCutToolingsLength += 
                     Notch.GetTotalNotchToolingLength (Process.Workpiece.Model, 
                                                       toolingItem, [0.25, 0.5, 0.75], NotchWireJointDistance,
                                                       NotchApproachLength, mCurveLeastLength);
            else prevCutToolingsLength += toolingItem.Perimeter;
         }

         first = false;
         prevToolingItem = toolingItem;
         prevToolingSegs = modifiedToolingSegs;
         SetProgNo (toolingItem, mProgramNumber);
      }

      //if (toolingItems.Count > 0) SetProgNo (toolingItems.Last (), mProgramNumber);
      // Digit will be made 0 if it doesn't belong to this head
      if (shouldOutputDigit) {
         double x = MarkTextPosX, y = MarkTextPosY;
         var range = GetSerialDigitToOutput ();
         for (int i = range.Item1; i < range.Item2; i++) {
            int progNo = GetDigitProgNo (i) + 1;
            OutN (sw, progNo);
            sw.WriteLine ($"P1763={progNo}");
            if (i == 0) 
               sw.WriteLine ("M58\r\nG61");

            sw.WriteLine ($":P1707={i}");
            sw.WriteLine ($":P1708={DigitConst}+P{1860 + i}");
            sw.WriteLine ($":P1838=P1661+(P1707*{DigitPitch})+{x:F3)} " +
                          $"(X-Axis Actual Distance from Flux)");
            Point3 markTextPoint = new (x, y, 0);
            markTextPoint = XfmToMachine (markTextPoint);
            double yVal = markTextPoint.Y;
            sw.WriteLine ($":P1839=P2005{(Math.Sign (yVal) == 1 ? "+" : "-")}{Math.Abs (yVal)} " +
                          $"(Y-Axis Actual Distance from Flux)");
            string mark = PartConfigType == MCSettings.PartConfigType.LHComponent 
                              ? "L160" : "L161";
            sw.WriteLine ($"S100\r\nG93 X=P1838 Y=P1839\r\nG22 {mark} J=P1708\r\nG61\r\n");
         }
      }
   }

   /// <summary>
   /// This method writes the G Code segment for wire joint trace jump(skip). 
   /// The wire joint trace is a set of segments that start from a tooling segment
   /// end point with a rapid move, (G0), reach the position along the outward normal 
   /// on the flange, at a distance of notch approach distance, from the next tooling 
   /// segment's start point and machine (G1) from this point to the point on the tooling segment
   /// </summary>
   /// <param name="endNormalCrv1">The normal at any point of the above</param>
   /// <param name="crv2">The underlying curve of the next tooling segment</param>
   /// <param name="stNormalCrv2">The normal at the starting point of the curve above</param>
   /// <param name="scrapSideNormalCrv2">The scrap side direction from the start of the next 
   /// curve of the next segment</param>
   /// <param name="notchApproachDistance">The notch approach distance</param>
   /// <param name="prevPlaneType"></param>
   /// <param name="currFlangeType"></param>
   /// <param name="toolingName"></param>
   public void WriteWireJointTraceForNotch (Vector3 endNormalCrv1, Curve3 crv2, 
                                            Vector3 stNormalCrv2, Vector3 scrapSideNormalCrv2,
                                            double notchApproachDistance, ref Utils.EPlane prevPlaneType, 
                                            Utils.EFlange currFlangeType, string toolingName) {
      Utils.EPlane currPlaneType = Utils.GetArcPlaneType (endNormalCrv1);
      var nextMachiningStart = crv2.Start + scrapSideNormalCrv2.Normalized () * notchApproachDistance;

      RapidPositionWithClearance (nextMachiningStart, stNormalCrv2, mRetractClearance, toolingName);
      MoveToMachiningStartPosition (nextMachiningStart, stNormalCrv2, toolingName);
      prevPlaneType = currPlaneType;

      // Honouring notch approach distance 
      WriteLine (crv2.Start, stNormalCrv2, stNormalCrv2, 
                 currPlaneType, prevPlaneType, currFlangeType, toolingName);
   }

   /// <summary>
   /// This method positions the tool head exactly at the starting position 
   /// of the next tooling segment, but with a distance "clearance" along the 
   /// starting normal.
   /// </summary>
   /// <param name="toPoint">The next point of the tooling segment</param>
   /// <param name="endNormal">The normal at the next tooling starting point</param>
   /// <param name="clearance">A distance along the normal at the point</param>
   /// <param name="toolingName">Tooling name</param>
   public void RapidPositionWithClearance (Point3 toPoint, Vector3 endNormal, 
                                           double clearance, string toolingName) {
      var toPointOffset = Utils.MovePoint (toPoint, endNormal, clearance);
      var angle = Utils.GetAngleAboutXAxis (XForm4.mZAxis, endNormal).R2D ();
      var mcCoordsToPointOffset = XfmToMachine (toPointOffset);
      sw.WriteLine ("G0 X{0} Y{1} Z{2} A{3}", mcCoordsToPointOffset.X.ToString ("F3"),
                                              mcCoordsToPointOffset.Y.ToString ("F3"), 
                                              mcCoordsToPointOffset.Z.ToString ("F3"), angle.ToString ("F3"));
      mTraces[ToolHead].Add (new (mToolPos[ToolHead], toPointOffset, endNormal, endNormal,
                                  EGCode.G0, EMove.RapidPosition, toolingName));
      mToolPos[ToolHead] = toPointOffset;
      mToolVec[ToolHead] = endNormal.Normalized ();
   }

   /// <summary>
   /// This method writes the G Code segment for wire joint trace jump(skip). 
   /// The wire joint trace is a set of segments that start from a tooling segment
   /// end point with a rapid move, (G0), reach the position along the outward normal 
   /// on the flange, at a distance of notch approach distance, from the next tooling 
   /// segment's start point and machine (G1) from this point to the point on the tooling segment
   /// </summary>
   /// <param name="nextSegmentStartPoint">The starting point of the next tooling segment</param>
   /// <param name="stNormal">The start normal at the above point</param>
   /// <param name="endNormal">The end normal at the above point</param>
   /// <param name="scrapSideNormal">The direction in which the scrappable material exists</param>
   /// <param name="lastPosition">The last position of the tool head (from previous stroke)</param>
   /// <param name="notchApproachDistance">The notch approach distance</param>
   /// <param name="prevPlaneType">The previous plane type YPos, YNeg, or Top (for angle computation 
   /// about X axis)</param>
   /// <param name="currFlangeType">Web, Top or Bottom(for angle computation about X Axis)</param>
   /// <param name="toolingItem">The current tooling item</param>
   /// <param name="blockCutLength">The machining distance of the current wire joint trace</param>
   /// <param name="totalCutLength">The total machining length (of the notch)</param>
   /// <param name="comment">Comment to be written in G Code</param>
   public void WriteWireJointTraceForNotch (Point3 nextSegmentStartPoint, Vector3 stNormal, 
                                            Vector3 endNormal, Vector3 scrapSideNormal,
                                            Point3 lastPosition, double notchApproachDistance, 
                                            ref Utils.EPlane prevPlaneType, Utils.EFlange currFlangeType, 
                                            Tooling toolingItem, ref double blockCutLength, 
                                            double totalCutLength, string comment = "Notch: Wire Joint Jump Trace") {
      Utils.EPlane currPlaneType = Utils.GetArcPlaneType (endNormal);
      var nextMachiningStart = nextSegmentStartPoint + scrapSideNormal.Normalized () * notchApproachDistance;
      RapidPositionWithClearance (nextMachiningStart, stNormal, mRetractClearance, toolingItem.Name);
      MoveToMachiningStartPosition (nextMachiningStart, stNormal, toolingItem.Name);

      prevPlaneType = currPlaneType;
      var fromPt = GetLastToolHeadPosition ().Item1;
      List<Point3> pts = [];
      pts.Add (nextMachiningStart);
      pts.Add (nextSegmentStartPoint);
      pts.Add (lastPosition);

      InitializeNotchToolingBlock (toolingItem, pts, endNormal, comment);
      {
         EnableMachiningDirective ();
         WriteLine (nextSegmentStartPoint, endNormal, endNormal, 
                    currPlaneType, prevPlaneType,
                    currFlangeType, toolingItem.Name);
         DisableMachiningDirective ();

         blockCutLength += mToolPos[ToolHead].DistTo (fromPt);
      }
      FinalizeNotchToolingBlock (toolingItem, blockCutLength, totalCutLength);
   }

   /// <summary>
   /// This method is used to write G Code that moves the 
   /// tool head from current end of the tooling to the next tooling segment
   /// </summary>
   /// <param name="prevToolingEndNormal">The normal at the previous end</param>
   /// <param name="prevToolingEndSegment">The previous end segment</param>
   /// <param name="nextToolingStartPoint">The start point on the next tooling segment</param>
   /// <param name="nextToolingStartNormal">The start normal of the next tooling</param>
   /// <param name="nextToolingFlangeType">The Flange type of the next tooling segment</param>
   /// <param name="prevToolingItemName">The name of the previous tooling stroke</param>
   /// <param name="nextToolingItemName">The name of the current tooling stroke.</param>
   /// <param name="firstTime">A boolean flag that tells if the tooling item is the first one to start with.
   /// This is used for angle computation</param>
   public void MoveToNextTooling (Vector3 prevToolingEndNormal, ToolingSegment? prevToolingEndSegment,
                                  Point3 nextToolingStartPoint, Vector3 nextToolingStartNormal, 
                                  Utils.EFlange nextToolingFlangeType, string prevToolingItemName, 
                                  string nextToolingItemName, bool firstTime) {
      double changeInAngle;
      if (firstTime) 
         changeInAngle = Utils.GetAngleAboutXAxis (XForm4.mZAxis, nextToolingStartNormal).R2D ();
      else 
         changeInAngle = Utils.GetAngleAboutXAxis (prevToolingEndNormal, nextToolingStartNormal).R2D ();

      bool movedToCurrToolingRetractedPos = false;
      if (!changeInAngle.LieWithin (-10.0, 10.0)) {
         sw.WriteLine ("PlaneTransfer");
         MoveFromRetractToSafety (prevToolingEndSegment,
                                  prevToolingItemName, nextToolingStartPoint,
                                  nextToolingStartNormal, nextToolingItemName);
                                  MoveFromSafetyToRetract (nextToolingStartPoint,
                                  nextToolingStartNormal, nextToolingItemName);
         movedToCurrToolingRetractedPos = true;

         sw.WriteLine ("EndPlaneTransfer");
         sw.WriteLine ("(-----CUTTING ON {0} FLANGE-----)", nextToolingFlangeType.ToString ().ToUpper ());
      } else sw.WriteLine ("ToolPlane");

      if (!movedToCurrToolingRetractedPos)
         MoveFromSafetyToRetract (nextToolingStartPoint, nextToolingStartNormal, nextToolingItemName);
   }

   /// <summary>
   /// This method makes the tool move from previous tooling retract position, which is 
   /// previous tooling end position away from the position by end normal of the previous tooling
   /// TO the position, whose coordinates are X of the next tooling, Y of the next tooling and Z as safety
   /// value (28 mm).
   /// </summary>
   /// <param name="prevToolingSegs">Segments of the previous tooling</param>
   /// <param name="prevToolingName">Name of the previous tooling</param>
   /// <param name="currToolingSegs">Segments of the current tooling.</param>
   /// <param name="currentToolingName">Name of the current tooling</param>
   public void MoveFromRetractToSafety (ToolingSegment? prevToolingLastSegment, 
                                        string prevToolingName, Point3 currToolingStPoint, 
                                        Vector3 currToolingStNormal, string currentToolingName) {
      if (prevToolingLastSegment != null) {
         (var prevSegEndCurve, _, var prevSegEndCurveEndNormal) = prevToolingLastSegment.Value;
         var prevToolingEPRetracted =
                Utils.MovePoint (prevSegEndCurve.End, prevSegEndCurveEndNormal, 
                                 mRetractClearance);
         Point3 prevToolingEPRetractedSafeZ = new (prevToolingEPRetracted.X, prevToolingEPRetracted.Y,
                                                   mSafeClearance);
         var mcCoordsPrevToolingEPRetractedSafeZ = XfmToMachine (prevToolingEPRetractedSafeZ);
         sw.WriteLine ("G1 X{0} Y{1} Z{2} A0 F{3}", 
                           mcCoordsPrevToolingEPRetractedSafeZ.X.ToString ("F3"),
                           mcCoordsPrevToolingEPRetractedSafeZ.Y.ToString ("F3"),
                           mcCoordsPrevToolingEPRetractedSafeZ.Z.ToString ("F3"), Rapid);
         mTraces[ToolHead].Add (new GCodeSeg (mToolPos[ToolHead], prevToolingEPRetractedSafeZ, mToolVec[ToolHead],
                                              XForm4.mZAxis, EGCode.G0, EMove.Retract2SafeZ, prevToolingName));
         mToolPos[ToolHead] = prevToolingEPRetractedSafeZ;
         mToolVec[ToolHead] = XForm4.mZAxis;
      }

      // Move to the current tooling item start posotion safeZ
      var currToolingSPRetracted =
             Utils.MovePoint (currToolingStPoint, currToolingStNormal, mRetractClearance);
      Point3 currToolingSPRetractedSafeZ = new (currToolingSPRetracted.X, currToolingSPRetracted.Y,
                                                mSafeClearance);

      var mcCoordsCurrToolingSPRetractedSafeZ = XfmToMachine (currToolingSPRetractedSafeZ);
      sw.WriteLine ("G0 X{0} Y{1} Z{2} A0", 
                        mcCoordsCurrToolingSPRetractedSafeZ.X.ToString ("F3"),
                        mcCoordsCurrToolingSPRetractedSafeZ.Y.ToString ("F3"),
                        mcCoordsCurrToolingSPRetractedSafeZ.Z.ToString ("F3"));
      mTraces[ToolHead].Add (new (mToolPos[ToolHead], currToolingSPRetractedSafeZ, 
                                  mToolVec[ToolHead], XForm4.mZAxis, EGCode.G0,
                                  EMove.SafeZ2SafeZ, currentToolingName));
      mToolPos[ToolHead] = currToolingSPRetractedSafeZ;
      mToolVec[ToolHead] = XForm4.mZAxis;
   }

   /// <summary>
   /// Thius method gets the last position of the head
   /// </summary>
   /// <returns>The last position of the tool head</returns>
   public Tuple<Point3, Vector3> GetLastToolHeadPosition () {
      return new Tuple<Point3, Vector3> (mToolPos[ToolHead], mToolVec[ToolHead]);
   }

   public void WriteGCode (string input) => sw.WriteLine (input);

   // Tuple<Start, End> Start inclusive and End exclusive
   // That is in index format
   Tuple<int, int> GetSerialDigitToOutput () => Tuple.Create (0, (int)SerialNumber);
   static int GetDigitProgNo (int digitNo) => DigitProg + digitNo;
   #endregion
}