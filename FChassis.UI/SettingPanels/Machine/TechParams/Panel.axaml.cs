using Avalonia.Markup.Xaml;

namespace FChassis.UI.Settings.Machine.TechParams;
public partial class Panel : FChassis.UI.Settings.TabPanel {
   public Panel () {
      AvaloniaXamlLoader.Load (this);
      this.PopulateTabItemContent ([this.analogScaling, null, null, this.exhaustSystem,null]);
   }


   #region "Fields"
   TechParams.AnalogScaling analogScaling = new ();
   TechParams.ExhaustSystem exhaustSystem = new ();
   #endregion "Fields"
}