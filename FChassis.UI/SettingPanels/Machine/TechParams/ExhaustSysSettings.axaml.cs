using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.SettingPanels.Machine.Model;
using FChassis.UI.SettingPanels.Machine.ViewModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class ExhaustSysSettings : Panel {
   ExhaustSystemViewModel vm = null!;
   public ExhaustSysSettings () {
      AvaloniaXamlLoader.Load (this);
      vm = new ExhaustSystemViewModel ();
      this.DataContext = vm;

      ControlInfo[] ctrlInfos = new ControlInfo[] {
         new GroupControlInfo{label = "Sections"},
         createSectionDGrid() 
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

      #region Local function
      DGridControlInfo createSectionDGrid () {
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Sections",
            columns = new[] {
               new DGridControlInfo.ColInfo{type = ControlInfo.Type.Text_, header = "Section  Number",path ="SectionNumber"},
               new DGridControlInfo.ColInfo{type = ControlInfo.Type.Text_, header = "X ON",path="XOn"},
               new DGridControlInfo.ColInfo{type = ControlInfo.Type.Text_, header = "X OFF",path="XOff"},
               new DGridControlInfo.ColInfo{type = ControlInfo.Type.Text_, header = "Y ON" ,path="YOn"},
               new DGridControlInfo.ColInfo{type = ControlInfo.Type.Text_, header = "Y OFF" ,path="YOff"},
         }};

         return dGridCrtlInfo;
      }
      #endregion Local function
   }
}