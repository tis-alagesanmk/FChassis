using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.SettingPanels.Machine.Model;
using FChassis.UI.SettingPanels.Machine.ViewModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class ExhaustSysSettings : Panel {
   ExhaustSystemViewModel vm = null;
   public ExhaustSysSettings () {
      AvaloniaXamlLoader.Load (this);
      vm = new ExhaustSystemViewModel ();
      this.DataContext = vm;

      this.AddControls ();

      //Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      //grid.RowDefinitions.Add (new RowDefinition { Height = new (32) });
      //var dGris = this.GetDataGrid (vm.Sections);
      //dGris.SetCurrentValue (Grid.RowProperty, 0);
      //// dGris.SetCurrentValue (Grid.ColumnProperty, 0);
      //// dGris.SetCurrentValue (Grid.ColumnSpanProperty, 4);
      //grid.Children.Add (dGris);
   }

   private void AddControls () {
      ControlInfo[] ctrlInfos = new ControlInfo[]
      {
         new ControlInfo{type = ControlInfo.Type.Group, label = "Sections" },
         this.CreateSection(""),
          new ControlInfo{type = ControlInfo.Type.Group, label = "Splitters" },
         this.CreateSplitters(""),
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }

   DGridControlInfo CreateSection (object binding) 
   {
      DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
         binding = binding,
         Collections = vm.Sections.Cast<ExhaustSysModel> ().ToArray (),
         columns = new[]
         {
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Section  Number",path ="SectionNumber"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "X ON",path="XOn"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "X OFF",path="XOff" },
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Y ON" ,path="YOn"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Y OFF" ,path="YOff"},
         }
      };

      return dGridCrtlInfo;
   }
   DGridControlInfo CreateSplitters (object binding) {
      DGridControlInfo dGridCrtlInfo = new DGridControlInfo {
         binding = binding,
         Collections = vm.Splitters.Cast<ExhaustSysModel> ().ToArray (),
         columns = new[]
         {
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Section  Number",path ="SectionNumber"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "X ON",path="XOn"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "X OFF",path="XOff" },
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Y ON" ,path="YOn"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Y OFF" ,path="YOff"},
         }
      };

      return dGridCrtlInfo;
   }
}