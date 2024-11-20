using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.Generic;
using System.Text.Json;
using System.Reflection;
using System;

namespace FChassis.Core.File;
/// <summary>
/// </summary>
public class JSONFileWrite : FileWrite {
   #region Method
   public bool Write (string path, object obj, string objName = "Configuration") {
      this.node.set (obj, objName);
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
         if (p == null) continue;

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
            break;
      }
   }
   #endregion Implement
}

//-----------------------------------------------------------------------------
/// <summary>
/// </summary>
public class JSONFileRead : FileRead {
   #region Method
   public bool Read (string path, object obj, string objName = "Configuration") {
      this.node.set (obj, objName);
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
   bool readObject (TreeNode node, ref Utf8JsonReader reader, TreeNode parentNode = null!) {
      bool success = true;

      string name = "";
      TreeNode childNode;
      object childObj, obj = node?.content!;
      Type objType = obj?.GetType()!;

      do {
         while (success && reader.Read ()) {
            switch (reader.TokenType) {
               case JsonTokenType.StartObject:
                  if (parentNode != null! && parentNode.IsArray) {
                     childObj = Activator.CreateInstance(parentNode.ElementTye)!;
                     if (childObj == null)
                        continue;

                     childNode = new () { content = childObj };
                     parentNode!.children.Add (childNode!);

                     childNode.set(childNode.content!);
                  } else {
                     childNode = this.GetObject (node!, name);
                     if (childNode == null || childNode == node)
                        continue;
                  }

                  success = this.readObject (childNode!, ref reader, parentNode!);
                  break;

               case JsonTokenType.EndObject:
                  return true; // Object read completed


               case JsonTokenType.EndArray:
                  return true; // Array read completed

               case JsonTokenType.StartArray:
                  this.readObject (null!, ref reader, node!);
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

   bool readAttribute (object obj, Type objType, string name, ref Utf8JsonReader reader) {
      bool success = true;
      do {
         string cname = _capitalizeFirstLetter (name);
         PropertyInfo pi = null!;

         try {
            pi = objType.GetProperty (cname, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
         } catch (Exception) { }

         if (pi == null)
            return false;

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
               var dataType = _getExactPropertyType(pi.PropertyType).ToString();
               value = dataType switch {
                  "System.Int32"    => reader.GetInt32 ()!,
                  "System.UInt32"   => reader.GetUInt32 ()!,
                  "System.Double"   => reader.GetDouble ()!,
                  "System.Decimal"  => reader.GetDecimal ()!,
                  _ => null!
               };
               break;

            default:
               return false;
         }

         if (value != null) {
            if(pi.PropertyType.IsEnum) 
               value = Enum.Parse (pi.PropertyType, (string)value);

            pi.SetValue (obj, value);
         }
      } while (false);

      #region Local
      string _capitalizeFirstLetter (string str)
         => char.ToUpper (str[0]) + str.Substring (1);

      Type _getExactPropertyType (Type propertyType) 
         => Nullable.GetUnderlyingType (propertyType) ?? propertyType;
 
      #endregion Local
      return success;
   }
   #endregion Implement
}