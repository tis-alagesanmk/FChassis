using System.Collections.Generic;

namespace FChassis.Data.IO {
   //-----------------------------------------------------------------------------
   public class TreeNodes : List<TreeNode> { }

   //-----------------------------------------------------------------------------
   public class TreeNode {
      public object? content;
      public TreeNodes? children = new ();
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
      public abstract bool Read (string path);
      public abstract TreeNode GetObject (TreeNode node, string name);
   }

   //-----------------------------------------------------------------------------
   /// <summary>
   /// </summary>
   public abstract class FileWrite : File {
      // Overridable
      public abstract bool Write (string path);
   }
}