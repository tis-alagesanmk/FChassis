using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.PLCOptions;
public partial class FuncParamSettings : Panel {
   public FuncParamSettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo{label="PLC Custom Parameters"},
            new CheckControlInfo{label="Advanced"},

            new GroupControlInfo{label="Jog Parameters"},
            new _TextControlInfo{label="Acc/Dec", unit="m/sec²"},
            new _TextControlInfo{label="Ramp Time", unit="ms"},

            new GroupControlInfo{label="Miscellaneous"},
            new _TextControlInfo{label="Oil Lubrication Cycle Time", unit="h"},
            new _TextControlInfo{label="Grease Lubrication Cycle Time", unit="h"},
            new _TextControlInfo{label="Chip Conveyor Time", unit="min"},
            new ComboControlInfo{label="Laser cut head type"},
            new ComboControlInfo{label="Laser type"},
            new _TextControlInfo{label="Delay for stop-pause", unit="ms"},
            new _TextControlInfo{label="Delay for stop-abort", unit="ms"},
            new ComboControlInfo{label="Machine origin"},
            new ComboControlInfo{label="Jog keys"},
            new ComboControlInfo{label="Override keys"},

            new GroupControlInfo{label="Tandem Operation Options"},
            new CheckControlInfo{label="Active Tandem operation"},
            new CheckControlInfo{label="Deactivate Red Zone Stop"},

            new GroupControlInfo{label="Nozzle Cleaning Options"},
            new CheckControlInfo{label="Nozzle cleaning active"},
            new CheckControlInfo{label="HC Calibration after Nozzle clean"},
            new CheckControlInfo{label="X-axis Limit Change"},
            new ComboControlInfo{label="Brush"},

            new GroupControlInfo{label="Auto Nozzle Clean Option"},
            new ComboControlInfo{label="Based on"},
            new ComboControlInfo{label="State of program"},

            new GroupControlInfo{label="Pallet Changer Configuration"},
            new ComboControlInfo{label="Pallet"},
            new CheckControlInfo{label="Pallet Lock"},
            new ComboControlInfo{label="Shuttle"},
            new ComboControlInfo{label="Up/Down motion"},
            new ComboControlInfo{label="Pallet Door"},
            new ComboControlInfo{label="Light Barrier"},

            new GroupControlInfo{label="Laser Technology Miscellaneous Control"},
            new CheckControlInfo{label="Auto Focus active"},
            new CheckControlInfo{label="HC Cslibration On Ext. Plate"},
            new CheckControlInfo{label="High Peak Power"},
            new CheckControlInfo{label="HC in 2 steps for position > 9mm &amp; measure range = 20mm"},
            new ComboControlInfo{label="High Pressure Valve"},
            new ComboControlInfo{label="Low Pressure Valve"},
            new CheckControlInfo{label="Adaptive optics"},

            new GroupControlInfo{label="Machine Miscellaneous Control"},
            new CheckControlInfo{label="External Start Stop"},
            new CheckControlInfo{label="Mode selection via external keys"},
            new CheckControlInfo{label="Z-axis Dynamic Enable bit"},
            new CheckControlInfo{label="Activate Exhaust system"},
            new CheckControlInfo{label="Activate Sheet Edge function"},
            new CheckControlInfo{label="Auto lubrication activate"},
            new CheckControlInfo{label="Auto Chip conveyor activate"},
            new CheckControlInfo{label="Auto Exhaust activate"},
            new CheckControlInfo{label="Enable park position at program end"},
            new CheckControlInfo{label="Activate auto nozzle changer"},

            new GroupControlInfo{label="Emulate Hardware"},
            new CheckControlInfo{label="EStop"},
            new CheckControlInfo{label="Field Bus module device"},
            new CheckControlInfo{label="Axis device"},
            new CheckControlInfo{label="Exhaust device"},
            new CheckControlInfo{label="Height Control"},
            new CheckControlInfo{label="Laser device"},
            new CheckControlInfo{label="No collision input"},
            new CheckControlInfo{label="Beam mode index"},
      ]);
   }
}