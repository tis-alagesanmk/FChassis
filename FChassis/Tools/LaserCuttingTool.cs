using Flux.API;
using System.Windows.Threading;

namespace FChassis.Tools;

public class Nozzle {
   readonly Cylinder mCylinder;
   public Nozzle (double diameter, double height, int segments) 
      => mCylinder = new Cylinder (diameter, height, segments);

   public void Draw (XForm4 LHCompTransform, Color32 LHToolColor, 
                     XForm4 RHCompTransform, Color32 RHToolColor) 
      => mCylinder.Draw (LHCompTransform, LHToolColor, RHCompTransform, RHToolColor);
}
