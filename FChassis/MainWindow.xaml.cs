using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FChassis.Processes;
using Flux.API;
using Microsoft.Win32;
using static FChassis.Processes.Processor;
using SPath = System.IO.Path;
namespace FChassis;

/// <summary>Interaction logic for MainWindow.xaml</summary>
public partial class MainWindow : UserControl, INotifyPropertyChanged {
   // Delegates
   public delegate void ProcessDrawDelegate ();
   Part mPart = null;
   public MainWindow () {
      InitializeComponent ();
      this.DataContext = this;

      /*Logger.SetControl (this.LogRichTextBox);

      // [TODO:Alag] remove if Test not required
      if (true) {
         Logger.Instance.Add ("TestNormal");
         (Logger.LogType, string)[] logParams = [
            (Logger.LogType.Info,    "TestInfo"),
            (Logger.LogType.Warning, "TestWarning"),
            (Logger.LogType.Error,   "Test Error"),
            (Logger.LogType.Line,    null),
            (Logger.LogType.Blank,   null)
         ];

         foreach(var param in logParams) 
            Logger.Instance.Add (param.Item1, param.Item2);

         if (true)
            for(int i = 0; i < 20; i++) 
               Logger.Instance.Add (Logger.LogType.Error, $"Test Error{i}");
      }*/

      Library.Init ("W:/FChassis/Data", "C:/FluxSDK/Bin", this);
      Area.Child = (UIElement)Lux.CreatePanel ();
      Files.ItemsSource = System.IO.Directory.GetFiles (mSrcDir, "*.fx").Select (SPath.GetFileName);
      Sys.SelectionChanged += OnSelectionChanged;
   }

   void TriggerRedraw () 
      => Dispatcher.Invoke (() => mOverlay?.Redraw ());

   void OnSimulationFinished () 
      => Process.SimulationStatus = Processor.ESimulationStatus.NotRunning;

   Processor.ESimulationStatus mSimulationStatus = ESimulationStatus.NotRunning;

   public Processor.ESimulationStatus SimulationStatus {
      get => mSimulationStatus;
      set {
         if (mSimulationStatus != value) {
            mSimulationStatus = value;
            OnPropertyChanged (nameof (SimulationStatus));
         }
      }
   }
   readonly string mSrcDir = "W:/FChassis/Sample";
   public event PropertyChangedEventHandler PropertyChanged;
   protected virtual void OnPropertyChanged (string propertyName) 
      => PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));

   void OnFileSelected (object sender, RoutedEventArgs e) {
      if (Files.SelectedItem != null)
         LoadPart (SPath.Combine (mSrcDir, (string)Files.SelectedItem));
   }

   void OnSelectionChanged (object obj) {
      if (this.Parent != null) {
         HostMainWindow parentWindow = this.Parent as HostMainWindow;
         parentWindow.Title = obj?.ToString () ?? "NONE";
      }

      mOverlay?.Redraw ();
   }

   void LoadPart (string file) {
      file = file.Replace ('\\', '/');
      mPart = Part.Load (file);
      mPart.Info.FileName = file;
      if (mPart.Info.MatlName == "NONE") 
         mPart.Info.MatlName = "1.0038";

      if (mPart.Model == null) {
         if (mPart.Dwg != null) 
            mPart.FoldTo3D ();
         else if (mPart.SurfaceModel != null) 
            mPart.SheetMetalize ();
         else 
            throw new Exception ("Invalid part");
      }

      mOverlay = new SimpleVM (DrawOverlay);
      Lux.UIScene = mScene = new Scene (new GroupVModel (VModel.For (mPart.Model), mOverlay), mPart.Model.Bound);
      Work = new Workpiece (mPart.Model, mPart);

      // Clear the zombies if any
      mProcess?.ClearZombies ();
   }
   SimpleVM mOverlay;
   Scene mScene;

   public Workpiece Work {
      get => mWork;
      set {
         mWork = value;
         mProcess.Workpiece = mWork;
         OnPropertyChanged (nameof (Work));
      }
   }
   Workpiece mWork;

   public Processor Process {
      get => mProcess;
      set {
         if (mProcess != value) {
            if (mProcess != null) {
               mProcess.PropertyChanged -= OnProcessPropertyChanged;
               mProcess = value;

               if (mProcess != null)
                  mProcess.PropertyChanged += OnProcessPropertyChanged;

               OnPropertyChanged (nameof (Process));
               OnPropertyChanged (nameof (SimulationStatus));
            }
         }
      }
   }
   Processor mProcess;

   private void OnProcessPropertyChanged (object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == nameof (Processor.SimulationStatus))
         OnPropertyChanged (nameof (SimulationStatus));
   }

   void DrawOverlay () {
      DrawTooling ();
      mProcess.DrawGCode ();
      mProcess.DrawToolInstance ();
   }

   void DrawTooling () {
      if (Process.SimulationStatus == Processor.ESimulationStatus.NotRunning 
         || Process.SimulationStatus == Processor.ESimulationStatus.Paused) {
         Lux.HLR = false;
         Lux.Color = new Color32 (255, 255, 0);
         switch (Sys.Selection) {
            case E3Plane ep: 
               Lux.Draw (EMarker2D.CSMarker, ep.Xfm.ToCS (), 25); 
               break;

            case E3Flex ef: 
               Lux.Draw (EMarker2D.CSMarker, ef.Socket, 25); 
               break;

            default: 
               if (Work.Cuts.Count == 0) 
                  Lux.Draw (EMarker2D.CSMarker, CoordSystem.World, 25); 
               break;
         }
         foreach (var cut in Work.Cuts) {
            cut.DrawSegs (Color32.Yellow, 10);
            cut.DrawSeqNo (15);
            cut.DrawWaypoints (Color32.White, 10);
         }
      }
   }

   void DoAlign (object sender, RoutedEventArgs e) {
      if (!HandleNoWorkpiece ()) { 
         Work.Align (); 
         mScene.Bound3 = Work.Model.Bound; 
      }
   }

   void DoAddHoles (object sender, RoutedEventArgs e) {
      if (!HandleNoWorkpiece ()) { 
         Work.DoAddHoles (); 
         mOverlay.Redraw (); 
      }
   }

   void DoTextMarking (object sender, RoutedEventArgs e) {
      if (!HandleNoWorkpiece ()) { 
         Work.DoTextMarking (); 
         mOverlay.Redraw (); 
      }
   }

   void DoCutNotches (object sender, RoutedEventArgs e) {
      if (!HandleNoWorkpiece ()) { 
         Work.DoCutNotchesAndCutouts (); 
         mOverlay.Redraw (); 
      }
   }

   void DoSorting (object sender, RoutedEventArgs e) {
      if (!HandleNoWorkpiece ()) { 
         Work.DoSorting (); 
         mOverlay.Redraw (); 
      }
   }

   void DoGenerateGCode (object sender, RoutedEventArgs e) {
      if (!HandleNoWorkpiece ()) {
#if DEBUG
         mProcess.ComputeGCode ();
#else
         try {
            mProcess.ComputeGCode ();
         } catch (Exception ex) {
            if (ex is NegZException) MessageBox.Show ("Part might not be aligned", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else MessageBox.Show ("G Code generation failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
#endif
         mOverlay.Redraw ();
      }
   }

   void Simulate (object sender, RoutedEventArgs e) {
      if (!HandleNoWorkpiece ()) {
         Process.SimulationFinished += OnSimulationFinished;
         Task.Run (Process.Run);
      }
   }

   void PauseSimulation (object sender, RoutedEventArgs e) {
      if (!HandleNoWorkpiece ()) 
         Process.Pause ();
   }

   void StopSimulation (object sender, RoutedEventArgs e) {
      if (!HandleNoWorkpiece ()) 
         Process.Stop ();
   }

   void OnMenuFileOpen (object sender, RoutedEventArgs e) {
      OpenFileDialog openFileDialog = new () {
         Filter = "FX Files (*.fx)|*.fx|IGS Files (*.igs;*.iges)|*.igs;*.iges|All files (*.*)|*.*",
         InitialDirectory = @"W:\FChassis\Sample"
      };

      // Handle file opening, e.g., load the file into your application
      if (openFileDialog.ShowDialog () == true) {
         if (!string.IsNullOrEmpty (openFileDialog.FileName))
            LoadPart (openFileDialog.FileName);
      }
   }

   void OnMenuImportFile (object sender, RoutedEventArgs e) {
      OpenFileDialog openFileDialog = new () {
         Filter = "GCode Files (*.din)|*.din|All files (*.*)|*.*",
         InitialDirectory = @"W:\FChassis\Sample"
      };

      // Handle file opening, e.g., load the file into your application
      if (openFileDialog.ShowDialog () == true) {
         if (!string.IsNullOrEmpty (openFileDialog.FileName)) {
            var extension = SPath.GetExtension (openFileDialog.FileName).ToLower ();
            if (extension == ".din")
               LoadGCode (openFileDialog.FileName);
         }
      }
   }
   void OnMenuFileSave (object sender, RoutedEventArgs e) {
      SaveFileDialog saveFileDialog = new () {
         Filter = "FX files (*.fx)|*.fx|All files (*.*)|*.*",
         DefaultExt = "fx",
      };

      // Process save file dialog box results
      if (saveFileDialog.ShowDialog () == true) {
         string filePath = saveFileDialog.FileName;
         try {
            mPart.SaveFX (filePath);
         } catch (Exception ex) {
            MessageBox.Show ("Error: Could not write file to disk. Original error: " + ex.Message);
         }
      }
   }
   bool HandleNoWorkpiece () {
      if (Work == null) {
         MessageBox.Show ("No Part is Loaded.", "Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
         return true;
      }

      return false;
   }

   void OnFileClose (object sender, RoutedEventArgs e) { }

   void OnSettings (object sender, RoutedEventArgs e) 
      => new SettingsDlg ().ShowDialog ();

   void OnWindowLoaded (object sender, RoutedEventArgs e) {
      mProcess = new Processor ();
      mProcess.TriggerRedraw += TriggerRedraw;
      mProcess.SetSimulationStatus += status => SimulationStatus = status;
   }

   void OnSanityCheck (object sender, RoutedEventArgs e) {
      mProcess.ResetGCodeGenForTesting ();
      SanityCheck.Run (mProcess);
   }

   void LoadGCode (string filename) 
      => mProcess.LoadGCode (filename);
}