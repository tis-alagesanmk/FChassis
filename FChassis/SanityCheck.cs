using System.Diagnostics;
using System.IO;
using System.Windows;
using FChassis.Processes;
using Flux.API;
namespace FChassis;

/// <summary>Implements a very basic sanity check</summary>
/// This is just a placeholder for a more elaborate test system. For now, we just
/// want to ensure that as we make changes to the code generator, the generated code does
/// not start varying
static class SanityCheck {
   /// <summary>Global routine used to run the sanity checks</summary>
   static public void Run (Processor processor) {
      bool curRes = true, result = true;
      string status = "";

      // Tests for holes, cutouts and texts -----------------------------------
      (string, double)[] holeCutoutTextTestParams = [
         ("Measure0",               1.0),
         ("TRIVIAL-2",              0.3)
      ];
      int count = holeCutoutTextTestParams.Length;
      for (int i = 0; i < count; i++)
         _check_Hole_CutOuts_Texts (holeCutoutTextTestParams[i].Item1, 
                                    holeCutoutTextTestParams[i].Item2);

      // Tests for notches with Wire joint Distance = 2.0 ---------------------
      (string, double)[] notchTestParams = [
         ("FGK01014_03",            2.0),
         ("FGK01014_04",            2.0),
         ("SM LH IC356716_E",       0.0),
         ("FGJ04513_05",            0.0),
      ];
      count = notchTestParams.Length;
      for (int i = 0; i < count; i++)
         _check_Notches (notchTestParams[i].Item1, 
                         notchTestParams[i].Item2);

      string msg = result ?"Code generation tests passed" 
                          :"One or more Code generation tests failed";
      MessageBox.Show ($"{msg}\n\n{status}", "FChassis", MessageBoxButton.OK, MessageBoxImage.Error);

      #region inline Functions -----------------------------------------------
      void _check_Hole_CutOuts_Texts (string fileName, double distance) {
         processor.PartitionRatio = distance;
         curRes = SanityCheck.check (fileName, processor);
         status += fileName;
         status += curRes ? "passed\n" : "failed\n";
         result &= curRes;
      }

      void _check_Notches (string fileName, double distance) {
         processor.NotchWireJointDistance = distance;
         curRes = SanityCheck.check (fileName, processor,
                                     cutHoles: false, cutNotches: true,
                                     cutOuts: false, textMark: false);

         status += fileName;
         status += curRes ? "passed\n" : "failed\n";
         result &= curRes;
      }
      #endregion inline Functions --------------------------------------------
   }

   // Internal check routine - loads a part, assigns tooling, sorts tooling,
   // and generates code with a fixed partition ratio of 0.5
   static bool check (string file, Processor processor, bool cutHoles = true, 
                      bool cutOuts = true, bool cutNotches = true, bool textMark = true) {
      var part = Part.Load ($"W:/FChassis/Sample/{file}.fx");
      if (part.Info.MatlName == "NONE") 
         part.Info.MatlName = "1.0038";
      if (part.Model == null) {
         if (part.Dwg != null) 
            part.FoldTo3D ();
         else if (part.SurfaceModel != null) 
            part.SheetMetalize ();
         else 
            throw new Exception ("Invalid part");
      }

      var work = new Workpiece (part.Model, part);
      work.Align ();
      if (cutHoles) 
         work.DoAddHoles ();

      if (textMark) 
         work.DoTextMarking ();

      if (cutNotches || cutOuts) 
         work.DoCutNotchesAndCutouts ();

      work.DoSorting ();

      processor.Workpiece = work;
      processor.CutHoles = cutHoles;
      processor.CutNotches = cutNotches;
      processor.CutMark = textMark;
      processor.Cutouts = cutOuts;
      try {
         processor.ComputeGCode (true);
      } catch (Exception) { }

      processor.ResetGCodeGenForTesting ();

      bool result = false;
      do {
         if (!CheckDIN ("Head1", $"{file}-(LH).din"))
            break;

         if (processor.PartitionRatio < 1)
            if(!CheckDIN ("Head2", $"{file}-(LH).din"))
              break;

         result = true;
      } while (false);

      return result;
   }

   // Compares two generated DIN files for sameness. If any file is not matching the
   // expected reference, we simply display a message and stop. 
   static bool CheckDIN (string folder, string dinfile) {
      string reference = $"W:/FChassis/TData/{folder}/{dinfile}";
      string testfile = $"W:/FChassis/Sample/{folder}/{dinfile}";
      if (!System.IO.File.Exists (reference)) 
         System.IO.File.Copy (testfile, reference);

      string reftext = System.IO.File.ReadAllText (reference), 
             testtext = System.IO.File.ReadAllText (testfile);
      bool res = true;
      if (reftext != testtext) {
         res = false;
         DoDINCompare (reference, testfile);
         //MessageBox.Show ($"Files different: {folder}-{dinfile}", "FChassis", MessageBoxButton.OK, MessageBoxImage.Error);
      }

      return res;
   }

   /// <summary>
   /// This method performs a comparison between the G Code files and the baseline under the
   /// following condition
   /// <list type="number">
   /// <item>
   /// <description>In DEBUG configuration:
   /// <list type="bullet">
   /// <item>If the <c>FC_REG_DIFF_COMPARE</c> flag is set to <c>True</c> and <c>WinmergeU.exe</c> 
   /// is available in the environment variable <c>PATH</c>, 
   /// the method uses WinMerge to display the differences.</item>
   /// <item>If the flag is not set or <c>WinmergeU.exe</c> is not found, 
   /// direct string comparison is performed.</item>
   /// </list>
   /// </description>
   /// </item>
   /// <item>
   /// <description>In RELEASE configuration:
   /// <list type="bullet">
   /// <item>The method always performs a direct string comparison 
   /// between the G Code files and the baseline.</item>
   /// </list>
   /// </description>
   /// </item>
   /// </list>
   /// </summary>
   /// <param name="reference">The path to the reference G Code file (baseline).</param>
   /// <param name="testfile">The path to the test G Code file to be compared.</param>
   /// <returns>Returns <c>true</c> if there are no changes 
   /// between the G Code files and the baseline; 
   /// otherwise, returns <c>false</c>.</returns>

   static bool DoDINCompare (string reference, string testfile) {
      bool res = false;
#if DEBUG
      string winmergePath = SanityCheck.isFileComparerInstalled ();
      if (!string.IsNullOrEmpty (winmergePath)) {
         if (!System.IO.File.Exists (winmergePath)) {
            Console.WriteLine ("Winmerge is not installed");
            return res;
         }

         ProcessStartInfo startInfo = new() {
            FileName = winmergePath,
            Arguments = $"\"{reference}\" \"{testfile}\"",
            UseShellExecute = false
         };

         try {
            using (Process process = Process.Start (startInfo)) {
               Console.WriteLine ("WinMergeU.exe started successfully.");

               // Wait for the process to exit
               process.WaitForExit ();

               Console.WriteLine ("WinMergeU.exe has exited.");
               res = true;
            }
         } catch (Exception ex) {
            Console.WriteLine ($"Error starting WinMergeU.exe: {ex.Message}");
         }
      }
#endif
      return res;
   }

   /// <summary>
   /// This is an utility method which checks if the FC_REG_DIFF_COMPARE env variable
   /// is set to TRUE and if the winmergeu.exe is available.
   /// </summary>
   /// <returns>The path to the WinMergeU.exe</returns>
   static string isFileComparerInstalled () {
      string swtch = Environment.GetEnvironmentVariable ("FC_REG_DIFF_COMPARE");
      if (string.IsNullOrEmpty (swtch) 
         || string.Equals (swtch, "true", StringComparison.OrdinalIgnoreCase)) 
         return "";

      string pathEnv = Environment.GetEnvironmentVariable ("PATH");
      if (pathEnv == null) {
         Console.WriteLine ("PATH environment variable is not set.");
         return "";
      }

      // Split the PATH environment variable into individual directories
      string[] paths = pathEnv.Split (Path.PathSeparator);

      // Try to find WinMergeU.exe in each directory
      string winMergePath = null;
      foreach (string path in paths) {
         string potentialPath = Path.Combine (path, "WinMergeU.exe");
         if (System.IO.File.Exists (potentialPath)) {
            winMergePath = potentialPath;
            break;
         }
      }

      if (winMergePath == null) {
         Console.WriteLine ("WinMergeU.exe not found in PATH directories.");
         return "";
      }

      return winMergePath;
   }
}