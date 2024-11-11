namespace FChassis.Data.ViewModel;
public class MainViewModel {
   public static Settings.Machine.General.HMIViewModel hmiVM = new ();
   public static Settings.Machine.General.MachineViewModel machineVM = new ();

   public static Settings.Machine.AxisParams.AxisViewModel xAxisVM = new ();
   public static Settings.Machine.AxisParams.AxisViewModel yAxisVM = new ();
   public static Settings.Machine.AxisParams.AxisViewModel zAxisVM = new ();

   public static Settings.Machine.AxisParams.LPC1ViewModel lpc1VM = new ();
   public static Settings.Machine.AxisParams.Pallet1ViewModel pallet1ViewModelVM = new ();
}