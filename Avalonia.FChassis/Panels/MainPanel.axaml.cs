using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Avalonia.Desktop;

public partial class MainPanel : UserControl
{
   public event EventHandler ButtonRaised;
   public MainPanel()
    {
        InitializeComponent();
    }
   
   protected virtual void OnSwitchEvent () {

      ButtonRaised?.Invoke (this, EventArgs.Empty);
   }
   public void PressButton (string content) 
   {
      ButtonRaised?.Invoke (content, EventArgs.Empty);
   }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
      var tag = (sender as Button).Tag.ToString ().Trim ();
      this.PressButton (tag);
   }
}