using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace FChassis.UI.SettingPanels.Machine.ViewModel;
public partial class HMISettingsViewModel: ObservableObject {
   private ObservableCollection<string> comboBoxItems;
   private bool isCheckBoxChecked;

   [ObservableProperty]
   public string property1Text;

   public bool IsCheckBoxChecked {
      get => isCheckBoxChecked;
      set {
         isCheckBoxChecked = value;
         OnPropertyChanged (nameof (IsCheckBoxChecked));
      }
   }

   public ObservableCollection<string> ComboBoxItems {
      get => comboBoxItems;
      set {
         comboBoxItems = value;
         OnPropertyChanged (nameof (ComboBoxItems));
      }
   }

   public HMISettingsViewModel () {
      Property1Text = "ValueH";
      IsCheckBoxChecked = false;
      comboBoxItems = new () { "item1", "item2", "item3", "item4", "item5" };
   }

   [RelayCommand]
   protected void ButtonClicked () {
      Property1Text = "Button click";
   }

   [RelayCommand]
   protected void CheckboxClicked () {
      if (IsCheckBoxChecked)
         Property1Text = "Check Box checked";
      else
         Property1Text = "Check Box Unchecked";
   }
}
