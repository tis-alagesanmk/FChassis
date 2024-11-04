using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChassis.Data.Model.Settings.Machine.ProcessingDefaults {
   public partial class Import : ObservableObject {

       [ObservableProperty,Prop ("Import settings", Prop.Type.Combo, "", null, null, ["mm","inch"])]
       string? unitForDXFFiles = "mm";

       [ObservableProperty, Prop (Prop.Type.Text, "Stitch together lines/arcs closer than this")]
       double? stitchTogetherLinesOrArcsCloserThanThis = 0.23;

       [ObservableProperty, Prop (Prop.Type.Text, "Maximum thickness for sheet-metal part")]
       double? maximumThicknessForSheetMetalPart = 21.52;

       [ObservableProperty, Prop (Prop.Type.Check, "Ignore layer in DXF/DWG files")]
       bool? ignoreLayerInDXFOrDWGFiles = false;

       [ObservableProperty, Prop (Prop.Type.Check, "Explode blocks in 2D drawing")]
       bool? explodeBlockIn2DDrawing = false;

       [ObservableProperty, Prop (Prop.Type.Check, "Convert white entities to black")]
       bool? convertWhiteEntitiesToBlock = true;

       [ObservableProperty, Prop (Prop.Type.Check, "Darken colors during DXF import")]
       bool? darkenColorsDuringDXFImport = true;

      [ObservableProperty, Prop ("DXF Settings", Prop.Type.Check, "Angles in DXF are interior angles")]
      bool? anglesInDXFAreInteriorAngles = true;

      [ObservableProperty, Prop ("Spline Coversion", Prop.Type.Combo, "Covert splines on import", null, null, ["Off","Lines","Arcs"])]
      string? covertSplinesOnImport = "Lines";

      [ObservableProperty, Prop ("Layer mapping", Prop.Type.Text, "Auxilary Layers Names")]
      string? auxilaryLayersNames = "_AUX,AUX,BENDLIMIT";

      [ObservableProperty, Prop (Prop.Type.Text, "Mark Layers Names")]
      string? markLayersNames = "_AUX,AUX,BENDLIMIT";

   }
}
