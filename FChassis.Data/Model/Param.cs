using System.Collections;
using System.Collections.Generic;

namespace FChassis.Data.Model;
[System.AttributeUsage (System.AttributeTargets.Field, AllowMultiple = true)]
public class Prop (Prop.Type type, string label, string unit = null!, string itemsName = null!,string dGridPath = null!) : System.Attribute {
   internal Prop (string _groupName, Prop.Type type, string label, string unit = null!, string itemsName = null!, string dGridPath = null!)
      : this (type, label, unit, itemsName,dGridPath) {
      groupName = _groupName;
   }

   public string groupName = null!;
   public Type type = type;
   public string label = label;
   public string? unit = unit;
   public string itemsName = itemsName;
   public object control = null!;
   public List<BindInfo> bindInfos = new ();
   public string? dGridPath = dGridPath!;

   public IEnumerable collections = null!;
   public ColInfo[] columns = null!;

   #region Inner Class --------------------------------------------------------
   public enum Type {
      Text,
      Combo,
      Check,
      Button,
      DGrid,
   };

   public class ColInfo {
      public Type type = Type.Text;
      public string header = null!;
      public string bindName = null!;
   }

   public class BindInfo (string name, object property) {
      public object property = property;
      public string name = name;
   }
   #endregion Inner Class
}