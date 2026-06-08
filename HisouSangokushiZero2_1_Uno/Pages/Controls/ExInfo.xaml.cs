using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class ExInfo:UserControl {
  private enum ExInfoState { Explain, WinCond, ParamList, ProductionInfo, Setting };
  private static readonly Dictionary<ExInfoState,UserControl> exInfoStateMap = [];
  internal ExInfo() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      Explain.Init(InfoContentPanel);
      WinCond.Init(InfoContentPanel);
      ParamList.Init(InfoContentPanel);
      ProductionInfo.Init(InfoContentPanel);
      Setting.Init(InfoContentPanel);
      AttachEvent();
      LoadPage();
      void AttachEvent() {
        ExplainButton.Click += (_,_) => SwitchInfoButton(ExInfoState.Explain);
        WinCondButton.Click += (_,_) => SwitchInfoButton(ExInfoState.WinCond);
        ParamListButton.Click += (_,_) => SwitchInfoButton(ExInfoState.ParamList);
        ProductionInfoButton.Click += (_,_) => SwitchInfoButton(ExInfoState.ProductionInfo);
        SettingButton.Click += (_,_) => SwitchInfoButton(ExInfoState.Setting);
      }
      void LoadPage() => SwitchInfoButton(ExInfoState.Explain);
      void SwitchInfoButton(ExInfoState newState) {
        ChangeButtonColor();
        ChangeContentView();
        void ChangeButtonColor() {
          Dictionary<ExInfoState, Button> buttonMap = new([
            new(ExInfoState.Explain,ExplainButton),
            new(ExInfoState.WinCond,WinCondButton),
            new(ExInfoState.ParamList,ParamListButton),
            new(ExInfoState.ProductionInfo,ProductionInfoButton),
            new(ExInfoState.Setting,SettingButton)
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
            ExInfoState.Explain => new Explain(),
            ExInfoState.WinCond => new WinCond(),
            ExInfoState.ParamList => new ParamList(),
            ExInfoState.ProductionInfo => new ProductionInfo(),
            ExInfoState.Setting => new Setting()
          };
        }
      }
    }
  }
}
