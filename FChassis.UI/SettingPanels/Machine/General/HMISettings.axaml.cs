using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.Settings;

namespace FChassis.UI.Settings.Machine.General;
public partial class HMISettings : Panel {
   public HMISettings () {
      AvaloniaXamlLoader.Load (this);

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, [
            new GroupControlInfo ("General"),
            new ComboControlInfo ("Orientation"),
            new _TextControlInfo ("Step size to increment"),
            new _TextControlInfo ("Maximum days keep back up files"),
            new _TextControlInfo ("Minimum storage to keep back up files", "BindName", "GB"),                                  
            new ComboControlInfo ("PLC messages to display"),
            new CheckControlInfo ("Caption for command-bar icons","BindName"),
            new CheckControlInfo ("Mini player"),
            new ComboControlInfo ("Language"),
            new ComboControlInfo ("Theme"),

            new GroupControlInfo ("Screen size"),
            new _TextControlInfo ("Width"),
            new _TextControlInfo ("Height"),
      ]);
   }
}