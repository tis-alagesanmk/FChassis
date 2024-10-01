using System.Globalization;
using System.Windows;
namespace FChassis;

public partial class App : Application {
   protected override void OnStartup (StartupEventArgs e) {
      base.OnStartup (e);
      // Set the current culture to "en-US"
      CultureInfo.CurrentCulture = new CultureInfo ("en-US");
      CultureInfo.CurrentUICulture = new CultureInfo ("en-US");
   }
}

