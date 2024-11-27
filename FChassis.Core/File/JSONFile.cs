using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace FChassis.Core.File;
/// <summary></summary>
public class JSONFileWrite : FileWrite {
   #region Method
   public bool Write (string path, string objName, object obj) {
      this.rootNode.AddObject (objName, obj);
      return Write (path);
   }

   public override bool Write (string path) {
      using var fileStream = System.IO.File.Create (path);
      JsonWriterOptions options = new () { Indented = true };
      using Utf8JsonWriter writer = new (fileStream, options);

      writer.WriteStartObject ();
      foreach (var pair in this.rootNode.children)
         if(!this.writeObject(writer, pair.Value, pair.Key!))
            return false;

      writer.WriteEndObject ();
      fileStream.Flush ();
      return true;
   }
   #endregion Method

   #region Implement
   bool writeObject (Utf8JsonWriter writer, TreeNode childNode, string name) {
      if (childNode.obj != null) { // Object node
         if (!this.writeObject (writer, childNode.obj!, name))
            return false;
      }

      // Otherwise Container node
      foreach (var pair in childNode.children)
         if (!this.writeObject (writer, pair.Value, pair.Key!))
            return false;

      return true;
   }

   bool writeObject (Utf8JsonWriter writer, object obj, string name) {

      writer.WriteStartObject (name);

      Type elementType = null!,
           objType = obj.GetType ();

      if (TreeNode.IsListType (obj, objType!, ref elementType)
            || objType.IsArray) {
         if (objType.IsArray)
            elementType = objType.GetElementType ()!;

         if (!this.writeArrayObject (writer, obj!, elementType, name)) // list or array 
            return false;
      } else
         this.writeObjectAttributes (writer, obj, objType!);

      writer.WriteEndObject ();
      return true;
   }

   // For List and Array
   bool writeArrayObject (Utf8JsonWriter writer, dynamic iteratable, Type elementType, string name) {
      writer.WriteStartArray (name);

      foreach (var childObj in iteratable) {
         writer.WriteStartObject ();
         if (!writeObjectAttributes (writer, childObj, elementType))
            return false;

         writer.WriteEndObject ();
      }

      writer.WriteEndArray ();
      return true;
   }

   bool writeObjectAttributes (Utf8JsonWriter writer, object obj) {
      List<Type> types = Reflection.Object.GetTypeList (obj.GetType (), typeof (ObservableObject));
      foreach (Type type in types)
         if(!this.writeObjectAttributes (writer, obj, type))
            return false;

      return true;
   }

   bool writeObjectAttributes (Utf8JsonWriter writer, object obj, Type type) {
      Type attrType, attrElementType = null!;
      var fields = type.GetFields (BindingFlags.NonPublic | BindingFlags.Instance);
      foreach (FieldInfo fi in fields) {
         var p = fi.GetCustomAttribute<ObservablePropertyAttribute> ()!;
         if (p == null) 
            continue;

         object attrObj = fi?.GetValue (obj)!;
         if (attrObj == null)
            return setError ($"Property {fi!.Name} not found");

         attrType = attrObj.GetType ();
         if (TreeNode.IsUserDefinedClass (attrElementType) &&
            (TreeNode.IsListType (attrObj, attrType!, ref attrElementType!)|| attrType.IsArray)) {
            if (!writeObject (writer, attrObj!, fi!.Name))
               return false;
         } else
            this.writeObjectAttribute (writer, fi?.Name!, attrObj);
      }
      return true;
   }

   void writeObjectAttribute (Utf8JsonWriter writer, string name, object value) {
      Type dataType = value.GetType ();
      //string type = dataType.ToString ();
      switch (Type.GetTypeCode(dataType)) {
         case TypeCode.String:
            writer.WriteString (name, value.ToString ());
            break;

         case TypeCode.Int32:
            writer.WriteNumber(name, (Int32)value);
            break;

         case TypeCode.Double:
            writer.WriteNumber (name, (double)value);
            break;

         case TypeCode.Decimal:
            writer.WriteNumber (name, (decimal)value);
            break;

         case TypeCode.Boolean:
            writer.WriteBoolean (name, (bool)value);
            break;
         case TypeCode.Object:
            if (dataType.IsEnum)
               writer.WriteString (name, value.ToString ());
            else if (dataType.IsArray && !dataType.IsClass) {
               if (_isUserDefinedClassArray (dataType))
                  return;

               this.writeArray (writer, name, (Array)value);
            } else
            this.writeObject (writer, value,name);
            break;

         default:
            //if(dataType.IsEnum)
            //   writer.WriteString (name, value.ToString());
            //else if (dataType.IsArray) {
            //   if(_isUserDefinedClassArray(dataType)) 
            //      return; 

            //   this.writeArray (writer, name, (Array)value);
            //}
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

      Type arrayType = array.GetType ();
      Type elementType = arrayType.GetElementType ()!;

      foreach (object element in array) {
         switch (Type.GetTypeCode (elementType)) {
            case TypeCode.String:
               writer.WriteStringValue (element.ToString ());
               break;

            case TypeCode.Double:
               writer.WriteNumberValue ((double)element);
               break;
            default:
               if (elementType.IsEnum) 
                    writer.WriteStringValue (element.ToString ());
               break;
         }
      }
        
      writer.WriteEndArray ();
   }
   #endregion Implement
}

//-----------------------------------------------------------------------------
/// <summary></summary>
public class JSONFileRead : FileRead {
   #region Method

   public bool Read (string path, object obj, string objName) {
      this.rootNode.AddObject (objName, obj);
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

      return this.readObject (this.rootNode, ref reader, null!);
   }
   #endregion Method

   #region Implement
   /// <summary></summary>
   /// <param name="targetObj">This can be either TreeNode or object</param>
   /// <param name="reader"></param>
   /// <param name="name"></param>
   /// <returns></returns>
   bool readObject (object targetObj, ref Utf8JsonReader reader, string name) {
      bool success = true;

      object obj = targetObj;
      if(targetObj is TreeNode)
         obj = ((TreeNode)targetObj).obj!;
      
      object childObj = null!;
      Type elementType = null!,
           objType = obj?.GetType()!;
      TreeNode.Type type = TreeNode.Type.obj;

      if (obj != null) {
         if (TreeNode.IsListType (obj!, objType!, ref elementType))
            type = TreeNode.Type.list;
         else if (objType.IsArray)
            type = TreeNode.Type.array;
      }

      do {
          while (success && reader.Read ()) {
            switch (reader.TokenType) {
               case JsonTokenType.StartObject:
                  if (!readObject (targetObj, obj!, objType, elementType, type, name, ref reader))
                     return false;
                  break;

               case JsonTokenType.EndObject:
               case JsonTokenType.EndArray:
                  return true;

               case JsonTokenType.StartArray:
                  if (obj != null) {
                     if(!this.readArray (obj, objType, elementType, type, name, ref reader))
                        return false;
                  } else {
                     childObj = TreeNode.GetObject (targetObj, objType, name);
                     if (childObj == null)
                        return setError ($"Object '{name}' not found");
                  }

                  Debug.Assert (childObj != null);
                  if (!this.readObject (childObj, ref reader, name))
                     return false;
                  childObj = null!;
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

   bool readObject (object targetObj, object obj, Type objType, Type elementType, 
                    TreeNode.Type type, string name, ref Utf8JsonReader reader) {
      object childObj;
      if (obj != null && objType.IsArray) {
         childObj = TreeNode.CreateElement (obj!, objType, elementType, type);
         if (childObj == null)
            return setError ($"Element Object for '{name}'create failed");
      } else {
         childObj = TreeNode.GetObject (targetObj, objType, name);
         if (childObj == null)
            return setError ($"Object '{name}' not found");
      }

      if (!this.readObject (childObj, ref reader, name))
         return false;

      return true;
   }

   bool readArray (object obj, Type objType, Type elementType, 
                   TreeNode.Type type, string name, ref Utf8JsonReader reader) {
      object childObj = null!;
      string cname = this._capitalizeFirstLetter (name);
      PropertyInfo? pi = objType.GetProperty (cname, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      if (pi == null)
         return setError ($"Property '{cname}' not found");

      if (pi.PropertyType.IsArray) {
         // Is UserDefined class array
         elementType = pi.PropertyType.GetElementType ()!;
         if (TreeNode.IsUserDefinedClass (elementType)) {
            childObj = TreeNode.GetObject (obj, objType, name);
            if (childObj == null)
               return setError ($"Array '{cname}' Object not found");
         } else {
            if (!this.readElements (obj!, type, pi, elementType, ref reader))
               return false;
         }
      }

      return true;
   }

   bool readElements (object obj, TreeNode.Type type, PropertyInfo pi, 
                      Type elementType, ref Utf8JsonReader reader) {
      Type listType = typeof (List<>).MakeGenericType (elementType);
      object list = Activator.CreateInstance (listType)!;
      var addMethod = listType.GetMethod ("Add")!;

      object value;
      while (reader.Read ()) {
         switch (reader.TokenType) {
            case JsonTokenType.EndArray:
               switch (type) {
                  case TreeNode.Type.array:
                     var toArrayMethod = listType.GetMethod ("ToArray")!;
                     Array array = (Array)toArrayMethod.Invoke (list, null)!;
                     pi.SetValue (obj, array);
                     break;

                  case TreeNode.Type.list:
                     pi.SetValue (obj, list);
                     break;

                  default:
                     Debug.Assert (false);
                     break;
               }
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

   bool readAttribute (object obj, Type attrType, string name, ref Utf8JsonReader reader) {
      object value;
      do {
         string cname = this._capitalizeFirstLetter (name);
         PropertyInfo pi = attrType.GetProperty (cname, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!; 
         if (pi == null) 
            return setError ($"Property '{cname}' not found"); 

         value = this.readAttributeValue (pi.PropertyType, ref reader);
         if(value == null)
            return false;

         pi.SetValue (obj, value);
      } while (false);

      return true;
   }

   object readAttributeValue (Type attrType, ref Utf8JsonReader reader) {
      Type dataType;
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
            dataType = _getExactPropertyType (attrType);
            value = dataType switch {
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

      if (attrType.IsEnum) 
         value = Enum.Parse (attrType, (string)value);

      return value!;
   }

   Type _getExactPropertyType (Type propertyType)
     => Nullable.GetUnderlyingType (propertyType) ?? propertyType;

   string _capitalizeFirstLetter (string str)
     => char.ToUpper (str[0]) + str.Substring (1);
   #endregion Implement
}