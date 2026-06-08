using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Text = HisouSangokushiZero2_1_Uno.Data.Language.Text;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal record WinCondData(Brush Brush,ECountry Country,string WinCondText);
internal sealed partial class WinCond:UserControl {
  private const double itemsRepeaterWidth = 750;
  private const double itemsRepeaterHeight = 350;
  private static UIElement? parent = null;
  internal WinCond() {
    InitializeComponent();
    MyInit(this);
    void MyInit(WinCond page) {
      AttachEvent(page);
      SetUIElements();
      void AttachEvent(WinCond page) {
        page.SizeChanged += (_,_) => parent?.MyApply(ResizeElem);
        void ResizeElem(UIElement parent) {
          double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize);
          double contentWidth = RenderSize.Width / scaleFactor - 5;
          ContentPanel.Width = contentWidth;
          ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
          ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),ContentPanel.Children.Sum(v => v.RenderSize.Height) * (scaleFactor - 1));
        }
      }
      void SetUIElements() {
        List<TextBlock> WinCondScenarioNames = [WinCondScenarioName1,WinCondScenarioName2];
        List<ItemsRepeater> winCondItemsRepeaters = [WinCondItemsRepeater1, WinCondItemsRepeater2];
        List<ScrollViewer> winCondScrolls = [WinCondScroll1, WinCondScroll2];
        WinCondScenarioNames.ForEachWithIndex((v,i) => v.Text = ScenarioBase.GetScenarioId(i)?.Value);
        winCondItemsRepeaters.ForEachWithIndex((v,i) => v.ItemsSource = ScenarioBase.GetScenarioId(i)?.MyPipe(ScenarioBase.GetScenarioData)?.MyPipe(scenario => GetWinCondListData(scenario)));
        winCondScrolls.Zip(winCondItemsRepeaters).ToList().ForEach(v => v.First.SizeChanged += (_, _) => {
          v.Second.Width = v.First.RenderSize.Width;
          v.First.UpdateLayout();
        });
        List<WinCondData> GetWinCondListData(ScenarioData scenario) {
          return [.. scenario.CountryMap.Select(ToCountryListItem)];
          WinCondData ToCountryListItem(KeyValuePair<ECountry,CountryData> countryInfo) {
            WinCondMessage? maybeWinCondMessage = scenario.WinConditionMap.GetValueOrDefault(countryInfo.Key)?.Messages;
            string?[] maybeWinCondText = [maybeWinCondMessage?.Basic?.MyPipe(v => string.Join('＆',v)) ?? Text.NoBasicWinCondText(),maybeWinCondMessage?.Extra?.MyPipe(v => string.Join(' ',v))];
            string winCondMessage = maybeWinCondMessage.MyPipe(v => string.Join('\n',maybeWinCondText.MyNonNull()));
            return new WinCondData(countryInfo.Value.ViewColor.ToBrush(),countryInfo.Key,winCondMessage);
          }
        }
      }
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
}