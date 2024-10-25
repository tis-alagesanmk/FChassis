using CommunityToolkit.Mvvm.ComponentModel;
using FChassis.Data.Model.Setting.Machine.General;
using System.Collections.Generic;

namespace FChassis.Data.ViewModels.Setting.Machine.General;
public partial class HMIViewModel : HMI {
   [ObservableProperty] string[] orientations = ["Portrait", "Landscape"];
   [ObservableProperty] string[] plcMessages = ["Only error", "Warn & error", "info, warn & error"];
   [ObservableProperty] string[] themes = ["Grey", "Blue"];
   [ObservableProperty] string[] languages = ["EN", "FR"];
}
