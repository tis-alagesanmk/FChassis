using System.Collections;
using System.Collections.Generic;

namespace FChassis.Data.Model;
[System.AttributeUsage (System.AttributeTargets.Field, AllowMultiple = true)]
public class Prop (Prop.Type type, string label, string unit = null!, object[] items = null!, string itemsName = null!,string dGidPath = null!) : System.Attribute {
   public Prop (string _groupName, Prop.Type type, string label, string unit = null!, object[] items = null!, string itemsName = null!, string dGidPath = null!)
      : this (type, label, unit, items, itemsName, dGidPath) {
      groupName = _groupName;
   }

   public string groupName = null!;
   public Type type = type;
   public string label = label;
   public string? unit = unit;
   public string dGridPath = dGidPath;
   public object[] items = items;
   public string itemsName = itemsName;
   public object control = null!;
   public List<BindInfo> bindInfos = new ();

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