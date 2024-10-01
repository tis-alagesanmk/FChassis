using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FChassis;

/// <summary>All fields that can be set through the Options/Settings dialog</summary>
public class MCSettings : INotifyPropertyChanged {
   #region Constructors
   // Singleton instance
   public static MCSettings It => sIt ??= new ();
   static MCSettings sIt;

   // Notify event, to bind the changes with SettingsDlg
   public event PropertyChangedEventHandler PropertyChanged;
   #endregion

   #region Delegates and Events
   // Any changes to the properties here will also change 
   // elsewhere where the OnSettingValuesChangedEvent is subscribed with
   public delegate void SettingValuesChangedEventHandler ();
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
   // Helper method to set a property and raise the event
   private void SetProperty<T> (ref T field, T value) {
      if (!Equals (field, value)) {
         field = value;
         OnSettingValuesChangedEvent?.Invoke ();
      }
   }

   // Method to raise the PropertyChanged event
   protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null) {
      PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
   }
   #endregion

   #region Settings Properties
   public EHeads Heads { 
      get => mHeads; 
      set => SetProperty (ref mHeads, value); }
   EHeads mHeads = EHeads.Both;

   /// <summary>Stand-off distance between laser nozzle tip and workpiece</summary>
   public double Standoff { 
      get => mStandoff;  
      set => SetProperty (ref mStandoff, value); }
   double mStandoff;

   public EKind[] ToolingPriority { 
      get => mToolingPriority; 
      set => SetProperty (ref mToolingPriority, value); }
   EKind[] mToolingPriority;

   public MCSettings () {
      mToolingPriority = [EKind.Hole, EKind.Cutout, EKind.Notch, EKind.Mark];
      mStandoff = 0.0;
      mMarkText = "Deluxe";
      mPartitionRatio = 0.5;
      mHeads = EHeads.Both;
      mApproachLength = 2;
      PartConfig = PartConfigType.LHComponent;
      MarkTextPosX = 700.4;
      MarkTextPosY = 10.0;
      NotchWireJointDistance = 2.0;
      NotchApproachLength = 5.0;
   }

   public double MarkTextPosX { 
      get => mMarkTextPosX; 
      set => SetProperty (ref mMarkTextPosX, value); }
   double mMarkTextPosX;

   public double MarkTextPosY { 
      get => mMarkTextPosY; 
      set => SetProperty (ref mMarkTextPosY, value); }
   double mMarkTextPosY;
   
   public string MarkText { 
      get => mMarkText; 
      set => SetProperty (ref mMarkText, value); }
   string mMarkText;
   
   public ERotate MarkAngle { 
      get => mMarkAngle;  
      set => SetProperty (ref mMarkAngle, value);  }
   ERotate mMarkAngle = ERotate.Rotate0;
   
   public bool OptimizeSequence { 
      get => mOptimizeSequence; 
      set => SetProperty (ref mOptimizeSequence, value); }
   bool mOptimizeSequence = false;
   
   public int ProgNo { 
      get => mProgNo; 
      set => SetProperty (ref mProgNo, value); }
   int mProgNo = 1;

   public string Head1NCFilePath { 
      get=> mHead1NCFilePath; 
      set => SetProperty (ref mHead1NCFilePath, value); }
   string mHead1NCFilePath;
   
   public string Head2NCFilePath { 
      get=> mHead2NCFilePath; 
      set => SetProperty (ref mHead2NCFilePath, value); }
   string mHead2NCFilePath;
   
   public double SafetyZone { 
      get=> mSafetyZone; 
      set => SetProperty (ref mSafetyZone, value); }
   double mSafetyZone;
   
   public uint SerialNumber { 
      get=> mSerialNumber; 
      set => SetProperty (ref mSerialNumber, value); }
   uint mSerialNumber;
   
   public bool SyncHead { 
      get=>mSyncHead; 
      set => SetProperty (ref mSyncHead, value); }
   bool mSyncHead;

   public bool UsePingPong { 
      get=> mUsePingPong; 
      set => SetProperty (ref mUsePingPong, value); }
   bool mUsePingPong = true;
   
   public PartConfigType PartConfig { 
      get => mPartConfig; 
      set => SetProperty (ref mPartConfig, value); }
   PartConfigType mPartConfig;

   public bool OptimizePartition { 
      get=> mOptimizePartition; 
      set => SetProperty (ref mOptimizePartition, value); }
   bool mOptimizePartition;

   public bool RotateX180 { 
      get=> mRotateX180; 
      set => SetProperty (ref mRotateX180, value); }
   bool mRotateX180;

   public bool IncludeFlange { 
      get=>mIncludeFlange; 
      set => SetProperty (ref mIncludeFlange, value); }
   bool mIncludeFlange;

   public bool IncludeCutout { 
      get=> mIncludeCutout; 
      set => SetProperty (ref mIncludeCutout, value); }
   bool mIncludeCutout;

   public bool IncludeWeb { 
      get=> mIncludeWeb; 
      set => SetProperty (ref mIncludeWeb, value);  }
   bool mIncludeWeb;

   public double PartitionRatio { 
      get => mPartitionRatio; 
      set => SetProperty (ref mPartitionRatio, value); }
   double mPartitionRatio;

   public double ProbeMinDistance { 
      get=>mProbeMinDistance; 
      set => SetProperty (ref mProbeMinDistance, value); }
   double mProbeMinDistance;

   public double NotchApproachLength { 
      get=> mNotchApproachLength; 
      set => SetProperty (ref mNotchApproachLength, value); }
   double mNotchApproachLength;

   public double ApproachLength { 
      get => mApproachLength; 
      set => mApproachLength = value; }
   double mApproachLength;

   public double NotchWireJointDistance { 
      get=> mNotchWireDistance; 
      set => SetProperty (ref mNotchWireDistance, value); }
   double mNotchWireDistance;

   public double FlexOffset { 
      get => mFlexOffset; 
      set => SetProperty (ref mFlexOffset, value); }
   double mFlexOffset;

   public double StepLength { 
      get => mLengthPerStep; 
      set => SetProperty (ref mLengthPerStep, value); }
   double mLengthPerStep = 1.0;
   #endregion
}
