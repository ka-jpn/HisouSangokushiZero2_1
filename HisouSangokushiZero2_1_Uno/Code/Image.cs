using System;
using System.Collections.Generic;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Image {
  internal static Dictionary<string,ImageSource> imageCache = [];
  internal static ImageSource GetSvgImageSource(string imageName,double rasterizePixelWidth,double rasterizePixelHeight) {
    return imageCache.GetValueOrDefault($"{imageName}.svg") ?? new SvgImageSource(new Uri($"ms-appx:///Assets/Svg/{imageName}.svg")).MyApply(v=>v.RasterizePixelWidth=rasterizePixelWidth).MyApply(v=>v.RasterizePixelHeight=rasterizePixelHeight).MyApply(v=>imageCache.Add($"{imageName}.svg",v)) ?? new SvgImageSource();
  }
}