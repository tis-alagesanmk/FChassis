using System.Collections.Generic;

namespace FChassis.Data.IO;
using Nodes = LinkedList<object>;
abstract class File {
   // Overridable
   protected abstract bool Write (string path);
   protected abstract bool Read (string path);

   // Field
   public Nodes nodes = new ();
}