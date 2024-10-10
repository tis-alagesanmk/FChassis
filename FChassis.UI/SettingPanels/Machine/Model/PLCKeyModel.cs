namespace FChassis.UI.SettingPanels.Machine.Model;
public class PLCKeyModel {
   public string Name { get; set; }
   public string Type { get; set; }
   public string Function { get; set; }

   public PLCKeyModel (string name, string type="None", string function="") {
      Name = name; 
      Type = type; 
      Function = function;
   }
}
