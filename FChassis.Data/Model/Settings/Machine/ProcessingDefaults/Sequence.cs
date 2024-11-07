using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.ProcessingDefaults {
   public partial class Sequence : ObservableObject {
      [ObservableProperty, Prop ("Laser Sequence", Prop.Type.Combo, "Laser processing sequence", null!, null!, ["Standard", "MarkCut","PierceCut", "MarkPierceCut", "PierceMarkCut"])]
      string? laserProcessingSeq = "Standard";

      [ObservableProperty, Prop (Prop.Type.Combo, "Laser Seq", null!, null!, ["Partwise", "PartwiseNative", "InnerFirst", "SortByCC"])]
      string? laserSeq = "PartwiseNative";

      [ObservableProperty, Prop (Prop.Type.Check, "Do pre-piercing part-by-part")]
      bool? doPrePiercingPartByPart = true;


      [ObservableProperty, Prop ("Route Traverse", Prop.Type.Check, "Move pierce points to reduce traverse")]
      bool? movePiercePointsToReduceTraverse = true;

      [ObservableProperty, Prop (Prop.Type.Check, "Move pierce points to prevent tilting")]
      bool? movePiercePointsToPreventTilting = true;

      [ObservableProperty, Prop (Prop.Type.Check, "Microjoint nested holes if tilting")]
      bool? microjointNestedHolesIfTilting = true;

      [ObservableProperty, Prop (Prop.Type.Text, "Ignore holes smaller tha this")]
      double? ignoreHolesSmallerThanThis = 12.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Minimum cutting head height when traversing")]
      double? minimumCuttingHeadHeightWhenTraversing = 12.23;

      [ObservableProperty, Prop (Prop.Type.Check, "Route traverse lines around holes")]
      bool? routeTraverseLinesAroundHoles = true;

      [ObservableProperty, Prop (Prop.Type.Text, "Allowance when routing around holes")]
      double? allowanceWhenRoutingAroundHoles = 25.23;

      [ObservableProperty, Prop (Prop.Type.Text, "lift nozzle if routing penalty more than")]
      double? liftNozzleIfRoutingPenaltyMoreThan = 25.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Allowance when routing around tilting holes")]
      double? allowanceWhenRoutingAroundTiltingHoles = 25.23;

      [ObservableProperty, Prop (Prop.Type.Text, "Max.head down traverse distance")]
      double? maxHeadDownTraverseDistance = 25.23;


      [ObservableProperty, Prop ("Laser Heads", Prop.Type.Check, "Cut with single head")]
      bool? cutWithSingleHead = true;
   }
}
