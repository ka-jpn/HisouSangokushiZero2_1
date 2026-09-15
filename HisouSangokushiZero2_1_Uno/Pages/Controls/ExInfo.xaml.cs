using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class ExInfo:UserControl {
  private enum ExInfoState { Option, Explain, FillDream, PersonList, CountryList, ProductionInfo };
  private static readonly Dictionary<ExInfoState,UserControl> exInfoStateMap = [];
  internal ExInfo() {
    InitializeComponent();
    MyInit(this);
    void MyInit(ExInfo page) {
      AttachEvent();
      SwitchInfoButton(ExInfoState.Option);
      void AttachEvent() {
        OptionButton.Click += (_,_) => SwitchInfoButton(ExInfoState.Option);
        ExplainButton.Click += (_,_) => SwitchInfoButton(ExInfoState.Explain);
        FillDreamConditionButton.Click += (_,_) => SwitchInfoButton(ExInfoState.FillDream);
        PersonListButton.Click += (_,_) => SwitchInfoButton(ExInfoState.PersonList);
        CountryListButton.Click += (_,_) => SwitchInfoButton(ExInfoState.CountryList);
        ProductionInfoButton.Click += (_,_) => SwitchInfoButton(ExInfoState.ProductionInfo);
      }
      void SwitchInfoButton(ExInfoState newState) {
        ChangeButtonColor();
        ChangeContentView();
        void ChangeButtonColor() {
          Dictionary<ExInfoState, Button> buttonMap = new([
            new(ExInfoState.Option,OptionButton),
            new(ExInfoState.Explain,ExplainButton),
            new(ExInfoState.FillDream,FillDreamConditionButton),
            new(ExInfoState.PersonList,PersonListButton),
            new(ExInfoState.CountryList,CountryListButton),
            new(ExInfoState.ProductionInfo,ProductionInfoButton)
          ]);
          buttonMap.ToList().ForEach(v => v.Value.Background = v.Key == newState ? Colors.LightGray : Colors.WhiteSmoke);
        }
        void ChangeContentView() {
          if (exInfoStateMap.GetValueOrDefault(newState) is UserControl elem) {
            InfoContentPanel.MySetChildren([elem]);
          } else {
            UserControl createdControl = CreateInfoPanel(newState);
            exInfoStateMap.TryAdd(newState, createdControl);
            InfoContentPanel.MySetChildren([createdControl]);
          }
          static UserControl CreateInfoPanel(ExInfoState state) => state switch {
            ExInfoState.Option => new Option(),
            ExInfoState.Explain => new Explain(),
            ExInfoState.FillDream => new FillDreamCondition(),
            ExInfoState.PersonList => new PersonList(),
            ExInfoState.CountryList => new CountryList(),
            ExInfoState.ProductionInfo => new ProductionInfo()
          };
        }
      }
    }
  }
}
