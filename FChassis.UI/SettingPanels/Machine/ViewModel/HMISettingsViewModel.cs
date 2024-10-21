using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace FChassis.UI.SettingPanels.Machine.ViewModel;
public partial class HMISettingsViewModel: ObservableObject {
   private string property1Text;
   public string Property1Text {
      get => property1Text;
      set {
         property1Text = value;
         OnPropertyChanged(nameof(Property1Text));
      }
   }

   public HMISettingsViewModel () {
      Property1Text = "ValueH";
   }

   [RelayCommand]
   protected void ButtonClicked() {
      Property1Text = "Button click";
   }
   //public event PropertyChangedEventHandler PropertyChanged;

   //protected virtual void OnPropertyChanged (string propertyName) {
   //   PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
   //}
}
