using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.ProcessingDefaults {
   public partial class SkeletonCuts : ObservableObject {

      [ObservableProperty, Prop ("Sheet cutting rules", Prop.Type.Check, "Create sheet cut")]
      bool? unitForDXFFiles = true;

      [ObservableProperty, Prop (Prop.Type.Text, "X spacing between vertical sheet cuts")]
      double? xSpacingBetweenVerticalSheetCuts = 500.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Y spacing between horizontal sheet cuts")]
      double? ySpacingBetweenHorizontalSheetCuts = 400.23;

      [ObservableProperty, Prop (Prop.Type.Check, "Create remainder sheet")]
      bool? createRemainderSheet = true;

      [ObservableProperty, Prop (Prop.Type.Text, "Minimum remainder sheet width")]
      double? minimumRemainderSheetWidth = 300.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Final cut X offset")]
      double? finalCustXOffset = 456.23;

      [ObservableProperty, Prop (Prop.Type.Check, "Process sheet cut after all part")]
      bool? processSheetCutAfterAllPart = true;


      [ObservableProperty, Prop ("Sheet cut parameters", Prop.Type.Text, "Micro joint gap at sheet edge")]
      double? microJointGapAtSheetEdge = 12.34;

      [ObservableProperty, Prop (Prop.Type.Text, "Micro joint gap at part edge")]
      double? microJointGapAtPartEdge = 12.34;

      [ObservableProperty, Prop (Prop.Type.Text, "Pierce distance from part edge")]
      double? pierceDistanceFromPartEdge = 12.34;

      [ObservableProperty, Prop (Prop.Type.Text, "Measuring distance from sheet edge")]
      double? measuringDistanceFromSheetEdge = 12.34;

      [ObservableProperty, Prop (Prop.Type.Text, "Overtravel after sheet edge")]
      double? overtravelAfterSheetEdge = 12.34;
   }
}
