using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Option:UserControl {
  private const int minMapZoomLevel = 0, maxMapZoomLevel = 5;
  private const int minInfoTextZoomLevel = 0, maxInfoTextZoomLevel = 5;
  internal Option() {
    InitializeComponent();
    MyInit(this);
    void MyInit(Option page) {
      AttachEvents(page);
      RefreshUIElements();
      RefreshMapZoomButtonEnable();
      RefreshInfoTextZoomButtonEnable();
      UIUtil.SwitchViewModeActions.Add(RefreshUIElements);
      void AttachEvents(Option page) {
        page.SizeChanged += (_,_) => ResizeElem();
        ZoomInMapButton.Click += (_,e) => ChangeMapZoomLevel(1);
        ZoomOutMapButton.Click += (_,e) => ChangeMapZoomLevel(-1);
        ZoomInInfoTextButton.Click += (_,e) => ChangeInfoTextZoomLevel(1);
        ZoomOutInfoTextButton.Click += (_,e) => ChangeInfoTextZoomLevel(-1);
        InnerSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
        SaveGameButton.Click += (_,_) => UIUtil.SaveGame();
        LoadGameButton.Click += (_,_) => UIUtil.LoadGame();
        InitGameButton.Click += (_,_) => UIUtil.InitGame();
        BackTitleButton.Click += (_,_) => (Window.Current?.Content as Frame)?.Navigate(typeof(Title));
        void ResizeElem() {
          double contentWidth = RenderSize.Width - 5;
          ContentPanel.Width = contentWidth;
          InnerSwitchViewModeButton.MaxWidth = contentWidth - ViewModeCaption.RenderSize.Width - ViewModeText.Width - 10;
          SaveGameButton.MaxWidth = contentWidth - 10;
          LoadGameButton.MaxWidth = contentWidth - 10;
          InitGameButton.MaxWidth = contentWidth - 10;
          BackTitleButton.MaxWidth = contentWidth - 10;
          ZoomInMapButton.MaxWidth = (contentWidth - MapZoomCaption.Width - 10) / 2;
          ZoomOutMapButton.MaxWidth = (contentWidth - MapZoomCaption.Width - 10) / 2;
          ZoomInInfoTextButton.MaxWidth = (contentWidth - InfoTextZoomCaption.Width - 10) / 2;
          ZoomOutInfoTextButton.MaxWidth = (contentWidth - InfoTextZoomCaption.Width - 10) / 2;
        }
        void ChangeMapZoomLevel(double zoomLevelDiff) {
          UIUtil.mapZoomLevel = Math.Clamp(UIUtil.mapZoomLevel + zoomLevelDiff,minMapZoomLevel,maxMapZoomLevel);
          RefreshMapZoomButtonEnable();
          UIUtil.ChangeMapScaleActions.ForEach(v => v());
        }
        void ChangeInfoTextZoomLevel(double zoomLevelDiff) {
          UIUtil.infoTextZoomLevel = Math.Clamp(UIUtil.infoTextZoomLevel + zoomLevelDiff,minInfoTextZoomLevel,maxInfoTextZoomLevel);
          RefreshInfoTextZoomButtonEnable();
          UIUtil.ChangeInfoTextZoomActions.ForEach(v => v());
        }
      }
      void RefreshMapZoomButtonEnable() {
        ZoomInMapButton.IsEnabled = UIUtil.mapZoomLevel < maxMapZoomLevel;
        ZoomOutMapButton.IsEnabled = UIUtil.mapZoomLevel > minMapZoomLevel;
      }
      void RefreshInfoTextZoomButtonEnable() {
        ZoomInInfoTextButton.IsEnabled = UIUtil.infoTextZoomLevel < maxInfoTextZoomLevel;
        ZoomOutInfoTextButton.IsEnabled = UIUtil.infoTextZoomLevel > minInfoTextZoomLevel;
      }
      void RefreshUIElements() {
        ViewModeText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "固定幅" : "フィット";
      }
    }
  }
}