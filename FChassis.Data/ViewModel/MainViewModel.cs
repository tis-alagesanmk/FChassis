namespace FChassis.Data.ViewModel;
public class MainViewModel {
   // Machine -------------------------------------------------------------------------------------
   // General Tab
   public static Settings.Machine.General.HMIViewModel hmiVM = new ();
   public static Settings.Machine.General.MachineViewModel machineVM = new ();

   // AxisParams Tab
   public static Settings.Machine.AxisParams.AxisViewModel xAxisVM = new ();
   public static Settings.Machine.AxisParams.AxisViewModel yAxisVM = new ();
   public static Settings.Machine.AxisParams.AxisViewModel zAxisVM = new ();

   public static Settings.Machine.AxisParams.LPC1ViewModel lpc1VM = new ();
   public static Settings.Machine.AxisParams.Pallet1ViewModel pallet1ViewModelVM = new ();

   // TechParams Tab
   public static Settings.Machine.TechParams.AnalogScalingViewModel analogScalingVM = new ();
   public static Settings.Machine.TechParams.MFunctionsViewModel mFunctionsVM = new ();
   public static Settings.Machine.TechParams.ExhaustSysViewModel exhaustSysVM = new ();
   public static Settings.Machine.TechParams.LaserSysViewModel laserTechVM = new ();
   public static Settings.Machine.TechParams.MachineDbViewModel MachineDbVM = new ();

   // PLCOptions Tab
   public static Settings.Machine.PLCOptions.FuncParamViewModel funcParamVM = new ();
   public static Settings.Machine.PLCOptions.ControlParamViewModel controlParamVM = new ();
   public static Settings.Machine.PLCOptions.PLCKeyViewModel plcKeyVM = new ();
   
   // Process Defaults
   public static Settings.Machine.ProcessingDefaults.ImportViewModel importVM = new ();
   public static Settings.Machine.ProcessingDefaults.CutCamViewModel curcamVM = new ();
   public static Settings.Machine.ProcessingDefaults.ProfileCamViewModel profileCamVM = new ();
   public static Settings.Machine.ProcessingDefaults.SequenceViewModel sequenceVM = new ();
   public static Settings.Machine.ProcessingDefaults.WorkSupportViewModel workSupportVM = new ();
   public static Settings.Machine.ProcessingDefaults.SkeletonCutsViewModel skeletonCutsVM = new ();

   //Laser db/LaserCutting/Piercing
   public static Settings.Laser.LaserCutting.Piercing.PeckViewModel peckVM = new ();
   public static Settings.Laser.LaserCutting.Piercing.MultipleViewModel multipleVM = new ();
   public static Settings.Laser.LaserCutting.Piercing.RampViewModel rampVM = new ();
   public static Settings.Laser.LaserCutting.Piercing.SingleViewModel singleVM = new ();
   public static Settings.Laser.LaserCutting.Piercing.NormalViewModel normalVM = new ();
   public static Settings.Laser.LaserCutting.Piercing.GentleViewModel gentleVM = new ();
   public static Settings.Laser.LaserCutting.Piercing.DotPunchViewModel dotpunchVM = new ();

   // WorkOffsets ---------------------------------------------------------------------------------
   // WorkOffsets
   public static Settings.WorkOffsetsViewModel workOFfsetsVM = new ();

   // Laser ---------------------------------------------------------------------------------------
   public static Settings.Laser.DCAPageViewModel dcaPageVM = new ();
}