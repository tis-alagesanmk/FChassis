using CommunityToolkit.Mvvm.ComponentModel;
using FChassis.Data.Model.Setting.Machine.General;
using System.Collections.Generic;

namespace FChassis.Data.ViewModels.Setting.Machine.General;
public class HMIViewModel : HMI {   
   string[] plcMessages = ["Only error", "Warn & error", "info, warn & error"];
   string[] themes = ["Grey", "Blue"];
}
