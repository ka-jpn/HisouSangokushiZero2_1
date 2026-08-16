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
namespace HisouSangokushiZero2_1_Uno.Pages;
internal record CountryListData(Brush Brush,ECountry Country,decimal Fund,int SleepTurnNum,string AreasText);
internal sealed partial class CountryList:UserControl {
  private static UIElement? parent = null;
  internal const double itemsRepeaterWidth = 750;
  internal CountryList() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      AttachEvent();
      SetUIElements();
      void AttachEvent() {
        SizeChanged += (_, _) => parent?.MyApply(ResizeElem);
        CountryScroll.SizeChanged += (_, _) => CountryItemsRepeater.Width = CountryScroll.RenderSize.Width;
        void ResizeElem(UIElement parent) {
          double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize);
          double pageWidth = RenderSize.Width;
          double contentWidth = RenderSize.Width / scaleFactor - 5;
          ContentPanel.Width = contentWidth;
          ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor, ScaleY = scaleFactor };
          ContentPanel.Margin = new(0, 0, contentWidth * (scaleFactor - 1), ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
          ScenarioComboBox.ItemContainerStyle = new Style(typeof(ComboBoxItem)).MyApply(style=>style.Setters.Add(new Setter(FontSizeProperty,BasicStyle.fontsize*UIUtil.GetScaleFactor(parent.RenderSize))));
        }
      }
      void SetUIElements() {
        ScenarioBase.GetScenarioIds().Select(v => v.Value).ToList().ForEach(ScenarioComboBox.Items.Add);
        LoadScenarioData(0);
        ScenarioComboBox.SelectedIndex = 0;
        ScenarioComboBox.SelectionChanged += (_, _) => LoadScenarioData(ScenarioComboBox.SelectedIndex);
        void LoadScenarioData(int scenarioNo) {
          ScenarioData? maybeScenario = GetScenarioData(scenarioNo);
          UpdateCountryItemsRepeater(maybeScenario);
        }
        ScenarioData? GetScenarioData(int scenarioNo) => ScenarioBase.GetScenarioId(scenarioNo)?.MyPipe(ScenarioBase.GetScenarioData);
        void UpdateCountryItemsRepeater(ScenarioData? maybeScenario) => maybeScenario?.MyApply(scenario => CountryItemsRepeater.ItemsSource = GetCountryListData(scenario));
      }
      List<CountryListData> GetCountryListData(ScenarioData scenario) {
        return [.. scenario.CountryMap.OrderBy(v => v.Key).Select(ToCountryListItem)];
        CountryListData ToCountryListItem(KeyValuePair<ECountry, CountryData> countryInfo) {
          return new CountryListData((countryInfo.Value.ViewColor ?? UIUtil.transparentColor).ToBrush(), countryInfo.Key, countryInfo.Value.Fund, countryInfo.Value.SleepTurnNum, string.Join(",", scenario.AreaMap.Where(v => v.Value.Country == countryInfo.Key).Select(v => v.Key.ToString())));
        }
      }
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
}