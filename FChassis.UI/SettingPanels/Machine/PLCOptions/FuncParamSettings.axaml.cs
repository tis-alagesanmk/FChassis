using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.PLCOptions;
public partial class FuncParamSettings : Panel {
   public FuncParamSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("PLC Custom Parameters", null!),
            new CheckControlInfo ("Advanced", null!),

            new GroupControlInfo ("Jog Parameters"),
            new _TextControlInfo ("Acc/Dec", null!, "m/sec²"),
            new _TextControlInfo ("Ramp Time", null!, "ms"),

            new GroupControlInfo ("Miscellaneous"),
            new _TextControlInfo ("Oil Lubrication Cycle Time", null!, "h"),
            new _TextControlInfo ("Grease Lubrication Cycle Time", null!, "h"),
            new _TextControlInfo ("Chip Conveyor Time", null!, "min"),
            new ComboControlInfo ("Laser cut head type", null!, null!),
            new ComboControlInfo ("Laser type", null!, null!),
            new _TextControlInfo ("Delay for stop-pause", null!, "ms"),
            new _TextControlInfo ("Delay for stop-abort", null!, "ms"),
            new ComboControlInfo ("Machine origin", null!, null!),
            new ComboControlInfo ("Jog keys", null!, null!),
            new ComboControlInfo ("Override keys", null!, null!),

            new GroupControlInfo ("Tandem Operation Options"),
            new CheckControlInfo ("Active Tandem operation", null!),
            new CheckControlInfo ("Deactivate Red Zone Stop", null!),

            new GroupControlInfo ("Nozzle Cleaning Options"),
            new CheckControlInfo ("Nozzle cleaning active", null!),
            new CheckControlInfo ("HC Calibration after Nozzle clean", null!),
            new CheckControlInfo ("X-axis Limit Change", null!),
            new ComboControlInfo ("Brush", null!, null!),

            new GroupControlInfo ("Auto Nozzle Clean Option"),
            new ComboControlInfo ("Based on", null!, null!),
            new ComboControlInfo ("State of program", null!, null!),

            new GroupControlInfo ("Pallet Changer Configuration"),
            new ComboControlInfo ("Pallet", null!, null!),
            new CheckControlInfo ("Pallet Lock", null!),
            new ComboControlInfo ("Shuttle", null!, null!),
            new ComboControlInfo ("Up/Down motion", null!, null!),
            new ComboControlInfo ("Pallet Door", null!, null!),
            new ComboControlInfo ("Light Barrier", null!, null!),

            new GroupControlInfo ("Laser Technology Miscellaneous Control"),
            new CheckControlInfo ("Auto Focus active", null!),
            new CheckControlInfo ("HC Cslibration On Ext. Plate", null!),
            new CheckControlInfo ("High Peak Power", null!),
            new CheckControlInfo ("HC in 2 steps for position > 9mm &amp; measure range = 20mm", null!),
            new ComboControlInfo ("High Pressure Valve", null!, null!),
            new ComboControlInfo ("Low Pressure Valve", null!, null!),
            new CheckControlInfo ("Adaptive optics", null!),

            new GroupControlInfo ("Machine Miscellaneous Control"),
            new CheckControlInfo ("External Start Stop", null!),
            new CheckControlInfo ("Mode selection via external keys", null!),
            new CheckControlInfo ("Z-axis Dynamic Enable bit", null!),
            new CheckControlInfo ("Activate Exhaust system", null!),
            new CheckControlInfo ("Activate Sheet Edge function", null!),
            new CheckControlInfo ("Auto lubrication activate", null!),
            new CheckControlInfo ("Auto Chip conveyor activate", null!),
            new CheckControlInfo ("Auto Exhaust activate", null!),
            new CheckControlInfo ("Enable park position at program end", null!),
            new CheckControlInfo ("Activate auto nozzle changer", null!),

            new GroupControlInfo ("Emulate Hardware"),
            new CheckControlInfo ("EStop", null!),
            new CheckControlInfo ("Field Bus module device", null!),
            new CheckControlInfo ("Axis device", null!),
            new CheckControlInfo ("Exhaust device", null!),
            new CheckControlInfo ("Height Control", null!),
            new CheckControlInfo ("Laser device", null!),
            new CheckControlInfo ("No collision input", null!),
            new CheckControlInfo ("Beam mode index", null!),
      ]);
   }
}