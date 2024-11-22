using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.Generic;
using System.Text.Json;
using System.Reflection;
using System;

namespace FChassis.Core.File;
/// <summary></summary>
public class JSONFileWrite : FileWrite {
   #region Method
   public bool Write (string path, object obj, string objName = "Configuration") {
      this.write = true;
      this.node.Set (obj, this.write, objName);
      return Write (path);
   }

   public override bool Write (string path) {
      using var fileStream = System.IO.File.Create (path);
      JsonWriterOptions options = new () { Indented = true };
      using Utf8JsonWriter writer = new (fileStream, options);

      writer.WriteStartObject ();
      if(!this.writeObject(writer, this.node))
         return false;

      writer.WriteEndObject ();
      fileStream.Flush ();
      return true;
   }
   #endregion Method

   #region Implement
   bool writeObject (Utf8JsonWriter writer, TreeNode node, TreeNode parentNode = null!) {
      bool success = false;

      string? name = node.content as string;
      object obj = node.content! as object;
      if (name != null) {
         if (node.IsArray)
            writer.WriteStartArray (name);
         else
            writer.WriteStartObject (name);
      }         
      else {
         if(parentNode != null && parentNode.IsArray)
            writer.WriteStartObject ();
         else
            writer.WriteStartObject (obj.GetType ().Name);

         this.writeObjectAttributes (writer, obj);
      }

      foreach (var childNode in node.children)
         this.writeObject (writer, childNode, node);

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
   public bool Read (string path, object obj, string objName = "Configuration") {
      this.node.Set (obj, false, objName);
      return Read (path);
   }

   public override bool Read (string path) {
      if(!System.IO.File.Exists (path))
         return false;

      ReadOnlySpan<byte> jsonReadOnlySpan = System.IO.File.ReadAllBytes (path);
      var reader = new Utf8JsonReader (jsonReadOnlySpan);

      bool success = false;
      if (reader.Read ())
         success = this.readObject (this.node, ref reader);

      return true;
   }

   public override TreeNode GetObject (TreeNode node, string name) {
      if(node.content as string == name || node?.content?.GetType ().Name == name)
         return node;

      foreach (TreeNode childNode in node!.children) {
         if (childNode.content as string == name || childNode?.content?.GetType ().Name == name)
            return childNode;
      }

      return null!;
   }
   #endregion Method

   #region Implement
   bool readObject (TreeNode node, ref Utf8JsonReader reader, TreeNode parentNode = null!, string name = "") {
      bool success = true;

      TreeNode childNode;
      object childObj, obj = node?.content!;
      Type objType = obj?.GetType()!;

      do {
         while (success && reader.Read ()) {
            switch (reader.TokenType) {
               case JsonTokenType.StartObject:
                  if (parentNode != null! && parentNode.IsArray) {
                     childObj = Activator.CreateInstance(parentNode.ElementType)!;
                     if (childObj == null)
                        continue;

                     childNode = new () { content = childObj };
                     parentNode!.children.Add (childNode!);

                     childNode.Set(childNode.content!, false);
                  } else {
                     childNode = this.GetObject (node!, name);
                     if (childNode == null || childNode == node)
                        continue;
                  }

                  success = this.readObject (childNode!, ref reader, node!);
                  break;

               case JsonTokenType.EndObject:
               case JsonTokenType.EndArray:
                  return true;

               case JsonTokenType.StartArray:
                  string cname = this._capitalizeFirstLetter (name);
                  PropertyInfo? pi = objType.GetProperty (cname, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                  if (pi != null) {
                     if (pi.PropertyType.IsArray) {
                        // Is UserDefined class array
                        Type elementType = pi.PropertyType.GetElementType ()!;
                        if (TreeNode.IsUserDefinedClass (elementType)) { 
                           childNode = this.GetObject (node!, name);
                           if (childNode != null)
                              this.readObject (null!, ref reader, childNode!, name);
                        }
                        else 
                           this.readArray (obj!, cname, pi, ref reader);
                     }
                  }
                  break;

               case JsonTokenType.Comment:
                  break;

               case JsonTokenType.PropertyName:
                  name = reader.GetString ()!;
                  break;

               default:
                  success = this.readAttribute(obj!, objType, name, ref reader);
                  break;
            } // switch - reader.TokenType
         } // while = reader.Read ();
      } while (false);

      return success;
   }

   private bool readArray (object obj, string cname, PropertyInfo pi, ref Utf8JsonReader reader) {
      bool success = true;
      object value;

      Type elementType = pi.PropertyType.GetElementType ()!;
      Type listType = typeof (List<>).MakeGenericType (elementType);
      object list = Activator.CreateInstance (listType)!;
      var addMethod = listType.GetMethod ("Add")!;

      while (success && reader.Read ()) {
         switch (reader.TokenType) {
            case JsonTokenType.EndArray:
               var toArrayMethod = listType.GetMethod ("ToArray")!;
               Array array = (Array)toArrayMethod.Invoke (list, null)!;
               pi.SetValue (obj, array);
               return success;

            case JsonTokenType.String:
            case JsonTokenType.Number:
            case JsonTokenType.True:
            case JsonTokenType.False:
               value = this.readAttributeValue (elementType, ref reader);
               value = Convert.ChangeType (value, elementType)!;
               addMethod.Invoke (list, [value]);
               break;

            default:
               break;
         }
      }

      return success;
   }

   bool readAttribute (object obj, Type objType, string name, ref Utf8JsonReader reader) {
      bool success = true;
      do {
         string cname = this._capitalizeFirstLetter (name);
         PropertyInfo pi = objType.GetProperty (cname, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!; 
         if (pi == null) return false;

         object value = this.readAttributeValue (pi.PropertyType, ref reader);
         pi.SetValue (obj, value);
      } while (false);

      return success;
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
               JsonTokenType.True => reader.GetBoolean ()!,
               JsonTokenType.False => reader.GetBoolean ()!,
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
            return null!;
      }

      if (value != null && objType.IsEnum) 
         value = Enum.Parse (objType, (string)value);

      return value!;
   }

   //Type _getExactPropertyType (Type propertyType)
   //  => Nullable.GetUnderlyingType (propertyType) ?? propertyType;

   string _capitalizeFirstLetter (string str)
     => char.ToUpper (str[0]) + str.Substring (1);
   #endregion Implement
}