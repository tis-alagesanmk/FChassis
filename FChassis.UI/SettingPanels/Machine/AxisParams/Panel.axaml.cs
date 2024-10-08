using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.AxisParams;
public partial class Panel : FChassis.UI.Settings.TabPanel {

   public Panel () {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([this.xAxis]);
   }


   #region "Fields"
   AxisParams.XAxis xAxis = new ();
   #endregion "Fields"
}