using FChassis.Data.ViewModels.Setting.Machine.General;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using static FChassis.UI.Settings.ControlInfo;

namespace FChassis.UI.Settings.Machine.General;
public partial class HMISettings : Panel {
   public HMISettings () {
      AvaloniaXamlLoader.Load (this);

      HMIViewModel vm = new HMIViewModel ();
      this.DataContext = vm;

      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)

         this.AddParameterControls (grid, [
            new GroupControlInfo ("General"),
            new ComboControlInfo ("Orientation") {bindInfos = [ControlInfo.Combo.Binding ("Portrait"), ControlInfo.Combo.BindingItems(nameof(vm.Orientation))]},
            new _TextControlInfo ("Step size to increment", nameof(vm.StepSizetoIncrement),""),
            new _TextControlInfo ("Maximum days keep back up files", nameof(vm.MaximumDaysKeepBackupFiles),""),
            new _TextControlInfo ("Minimum storage to keep back up files", nameof(vm.MinimumStoragetoKeepBackupFiles), "GB"),                                  
            new ComboControlInfo ("PLC messages to display", nameof(vm.PLCMessagesToDisplay)),
            new CheckControlInfo ("Caption for command-bar icons",nameof(vm.CaptionForcommandBarIcons)),
            new CheckControlInfo ("Mini player", nameof(vm.CaptionForcommandBarIcons)),
            new ComboControlInfo ("Language") {bindInfos = [ControlInfo.Combo.Binding ("EN"), ControlInfo.Combo.BindingItems(nameof(vm.Language))]},
            new ComboControlInfo ("Theme") {bindInfos = [ControlInfo.Combo.Binding ("Grey"), ControlInfo.Combo.BindingItems(nameof(vm.Theme))]},
            
            new GroupControlInfo ("Screen size"),
            new _TextControlInfo ("Width", nameof(vm.Width),""),
            new _TextControlInfo ("Height", nameof(vm.Height),""),
      ]);
   }
}