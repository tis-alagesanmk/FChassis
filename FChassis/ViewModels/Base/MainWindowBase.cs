using System.Windows.Input;

namespace FChassis.ViewModels;

public class MainWindowBase : ViewModelBase {
   #region "FileOpenCommand"
   private ICommand fileOpenCommand;
   public ICommand FileOpenCommand {
      get {
         fileOpenCommand = fileOpenCommand ?? new Base.Command (param => this.FileOpen (), null);
         return fileOpenCommand;
      }
   }
   virtual protected void FileOpen () { }
   #endregion "FileOpenCommand"

   #region "FileSaveCommand"
   private ICommand fileSaveCommand;
   public ICommand FileSaveCommand {
      get {
         fileSaveCommand = fileSaveCommand ?? new Base.Command (param => this.FileSave (), null);
         return fileSaveCommand;
      }
   }
   virtual protected void FileSave () { }
   #endregion "FileSaveCommand"

   #region "ImportFileCommand"
   private ICommand importFileCommand;
   public ICommand ImportFileCommand {
      get {
         importFileCommand = importFileCommand ?? new Base.Command (param => this.ImportFile (), null);
         return importFileCommand;
      }
   }
   virtual protected void ImportFile () { }
   #endregion "ImportFileCommand"

   #region "AlignCommand"
   private ICommand alignCommand;
   public ICommand AlignCommand {
      get {
         alignCommand = alignCommand ?? new Base.Command (param => this.Align (), null);
         return alignCommand;
      }
   }
   virtual protected void Align () { }
   #endregion "AlignCommand"

   #region "AddHolesCommand"
   private ICommand addHolesCommand;
   public ICommand AddHolesCommand {
      get {
         addHolesCommand = addHolesCommand ?? new Base.Command (param => this.AddHoles (), null);
         return alignCommand;
      }
   }
   virtual protected void AddHoles () { }
   #endregion "AddHolesCommand"

   #region "TextMarkingCommand"
   private ICommand textMarkingCommand;
   public ICommand TextMarkingCommand {
      get {
         textMarkingCommand = textMarkingCommand ?? new Base.Command (param => this.TextMarking (), null);
         return textMarkingCommand;
      }
   }
   virtual protected void TextMarking () { }
   #endregion "TextMarkingCommand"

   #region "CutNotchesCommand"
   private ICommand cutNotchesCommand;
   public ICommand CutNotchesCommand {
      get {
         cutNotchesCommand = cutNotchesCommand ?? new Base.Command (param => this.TextMarking (), null);
         return cutNotchesCommand;
      }
   }
   virtual protected void CutNotches () { }
   #endregion "CutNotchesCommand"

   #region "SortingCommand"
   private ICommand sortingCommand;
   public ICommand SortingCommand {
      get {
         sortingCommand = sortingCommand ?? new Base.Command (param => this.Sorting (), null);
         return sortingCommand;
      }
   }
   virtual protected void Sorting () { }
   #endregion "SortingCommand"

   #region "GenerateGCodeCommand"
   private ICommand generateGCodeCommand;
   public ICommand GenerateGCodeCommand {
      get {
         generateGCodeCommand = generateGCodeCommand ?? new Base.Command (param => this.GenerateGCode (), null);
         return generateGCodeCommand;
      }
   }
   virtual protected void GenerateGCode () { }
   #endregion "GenerateGCodeCommand"

   virtual protected void Simulate () { }
   virtual protected void PauseSimulation () { }
   virtual protected void StopSimulation () { }
}