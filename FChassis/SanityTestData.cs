using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Text.Json;

namespace FChassis;

/// <summary>
/// This structure holds the information for each sanity test.
/// </summary>
public partial class SanityTestData : ObservableObject {
   public SanityTestData () { }
   [ObservableProperty] string fxFileName;
   [ObservableProperty] bool toRun;
   [ObservableProperty] MCSettings mCSettings = new MCSettings ();

   #region Data Members
   //JsonSerializerOptions mJSONWriteOptions, mJSONReadOptions;
   #endregion

   #region JSON read/write utilities
   /// <summary>
   /// This method deserializes the sanity test suite and creates SanityTestData instance
   /// </summary>
   /// <param name="element">The complete path to the sanity test suite (JSON file)</param>
   /// <returns>Sanity Data Instance</returns>
   /// <exception cref="FileNotFoundException">Throws this exception if the file is not found</exception>
   public SanityTestData LoadFromJsonElement (JsonElement element) {
      
      string filePath = element.GetProperty (nameof (FxFileName)).GetString ();
      FChassis.MCSettings mcSettings = new ();
      
      Core.File.JSONFileRead reader = new ();
      reader.Read (filePath, mcSettings);

      return new SanityTestData {
         FxFileName = filePath,
         ToRun = element.GetProperty (nameof (ToRun)).GetBoolean (),
         MCSettings = mcSettings
      };
   }
   #endregion
}
