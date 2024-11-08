using CommunityToolkit.Mvvm.ComponentModel;

namespace FChassis.Data.Model.Settings.Machine.TechParams {
   public partial class AnalogScaling : ObservableObject{

      [ObservableProperty,Prop("Channels", Prop.Type.Text,"Channel 0")]
      double? channel0 = 0;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 1")]
      double? channel1 = 1;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 2")]
      double? channel2 = 2;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 3")]
      double? channel3 = 3;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 4")]
      double? channel4 = 4;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 5")]
      double? channel5 = 5;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 6")]
      double? channel6 = 6;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 7")]
      double? channel7 = 7;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 8")]
      double? channel8 = 8;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 9")]
      double? channel9 = 9;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 10")]
      double? channel10 = 10;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 11")]
      double? channel11 = 11;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 12")]
      double? channel12 = 12;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 13")]
      double? channel13 =13;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 14")]
      double? channel14 = 14;
      [ObservableProperty, Prop (Prop.Type.Text, "Channel 15")]
      double? channel15 = 15;
   }
}
