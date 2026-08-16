using HisouSangokushiZero2_1_Uno;
using Uno.UI.Hosting;
using Uno.UI.Xaml.Media;
_ = FontFamilyHelper.PreloadAsync("ms-appx:///Assets/Fonts/SourceHanSansJP-Medium.woff2",new(500),Windows.UI.Text.FontStretch.Normal,Windows.UI.Text.FontStyle.Normal);
_ = FontFamilyHelper.PreloadAsync("ms-appx:///Assets/Fonts/SourceHanSansJP-Bold.woff2",new(700),Windows.UI.Text.FontStretch.Normal,Windows.UI.Text.FontStyle.Normal);
var host = UnoPlatformHostBuilder.Create()
    .App(() => new App())
    .UseWebAssembly()
    .Build();
await host.RunAsync();
