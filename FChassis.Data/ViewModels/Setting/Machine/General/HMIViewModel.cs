using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.ViewModel.Settings.Machine.General;
public partial class HMIViewModel : Model.Settings.Machine.General.HMI {
   [ObservableProperty]
   GridItem[] gridItems = new GridItem[2];
}

public class GridItem {
   public string Col1 { get; set; } = "Col1";
   public string Col2 { get; set; } = "Col2";
}
