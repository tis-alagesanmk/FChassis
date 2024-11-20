using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis;

/// <summary>All fields that can be set through the Options/Settings dialog</summary>
public partial class MCSettings : ObservableObject {
   #region Constructors
   // Singleton instance
   public static MCSettings It => sIt ??= new ();
   static MCSettings sIt;
   #endregion

   #region Delegates and Events
   public delegate void SettingValuesChangedEventHandler ();
   
   // Any changes to the properties here will also change 
   // elsewhere where the OnSettingValuesChangedEvent is subscribed with
   public event SettingValuesChangedEventHandler OnSettingValuesChangedEvent;
   #endregion

   #region Enums
   public enum ERotate {
      Rotate0, Rotate90, Rotate180, Rotate270
   }
   
   public enum EHeads {
      Left,
      Right,
      Both,
      None
   }

   public enum PartConfigType {
      LHComponent,
      RHComponent
   }
   #endregion

   #region Helpers 
   protected override void OnPropertyChanged (PropertyChangedEventArgs e) {
      base.OnPropertyChanged (e);
      OnSettingValuesChangedEvent?.Invoke ();
   }
   #endregion

   #region Properties
   [ObservableProperty] EHeads heads = EHeads.Both;
   [ObservableProperty] double standoff = 0.0;
   [ObservableProperty] public EKind[] toolingPriority = [EKind.Hole, EKind.Notch, EKind.Cutout, EKind.Mark];
   [ObservableProperty] double markTextPosX = 700.4;
   [ObservableProperty] double markTextPosY = 10.0;
   [ObservableProperty] string markText = "Deluxe";
   [ObservableProperty] ERotate markAngle = ERotate.Rotate0;
   [ObservableProperty] bool optimizeSequence = false;
   [ObservableProperty] int progNo = 1;
   [ObservableProperty] string nCFilePath = System.IO.Directory.Exists ("W:\\FChassis\\Sample") 
                           ?"W:\\FChassis\\Sample" : "";

   [ObservableProperty] double safetyZone;
   [ObservableProperty] uint serialNumber;
   [ObservableProperty] bool syncHead;
   [ObservableProperty] bool usePingPong = true;
   [ObservableProperty] PartConfigType partConfig = PartConfigType.LHComponent;
   [ObservableProperty] bool optimizePartition;
   [ObservableProperty] bool rotateX180 = false;
   [ObservableProperty] bool showToolingNames;
   [ObservableProperty] bool includeFlange;
   [ObservableProperty] bool includeCutout;
   [ObservableProperty] bool includeWeb;
   [ObservableProperty] double partitionRatio = 0.5;
   [ObservableProperty] double probeMinDistance;
   [ObservableProperty] double notchApproachLength = 5.0;
   [ObservableProperty] double approachLength = 2;
   [ObservableProperty] double notchWireJointDistance = 2.0;
   [ObservableProperty] double flexOffset;
   [ObservableProperty] double stepLength = 1.0;
   [ObservableProperty] bool enableMultipassCut = true;
   [ObservableProperty] double maxFrameLength = 3500;
   [ObservableProperty] bool maximizeFrameLengthInMultipass = true;
   [ObservableProperty] bool cutHoles = true;
   [ObservableProperty] bool cutNotches = true;
   [ObservableProperty] bool cutCutouts = true;
   [ObservableProperty] bool cutMarks = true;
   [ObservableProperty] double minThresholdForPartition = 585.0;
   [ObservableProperty] double minNotchLengthThreshold = 210;
   [ObservableProperty] string dINFilenameSuffix = "";
   [ObservableProperty] string workpieceOptionsFilename = @"W:\FChassis\LCM2HWorkpieceOptions.json";
   [ObservableProperty] MachineType machine;
   [ObservableProperty] double deadbandWidth = 600.0;
   #endregion

   #region JSON Read/Write Methods
   // Method to serialize the singleton instance to a JSON file
   public void SaveToJson (string filePath) {
      Core.File.JSONFileWrite writer = new ();
      writer.Write (filePath, this);
   }

   // Method to deserialize from JSON and set the singleton instance
   public void LoadFromJson (string filePath) {
      Core.File.JSONFileRead reader = new ();
      reader.Read (filePath, this);
   }
   #endregion
}
