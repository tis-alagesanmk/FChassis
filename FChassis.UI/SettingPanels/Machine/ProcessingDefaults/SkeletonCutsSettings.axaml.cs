using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class SkeletonCutsSettings : Panel{
   public SkeletonCutsSettings () {
      AvaloniaXamlLoader.Load (this);
      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, new ControlInfo[] {
            new GroupControlInfo{label="Sheet cutting rules"},
            new CheckControlInfo{label="Create sheet cut"},
            new _TextControlInfo{label="X spacing between vertical sheet cuts"},
            new _TextControlInfo{label="Y spacing between horizontal sheet cuts"},
            new CheckControlInfo{label="Create remainder sheet"},
            new _TextControlInfo{label="Minimum remainder sheet width"},
            new _TextControlInfo{label="Final cut X offset"},
            new CheckControlInfo{label="Process sheet cut after all part"},

            new GroupControlInfo{label="Sheet cut parameters"},
            new _TextControlInfo{label="Micro joint gap at sheet edge"},
            new _TextControlInfo{label="Micro joint gap at part edge"},
            new _TextControlInfo{label="Pierce distance from part edge"},
            new _TextControlInfo{label="Measuring distance from sheet edge"},
            new _TextControlInfo{label="Overtravel after sheet edge"},
      });
   }
}