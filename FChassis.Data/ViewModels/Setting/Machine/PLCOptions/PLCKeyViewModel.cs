using CommunityToolkit.Mvvm.ComponentModel;
using FChassis.Data.Model;
using FChassis.Data.Model.Settings.Machine.PLCOptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChassis.Data.ViewModels.Setting.Machine.PLCOptions {
   public partial class PLCKeyViewModel : PLCKey {

      [ObservableProperty, Prop (Prop.Type.DGrid, null!, null!, null!, "PLCKeys")]
      private ObservableCollection<PLCKey> pLCKeys;

      public PLCKeyViewModel () {
         this.pLCKeys = new ObservableCollection<PLCKey> ();

         this.pLCKeys.Add (new PLCKey () { Name = "Test" });
      }
   }
}
