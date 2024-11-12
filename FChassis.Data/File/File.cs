using System.Collections.Generic;

namespace FChassis.Data.IO;
using Nodes = LinkedList<object>;
abstract class File {
   // Field
   public Nodes nodes = new ();
}

abstract class FileRead : File {
   // Overridable
   protected abstract bool Read (string path);

}

abstract class FileWrite: File {
   // Overridable
   protected abstract bool Write (string path);
}