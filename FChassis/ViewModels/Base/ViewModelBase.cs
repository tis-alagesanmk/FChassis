using System.ComponentModel;

namespace FChassis.ViewModels;

public class ViewModelBase : INotifyPropertyChanged
 {
     public event PropertyChangedEventHandler PropertyChanged;

     protected void OnPropertyChanged(string propertyName)
     {
         if (PropertyChanged!=null)
         {
             PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
         }
     }
 }
