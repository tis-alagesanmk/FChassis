using Avalonia.Markup.Xaml;
using FChassis.UI.Settings.Machine.TechParams;

namespace FChassis.UI.Settings.Machine.PLCOptions;
public partial class Panel : FChassis.UI.Settings.TabPanel {
   public Panel () {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([this.functionParameters, this.controlParameters, this.pLCKeys]);
   }

   #region "Fields"
   FunctionParameters functionParameters = new ();
   ControlParameters controlParameters = new ();
   PLCKeys pLCKeys = new ();
   #endregion "Fields"
}