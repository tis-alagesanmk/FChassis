using FChassis.GCodeGen;
using FChassis.Tools;
using FChassis.Core;

using Flux.API;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace FChassis.Processes;

/// <summary>Processor is used to generate G-Code, and the Traces for simulation</summary>
public class Processor : INotifyPropertyChanged {

   #region Enums
   public enum RefCSys {
      WCS,
      MCS
   }
   public enum ESimulationStatus {
      Running,
      Paused,
      NotRunning
   }
   #endregion

   #region G Code Drawables and Utilities
   List<List<GCodeSeg>> mTraces = [[], []];
   public List<List<GCodeSeg>> Traces { get => mTraces; }

   public void ClearTraces () {
      mTraces[0]?.Clear ();
      mTraces[1]?.Clear ();
   }

   readonly List<XForm4>[] mXForms = [[], []];
   public List<XForm4>[] XForms => mXForms;
   public void ClearXForms () { mXForms[0].Clear (); mXForms[1].Clear (); }

   RefCSys mReferenceCS = RefCSys.WCS;
   public RefCSys ReferenceCS { 
      get => mReferenceCS; 
      set => mReferenceCS = value; }
   #endregion

   #region Digital Twins - Resources and Workpiece
   public Workpiece Workpiece {
      get => mWorkpiece;
      set {
         mWorkpiece = value;
         mGCodeGenerator.OnNewWorkpiece ();
      }
   }
   Workpiece mWorkpiece;

   public Nozzle MachiningTool { 
      get => mMachiningTool; 
      set => mMachiningTool = value; }
   Nozzle mMachiningTool;
   #endregion

   #region Simulation and Redraw Data members
   public delegate void TriggerRedrawDelegate ();
   public delegate void SetSimulationStatusDelegate (Processor.ESimulationStatus status);
   public event TriggerRedrawDelegate TriggerRedraw;
   public event Action SimulationFinished;
   public event SetSimulationStatusDelegate SetSimulationStatus;
   ESimulationStatus mSimulationStatus = ESimulationStatus.NotRunning;
   public ESimulationStatus SimulationStatus {
      get => mSimulationStatus;
      set {
         if (mSimulationStatus != value) {
            mSimulationStatus = value;
            SetSimulationStatus?.Invoke (value);
         }
      }
   }

   public event PropertyChangedEventHandler PropertyChanged;
   protected void OnPropertyChanged (string propertyName) {
      PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
   }
   #endregion

   #region Constructor
   public Processor () {
      MachiningTool = new Nozzle (5.0, 40.0, 100);
      mGCodeGenerator = new GCodeGenerator (this);
      mGCodeParser = new GCodeParser ();
      mReferenceCS = RefCSys.WCS;
      CutHoles = true;
      CutMark = true;
      CutNotches = true;
      Cutouts = true;
   }
   #endregion

   #region GCode generator properties
   public MCSettings.PartConfigType PartConfigType {
      get => mGCodeGenerator.PartConfigType;
      set => mGCodeGenerator.PartConfigType = value;
   }
   public bool Cutouts { 
      get =>mGCodeGenerator.Cutouts; 
      set=>mGCodeGenerator.Cutouts = value; }

   public bool CutHoles { 
      get => mGCodeGenerator.CutHoles; 
      set => mGCodeGenerator.CutHoles = value; }

   public bool CutMark { 
      get => mGCodeGenerator.CutMarks; 
      set => mGCodeGenerator.CutMarks = value; }

   public bool CutNotches { 
      get => mGCodeGenerator.CutNotches; 
      set => mGCodeGenerator.CutNotches = value; }

   public MCSettings.EHeads Heads { 
      get => mGCodeGenerator.Heads; 
      set => mGCodeGenerator.Heads = value; }

   public double PartitionRatio { 
      get => mGCodeGenerator.PartitionRatio; 
      set => mGCodeGenerator.PartitionRatio = value; }

   public double NotchWireJointDistance { 
      get => mGCodeGenerator.NotchWireJointDistance; 
      set => mGCodeGenerator.NotchWireJointDistance = value; }
   #endregion

   #region GCode Generator and Utilities
   GCodeParser mGCodeParser;
   readonly GCodeGenerator mGCodeGenerator;
   public void ClearZombies () {
      ClearTraces ();
      ClearXForms ();
      RewindEnumerator (0);
      RewindEnumerator (1);
      TriggerRedraw?.Invoke ();
   }

   public void LoadGCode (string filename) {
      try {
         mGCodeParser.Parse (filename);
      } catch (Exception e) {
         string formattedString = String.Format ("Parsing GCode file {0} failed. Error: {1}", filename, e.Message);
         MessageBox.Show (formattedString, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         return;
      }

      mTraces[0] = mGCodeParser.Traces[0];
      mTraces[1] = mGCodeParser.Traces[1];
      TriggerRedraw?.Invoke ();
   }

   public void ResetGCodeGenForTesting () => mGCodeGenerator?.ResetForTesting ();

   /// <summary>Uses Processor to generate code, and to generate the simulation traces</summary>
   /// If 'testing' is set to true, we reset the settings to a known stable value
   /// used for testing, and create always a partition at 0.5. Otherwise, we use a 
   /// dynamically computed optimal partitioning
   //public void ComputeGCode (bool testing = false, double ratio = 0.5) {
   public void ComputeGCode (bool testing = false) {
      ClearZombies ();
      if (testing) mGCodeGenerator.CreatePartition (false);
      else {
         // Sanity test might have changed the instance of setting properties
         mGCodeGenerator.SetFromMCSettings ();
         mGCodeGenerator.ResetBookKeepers ();
         mGCodeGenerator.CreatePartition (MCSettings.It.OptimizePartition);
      }
      mGCodeGenerator.GenerateGCode (0);
      mTraces[0] = mGCodeGenerator.Traces[0];
      mGCodeGenerator.GenerateGCode (1);
      mTraces[1] = mGCodeGenerator.Traces[1];
   }
   #endregion

   #region Simulation Implementation
   List<Tuple<Point3, Vector3>>[] mWayPoints = new List<Tuple<Point3, Vector3>>[2];

   struct GCodeSegmentIndices {
      public GCodeSegmentIndices () {
         gCodeSegIndex = 0;
         wayPointIndex = 0;
      }

      public int gCodeSegIndex, wayPointIndex;
   }
   GCodeSegmentIndices[] mNextXFormIndex = [new (), new ()];

   XForm4 GetNextToolXForm (int head) {
      XForm4 xFormRes;
      if (mTraces[head] == null || MachiningTool == null) 
         return null;

      if (mNextXFormIndex[head].gCodeSegIndex >= mTraces[head].Count) 
         return null;

      int steps = (int)(mTraces[head][mNextXFormIndex[head].gCodeSegIndex].Length / MCSettings.It.StepLength);

      if (mNextXFormIndex[head].wayPointIndex == 0) {
         if (mTraces[head][mNextXFormIndex[head].gCodeSegIndex].GCode is EGCode.G0 or EGCode.G1) 
            mWayPoints[head] =
               Utils.DiscretizeLine (mTraces[head][mNextXFormIndex[head].gCodeSegIndex], steps);
         else if (mTraces[head][mNextXFormIndex[head].gCodeSegIndex].GCode is EGCode.G2 or EGCode.G3) 
            mWayPoints[head] =
               Utils.DiscretizeArc (mTraces[head][mNextXFormIndex[head].gCodeSegIndex], steps);
      }

      if (mWayPoints[head].Count == 0) 
         throw new Exception ("Unable to compute treadingPoints");

      var waypointVec = mWayPoints[head][mNextXFormIndex[head].wayPointIndex];
      mNextXFormIndex[head].wayPointIndex++;
      if (mNextXFormIndex[head].wayPointIndex >= mWayPoints[head].Count) {
         mNextXFormIndex[head].gCodeSegIndex += 1;
         mNextXFormIndex[head].wayPointIndex = 0;
      }

      var (wayPt, wayVecAtPt) = waypointVec;
      var yComp = Geom.Cross (wayVecAtPt, XForm4.mXAxis).Normalized ();
      xFormRes = new XForm4 (XForm4.mXAxis, yComp, 
                             wayVecAtPt.Normalized (), 
                             Geom.P2V (wayPt));
      if (ReferenceCS == RefCSys.MCS) 
         xFormRes = GCodeGenerator.XfmToMachine (mGCodeGenerator, xFormRes);

      return xFormRes;
   }

   void RewindEnumerator (int head) {
      mNextXFormIndex[head].wayPointIndex = 0;
      mNextXFormIndex[head].gCodeSegIndex = 0;
   }

   XForm4 mTransform0, mTransform1;

   // This method is the main workhouse of drawing the laser cutting tool.
   // This is called every time DrawOverLay() is called. This method also implements a
   // basic tool wait when there is another tool, which is also employed to cut
   void DrawToolSim (int head) {
      if (head == 3) {
         mTransform0 = GetNextToolXForm (0);
         mTransform1 = GetNextToolXForm (1);
      } else if (head == 0) 
         mTransform0 = GetNextToolXForm (0);
      else if (head == 1) 
         mTransform1 = GetNextToolXForm (1);

      if (mTransform0 == null && mTransform1 == null 
            && SimulationStatus != ESimulationStatus.NotRunning) {
         SimulationFinished?.Invoke ();
         SimulationStatus = ESimulationStatus.NotRunning;
         Lux.StopContinuousRender (GFXCallback);
         return;
      }

      Color32 neonGreen = new (57, 255, 20);
      Color32 neonRed = new (255, 87, 51);
      mMachiningTool.Draw (mTransform0, neonGreen, mTransform1, neonRed);
   }

   public void DrawToolInstance () {
      int head = 0;
      if (MCSettings.It.Heads == MCSettings.EHeads.Right) 
         head = 1;

      if (MCSettings.It.Heads == MCSettings.EHeads.Both) 
         DrawToolSim (3);
      else 
         DrawToolSim (head);
   }

   /// <summary>Called when the SIMULATE button is clicked</summary>
   public void Run () {
      if (SimulationStatus == ESimulationStatus.Running) 
         return;

      var prevSimulationStatus = SimulationStatus;
      if (mTraces[0] == null && mTraces[1] == null) 
         return;

      if (mTraces[0] != null && mTraces[0].Count > 0) {
         SimulationStatus = ESimulationStatus.Running;
         mXForms[0].Clear ();
      }

      if (mTraces[1] != null && mTraces[1].Count > 0) {
         SimulationStatus = ESimulationStatus.Running;
         mXForms[1].Clear ();
      }

      //[Alag:Review] need to use switch
      if (SimulationStatus == ESimulationStatus.Running) {
         if (MCSettings.It.Heads is MCSettings.EHeads.Left or MCSettings.EHeads.Both 
            && prevSimulationStatus == ESimulationStatus.NotRunning) 
            RewindEnumerator (0);
         if (MCSettings.It.Heads is MCSettings.EHeads.Right or MCSettings.EHeads.Both 
            && prevSimulationStatus == ESimulationStatus.NotRunning) 
            RewindEnumerator (1);

         Lux.StartContinuousRender (GFXCallback);
      }
   }
   void GFXCallback (double elapsed) {
      // TODO : Based on the elapsed time, the speed of the tool(s)
      // should be calculated.
      TriggerRedraw?.Invoke ();
   }
   public void Stop () {
      Lux.StopContinuousRender (GFXCallback);
      SimulationStatus = ESimulationStatus.NotRunning;
      //[Alag:Review] need to use switch
      if (MCSettings.It.Heads is MCSettings.EHeads.Left or MCSettings.EHeads.Both) 
         RewindEnumerator (0);

      if (MCSettings.It.Heads is MCSettings.EHeads.Right or MCSettings.EHeads.Both) 
         RewindEnumerator (1);

      SimulationFinished?.Invoke ();
      GFXCallback (0.01);
   }

   public void Pause () {
      SimulationStatus = ESimulationStatus.Paused;
      Lux.StopContinuousRender (GFXCallback);
   }
   #endregion

   #region GCode Draw Implementation
   public void DrawGCode () {
      List<List<GCodeSeg>> listOfListOfDrawables = [];
      if (Traces[0].Count > 0) 
         listOfListOfDrawables.Add (Traces[0]);

      if (Traces[1].Count > 0) 
         listOfListOfDrawables.Add (Traces[1]);

      foreach (var drawables in listOfListOfDrawables) {
         for (int ii = 0; ii < drawables.Count; ii++) {
            var seg = drawables[ii];
            if (ReferenceCS == RefCSys.MCS)
               seg = seg.XfmToMachineNew (mGCodeGenerator);

            Color32 segColor = Color32.Nil;
            if (seg.IsLine ()) {
               if (seg.GCode == EGCode.G0 || seg.MoveType == EMove.Retract2Machining) 
                  segColor = new Color32 (255, 255, 255); 
               else 
                  segColor = Color32.Blue;

               AppUI.ThreadDispatcher.Invoke (() => {
                  Lux.HLR = true;
                  Lux.Color = segColor;
                  Lux.Draw (EDraw.Lines, [seg.StartPoint, seg.EndPoint]);
               });
            } else if (seg.IsArc ()) {
               if (seg.GCode == EGCode.G3) 
                  segColor = Color32.Cyan; 
               else 
                  segColor = Color32.Magenta;

               var arcPoints = Utils.DiscretizeArc (seg, 50);
               using var enumerator = arcPoints.GetEnumerator ();
               if (!enumerator.MoveNext ()) 
                  return;

               var currentPoint = enumerator.Current;
               while (enumerator.MoveNext ()) {
                  var nextPoint = enumerator.Current;
                  AppUI.ThreadDispatcher.Invoke (() => {
                        Lux.HLR = true;
                        Lux.Color = segColor;
                        Lux.Draw (EDraw.Lines, [currentPoint.Item1, nextPoint.Item1]);
                        });

                  currentPoint = nextPoint;
               }
            }
         }
      }
   }
   #endregion
}
