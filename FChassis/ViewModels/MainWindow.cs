﻿using System.Windows;
using System.IO;
using System.ComponentModel;
using Microsoft.Win32;
using Avalonia.Threading;

using Flux.API;
using FChassis.Processes;
using FChassis.Core;

using static FChassis.Processes.Processor;
using System.Collections.ObjectModel;
using System;
using System.Windows.Controls;

namespace FChassis.ViewModels;

public class MainWindow : MainWindowBase {
   #region "Proptery" ---------------------------------------------------------
   public string SelectedFileItem {
      get => selectedFileItem;
      set {
         selectedFileItem = value;
         this.loadPart (selectedFileItem);
         this.OnPropertyChanged (selectedFileItem);
      }
   }
   string selectedFileItem = "";

   public Processor.ESimulationStatus SimulationStatus {
      get => mSimulationStatus;
      set {
         if (mSimulationStatus != value) {
            mSimulationStatus = value;
            OnPropertyChanged (nameof (SimulationStatus));
         }
      }
   }
   Processor.ESimulationStatus mSimulationStatus = ESimulationStatus.NotRunning;

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

   public Workpiece Work {
      get => mWork;
      set {
         mWork = value;
         mProcess.Workpiece = mWork;
         OnPropertyChanged (nameof (Work));
      }
   }
   Workpiece mWork;
   #endregion "Proteries"

   #region "Method" -----------------------------------------------------------
   static public UIElement CreateViewerPanel () {
      UIElement viewerPanel = (UIElement)Lux.CreatePanel ();
      return viewerPanel;
   }

   public void Initialize (Dispatcher dispatcher, 
                           Avalonia.Controls.ListBox files) {
      Library.Init ("W:/FChassis/Data", "C:/FluxSDK/Bin", this);

      AppUI.ThreadDispatcher = dispatcher;
      mProcess = new Processor ();
      mProcess.TriggerRedraw += TriggerRedraw;
      mProcess.SetSimulationStatus += status => SimulationStatus = status;

      Sys.SelectionChanged += OnSelectionChanged;

      files.ItemsSource = Directory.GetFiles (mSrcDir, "*.fx").Select (Path.GetFileName);
   }
   #endregion "Method"

   #region "Command" --------------------------------------------------
   override protected void FileOpen() {
      OpenFileDialog openFileDialog = new () {
         Filter = "FX Files (*.fx)|*.fx|IGS Files (*.igs;*.iges)|*.igs;*.iges|All files (*.*)|*.*",
         InitialDirectory = @"W:\FChassis\Sample"
      };

      // Handle file opening, e.g., load the file into your application
      if (openFileDialog.ShowDialog () == true) {
         if (!string.IsNullOrEmpty (openFileDialog.FileName))
            loadPart (openFileDialog.FileName);
      }
   }

   override protected void ImportFile () {
      OpenFileDialog openFileDialog = new () {
         Filter = "GCode Files (*.din)|*.din|All files (*.*)|*.*",
         InitialDirectory = @"W:\FChassis\Sample"
      };

      // Handle file opening, e.g., load the file into your application
      if (openFileDialog.ShowDialog () == true) {
         if (!string.IsNullOrEmpty (openFileDialog.FileName)) {
            var extension = Path.GetExtension (openFileDialog.FileName).ToLower ();
            if (extension == ".din")
               mProcess.LoadGCode (openFileDialog.FileName);
         }
      }
   }

   override protected void FileSave () {
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

   void OpenSettings ()
      => new SettingsDlg ().ShowDialog ();

   void SanityCheck () {
      mProcess.ResetGCodeGenForTesting ();
      FChassis.SanityCheck.Run (mProcess);
   }

   override protected void Align ()  {
      if (!handleNoWorkpiece ()) {
         Work.Align ();
         mScene.Bound3 = Work.Model.Bound;
      }
   }

   override protected void AddHoles () {
      if (!handleNoWorkpiece ()) {
         Work.DoAddHoles ();
         mOverlay.Redraw ();
      }
   }

   override protected void TextMarking () {
      if (!handleNoWorkpiece ()) {
         Work.DoTextMarking ();
         mOverlay.Redraw ();
      }
   }

   override protected void CutNotches () {
      if (!handleNoWorkpiece ()) {
         Work.DoCutNotchesAndCutouts ();
         mOverlay.Redraw ();
      }
   }

   override protected void Sorting () {
      if (!handleNoWorkpiece ()) {
         Work.DoSorting ();
         mOverlay.Redraw ();
      }
   }

   override protected void GenerateGCode () {
      if (!handleNoWorkpiece ()) {
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

   override protected void Simulate () {
      if (!handleNoWorkpiece ()) {
         Process.SimulationFinished += OnSimulationFinished;
         Task.Run (Process.Run);
      }
   }

   override protected void PauseSimulation () {
      if (!handleNoWorkpiece ())
         Process.Pause ();
   }

   override protected void StopSimulation () {
      if (!handleNoWorkpiece ())
         Process.Stop ();
   }
   #endregion "Command"

   #region "Implementation" --------------------------------------------------
   public void loadPart (string fileName) {
      fileName = Path.Combine (mSrcDir, fileName);
      fileName = fileName.Replace ('\\', '/');
      mPart = Part.Load (fileName);
      mPart.Info.FileName = fileName;
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

      mOverlay = new SimpleVM (drawOverlay);
      Lux.UIScene = mScene = new Scene (new GroupVModel (VModel.For (mPart.Model), mOverlay), mPart.Model.Bound);
      Work = new Workpiece (mPart.Model, mPart);

      // Clear the zombies if any
      mProcess?.ClearZombies ();
   }

   void drawOverlay () {
      drawTooling ();
      mProcess.DrawGCode ();
      mProcess.DrawToolInstance ();
   }

   void drawTooling () {
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

   bool handleNoWorkpiece () {
      if (Work == null) {
         MessageBox.Show ("No Part is Loaded.", "Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
         return true;
      }

      return false;
   }

   #region "EventHandler"
   void TriggerRedraw ()
         => Dispatcher.UIThread.Invoke (() => mOverlay?.Redraw ());

   void OnSelectionChanged (object obj) {
      //if (this.Parent != null) {
      //   HostMainWindow parentWindow = this.Parent as HostMainWindow;
      //   parentWindow.Title = obj?.ToString () ?? "NONE";
      //}

      mOverlay?.Redraw ();
   }
   void OnSimulationFinished ()
         => Process.SimulationStatus = Processor.ESimulationStatus.NotRunning;
   private void OnProcessPropertyChanged (object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == nameof (Processor.SimulationStatus))
         OnPropertyChanged (nameof (SimulationStatus));
   }
   #endregion "EventHandler"
   #endregion "Implementation"

   #region "Field" -----------------------------------------------------------
   readonly string mSrcDir = "W:/FChassis/Sample";

   SimpleVM mOverlay;
   Scene mScene;
   Part mPart = null;
   #endregion "Field"
}