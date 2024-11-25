using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System;
using System.Xml.Linq;
using System.Diagnostics;

namespace FChassis.Core.File;
/// <summary></summary>
public class JSONFileWrite : FileWrite {
   #region Method
   public bool Write (string path, object obj, string objName) {
      this.write = true;
      this.node.SetRoot (obj, this.write, objName);
      return Write (path);
   }

   public override bool Write (string path) {
      using var fileStream = System.IO.File.Create (path);
      JsonWriterOptions options = new () { Indented = true };
      using Utf8JsonWriter writer = new (fileStream, options);

      writer.WriteStartObject ();
      foreach(var childNode in this.node.children)
         if(!this.writeObject(writer, childNode, childNode.name!))
            return false;

      writer.WriteEndObject ();
      fileStream.Flush ();
      return true;
   }
   #endregion Method

   #region Implement
   bool writeObject (Utf8JsonWriter writer, TreeNode node, string name, TreeNode parentNode = null!) {
      bool success = false;

      object obj = node.obj!;
      if (node.IsArray)
         writer.WriteStartArray (name);
      else {
         if (parentNode != null && parentNode.IsArray)
            writer.WriteStartObject ();
         else {
            Debug.Assert (name != null);
            writer.WriteStartObject (name);
         }

         this.writeObjectAttributes (writer, obj);
      }      

      foreach (var childNode in node.children)
         this.writeObject (writer, childNode, childNode.name!, node);

      if (node.IsArray)
         writer.WriteEndArray ();
      else
         writer.WriteEndObject ();

      success = true;
      return success;
   }

   void writeObjectAttributes (Utf8JsonWriter writer, object obj) {
      List<Type> types = Reflection.Object.GetTypeList (obj.GetType (), typeof (ObservableObject));
      foreach (Type type in types)
         this.writeObjectTypeAttributes (writer, obj, type);
   }

   void writeObjectTypeAttributes (Utf8JsonWriter writer, object obj, Type type) {
      var fields = type.GetFields (BindingFlags.NonPublic | BindingFlags.Instance);
      foreach (FieldInfo fi in fields) {
         var p = fi.GetCustomAttribute<ObservablePropertyAttribute> ()!;
         if (p == null) 
            continue;

         object attr = fi?.GetValue (obj)!;
         if (attr != null)
            this.writeObjectAttribute (writer, fi?.Name!, attr);
      }
   }

   void writeObjectAttribute (Utf8JsonWriter writer, string name, object value) {
      Type dataType = value.GetType ();
      string type = dataType.ToString ();
      switch (type) {
         case "System.String":
            writer.WriteString (name, value.ToString ());
            break;

         case "System.Int32":
            writer.WriteNumber(name, (Int32)value);
            break;

         case "System.Double":
            writer.WriteNumber (name, (double)value);
            break;

         case "System.Decimal":
            writer.WriteNumber (name, (decimal)value);
            break;

         case "System.Boolean":
            writer.WriteBoolean (name, (bool)value);
            break;
            
         default:
            if(dataType.IsEnum)
               writer.WriteString (name, value.ToString());
            else if (dataType.IsArray) {
               if(_isUserDefinedClassArray(dataType)) 
                  return; 

               this.writeArray (writer, name, (Array)value);
            }
            break;
      }

      #region Local
      bool _isUserDefinedClassArray(Type type) {
         Type elemType = type.GetElementType ()!;
         return TreeNode.IsUserDefinedClass (elemType);
      }
      #endregion Local
   }

   void writeArray(Utf8JsonWriter writer, string name, Array array) {
      writer.WriteStartArray (name);

      foreach (object element in array)
         writer.WriteStringValue (element.ToString ());

      writer.WriteEndArray ();
   }
   #endregion Implement
}

//-----------------------------------------------------------------------------
/// <summary></summary>
public class JSONFileRead : FileRead {
   #region Method

   public bool Read (string path, object obj, string objName) {
      this.node.SetRoot (obj, false, objName);
      return Read (path);
   }

   public override bool Read (string path) {
      if(!System.IO.File.Exists (path))
         return false;

      ReadOnlySpan<byte> jsonReadOnlySpan = System.IO.File.ReadAllBytes (path);
      var reader = new Utf8JsonReader (jsonReadOnlySpan);

      try {
         if (!reader.Read ())
            return true;
      } catch (Exception e) {
         this.error = e.Message;
         return false;
      }

      return this.readObject (this.node, ref reader);
   }

   public override TreeNode GetObject (TreeNode node, string name) {
      if(node.name as string == name || node.obj?.GetType ().Name == name)
         return node;

      foreach (TreeNode childNode in node!.children) {
         if (childNode.name as string == name || childNode.obj?.GetType ().Name == name)
            return childNode;
      }

      return null!;
   }
   #endregion Method

   #region Implement
   bool readObject (TreeNode node, ref Utf8JsonReader reader, string name = "") {
      bool success = true;

      TreeNode childNode = null!;
      object obj = node.obj!;
       Type objType = obj?.GetType()!;

      do {
          while (success && reader.Read ()) {
            switch (reader.TokenType) {
               case JsonTokenType.StartObject:
                  if (node!.IsArray) {
                     childNode = node.CreateElementNode ();
                     if(childNode == null)
                        return setError ("Element Object create failed");
                  } else { 
                     childNode = this.GetObject (node!, name);
                     if(childNode == null)
                        return setError ("Object not found");
                  }

                  if(!this.readObject (childNode!, ref reader, name))
                     return false;

                  break;

               case JsonTokenType.EndObject:
               case JsonTokenType.EndArray:
                  return true;

               case JsonTokenType.StartArray:
                  if (obj != null) {
                     string cname = this._capitalizeFirstLetter (name);
                     PropertyInfo? pi = objType.GetProperty (cname, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                     if (pi == null)
                        return setError ($"'{cname}' Property not found");                     

                     if (pi.PropertyType.IsArray) {
                        // Is UserDefined class array
                        Type elementType = pi.PropertyType.GetElementType ()!;
                        if (TreeNode.IsUserDefinedClass (elementType)) {
                           childNode = this.GetObject (node!, name);
                           if (childNode == null)
                              return setError ($"Array '{cname}' Object not found");
                        } else {
                           if (!this.readArray (obj!, cname, pi, ref reader))
                              return false;

                           continue;
                        }
                     }
                  } else {
                     childNode = this.GetObject (node!, name);
                     if (childNode == null)
                        return setError ($"{name} Object not found");
                  }

                  if (!this.readObject (childNode!, ref reader, name))
                     return false;
                  break;

               case JsonTokenType.Comment:
                  break;

               case JsonTokenType.PropertyName:
                  name = reader.GetString ()!;
                  break;

               default:
                  success = this.readAttribute(obj!, objType!, name, ref reader);
                  break;
            } // switch - reader.TokenType
         } // while = reader.Read ();
      } while (false);

      return success;
   }

   private bool readArray (object obj, string cname, PropertyInfo pi, ref Utf8JsonReader reader) {
      object value;

      Type elementType = pi.PropertyType.GetElementType ()!;
      Type listType = typeof (List<>).MakeGenericType (elementType);
      object list = Activator.CreateInstance (listType)!;
      var addMethod = listType.GetMethod ("Add")!;

      while (reader.Read ()) {
         switch (reader.TokenType) {
            case JsonTokenType.EndArray:
               var toArrayMethod = listType.GetMethod ("ToArray")!;
               Array array = (Array)toArrayMethod.Invoke (list, null)!;
               pi.SetValue (obj, array);
               return true;

            case JsonTokenType.String:
            case JsonTokenType.Number:
            case JsonTokenType.True:
            case JsonTokenType.False:
               value = this.readAttributeValue (elementType, ref reader);
               value = Convert.ChangeType (value, elementType)!;
               addMethod.Invoke (list, [value]);
               break;

            default:
               return setError ($"Unsupported Data Type {reader.TokenType.ToString ()}");
         }
      }

      return true;
   }

   bool readAttribute (object obj, Type objType, string name, ref Utf8JsonReader reader) {
      do {
         string cname = this._capitalizeFirstLetter (name);
         PropertyInfo pi = objType.GetProperty (cname, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!; 
         if (pi == null) 
            return setError ($"Property[{cname}] not found"); 

         object value = this.readAttributeValue (pi.PropertyType, ref reader);
         pi.SetValue (obj, value);
      } while (false);

      return true;
   }

   object readAttributeValue (Type objType, ref Utf8JsonReader reader) {
      Type MyType = typeof (int);
      object value = null!;
      switch (reader.TokenType) {
         case JsonTokenType.String:
         case JsonTokenType.True:
         case JsonTokenType.False:
            value = reader.TokenType switch {
               JsonTokenType.String => reader.GetString ()!,
               JsonTokenType.True   => reader.GetBoolean ()!,
               JsonTokenType.False  => reader.GetBoolean ()!,
                                  _ => null!
            };
            break;

         case JsonTokenType.Number:
            value = objType switch {
               Type t when t == typeof (Int32)   => reader.GetInt32 ()!,
               Type t when t == typeof (UInt32)  => reader.GetUInt32 ()!,
               Type t when t == typeof (double)  => reader.GetDouble ()!,
               Type t when t == typeof (decimal) => reader.GetDecimal ()!,
                                               _ => null!
            };
            break;

         default:
            break;
           
      }

      if(value == null) {
         this.error = $"Unsupported Data Type {reader.TokenType.ToString ()}";
         return null!;
      }

      if (objType.IsEnum) 
         value = Enum.Parse (objType, (string)value);

      return value!;
   }

   //Type _getExactPropertyType (Type propertyType)
   //  => Nullable.GetUnderlyingType (propertyType) ?? propertyType;

   string _capitalizeFirstLetter (string str)
     => char.ToUpper (str[0]) + str.Substring (1);
   #endregion Implement
}