using System.Runtime.InteropServices.JavaScript;
namespace HisouSangokushiZero2_1_Uno.Code;
public partial class JsInterop {
  [JSImport("globalThis.fixBody")] public static partial void FixBody();
  [JSImport("globalThis.fitBody")] public static partial void FitBody();
 }