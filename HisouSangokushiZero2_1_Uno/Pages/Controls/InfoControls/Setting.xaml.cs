using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Setting:UserControl {
  private static readonly int minZoomLevel = 0, maxZoomLevel = 5;
  private static UIElement? parent = null;
  internal Setting() {
    InitializeComponent();
    MyInit(this);
    void MyInit(Setting page) {
      AttachEvents(page);
      RefreshUIElements();
      RefreshZoomButtonEnable();
      UIUtil.SwitchViewModeActions.Add(RefreshUIElements);
      void AttachEvents(Setting page) {
        page.SizeChanged += (_,_) => parent?.MyApply(ResizeElem);
        ZoomInButton.Click += (_,e) => ChangeZoomLevel(1);
        ZoomOutButton.Click += (_,e) => ChangeZoomLevel(-1);
        InnerSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
        SaveGameButton.Click += (_,_) => UIUtil.SaveGame();
        LoadGameButton.Click += (_,_) => UIUtil.LoadGame();
        InitGameButton.Click += (_,_) => UIUtil.InitGame();
        BackTitleButton.Click += (_,_) => (Window.Current?.Content as Frame)?.Navigate(typeof(Title));
        void ResizeElem(UIElement parent) {
          double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize);
          double contentWidth = RenderSize.Width / scaleFactor - 5;
          ContentPanel.Width = contentWidth;
          ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
          ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),ContentPanel.Children.Sum(v => v.RenderSize.Height) * (scaleFactor - 1));
          InnerSwitchViewModeButton.MaxWidth = contentWidth - ViewModeCaption.RenderSize.Width - ViewModeText.Width - 10;
          SaveGameButton.MaxWidth = contentWidth - 10;
          LoadGameButton.MaxWidth = contentWidth - 10;
          InitGameButton.MaxWidth = contentWidth - 10;
          BackTitleButton.MaxWidth = contentWidth - 10;
          ZoomInButton.MaxWidth = (contentWidth - MapZoomCaption.Width - 10) / 2;
          ZoomOutButton.MaxWidth = (contentWidth - MapZoomCaption.Width - 10) / 2;
        }
        void ChangeZoomLevel(double zoomLevelDiff) {
          Game.zoomLevel = Math.Clamp(Game.zoomLevel + zoomLevelDiff,minZoomLevel,maxZoomLevel); RefreshZoomButtonEnable(); UIUtil.ChangeScaleActions.ForEach(v => v());
        }
      }
      void RefreshUIElements() {
        ViewModeText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "固定幅" : "フィット";
      }
      void RefreshZoomButtonEnable() {
        ZoomInButton.IsEnabled = Game.zoomLevel < maxZoomLevel;
        ZoomOutButton.IsEnabled = Game.zoomLevel > minZoomLevel;
      }
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
}