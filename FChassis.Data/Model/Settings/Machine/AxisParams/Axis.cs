using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Setting.Machine.AxisParams {
   public partial class Axis : ObservableObject {

      [ObservableProperty, Prop ("Configuration parameters", Prop.Type.Check, "Advanced")]
      bool? advanced;

      [ObservableProperty, Prop (Prop.Type.Combo, "Axis type", null!, null!, ["Linear Axis","Rotary Axis"])]
      string? axisType = "Linear Axis";

      [ObservableProperty, Prop (Prop.Type.Combo,"Axis connection", null!, null!, ["Analog Axis","CAN Axis","Virtual Axis", "EtherCAT Axis"])]     
      string? axisConnection = "EtherCAT Axis";

      [ObservableProperty, Prop (Prop.Type.Text, "Axis address")]
      int? axisAddress = 1;

      [ObservableProperty,Prop(Prop.Type.Text, "Sync connection")]
      int? syncConnection = -1;

      [ObservableProperty,Prop(Prop.Type.Text, "Interpolation filter time","s")]
      double? interpolationFilterTime = 1.5;


      [ObservableProperty,Prop("Resolution", Prop.Type.Text, "Increaments per distance")]
      double? incrementsPerDistance = 1762.23;

      [ObservableProperty,Prop(Prop.Type.Text,"Distance")]
      double? distance = 22.67;


      [ObservableProperty,Prop("Monitoring", Prop.Type.Text, "Software limit active", "mm")]
      bool? softwareLimitActive;

      [ObservableProperty,Prop(Prop.Type.Text, "Software limit negative", "mm")]
      double? softwareLimitNegative = 98.56;

      [ObservableProperty,Prop(Prop.Type.Text, "Software limit positive", "mm")]
      double? softwareLimitPositive = 54.23;

      [ObservableProperty,Prop(Prop.Type.Text, "Exact stop lag window","mm")]
      double? exactStopLagWindow = 12.23;

      [ObservableProperty,Prop(Prop.Type.Text, "Exact stop time window", "s")]
      double? exactStopTimeWindow = 12.34;


      [ObservableProperty,Prop("Referencing", Prop.Type.Text, "Homing velocity2", "m/min")]
      double? homeVelocity2 = 12.23;

      [ObservableProperty,Prop(Prop.Type.Text, "Homing acceleration2", "m/sec²")]
      double? homingAcceration2 = 89.98;

      [ObservableProperty, Prop (Prop.Type.Text, "Homing mode", "")]
      int? homingMode = 12;

      [ObservableProperty, Prop (Prop.Type.Text, "Homing velocity1", "m/min")]
      double? homingVelocity1 = 34.5;

      [ObservableProperty, Prop (Prop.Type.Text, "Homing acceleration1", "m/sec²")]
      double? homingAcceration1 = 67.89;

      [ObservableProperty, Prop (Prop.Type.Text, "Homing offset", "mm")]
      double? homingOffset = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Homing direction and sequence", "")]
      int? homingDirectionAndSequence = 3;


      [ObservableProperty, Prop ("Speed & Acceleration", Prop.Type.Text, "Moal velocity", "m/min")]
      double? modalVelocity = 12.3;

      [ObservableProperty, Prop (Prop.Type.Text, "Velocity", "m/min")]
      double? velocity = 12.3;

      [ObservableProperty, Prop (Prop.Type.Text, "Acceleration", "m/min")]
      double? acceleration = 12.3;

      [ObservableProperty, Prop (Prop.Type.Text, "Dcceleration", "m/sec²")]
      double? deceleration = 12.3;

      [ObservableProperty, Prop (Prop.Type.Text, "Ramp Time", "ms")]
      double? rampTime = 12.3;


      [ObservableProperty, Prop ("Corrections", Prop.Type.Text, "Blacklash compensation", "mm")]
      double? backlashCompensation = 12.52;

      [ObservableProperty, Prop ("Synchronous", Prop.Type.Text, "Synchronous offset", "mm")]
      double? synchronousOffset = 12.36;

      [ObservableProperty, Prop (Prop.Type.Text, "Synchronous position deviation", "mm")]
      double? synchronousPositionDeviation = 12.3;


      [ObservableProperty, Prop ("Handwheel", Prop.Type.Text, "Handwheel assignment")]
      int? handwheelAssignment = 1;

      [ObservableProperty, Prop ("Handwheel", Prop.Type.Text, "Handwheel factor")]
      double? handwheelFactor = 12.3;

   }
}
