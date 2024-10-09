using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FChassis.UI.SettingPanels.Machine.TechParams.Model;

namespace FChassis.UI.SettingPanels.Machine.TechParams.ViewModel {
   public class ExhaustSystemViewModel {
      public ObservableCollection<ExhaustSysModel> Sections { get; set; }
      public ObservableCollection<ExhaustSysModel> Splitters { get; set; }
      private const string Section = "Section";

      public ExhaustSystemViewModel () {
         this.Sections = new ObservableCollection<ExhaustSysModel> ();
         this.Splitters = new ObservableCollection<ExhaustSysModel> ();
         GetExhausSystem ();
      }

      private void GetExhausSystem () {

         for (int i = 1; i <= 36; i++)
            this.Sections.Add (new ExhaustSysModel {
               SectionNumber = Section + " " + i,
            }
            );

         for (int i = 1; i <= 6; i++)
            this.Splitters.Add (new ExhaustSysModel {
               SectionNumber = Section + " " + i,
            }
            );
      }
   }
}
