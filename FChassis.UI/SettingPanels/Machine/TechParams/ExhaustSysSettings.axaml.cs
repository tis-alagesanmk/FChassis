using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.SettingPanels.Machine.Model;
using FChassis.UI.SettingPanels.Machine.ViewModel;
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
         createSectionDGrid(),

         new GroupControlInfo{label = "Splitters"},
         createSplittersdDGrid()
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

      // Alag: [Testing]
      if (ctrlInfos[3].control != null) {
         DataGrid dataGrid = ctrlInfos[3].control as DataGrid;
         dataGrid.ItemsSource = new object[] {
            new string[] {"Section1", "X ON-1", "X OFF-1", "Y ON-1", "Y OFF-1" },
            new string[] {"Section2", "X ON-1", "X OFF-1", "Y ON-1", "Y OFF-1" },
            new string[] {"Section3", "X ON-1", "X OFF-1", "Y ON-1", "Y OFF-1" },
            new string[] {"Section4", "X ON-1", "X OFF-1", "Y ON-1", "Y OFF-1" },
         };
      }

      #region Local function
      DGridControlInfo createSectionDGrid () {
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Sections",
            collections = vm.Splitters.Cast<ExhaustSysModel> ().ToArray (),
            columns = [
               new DGridControlInfo.ColInfo{type=ControlInfo.Type.Text_, header="Section Number",  
                                                                                          path="SectionNumber"},
               new DGridControlInfo.ColInfo{type=ControlInfo.Type.Text_, header="X ON",   path="XOn"},
               new DGridControlInfo.ColInfo{type=ControlInfo.Type.Text_, header="X OFF",  path="XOff"},
               new DGridControlInfo.ColInfo{type=ControlInfo.Type.Text_, header="Y ON" ,  path="YOn"},
               new DGridControlInfo.ColInfo{type=ControlInfo.Type.Text_, header="Y OFF",  path="YOff"},
         ]};

         return dGridCrtlInfo;
      }

    
      DGridControlInfo createSplittersdDGrid () {
         DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
            binding = "Splitters",
            collections = vm.Splitters.Cast<ExhaustSysModel> ().ToArray (),
            columns = [
               new DGridControlInfo.ColInfo{type=ControlInfo.Type.Text_, header="Splitter Number", 
                                                                                          path ="SectionNumber"},
               new DGridControlInfo.ColInfo{type=ControlInfo.Type.Text_, header="X ON",   path="XOn"},
               new DGridControlInfo.ColInfo{type=ControlInfo.Type.Text_, header="X OFF",  path="XOff" },
               new DGridControlInfo.ColInfo{type=ControlInfo.Type.Text_, header="Y ON",   path="YOn"},
               new DGridControlInfo.ColInfo{type=ControlInfo.Type.Text_, header="Y OFF",  path="YOff"},
         ]};

         return dGridCrtlInfo;
      }
      #endregion Local function
   }
}