using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.TechParams {
   public partial class LaserSys : ObservableObject {

      [ObservableProperty, Prop ("Custom parameters 1", Prop.Type.Text, "X-axis park position","mm")]
      double? xAxisParkPosition = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Y-axis park position", "mm")]
      double? yAxisParkPosition = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Test Run Feedrate", "mm/min")]
      double? testRunFeedrate = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Frog Jump adjust distance", "mm")]
      double? frogJumpAdjustDistance = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Frog Jump adjust time", "ms")]
      double? frogJumpAdjustTime = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Contour End Control Time OFF", "ms")]
      double? contourEndControlTimeOff = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Contour End Parameter Time OFF", "ms")]
      double? contourEndParameterTimeOff = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Delay before start of cut", "ms")]
      double? delayBeforeStartOfCut = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Delay after end of cut", "ms")]
      double? delayAfterEndOfCut = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Laser HV ON Delay", "s")]
      double? laserHVOnDelay = 12.23;


      [ObservableProperty, Prop ("Custom parameters 2", Prop.Type.Text, "High Pressure Valve", "bar")]
      double? highPressureValve = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Low Pressure Valve", "bar")]
      double? lowPressureValve = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Gas Pressure difference to generate error", "%")]
      double? gasPressureDifferenceToGenerateError = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "High Pressure to clean kerf", "bar")]
      double? highPressureToCleanKerf = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Focus Ref Voltage", "V")]
      double? focusRefVoltage = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Focus Offset Voltage", "V")]
      double? focusOffsetVoltage = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Contour Length/No. of Pierce for Nozzle Cleaning", "mm")]
      double? contourLengthOrNoOfPierceForZozzleCleaning = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Height Control Sensor Gain")]
      double? heightControlSensorGain = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Height Control Retract Speed", "mm/min")]
      double? heightControlRetractSpeed = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Collision delay time", "ms")]
      double? collisionDelayTime = 12.23;


      [ObservableProperty, Prop ("Custom parameters 3", Prop.Type.Text, "Gas Idle purge pressure","bar")]
      double? gasIdlePurgePressure = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Z-axis Negative Limit - Shuttle Table", "mm")]
      double? zaxisNegativeLimitShuttleTable = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Exhaust Lag Time to Stop", "s")]
      double? exhaustLagTimeToStop = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Min distance between 2 gantries", "mm")]
      double? minDistanceBetweenGantries = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Tandem maximum Machine Stroke", "mm")]
      double? tandemMaximumMachineStroke = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Gas Idle Purge time", "s")]
      double? gasIdlePurgeTime = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Gas Purge pressure before start of program", "bar")]
      double? gasPurgePressureBeforeStartOfProgram = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Gas Purge time before start of program", "s")]
      double? gasPurgeTimeBeforeStartOfProgram = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Z-axis park position", "mm")]
      double? zaxisParkPosition = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "X-axis Limit value for Nozzle Cleaning", "mm")]
      double? xaxisLimitValueForNozzleCleaning = 12.23;


      [ObservableProperty, Prop ("Custom parameters 4", Prop.Type.Text, "Piercing Sensor Delay Time", "ms")]
      double? piercingSensorDelayTime = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Param 2")]
      double? param2 = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Param 3")]
      double? param3 = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Param 4")]
      double? param4 = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Param 5")]
      double? param5 = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Param 6")]
      double? param6 = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Param 7")]
      double? param7 = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Param 8")]
      double? param8 = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Param 9")]
      double? param9 = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Param 10")]
      double? param10 = 12.23;
   }
}
