using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FChassis.Core.File; 
//-----------------------------------------------------------------------------
public class TreeNode {
   public object? content;
   public List<TreeNode> children = new ();

   public Type ElementTye = null!;
   public bool IsArray { get => this.ElementTye != null!; }

   #region Protected
   internal void set (object obj, string name = "") {
      Type propObjType, objType = obj.GetType ();
      if (this.isListType (obj, objType)) {
         this.add_CallMethod (this, obj, name);
         return;
      }

      this.content = obj;

      object propObj;
      List<Type> types = Reflection.Object.GetTypeList (objType, typeof (ObservableObject));
      foreach (Type type in types) {
         var fields = type.GetFields (BindingFlags.NonPublic | BindingFlags.Instance);
         foreach (FieldInfo fi in fields) {
            var a = fi.GetCustomAttribute<ObservablePropertyAttribute> ()!;
            if (a == null) continue;
            if ((propObj = fi?.GetValue (obj)!) == null) continue;

            if (a != null && this.isUserDefinedClass (propObj, (propObjType = propObj.GetType ()))) {
               TreeNode propNode = new () { content = propObj };
               this.children.Add (propNode); // Add node for this property object

               if (this.isListType (propObj, propObjType))
                  this.add_CallMethod (propNode, propObj, fi!.Name);
               else
                  propNode.set (propObj);
            }
         }
      }
   }

   protected void add<T> (List<T> list, string name) {
      this.ElementTye = this.listElementType (list);
      this.content = name;

      foreach (object? obj in list) {
         TreeNode childNode = new ();
         this.children.Add (childNode);
         childNode.set (obj!);
      }
   }
   #endregion Protected

   #region Implemention
   void add_CallMethod (object instance, object list, string arrayName) {
      Type elementTye = this.listElementType (list);

      var method = typeof (TreeNode).GetMethod ("add", BindingFlags.NonPublic | BindingFlags.Instance);
      var genericMethod = method!.MakeGenericMethod (elementTye);

      genericMethod.Invoke (instance, new object[] { list, arrayName });
   }

   bool isListType(object propObj, Type propObjType) 
      => propObj is IList && propObjType.IsGenericType && propObjType.GetGenericTypeDefinition () == typeof (List<>);

   Type listElementType (object list) {
      Type listType = list.GetType ();
      Type elementTye = listType.GetGenericArguments ()[0];
      return elementTye;
   }

   bool isUserDefinedClass (object obj, Type type) 
      => type.IsClass
          && !type.IsPrimitive
          && !type.IsEnum
          && !type.IsArray
          && type != typeof (string)
          && type != typeof (decimal)
          && type != typeof (DateTime);   
   #endregion Implemention
}

//-----------------------------------------------------------------------------
/// <summary>
/// </summary>
public abstract class File {
   public TreeNode node = new ();
}

//-----------------------------------------------------------------------------
/// <summary>
/// </summary>
public abstract class FileRead : File {
   // Overridable
   public abstract TreeNode GetObject (TreeNode node, string name);
   public abstract bool Read (string path);
}

//-----------------------------------------------------------------------------
/// <summary>
/// </summary>
public abstract class FileWrite : File {
   // Overridable
   public abstract bool Write (string path);
}