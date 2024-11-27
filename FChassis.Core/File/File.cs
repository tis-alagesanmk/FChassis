using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FChassis.Core.File;
//-----------------------------------------------------------------------------
/// <summary></summary>
public partial class TreeNode {
   public enum Type {
      obj,
      list,
      array,
   }

   public object? obj;
   public Dictionary<string, TreeNode> children = new ();
}

//-----------------------------------------------------------------------------
/// <summary></summary>
public abstract class File {
   public TreeNode rootNode = new ();   
   public string? error;

   public bool setError(string error) {  
      this.error = error; return false; }
}

//-----------------------------------------------------------------------------
/// <summary></summary>
public abstract class FileRead : File {
   // Overridable   
   public abstract bool Read (string path);
}

//-----------------------------------------------------------------------------
/// <summary></summary>
public abstract class FileWrite : File {
   // Overridable
   public abstract bool Write (string path);
}

//-----------------------------------------------------------------------------
public partial class TreeNode {
   public TreeNode AddObject (string name, object obj) {
      TreeNode childNode = new () { obj = obj };
      this.children.Add (name, childNode);
      return childNode;
   }

   public static object GetObject (dynamic obj, System.Type objType, string name) {
      object childObj = null!;
      if (obj is TreeNode) {
         TreeNode childNode, node = (TreeNode) obj;
         if (node.children.TryGetValue (name, out childNode!)) {
            childObj = childNode!.obj!;
            if (childObj == null)
               childObj = childNode;  // Container Node
         }
      } else {
         FieldInfo fi = objType.GetField (name)!;
         if(fi != null!)
            childObj = fi.GetValue (obj);
      }

      return childObj;
   }

   static internal bool IsUserDefinedClass (System.Type type)
      =>  type is not null
          && !type.IsClass
          && !type.IsPrimitive
          && !type.IsEnum
          && !type.IsArray
          && type != typeof (string)
          && type != typeof (decimal)
          && type != typeof (DateTime);


   internal static bool IsListType (object propObj, System.Type propObjType, ref System.Type elementType) {
      bool isList = propObj is IList && propObjType.IsGenericType && propObjType.GetGenericTypeDefinition () == typeof (List<>);
      if (isList)
         elementType = propObjType.GetGenericArguments ()[0];

      return isList;
   }

   internal static object CreateElement (object obj, System.Type objType, System.Type elementType, TreeNode.Type type) {
      object childObj = Activator.CreateInstance (elementType)!;
      if (childObj == null)
         return null!;

      if(type == Type.list) {
         MethodInfo addMethod = objType!.GetMethod ("Add")!;
         addMethod.Invoke (obj, new object[] { childObj });
      }      

      return childObj;
   }
}