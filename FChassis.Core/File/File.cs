using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Xml.Linq;
using static CommunityToolkit.Mvvm.ComponentModel.__Internals.__TaskExtensions.TaskAwaitableWithoutEndValidation;

namespace FChassis.Core.File; 
//-----------------------------------------------------------------------------
public class TreeNodes : List<TreeNode> { }

//-----------------------------------------------------------------------------
public class TreeNode {
   public object? content;
   public TreeNodes children = new ();
   public object? tag = null;

   public delegate TreeNode DGCreateElement ();
   public DGCreateElement CreateElement = null!;
   TreeNode _CreateElement () => null!;
   public bool IsArray {
      get => this.CreateElement != null!; }

   //public delegate void DGReadElementCompleted (TreeNode childNode);
   //public DGReadElementCompleted ReadElementCompleted = null!;
   bool _isUserDefinedClass (Type type) {
      // Check if it's a class, but not a primitive or standard type
      return type.IsClass
          && !type.IsPrimitive
          && !type.IsEnum
          && type != typeof (string)
          && type != typeof (decimal)
          && type != typeof (DateTime);
   }

   public void Set (object obj) {
      this.content = obj;

      Type propObjType, objType = obj.GetType ();
      List<Type> types = Reflection.Object.GetTypeList (objType, typeof (ObservableObject));
      foreach (Type type in types) {
         var fields = type.GetFields (BindingFlags.NonPublic | BindingFlags.Instance);
         foreach (FieldInfo fi in fields) {
            var a = fi.GetCustomAttribute<ObservablePropertyAttribute> ()!;
            if (a == null) continue;
            object propObj = fi?.GetValue (obj)!;

            if (a != null && (propObjType = propObj.GetType ()).IsClass && this._isUserDefinedClass(propObjType)) {
               TreeNode propNode = new () { content = propObj };
               this.children.Add (propNode);

               if (propObj is IList && propObjType.IsGenericType && propObjType.GetGenericTypeDefinition() == typeof (List<>)) {
                  Type listType = propObj.GetType ();
                  Type itemType = listType.GetGenericArguments ()[0];

                  // Dynamically call Add<T>()
                  var method = typeof (TreeNode).GetMethod ("Add");
                  var genericMethod = method!.MakeGenericMethod (itemType);
                  genericMethod.Invoke (propNode, new object[] { propObj, fi!.Name });                  
               }
               else               
                  propNode.Set (propObj);
            }
         }
      }
   }

   public void Add<T> (List<T> list, string name) {
      this.content = name;
      this.CreateElement = this._CreateElement;
      foreach (object? obj in list) {
         TreeNode childNode = new ();
         this.children.Add(childNode);
         childNode.Set (obj!);
      }
   }
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

   public bool Read (string path, object obj) {
      this.node!.content = obj;

      return this.Read (path);
   }
}

//-----------------------------------------------------------------------------
/// <summary>
/// </summary>
public abstract class FileWrite : File {
   // Overridable
   public abstract bool Write (string path);

   public bool Write (string path, object obj) {
      this.node.content = obj;
      return this.Write (path);
   }
}