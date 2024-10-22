using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.PLCOptions;
public partial class PLCKeySettings : Panel {
   public PLCKeySettings () {
      AvaloniaXamlLoader.Load (this);
 
      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null) {
         string groupName;
         const int count = 12;
         ControlInfo[] controlInfos = new ControlInfo[count * 2];
         for (int i = 0; i < count; i++) {
            groupName = $"PLCKey {i + 1}";
            controlInfos[i * 2] = new GroupControlInfo { label = groupName };
            controlInfos[i * 2 + 1] = createPLCKey (groupName);
         }

         this.AddParameterControls (grid, controlInfos);
      }
      
      DGridControlInfo createPLCKey(object binding) {
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = binding,
            columns = new[] {
               new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Name" },
               new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Type" },
               new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Function" },
            }
         };

         return dGridCrtlInfo;
      }
   }
}