using Avalonia.Markup.Xaml;
using Avalonia.Controls;

using FChassis.UI.Panels;

namespace FChassis.UI;
public partial class ModelViewer : FChassis.UI.Panels.Child {

   public ModelViewer () {
      AvaloniaXamlLoader.Load (this);
      this.DataContext = ViewModels.Context.MainWindow;

      this.initializeControls ();
   }


   #region "Implementation" ---------------------------------------------------
   void initializeControls () {
      this.initElementNameField ();
      ViewModels.Context.MainWindow.Initialize (Avalonia.Threading.Dispatcher.UIThread, this.Files);
      if(this.FChassisHost != null) 
         this.FChassisHost.Content = new FChassisMainWindowHost ();

      Logger.SetControl (this.LogTextBlock);

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
   #endregion

   #region "Events" -----------------------------------------------------------
   private void CloseBtn_Click (object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
      Child.mainWindow?.Switch2MainPanel (); }
   #endregion

   #region "Element NameField" ------------------------------------------------
   void initElementNameField () {
      this.Files = this.FindControl<ListBox> ("Files");
      this.LogTextBlock = this.FindControl<TextBlock> ("LogTextBlock");
      this.FChassisHost = this.FindControl<ContentControl> ("FChassisHost");
   }

   ContentControl? FChassisHost = null!;
   TextBlock? LogTextBlock = null!;
   ListBox? Files = null!;
   #endregion "Element NameField"
}