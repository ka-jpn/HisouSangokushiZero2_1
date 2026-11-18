using Uno.UI.Composition.Skia;
using Uno.UI.Hosting;
namespace HisouSangokushiZero2_1_Uno.Platforms.WebAssembly;
public class Program{
  public static async Task Main(string[] args){
    Uno.UI.FeatureConfiguration.Font.DefaultTextFontFamily = "ms-appx:///Assets/Fonts/SourceHanSansJP-Medium.otf";//add code:set defaultFont
    var host = UnoPlatformHostBuilder.Create()
      .App(() => new App())
      .FontProvider(SkiaBackend.CreateFontProvider(false)) 
      .UseWebAssembly()
      .Build();
    await Uno.UI.Xaml.Media.FontFamilyHelper.PreloadAsync("ms-appx:///Assets/Fonts/SourceHanSansJP-Medium.otf",new(500),Windows.UI.Text.FontStretch.Normal,Windows.UI.Text.FontStyle.Normal);
    await Uno.UI.Xaml.Media.FontFamilyHelper.PreloadAsync("ms-appx:///Assets/Fonts/SourceHanSansJP-Bold.otf",new(700),Windows.UI.Text.FontStretch.Normal,Windows.UI.Text.FontStyle.Normal);
    await host.RunAsync();
  }
}