using System.Collections.Generic;

namespace FChassis.Data.IO { 
   public class TreeNode {
      public object? content;
      public List<TreeNode>? children = new ();
   }

   public class TreeNodes : List<TreeNode> { }

   //using Nodes = LinkedList<object>;
   public abstract class File {
      // Field
      public TreeNodes nodes = new ();
   }

   //-----------------------------------------------------------------------------
   public abstract class FileRead : File {
      // Overridable
      public abstract bool Read (string path);
      public abstract object CreateObject (string name);
   }

   //-----------------------------------------------------------------------------
   public abstract class FileWrite : File {
      // Overridable
      public abstract bool Write (string path);
   }
}