using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using FChassis.Data.Model;
using System.Reflection;

namespace FChassis.Data.IO;
using Node = LinkedListNode<object>;
using Nodes = LinkedList<object>;

class JSONFile : File {
   protected override bool Write (string path) {
      using var fileStream =  System.IO.File.Create(path);
      using Utf8JsonWriter writer = new (fileStream);

      writer.WriteStartObject ("Configuration");
      if (!this.writeNodes (writer, this.nodes))
         return false;

      writer.WriteEndObject ();
      fileStream.Flush ();
      return true;
   }

   protected override bool Read (string path) {
      return false;
   }

   bool writeNodes (Utf8JsonWriter writer, Nodes nodes) {
      foreach (Node node in nodes!)
         if(!this.writeNode (writer, node))
               return false;

      return true;
   }

   bool writeNode (Utf8JsonWriter writer, Node node) {
      bool success = false;
      writer.WriteStartObject (node.Value.GetType ().Name);
      this.writeNodeArrtribute (writer, node.Value);

      foreach (Node childNode in node.List!)
         this.writeNode (writer, childNode);

      writer.WriteEndObject ();

      success = true;
      return success;
   }

   bool writeNodeArrtribute (Utf8JsonWriter writer, object obj) {
      bool success = false;
      writer.WriteString ("date", DateTimeOffset.UtcNow);

      List<Type> types = Data.Reflection.Object.GetTypeList (obj.GetType(), typeof(ObservableObject));
      foreach (Type type in types) {
         if (!this.writeNodeArrtribute (writer, obj, type))
            return false;
      }
      
      success = true;
      return success;
   }

   bool writeNodeArrtribute (Utf8JsonWriter writer, object obj, Type type) {
      bool success = false;
      writer.WriteString ("date", DateTimeOffset.UtcNow);

      var fields = type.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      foreach (FieldInfo f in fields) {
         var p = f.GetCustomAttribute<Prop> ()!;
         if (p == null) continue;

         object attr = type?.GetField (f.Name)?.GetValue (obj)!;
         writer.WriteString (f.Name, attr.ToString());
      }

      return success;
   }
}