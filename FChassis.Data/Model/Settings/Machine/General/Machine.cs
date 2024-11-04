using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.General; 
public partial class Machine : ObservableObject{
   [ObservableProperty, Prop ("General", Prop.Type.Text, "Machine Id")]
   string machineId = "130166";

   [ObservableProperty, Prop (Prop.Type.Text, "Axis emulation")]
   string axisEmulation = "1";

   [ObservableProperty, Prop (Prop.Type.Text, "Cfg custom tech")]
   string cfgCustomTech = "ECUT";

   [ObservableProperty, Prop (Prop.Type.Text, "Presets")]
   string presets = "G122 X4;G17;S0;T0;G163;G175;M48;G31";

   [ObservableProperty, Prop (Prop.Type.Text, "Can open bit rate", "kbps")]
   string canOpenBitRate = "1000,1000";

   [ObservableProperty, Prop (Prop.Type.Check, "Netdisk server IP address")]
   string netDiskServerIPAddr = "1000,1000";

   [ObservableProperty, Prop (Prop.Type.Combo, "Software limit code", null!, null!, ["12", "13", "14", "15", "16"])]
   string code = "14";

   [ObservableProperty, Prop (Prop.Type.Text, "Override limit")]
   int overrideLimit = 1;

   [ObservableProperty, Prop (Prop.Type.Button, "Limit Incrementer")]
   int incrementLimitCommand;


   [ObservableProperty, Prop ("Controller", Prop.Type.Text, "Interpolation cycle time", "ms")]
   string interpolationCycleTime = "1";

   [ObservableProperty, Prop (Prop.Type.Text, "Interpolation divider")]
   string interpolationDivider = "2";


   [ObservableProperty, Prop (Prop.Type.Text, "Handwheel filter time", "ms")]
   string handWheelFilterTime = "0";

   [ObservableProperty, Prop (Prop.Type.Text, "Velocity", "m/min")]
   string velocity = "130";

   [ObservableProperty, Prop (Prop.Type.Text, "Acceleration", "m/sec²")]
   string acceleration = "40";

   [ObservableProperty, Prop (Prop.Type.Text, "Deceleration", "m/sec²")]
   string deceleration = "40";

   [ObservableProperty, Prop (Prop.Type.Text, "Ramp time", "ms")]
   string rampTime = "10";

   [ObservableProperty, Prop (Prop.Type.Text, "Position tolerance MM", "mm")]
   string positionToleranceMM = "0.05";

   [ObservableProperty, Prop (Prop.Type.Text, "Position tolerance Degree", "°")]
   string positionToleranceDDegree = "0.01";

   [ObservableProperty, Prop (Prop.Type.Text, "Quick stop time", "ms")]
   string quickStopTime = "20";

   [ObservableProperty, Prop (Prop.Type.Text, "Creep speed velocity", "m/min")]
   string creepSpeedVelocity = "1";



   [ObservableProperty, Prop ("Memory reservation", Prop.Type.Text, "Block count")]
   string blockCount = "1024";

   [ObservableProperty, Prop (Prop.Type.Text, "Reverse Block count")]
   string reverseBlockCount = "1000";

   [ObservableProperty, Prop (Prop.Type.Text, "Parameter aray size")]
   string parameterArraySize = "10000";

}
