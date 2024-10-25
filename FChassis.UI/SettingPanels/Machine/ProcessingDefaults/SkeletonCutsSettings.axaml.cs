using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.ProcessingDefaults;
public partial class SkeletonCutsSettings : Panel{
   public SkeletonCutsSettings () {
      AvaloniaXamlLoader.Load (this);
      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("Sheet cutting rules"),
            new CheckControlInfo ("Create sheet cut", null!),
            new _TextControlInfo ("X spacing between vertical sheet cuts", null!),
            new _TextControlInfo ("Y spacing between horizontal sheet cuts", null!),
            new CheckControlInfo ("Create remainder sheet", null!),
            new _TextControlInfo ("Minimum remainder sheet width", null!),
            new _TextControlInfo ("Final cut X offset", null!),
            new CheckControlInfo ("Process sheet cut after all part", null!),

            new GroupControlInfo ("Sheet cut parameters"),
            new _TextControlInfo ("Micro joint gap at sheet edge", null!),
            new _TextControlInfo ("Micro joint gap at part edge", null!),
            new _TextControlInfo ("Pierce distance from part edge", null!),
            new _TextControlInfo ("Measuring distance from sheet edge", null!),
            new _TextControlInfo ("Overtravel after sheet edge", null!),
      ]);
   }
}