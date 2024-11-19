namespace FChassis.Core.Reflection;
public class Object {
   static public List<Type> GetTypeList (Type type, Type baseType) {
      List<Type> types = new List<Type> ();
      Type _type = type;
      while (true) {
         types.Add (_type);
         if (baseType == null! || _type == baseType)
            break;

         _type = _type.BaseType!;
      }

      // Base to Derived classes
      types.Reverse ();
      return types;
   }
}