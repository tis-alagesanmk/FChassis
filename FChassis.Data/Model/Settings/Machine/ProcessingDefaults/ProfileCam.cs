using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.ProcessingDefaults {
   public partial class ProfileCam : ObservableObject {

      [ObservableProperty, Prop (Prop.Type.Check,"Advanced")]
      bool? unitForDXFFiles = true;


      [ObservableProperty, Prop ("Cutting", Prop.Type.Combo, "Choose cutting condition by", null!, null!, ["Max.segment length","Avg.segment length","Area",])]
      string? chooseCuttingConditionBy = "Avg.segment length";

      [ObservableProperty, Prop (Prop.Type.Combo, "Process for open polylines", null!, null!, ["None", "Mark", "Cut","EopenPlineProcess.ByLayer"])]
      string? processForOpenPolylines = "EopenPlineProcess.ByLayer";


      [ObservableProperty, Prop ("Pierce settings", Prop.Type.Check, "Allow approach that is more than 0.5 distance to opposite side")]
      bool? allowApproachMoreThanDistanceToOppositeSide = true;


      [ObservableProperty, Prop ("Scrap cutting", Prop.Type.Text, "Scrap grid width")]
      double? scrapGridWidth = 2.32;

      [ObservableProperty, Prop (Prop.Type.Text, "Approach length for separating cuts")]
      double? approachLengthForSeparatingCuts = 2.32;
   }
}
