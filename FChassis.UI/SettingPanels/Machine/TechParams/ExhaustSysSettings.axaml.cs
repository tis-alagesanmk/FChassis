using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FChassis.UI.SettingPanels.Machine.ViewModel;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class ExhaustSysSettings : Panel {
   ExhaustSystemViewModel vm = null;
   public ExhaustSysSettings () {
      AvaloniaXamlLoader.Load (this);
      vm = new ExhaustSystemViewModel ();
      this.DataContext = vm;

      this.AddControls ();
   }

   private void AddControls () {
      ControlInfo[] ctrlInfos = new ControlInfo[]
      {
         new ControlInfo{type = ControlInfo.Type.Group, label = "Sections" },
         createPLCKey("")
      };

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid, ctrlInfos);

   }

   DGridControlInfo createPLCKey (object binding) 
   {
      DGridControlInfo dGridCrtlInfo = new DGridControlInfo 
      {
         binding = binding,
         columns = new[] 
         {
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Section  Number",path ="SectionNumber", items = vm.Sections },
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "X ON",path="XOn"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "X OFF",path="XOff" },
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Y ON" ,path="YOn"},
            new DGridControlInfo.ColInfo { type = ControlInfo.Type.Text_, header = "Y OFF" ,path="YOff"},
         }
      };

      return dGridCrtlInfo;
   }
}