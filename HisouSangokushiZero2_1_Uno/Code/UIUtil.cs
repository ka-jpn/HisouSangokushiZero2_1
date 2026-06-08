using HisouSangokushiZero2_1_Uno.MyUtil;
using HisouSangokushiZero2_1_Uno.Pages;
using HisouSangokushiZero2_1_Uno.Pages.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code; 
internal static class UIUtil {
  internal enum ViewMode { fit, fix };
  internal enum PersonViewSortMode { Country_Role_BirthYear, Rank_BirthYear, BirthYear, DeathYear };
  internal const double frameRefreshRate = 60;
  internal const double nextStepDelaySeconds = 1 / frameRefreshRate;
  internal const double infoFrameWidth = 40;
  internal const double infoButtonHeight = 40;
  internal const double fixModeMaxWidth = 1000;
  internal const double postFrameWidth = 1;
  internal const double personRankFontScale = 1.25;
  internal const double personNameFontScale = 1.3;
  internal const double personPutFontScale = 1.6;
  internal const int capitalPieceRowNum = 3;
  internal const int capitalPieceColumnNum = 5;
  internal const int capitalPieceCellNum = capitalPieceRowNum * capitalPieceColumnNum;
  internal static readonly Size mapSize = new(1000,875);
  internal static readonly Point mapGridCount = new(9,10);
  internal static readonly Size areaSize = new(102,77);
  internal static readonly CornerRadius areaCornerRadius = new(15);
  internal static readonly Size personPutSize = new(50,35);
  internal static readonly double countryPersonPutPanelHeight = personPutSize.Height * 4 + postFrameWidth * 2 * 2 + BasicStyle.textHeight;
  internal static readonly Color landRoadColor = new(150,120,120,50);
  internal static readonly Color waterRoadColor = new(150,50,50,150);
  internal static readonly Color grayoutColor = new(100,100,100,100);
  internal static readonly Color transparentColor = new(0,0,0,0);
  internal static readonly Color dataBackColor = new(255,150,150,150);
  internal static readonly Thickness dataMargin = new(1);
  internal static SKSvg? mapSvg = null;
  internal static SKSvg? armySvg = null;
  internal static ViewMode viewMode = ViewMode.fix;
  internal static List<Action> ChangeScaleActions = [];
  internal static List<Action> SwitchViewModeActions = [];
  internal static List<Action> SaveGameActions = [];
  internal static List<Action> LoadGameActions = [];
  internal static List<Action> InitGameActions = [];
  internal static Task loadFontTask = new(() => { });
  internal static Task loadMapTask = new(() => { });
  internal static TextBlock[] CreateWithShadow(Func<TextBlock> createTextBlock, double shadowWidth, Windows.UI.Color shadowColor, int shadowCount = 8) {
    return [.. CreateShadowTranslates(shadowWidth, shadowCount).Select(margin => createTextBlock().MyApply(v => v.RenderTransform = margin).MyApply(v => v.Foreground = shadowColor)), createTextBlock()];
    static List<TranslateTransform> CreateShadowTranslates(double shadowWidth, int shadowCount) => [.. Enumerable.Range(0, shadowCount).Select(i => 2 * Math.PI / shadowCount * i).Select(angle => new TranslateTransform() { X = Math.Cos(angle) * shadowWidth, Y = Math.Sin(angle) * shadowWidth })];
  }
  internal static double SolveMapScale(double height,Windows.Foundation.Size size) {
    double initScale = GetScaleFactor(size);
    return mapSize.Width * initScale - size.Width < 0.1 ? initScale : (CaclScale() - 0.0002);
    double CaclScale() {
      return Average(Enumerable.Range(0, 30).Aggregate((initScale / 4, initScale * 4), (before, _) => Average(before).MyPipe(v => GetDeviation(v) > 0 ? (before.Item1, v) : (v, before.Item2))));
      double Average((double left, double right) range) => (range.left + range.right) / 2;
      double GetDeviation(double scale) => Math.Ceiling(mapSize.Height * scale) + Math.Ceiling(StateInfo.baseContentHeight * StateInfo.CookScaleY(scale)) - height;
    }
  }
  internal static double CalcTextWidthForFullWidth(string str) => str.Length - str.Count("0123456789-.() ".Contains) * 0.4 - str.Count(" ".Contains) * 0.8;
  internal static double CalcDataListElemWidth(double textlength) => BasicStyle.fontsize * textlength + dataMargin.Left + dataMargin.Right;
  internal static void SwitchViewMode() {
    viewMode = viewMode == ViewMode.fix ? ViewMode.fit : ViewMode.fix;
    SwitchViewModeActions.ForEach(v=>v());
#if __WASM__
    Uno.Foundation.WebAssemblyRuntime.InvokeJS($"window.parent.{viewMode}();");
#endif
  }
  internal static double GetScaleFactor(Windows.Foundation.Size size) => Math.Max(size.Width / mapSize.Width,size.Height / mapSize.Height);
  internal static void ReverseVisibility(UIElement elem) => elem.Visibility = ToVisibility(elem.Visibility.IsHidden());
  internal static void SetVisibility(UIElement elem,bool isShow) => ToVisibility(isShow).MyApply(v=>elem.Visibility = v,v=>elem.Visibility != v);
  internal static Visibility ToVisibility(bool isShow) => isShow ? Visibility.Visible : Visibility.Collapsed;
  internal static void SaveGame() => SaveGameActions.ForEach(v => v());
  internal static void LoadGame() => LoadGameActions.ForEach(v => v());
  internal static void InitGame() => InitGameActions.ForEach(v => v());
  internal static double GetContentMaxWidth() => viewMode == ViewMode.fix ? fixModeMaxWidth : double.PositiveInfinity;
}
internal static class 汎用拡張メソッド {
		internal static T MySetChildren<T>(this T panel,List<UIElement> elements) where T : Panel => panel.MyApply(v => v.Children.Clear()).MyApply(v => elements.ForEach(v.Children.Add));
		internal static T MySetChild<T>(this T control,UIElement element) where T : ContentControl => control.MyApply(v => v.Content=element);
		internal static Border MySetChild(this Border border,UIElement element) => border.MyApply(v => v.Child=element);
  internal static SolidColorBrush ToBrush(this Color color) => new(Windows.UI.Color.FromArgb(color.A,color.R,color.G,color.B));
}