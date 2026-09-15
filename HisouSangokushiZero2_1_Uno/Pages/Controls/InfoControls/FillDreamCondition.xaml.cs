using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using HisouSangokushiZero2_1_Uno.Pages.Common;
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
  internal FillDreamCondition() {
    InitializeComponent();
    MyInit(this);
    void MyInit(FillDreamCondition page) {
      AttachEvent(page);
      SetUIElements();
      void AttachEvent(FillDreamCondition page) {
        page.SizeChanged += (_, _) => ResizeElem();
        FillDreamConditionScroll.SizeChanged += (_, _) => FillDreamConditionItemsRepeater.Width = FillDreamConditionScroll.RenderSize.Width;
        void ResizeElem() {
          double contentWidth = RenderSize.Width - 5;
          ContentPanel.Width = contentWidth;
          ScenarioComboBox.ItemContainerStyle = new Style(typeof(ComboBoxItem)).MyApply(style=>style.Setters.Add(new Setter(FontSizeProperty,BasicStyle.fontsize*UIUtil.GetInfoTextZoomFactor()*Game.uiScale)));
        }
      }
      void SetUIElements() {
        ScenarioBase.GetScenarioIds().Select(v => v.Value).ToList().ForEach(ScenarioComboBox.Items.Add);
        LoadScenarioData(0);
        ScenarioComboBox.SelectedIndex = 0;
        ScenarioComboBox.SelectionChanged += (_, _) => LoadScenarioData(ScenarioComboBox.SelectedIndex);
        void LoadScenarioData(int scenarioNo) {
          ScenarioData? maybeScenario = GetScenarioData(scenarioNo);
          UpdateFillDreamConditionItemsRepeater(maybeScenario);
        }
        ScenarioData? GetScenarioData(int scenarioNo) => ScenarioBase.GetScenarioId(scenarioNo)?.MyPipe(ScenarioBase.GetScenarioData);
        void UpdateFillDreamConditionItemsRepeater(ScenarioData? maybeScenario) => maybeScenario?.MyApply(scenario => FillDreamConditionItemsRepeater.ItemsSource = GetFillDreamConditionListData(scenario));
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
}