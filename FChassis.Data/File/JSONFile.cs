using FChassis.Data.Model;

using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Reflection;
using System.Linq;

namespace FChassis.Data.IO;
/// <summary>
/// </summary>
public class JSONFileWrite : FileWrite {
   #region Method
   public override bool Write (string path) {
      using var fileStream =  System.IO.File.Create(path);
      JsonWriterOptions options = new () { Indented = true };
      using Utf8JsonWriter writer = new (fileStream, options);

      writer.WriteStartObject ();
      foreach(var node in this.nodes) { 
         if (!this.writeObject (writer, node))
            return false;
      }

      writer.WriteEndObject ();
      fileStream.Flush ();
      return true;
   }
   #endregion Method

   #region Implement
   bool writeObject (Utf8JsonWriter writer, TreeNode node) {
      bool success = false;

      string? name = node.content as string;
      object obj = node.content! as object;
      if (name != null)
         writer.WriteStartObject (name);
      else {
         writer.WriteStartObject (obj.GetType ().Name);
         this.writeObjectAttribute (writer, obj);
      }

      foreach (var childNode in node.children!)
         this.writeObject (writer, childNode);

      writer.WriteEndObject ();

      success = true;
      return success;
   }

   void writeObjectAttribute (Utf8JsonWriter writer, object obj) {
      List<Type> types = Data.Reflection.Object.GetTypeList (obj.GetType(), typeof(ObservableObject));
      foreach (Type type in types)
         this.writeObjectTypeAttributes (writer, obj, type);
   }

   void writeObjectTypeAttributes (Utf8JsonWriter writer, object obj, Type type) {
      writer.WriteString ("date", DateTimeOffset.UtcNow);

      var fields = type.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      foreach (FieldInfo f in fields) {
         var p = f.GetCustomAttribute<Prop> ()!;
         if (p == null) continue;

         object attr = f?.GetValue (obj)!;
         if (attr == null) continue;

         Type s = attr.GetType ();
         writer.WriteString (f?.Name!, attr.ToString());
      }
   }
   #endregion Implement
}

//-----------------------------------------------------------------------------
/// <summary>
/// </summary>
class JSONFileRead : FileRead {
   #region Method
   public override bool Read (string path) {
      ReadOnlySpan<byte> jsonReadOnlySpan = System.IO.File.ReadAllBytes (path);
      var reader = new Utf8JsonReader (jsonReadOnlySpan);

      foreach (var node in this.nodes)
         return this.readObject (node, reader);

      return true;
   }

   public override object CreateObject (string name) {
      return null!;
   }
   #endregion Method

   #region Implement
   bool readObject (TreeNode node, Utf8JsonReader reader) {
      bool success = true;

      Type objType = null!;
      string name, value;
      object obj = null!;
      JsonTokenType tokenType;
      TreeNode childNode;

      do {
         while (reader.Read ()) {
            tokenType = reader.TokenType;
            name = reader.GetString ()!;
            switch (tokenType) {
               case JsonTokenType.StartObject:
                  success = reader.Read ();
                  if (!success)
                     break;

                  name = reader.GetString ()!; // Object Name
                  obj = this.CreateObject (name);
                  success = obj != null;
                  if (success == true) {
                     childNode = new () { content = obj! };
                     node?.children?.Add (childNode);
                     objType = obj!.GetType ();
                  }
                  break;

               case JsonTokenType.EndObject:
                  return true; // Completed object read

               case JsonTokenType.PropertyName:
                  success = reader.Read ();
                  if (!success)
                     break;

                  value = reader.GetString ()!; // Property Value
                  success = this.readAttribute (obj!, objType!, name, value);
                  break;
            }

            if (!success)
               break;
         }

      } while (false);

      success = false;
      return success;
   }

   bool readAttribute (object obj, Type type, string name, string value) {
      FieldInfo fi = type.GetField (name)!;
      if(fi == null)
         return false;

      fi.SetValue(obj, value);
      return true;
   }
   #endregion Implement
   
}