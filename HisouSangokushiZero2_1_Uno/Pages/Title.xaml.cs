using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data;
using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Image = HisouSangokushiZero2_1_Uno.Code.Image;
namespace HisouSangokushiZero2_1_Uno.Pages;
public sealed partial class Title:Page {
  public Title() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      RefreshViewMode();
      MapImage.Source = Image.GetSvgImageSource("Map",UIUtil.mapSize.Width*2,UIUtil.mapSize.Height*2);
      StartButton.Click += (_,_) => {
        GameData.game = GetInitGameData();
        NavigateToGamePage();
      };
      LoadButton.Click += async (_, _) => {
        await SaveAndLoad.Show(SaveDataPanel, false, maybeRead => maybeRead?.MaybeGame?.MyApply(game => {
          GameData.game = game;
          NavigateToGamePage();
        }), () => UIUtil.SetVisibility(SaveDataPanel, false), Content.RenderSize);
        UIUtil.SetVisibility(SaveDataPanel, true);
      };
      TopSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
      Content.SizeChanged += (_,_) => ScalingElements();
      UIUtil.SwitchViewModeActions.Add(RefreshViewMode);
      GameState GetInitGameData() => GetGame.GetInitGameScenario(ScenarioBase.GetScenarioId(0));
      void NavigateToGamePage() => (Window.Current?.Content as Frame)?.Navigate(typeof(Game));
      void RefreshViewMode() {
        SwitchViewModeButtonText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "▼" : "▲";
        Content.MaxWidth = UIUtil.viewMode == UIUtil.ViewMode.fix ? UIUtil.fixModeMaxWidth : double.MaxValue;
      }
      void ScalingElements() {
        double scaleFactor = UIUtil.GetScaleFactor(Content.RenderSize);
        double infoFramebuttonMargin = UIUtil.infoFrameWidth * (scaleFactor - 1);
        TopSwitchViewModeButton.Margin = new(0,0,infoFramebuttonMargin,infoFramebuttonMargin);
        TopSwitchViewModeButton.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      }
    }
  }
}