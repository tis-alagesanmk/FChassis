using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FChassis;
using System.Collections;

namespace Avalonia.Desktop;
public partial class MainWindow : Window {
   FChassis.ViewModels.MainWindow vm = new FChassis.ViewModels.MainWindow ();
   public MainWindow () {
      InitializeComponent ();
      this.DataContext = vm;

      initializeControls ();
   }

   void initializeControls () {
      Logger.SetControl (this.LogTextBlock);

      FChassisHost.Content = new FChassisMainWindowHost { };
      vm.Initialize (Dispatcher.UIThread, this.Files);

      // [TODO:Alag] remove if Test not required
      if (true) {
         Logger.Instance.Add ("TestNormal");
         (Logger.LogType, string?)[] logParams = [
            (Logger.LogType.Info,    "TestInfo"),
            (Logger.LogType.Warning, "TestWarning"),
            (Logger.LogType.Error,   "Test Error"),
            (Logger.LogType.Line,    null),
            (Logger.LogType.Blank,   null)
         ];

         foreach (var param in logParams)
            Logger.Instance.Add (param.Item1, param.Item2);

         if (true)
            for (int i = 0; i < 20; i++)
               Logger.Instance.Add (Logger.LogType.Error, $"Test Error{i}");
      }
   }
}