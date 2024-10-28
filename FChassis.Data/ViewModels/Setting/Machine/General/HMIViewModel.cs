using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.ViewModel.Settings.Machine.General;
public partial class HMIViewModel : Model.Settings.Machine.General.HMI {
   [ObservableProperty] string[] orientations = ["Portrait", "Landscape"];
   [ObservableProperty] string[] plcMessages = ["Only error", "Warn & error", "info, warn & error"];
   [ObservableProperty] string[] themes = ["Grey", "Blue"];
   [ObservableProperty] string[] languages = ["EN", "FR"];
}
