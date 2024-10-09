using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.AxisParams;
public partial class Panel : FChassis.UI.Settings.TabPanel {
   public Panel () {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([this.lpc1, this.lpc1, this.lpc1, this.lpc1, this.pallet1]);
   }

   #region "Fields"
   LPC1 lpc1 = new ();
   Pallet1 pallet1 = new ();
   #endregion "Fields"
}