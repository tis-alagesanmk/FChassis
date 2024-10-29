using System.Collections.Generic;

namespace FChassis.Data.Model;
[System.AttributeUsage (System.AttributeTargets.Field, AllowMultiple = true)]
public class Prop (Prop.Type type, string label, string unit = null!, 
                   /*object[] items = null!, string itemsName = null!, */object[] colInfos = null!) 
             : System.Attribute {
   internal Prop (string _groupName, Prop.Type type, string label, string unit = null!/*, 
                  object[] items = null!, string itemsName = null!,
                  object[] colInfos = null!*/)
   : this (type, label, unit) {
      groupName = _groupName; }

   public string groupName = null!;
   public Type type = type;
   public string label = label;

   public string unit = unit;

   //public object[] items = items;
   //public string itemsName = itemsName;

   public object control = null!;
   public List<BindInfo> bindInfos = [];

   public object[] columns = colInfos;

   #region Inner Class --------------------------------------------------------
   public enum Type {
      Text,
      Combo,
      Check,
      Button,
      DGrid,
   };


   public class BindInfo (string name, object property) {
      public object property = property;
      public string name = name;
   }
   #endregion Inner Class
}

// -----------------------------------------------------------------
[System.AttributeUsage (System.AttributeTargets.Field, AllowMultiple = true)]
public class PropInfo (object[] items = null!)
             : System.Attribute {
   public PropInfo (string itemsName)
   : this ((object[])null!) { 
      this.itemsName = itemsName; }

   public string itemsName = null!;
   public object[] items = items;
}

// -----------------------------------------------------------------
[System.AttributeUsage (System.AttributeTargets.Field, AllowMultiple = true)]
public class DBGridPropInfo
             : PropInfo {
   internal DBGridPropInfo (object[] items, DBGridPropColInfo[] colInfos)
      : base (items) {
      this.colInfos = colInfos; }

   public DBGridPropInfo (string itemsName, object[] colInfos)
      : base (itemsName) {
      this.colInfos = colInfos; }

   public object[] colInfos;
}

// -----------------------------------------------------------------
[System.AttributeUsage (System.AttributeTargets.Field, AllowMultiple = true)]
public class DBGridPropColInfo (Prop.Type type, string header, string bindName)
            : System.Attribute {
   public Prop.Type type = type;
   public string header = header;
   public string bindName = bindName;
   public static object o(Prop.Type type, string header, string bindName) {
      return new DBGridPropColInfo (type, header, bindName);
   }
}