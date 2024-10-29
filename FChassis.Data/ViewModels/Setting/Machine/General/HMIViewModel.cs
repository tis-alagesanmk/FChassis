
using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.ViewModel.Settings.Machine.General;
public partial class HMIViewModel : Model.Settings.Machine.General.HMI {
   [ObservableProperty]
   object[] gridItems = [
      (Col1: "Col11", Col2: "Col12"),
      (Col1: "Col21", Col2: "Col22"),
      (Col1: "Col31", Col2: "Col32"),
      ];
}
