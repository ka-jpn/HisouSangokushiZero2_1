using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data;
using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using HisouSangokushiZero2_1_Uno.Pages.Common;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Image = HisouSangokushiZero2_1_Uno.Code.Image;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal record PersonListData(Brush Brush,ECountry? Country,string Name,string Role,ImageSource RoleImage,int Rank,int BirthYear,string AppearYear,int DeathYear,string Biography);
internal sealed partial class PersonList:UserControl {
  private enum SortButtonKind { 国役割別, ランク順, 生年順, 没年順 };
  private static UIElement? parent = null;
  internal const double itemsRepeaterWidth = 750;
  internal PersonList() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      SortButtonKind initSortKind = SortButtonKind.国役割別;
      Dictionary<SortButtonKind, Func<Dictionary<PersonId, PersonData>, Dictionary<PersonId, PersonData>>> buttonActionMap = new([
        new(SortButtonKind.国役割別,v =>v.OrderBy(v => v.Value.Country).ThenBy(v => v.Value.Role).ThenBy(v => v.Value.BirthYear).ToDictionary()),
        new(SortButtonKind.ランク順,v =>v.OrderByDescending(v => v.Value.Rank).ThenBy(v => v.Value.BirthYear).ToDictionary()),
        new(SortButtonKind.生年順,v =>v.OrderBy(v => v.Value.BirthYear).ToDictionary()),
        new(SortButtonKind.没年順,v =>v.OrderBy(v => v.Value.DeathYear).ToDictionary())
      ]);
      AttachEvent();
      SetUIElements();
      void AttachEvent() {
        SizeChanged += (_, _) => parent?.MyApply(ResizeElem);
        PersonScroll.SizeChanged += (_, _) => PersonItemsRepeater.Width = PersonScroll.RenderSize.Width;
        void ResizeElem(UIElement parent) {
          double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize);
          double pageWidth = RenderSize.Width;
          double contentWidth = RenderSize.Width / scaleFactor - 5;
          ContentPanel.Width = contentWidth;
          ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor, ScaleY = scaleFactor };
          ContentPanel.Margin = new(0, 0, contentWidth * (scaleFactor - 1), ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
          SortButtonPanel.Children.OfType<Button>().ToList().ForEach(v => v.Width = contentWidth / 4 - 5 * 2);
          ScenarioComboBox.ItemContainerStyle = new Style(typeof(ComboBoxItem)).MyApply(style=>style.Setters.Add(new Setter(FontSizeProperty,BasicStyle.fontsize*UIUtil.GetScaleFactor(parent.RenderSize))));
        }
      }
      void SetUIElements() {
        ScenarioBase.GetScenarioIds().Select(v => v.Value).ToList().ForEach(ScenarioComboBox.Items.Add);
        LoadScenarioData(0);
        ScenarioComboBox.SelectedIndex = 0;
        ScenarioComboBox.SelectionChanged += (_, _) => LoadScenarioData(ScenarioComboBox.SelectedIndex);
        SortButtonPanel.MySetChildren([.. CreateSortButtons()]);
        RefreshSortButtonPanelColor(SortButtonPanel, initSortKind);
        void LoadScenarioData(int scenarioNo) {
          ScenarioData? maybeScenario = GetScenarioData(scenarioNo);
          UpdatePersonItemsRepeater(maybeScenario, initSortKind);
        }
        List<Button> CreateSortButtons() {
          return [.. buttonActionMap.Keys.Select(CreateSortButton)];
          Button CreateSortButton(SortButtonKind buttonKind) {
            return new Button { MaxWidth = 150, Height = 25, Margin = new Thickness(2.5, 0) }.MySetChild(new TextBlock { Text = buttonKind.ToString() }).MyApply(v => v.Click += (_, _) => {
              ScenarioData? maybeScenario = GetScenarioData(ScenarioComboBox.SelectedIndex);
              UpdatePersonItemsRepeater(maybeScenario, buttonKind);
              RefreshSortButtonPanelColor(SortButtonPanel, buttonKind);
            });
          }
        }
        ScenarioData? GetScenarioData(int scenarioNo) => ScenarioBase.GetScenarioId(scenarioNo)?.MyPipe(ScenarioBase.GetScenarioData);
        void UpdatePersonItemsRepeater(ScenarioData? maybeScenario, SortButtonKind buttonKind) => maybeScenario?.MyApply(scenario => PersonItemsRepeater.ItemsSource = GetPersonListData(scenario, buttonKind));
      }
      void RefreshSortButtonPanelColor(StackPanel buttonPanel, SortButtonKind buttonKind) {
        buttonActionMap.Keys.MyGetIndex(v => v == buttonKind)?.MyApply(index => RefreshColor([.. buttonPanel.Children.OfType<Button>()], index));
        static void RefreshColor(List<Button> elems, int index) => elems.ForEachWithIndex((v, i) => v.Background = i == index ? Colors.LightGray : Colors.WhiteSmoke);
      }
      List<PersonListData> GetPersonListData(ScenarioData scenario, SortButtonKind buttonKind) {
        return [.. buttonActionMap.GetValueOrDefault(buttonKind)?.Invoke(scenario.PersonMap.ToDictionary()).Select(ToPersonListItem) ?? []];
        PersonListData ToPersonListItem(KeyValuePair<PersonId, PersonData> personInfo) {
          return new PersonListData((scenario.CountryMap.GetValueOrDefault(personInfo.Value.Country)?.ViewColor ?? UIUtil.transparentColor).ToBrush(), personInfo.Value.Country, personInfo.Key.Value, Data.Language.Text.RoleToText(personInfo.Value.Role),Image.GetSvgImageSource($"{personInfo.Value.Role}",80,80),personInfo.Value.Rank, personInfo.Value.BirthYear, Person.GetAppearYear(personInfo.Value).MyPipe(appearYear => appearYear >= scenario.StartYear ? appearYear.ToString() : "登場"), personInfo.Value.DeathYear, Biography.biographyMap.GetValueOrDefault(personInfo.Key) ?? string.Empty);
        }
      }
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
}