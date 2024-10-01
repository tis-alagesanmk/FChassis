﻿using FChassis.Processes;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FChassis;
public class SimulationStatusToVisibilityConverter : IValueConverter {
   public object Convert (object value, Type targetType, 
                          object parameter, CultureInfo culture) {
      if (value is Processor.ESimulationStatus status) {
         switch (status) {
            case Processor.ESimulationStatus.Running:
               if ((string)parameter == "Pause" || (string)parameter == "Stop")
                  return Visibility.Visible;
               break;

            case Processor.ESimulationStatus.Paused:
               if ((string)parameter == "Stop" || (string)parameter == "Simulate")  
                  return Visibility.Visible;
               break;

            case Processor.ESimulationStatus.NotRunning:
               if ((string)parameter == "Simulate")
                  return Visibility.Visible;
               break;
         }
      }

      return Visibility.Collapsed;
   }

   public object ConvertBack (object value, Type targetType, 
                              object parameter, CultureInfo culture) 
      => throw new NotImplementedException ();   
}
