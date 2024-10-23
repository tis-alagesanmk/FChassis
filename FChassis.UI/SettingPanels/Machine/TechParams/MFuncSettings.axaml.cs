using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class MFuncSettings : Panel {
   public MFuncSettings () {
      AvaloniaXamlLoader.Load (this);

      ControlInfo[] ctrlInfos = new ControlInfo[33];
      ctrlInfos[0] = new GroupControlInfo {label = "Sub program table"};
      for (int i = 1; i < 33; i++)
         ctrlInfos[i] = new _TextControlInfo {label = $"M number {i - 1}"};

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);
   }
}