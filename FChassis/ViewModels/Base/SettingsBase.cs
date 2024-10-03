using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace FChassis.ViewModels; 
public class SettingsBase : ViewModelBase{
   #region "OkCommand"
   private ICommand okCommand;
   public ICommand OkCommand {
      get {
         okCommand = okCommand ?? new Base.Command (param => this.CloseSettings (), null);
         return okCommand;
      }
   }
   virtual protected void CloseSettings () { }
   #endregion "OkCommand"

}
