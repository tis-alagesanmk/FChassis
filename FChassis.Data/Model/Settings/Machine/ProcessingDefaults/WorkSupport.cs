using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.ProcessingDefaults {
   public partial class WorkSupport : ObservableObject {

      [ObservableProperty, Prop ("Work support configuration",Prop.Type.Text, "Distance between slats")]
      double? distanceBetweenSlats = 12.56;

      [ObservableProperty, Prop (Prop.Type.Text, "Offset of first slat from sheet edge")]
      double? offsetOfFirstSlatFromSheetEdge = 12.56;

      [ObservableProperty, Prop (Prop.Type.Text, "Distance between support pins in a slat")]
      double? distanceBetweenSupportPinsInSlat = 12.56;
   }
}
