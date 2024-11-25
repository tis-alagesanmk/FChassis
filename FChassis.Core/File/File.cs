using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FChassis.Core.File;
//-----------------------------------------------------------------------------
/// <summary></summary>
public partial class TreeNode {
   public string? name;
   public object? obj;

   public List<TreeNode> children = new ();

   public Type ElementType = null!;
   public bool IsArray { get => this.ElementType != null!; }
}

//-----------------------------------------------------------------------------
/// <summary></summary>
public abstract class File {
   public TreeNode node = new ();
   public string? error;

   public bool setError(string error) {  
      this.error = error; return false; }
}

//-----------------------------------------------------------------------------
/// <summary></summary>
public abstract class FileRead : File {
   // Overridable
   public abstract TreeNode GetObject (TreeNode node, string name);
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
   static internal bool IsUserDefinedClass (Type type)
      => type.IsClass
          && !type.IsPrimitive
          && !type.IsEnum
          && !type.IsArray
          && type != typeof (string)
          && type != typeof (decimal)
          && type != typeof (DateTime);

   internal void SetRootObject (object obj, bool write, string name) {
      TreeNode childNode = new ();
      this.children.Add (childNode);

      childNode.SetObject (obj, write, name, this);
   }

   internal void SetObject (object obj, bool write, string name = null!, TreeNode parentNode = null!) {
      this.obj = obj;
      this.name = name;

      Type propObjType, objType = obj.GetType ();
      Type elementType = null!;
      if (TreeNode.IsListType (obj, objType, ref elementType)) {
         if (write)
            this.setListObject_CallMethod (this, obj, name, elementType, write);
         else
            this.setArrayObject (obj, name, elementType);
         
         return;
      }

      TreeNode propNode = null!;
      object propObj;
      List<Type> types = Reflection.Object.GetTypeList (objType, typeof (ObservableObject));
      foreach (Type type in types) {
         var fields = type.GetFields (BindingFlags.NonPublic | BindingFlags.Instance);
         foreach (FieldInfo fi in fields) {
            var a = fi.GetCustomAttribute<ObservablePropertyAttribute> ()!;
            if (a == null) continue;

            propObj = fi?.GetValue (obj)!;
            if (propObj == null) continue;

            propObjType = propObj.GetType ();
            if (propObjType.IsArray) {
               Type elemType = propObjType.GetElementType ()!;
               if (TreeNode.IsUserDefinedClass (elemType)) {
                  propNode = new ();
                  propNode.setIteratebleObject (propObj, fi!.Name, elemType, write);
               }
            } else if (TreeNode.IsUserDefinedClass (propObjType)) {
               propNode = new ();
               if (TreeNode.IsListType (propObj, propObjType, ref elementType))
                  this.setListObject_CallMethod (propNode!, propObj, fi!.Name, elementType, write);
               else
                  propNode!.SetObject (propObj, write, fi!.Name);
            }

            if (propNode != null) {
               this.children.Add (propNode); // Add node for this property object
               propNode = null!;
            }
         }
      }
   }

   internal static bool IsListType (object propObj, Type propObjType, ref Type elementType) {
      bool isList = propObj is IList && propObjType.IsGenericType && propObjType.GetGenericTypeDefinition () == typeof (List<>);
      if (isList)
         elementType = propObjType.GetGenericArguments ()[0];

      return isList;
   }

   internal TreeNode CreateElementNode (bool write = false) {
      object childObj = Activator.CreateInstance (this.ElementType)!;
      if (childObj == null)
         return null!;

      // Add Element node
      TreeNode childNode = new ();
      childNode.SetObject (childObj, write);
      this!.children.Add (childNode!);

      return childNode;
   }

   #region Protected
   protected void setListObject<T> (List<T> list, string name, Type elementType, bool write)
      => this.setIteratebleObject (list, name, elementType, write);

   protected void setArrayObject (dynamic iteratable, string name, Type elementType) {
      this.ElementType = elementType;
      this.obj = iteratable;
      this.name = name;      
   }

   protected void setIteratebleObject (dynamic iteratable, string name, Type elementType, bool write) {
      this.setArrayObject(iteratable, name, elementType);

      if (write) // add elements for writing
         foreach (object? obj in iteratable)
            this.add (obj, write);
   }
   #endregion Protected

   #region Implemention
   void add (object? obj, bool write) {
      TreeNode childNode = new ();
      this.children.Add (childNode);
      childNode.SetObject (obj!, write);
   }

   void setListObject_CallMethod (TreeNode node, object list, string arrayName, Type elementType, bool write) {
      var method = typeof (TreeNode).GetMethod ("setListObject", BindingFlags.NonPublic | BindingFlags.Instance);
      var genericMethod = method!.MakeGenericMethod (elementType);

      genericMethod.Invoke (node, [list, arrayName, elementType, write]);
   }   
   #endregion Implemention
}