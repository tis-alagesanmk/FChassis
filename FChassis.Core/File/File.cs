using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace FChassis.Core.File;
//-----------------------------------------------------------------------------
/// <summary></summary>
public class TreeNode {
   public object? content;
   public List<TreeNode> children = new ();

   public Type ElementType = null!;
   public bool IsArray { get => this.ElementType != null!; }

   static internal bool IsUserDefinedClass (Type type)
      => type.IsClass
          && !type.IsPrimitive
          && !type.IsEnum
          && !type.IsArray
          && type != typeof (string)
          && type != typeof (decimal)
          && type != typeof (DateTime);

   internal void Set (object obj, bool write, string name = "") {
      Type propObjType, objType = obj.GetType ();

      Type elementType = null!;
      if (this.isListType (obj, objType, ref elementType)) {
         this.addList_CallMethod (this, obj, name, elementType, write);
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

            propObj = fi?.GetValue (obj)!; 
            if (propObj == null) continue;

            propObjType = propObj.GetType ();
            if (propObjType.IsArray) {
               Type elemType = propObjType.GetElementType ()!;
               if(TreeNode.IsUserDefinedClass (elemType)) {
                  TreeNode propNode = new () { content = propObj };
                  this.children.Add (propNode); // Add node for this property object

                  propNode.addArray ((Array)propObj, fi!.Name, elemType, write);
                  continue;
               }
            }

            if (TreeNode.IsUserDefinedClass (propObjType)) {
               TreeNode propNode = new () { content = propObj };
               this.children.Add (propNode); // Add node for this property object

               if (this.isListType (propObj, propObjType, ref elementType))
                  this.addList_CallMethod (propNode, propObj, fi!.Name, elementType, write);
               else
                  propNode.Set (propObj, write);
            }
         }
      }
   }

   #region Protected
   protected void addList<T> (List<T> list, string name, Type elementType, bool write) 
      => this.add (list, name, elementType, write); 

   protected void addArray (Array array, string name, Type elementType, bool write)
      => this.add (array, name, elementType, write);

   void add (dynamic iteratable, string name, Type elementType, bool write) {
      this.ElementType = elementType;
      this.content = name;

      if (write) // add elements for writing
         foreach (object? obj in iteratable)
            this.add (obj, write);
   }
  
   #endregion Protected

   #region Implemention
   void add (object? obj, bool write) {
      TreeNode childNode = new ();
      this.children.Add (childNode);
      childNode.Set (obj!, write);
   }

   void addList_CallMethod (object instance, object list, string arrayName, Type elementType, bool write) {
      var method = typeof (TreeNode).GetMethod ("addList", BindingFlags.NonPublic | BindingFlags.Instance);
      var genericMethod = method!.MakeGenericMethod (elementType);

      genericMethod.Invoke (instance, [list, arrayName, elementType, write]);
   }

   bool isListType (object propObj, Type propObjType, ref Type elementType) {
      bool isList = propObj is IList && propObjType.IsGenericType && propObjType.GetGenericTypeDefinition () == typeof (List<>);
      if(isList) 
         elementType = propObjType.GetGenericArguments ()[0];

      return isList;
   }   
   #endregion Implemention
}

//-----------------------------------------------------------------------------
/// <summary></summary>
public abstract class File {
   public TreeNode node = new ();
   public bool write = false;
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