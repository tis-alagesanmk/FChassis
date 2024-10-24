using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FChassis.Data.Model.SettingPanels.Machine.General {
   public partial class HMI : ObservableObject {

      #region --- HMI Fields ---
      private IEnumerable<string>? orientation;
      private double? stepsizetoincrement = 10;
      private int? maximumdayskeepbackupfiles = 10;
      private int? minimumstoragetokeepbackupfiles = 10;
      private IEnumerable<string>? plcmessagestodisplay;
      private bool? captionforcommandbaricons = true;
      private bool? miniplayer =true;
      private IEnumerable<string>? language;
      private IEnumerable<string>? theme;

      private double? width = 10;
      private double? height = 10;

      #endregion

      #region --- HMI Properties ---
      public IEnumerable<string>? Orientation 
      {
         get => orientation;
         set => SetProperty(ref orientation, value);
      }
      public double? StepSizetoIncrement {
         get => stepsizetoincrement;
         set => SetProperty (ref stepsizetoincrement, value);
      }
      public int? MaximumDaysKeepBackupFiles {
         get => maximumdayskeepbackupfiles;
         set => SetProperty (ref maximumdayskeepbackupfiles, value);
      }
      public int? MinimumStoragetoKeepBackupFiles {
         get => minimumstoragetokeepbackupfiles;
         set => SetProperty (ref minimumstoragetokeepbackupfiles, value);
      }
      public IEnumerable<string>? PLCMessagesToDisplay {
         get => plcmessagestodisplay;
         set => SetProperty (ref plcmessagestodisplay, value);
      }
      public bool? CaptionForcommandBarIcons {
         get => captionforcommandbaricons;
         set => SetProperty (ref captionforcommandbaricons, value);
      }
      public bool? MiniPlayer {
         get => miniplayer;
         set => SetProperty (ref miniplayer, value);
      }
      public IEnumerable<string>? Language {
         get => language;
         set => SetProperty (ref language, value);
      }
      public IEnumerable<string>? Theme {
         get => theme;
         set => SetProperty (ref theme, value);
      }
      public double? Width {
         get => width;
         set => SetProperty (ref width, value);
      }
      public double? Height {
         get => height;
         set => SetProperty (ref height, value);
      }

      #endregion

      #region --- Selected Item ---
      [ObservableProperty]
      private string? selectedorientation = "Portrait";
      [ObservableProperty]
      private string? selecteplcmessage = "Only error";
      [ObservableProperty]
      private string? selectedlanguage = "EN";
      [ObservableProperty]
      private string? selectedtheme = "Grey";
      #endregion
   }
}
