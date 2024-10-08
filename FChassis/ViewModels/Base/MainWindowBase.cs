using System.Windows.Input;

namespace FChassis.ViewModels;

public class MainWindowBase : ViewModelBase {
   #region "FileOpenCommand"
   virtual protected void FileOpen () { }
   public ICommand FileOpenCommand {
      get {
         _fileOpenCommand = _fileOpenCommand ?? new Base.Command (param => this.FileOpen (), null);
         return _fileOpenCommand; }}
   private ICommand _fileOpenCommand;
   #endregion "FileOpenCommand"

   #region "FileSaveCommand"
   virtual protected void FileSave () { }
   public ICommand FileSaveCommand {
      get {
         _fileSaveCommand = _fileSaveCommand ?? new Base.Command (param => this.FileSave (), null);
         return _fileSaveCommand; }}
   private ICommand _fileSaveCommand;
   #endregion "FileSaveCommand"

   #region "ImportFileCommand"
   virtual protected void ImportFile () { }
   public ICommand ImportFileCommand {
      get {
         _importFileCommand = _importFileCommand ?? new Base.Command (param => this.ImportFile (), null);
         return _importFileCommand; }}
   private ICommand _importFileCommand;
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
   virtual protected void TextMarking () { }
   public ICommand TextMarkingCommand {
      get {
         _textMarkingCommand = _textMarkingCommand ?? new Base.Command (param => this.TextMarking (), null);
         return _textMarkingCommand; }}
   private ICommand _textMarkingCommand;
   #endregion "TextMarkingCommand"

   #region "CutNotchesCommand"
   virtual protected void CutNotches () { }
   public ICommand CutNotchesCommand {
      get {
         _cutNotchesCommand = _cutNotchesCommand ?? new Base.Command (param => this.TextMarking (), null);
         return _cutNotchesCommand;
      }
   }
   private ICommand _cutNotchesCommand;
   #endregion "CutNotchesCommand"

   #region "SortingCommand"
   virtual protected void Sorting () { }
   public ICommand SortingCommand {
      get {
         _sortingCommand = _sortingCommand ?? new Base.Command (param => this.Sorting (), null);
         return _sortingCommand; }}
   private ICommand _sortingCommand;
   #endregion "SortingCommand"

   #region "GenerateGCodeCommand"
   virtual protected void GenerateGCode () { }
   public ICommand GenerateGCodeCommand {
      get {
         _generateGCodeCommand = _generateGCodeCommand ?? new Base.Command (param => this.GenerateGCode (), null);
         return _generateGCodeCommand; }}
   private ICommand _generateGCodeCommand;
   #endregion "GenerateGCodeCommand"

   #region "SimulateCommand"
   virtual protected void Simulate () { }
   public ICommand SimulateCommand {
      get {
         _simulateCommand = _simulateCommand ?? new Base.Command (param => this.Simulate (), null);
         return _simulateCommand; }}
   private ICommand _simulateCommand;
   #endregion "SimulateCommand"

   #region "PauseSimulationCommand"
   virtual protected void PauseSimulation () { }
   public ICommand PauseSimulationCommand {
      get {
         _pauseSimulationCommand = _pauseSimulationCommand ?? new Base.Command (param => this.PauseSimulation (), null);
         return _pauseSimulationCommand; }}
   private ICommand _pauseSimulationCommand;
   #endregion "PauseSimulationCommand"

   #region "StopSimulationCommand"
   virtual protected void StopSimulation () { }
   public ICommand StopSimulationCommand {
      get {
         _stopSimulationCommand = _stopSimulationCommand ?? new Base.Command (param => this.StopSimulation (), null);
         return _stopSimulationCommand; }}
   private ICommand _stopSimulationCommand;
   #endregion "StopSimulationCommand"
}