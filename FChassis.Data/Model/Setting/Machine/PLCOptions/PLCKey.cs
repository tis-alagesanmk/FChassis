using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChassis.Data.Model.Setting.Machine.PLCOptions {
   public partial class PLCKey : ObservableObject{
      [ObservableProperty,Prop(Prop.Type.Text,"Number",null,"","Name")]
      private string? name;

   }
}
