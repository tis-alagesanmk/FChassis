using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.General;
public partial class HMISettings : Panel {
   public HMISettings () {
      AvaloniaXamlLoader.Load (this);

      var vm = this.DataContext as Data.ViewModel.Settings.Machine.General.HMIViewModel;
      Grid? grid = this.LogicalChildren[0].LogicalChildren[0] as Grid;
      if (grid != null)
         this.AddParameterControls (grid!, [
            new GroupControlInfo ("General"),
            new ComboControlInfo ("Orientation", nameof(vm.Orientation), nameof(vm.Orientations)),
            new _TextControlInfo ("Step size to increment", nameof(vm.StepSizetoIncrement)),
            new _TextControlInfo ("Maximum days keep back up files", nameof(vm.MaximumDaysKeepBackupFiles)),
            new _TextControlInfo ("Minimum storage to keep back up files", nameof(vm.MinimumStoragetoKeepBackupFiles), "GB"),                                  
            new ComboControlInfo ("PLC messages to display", nameof(vm.PlcMessagesToDisplay), nameof(vm.PlcMessages)),
            new CheckControlInfo ("Caption for command-bar icons", nameof(vm.CaptionForcommandBarIcons)),
            new CheckControlInfo ("Mini player", nameof(vm.MiniPlayer)),
            new ComboControlInfo ("Language", nameof(vm.Language), nameof(vm.Languages)),
            new ComboControlInfo ("Theme", nameof(vm.Theme), nameof(vm.Themes)),

            new GroupControlInfo ("Screen size"),
            new _TextControlInfo ("Width", nameof(vm.Width)),
            new _TextControlInfo ("Height", nameof(vm.Height)),
      ]);
   }
}