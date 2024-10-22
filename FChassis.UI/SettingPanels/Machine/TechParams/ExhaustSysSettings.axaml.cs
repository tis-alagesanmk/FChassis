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
      this.AddControls ();
   }

   private void AddControls () {
      ControlInfo[] ctrlInfos = new ControlInfo[]
      {       
         new GroupControlInfo{label = "Sections"},
         CreateSectionDGrid(),
         new GroupControlInfo{label = "Splitters"},
         CreateSplittersdDGrid()
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

      #region Local function
      DGridControlInfo CreateSectionDGrid () {
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Sections",
            Collections = vm.Sections.Cast<ExhaustSysModel> ().ToArray (),
            columns = new[] {
               new DGridControlInfo.ColInfo{type = ControlInfo.Type.Text_, header = "Section  Number",path ="SectionNumber"},
               new DGridControlInfo.ColInfo{type = ControlInfo.Type.Text_, header = "X ON",path="XOn"},
               new DGridControlInfo.ColInfo{type = ControlInfo.Type.Text_, header = "X OFF",path="XOff"},
               new DGridControlInfo.ColInfo{type = ControlInfo.Type.Text_, header = "Y ON" ,path="YOn"},
               new DGridControlInfo.ColInfo{type = ControlInfo.Type.Text_, header = "Y OFF" ,path="YOff"},
         }};


         return dGridCrtlInfo;
      }
   DGridControlInfo CreateSplittersdDGrid () {
      DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
         binding = "Splitters",
         Collections = vm.Splitters.Cast<ExhaustSysModel> ().ToArray (),
         columns = new[]
         {
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Splitter  Number",path ="SectionNumber"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "X ON",path="XOn"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "X OFF",path="XOff" },
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Y ON" ,path="YOn"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Y OFF" ,path="YOff"},
         }
      };
         return dGridCrtlInfo;
      }
      #endregion Local function
   }
}