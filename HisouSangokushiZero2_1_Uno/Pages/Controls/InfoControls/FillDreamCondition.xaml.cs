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
internal record FillDreamConditionData(Brush Brush,ECountry Country,string FillDreamConditionText);
internal sealed partial class FillDreamCondition:UserControl {
  private const double itemsRepeaterWidth = 750;
  private const double itemsRepeaterHeight = 350;
  private static UIElement? parent = null;
  internal FillDreamCondition() {
    InitializeComponent();
    MyInit(this);
    void MyInit(FillDreamCondition page) {
      AttachEvent(page);
      SetUIElements();
      void AttachEvent(FillDreamCondition page) {
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
        List<TextBlock> fillDreamConditionScenarioNames = [FillDreamConditionScenarioName1,FillDreamConditionScenarioName2];
        List<ItemsRepeater> fillDreamConditionItemsRepeaters = [FillDreamConditionItemsRepeater1, FillDreamConditionItemsRepeater2];
        List<ScrollViewer> fillDreamConditionScrolls = [FillDreamConditionScroll1, FillDreamConditionScroll2];
        fillDreamConditionScenarioNames.ForEachWithIndex((v,i) => v.Text = ScenarioBase.GetScenarioId(i)?.Value);
        fillDreamConditionItemsRepeaters.ForEachWithIndex((v,i) => v.ItemsSource = ScenarioBase.GetScenarioId(i)?.MyPipe(ScenarioBase.GetScenarioData)?.MyPipe(scenario => GetFillDreamConditionListData(scenario)));
        fillDreamConditionScrolls.Zip(fillDreamConditionItemsRepeaters).ToList().ForEach(v => v.First.SizeChanged += (_, _) => {
          v.Second.Width = v.First.RenderSize.Width;
          v.First.UpdateLayout();
        });
        List<FillDreamConditionData> GetFillDreamConditionListData(ScenarioData scenario) {
          return [.. scenario.CountryMap.Select(ToCountryListItem)];
          FillDreamConditionData ToCountryListItem(KeyValuePair<ECountry,CountryData> countryInfo) {
            FillDreamConditionMessage? maybeFillDreamConditionMessage = scenario.FillDreamConditionMap.GetValueOrDefault(countryInfo.Key)?.Messages;
            string?[] maybeFillDreamConditionText = [maybeFillDreamConditionMessage?.Basic?.MyPipe(v => string.Join('＆',v)) ?? Text.NoFillDreamConditionText(),maybeFillDreamConditionMessage?.Extra?.MyPipe(v => string.Join(' ',v))];
            string fillDreamConditionMessage = maybeFillDreamConditionMessage.MyPipe(v => string.Join('\n',maybeFillDreamConditionText.MyNonNull()));
            return new FillDreamConditionData(countryInfo.Value.ViewColor.ToBrush(),countryInfo.Key,fillDreamConditionMessage);
          }
        }
      }
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
}