using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class StateInfo:UserControl {
  private Func<Task<GameState>> nextButtonAction = () => throw new Exception();
  internal const double baseContentHeight = 45;
  internal StateInfo() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(StateInfo page) {
      page.Content.Height = baseContentHeight;
      page.NextButton.Click += async (_,_) => GameData.game = await page.nextButtonAction();
    }
  }
  internal static void Show(StateInfo page,List<UIElement> InfoContents,string? buttonText,Func<Task<GameState>> buttonAction) {
    page.InfoContentsPanel.MySetChildren([.. InfoContents]);
    if(buttonText is {}) {
      page.NextButtonText.Text = buttonText;
      page.nextButtonAction = buttonAction;
      UIUtil.SetVisibility(page.NextButton,true);
    } else {
      UIUtil.SetVisibility(page.NextButton,false);
    }
    page.ButtonArea.Width = new(page.NextButton.Visibility.IsHidden() ? 0 : 1, GridUnitType.Star);
  }
  internal static void ResizeElem(StateInfo page,double scaleFactorX,double scaleFactorY) {
    double scaleX = CookScaleX(scaleFactorX);
    double scaleY = CookScaleY(scaleFactorY);
    page.Content.RenderTransform = new ScaleTransform() { ScaleX = scaleX,ScaleY = scaleY };
    page.Content.Margin = new(0, 0, page.RenderSize.Width * (1 - 1 / scaleX), baseContentHeight * (scaleY - 1));
    static double CookScaleX(double rawScale) => rawScale switch { < 0.8 => rawScale / 0.8, > 1 => double.Lerp(rawScale,1,0.5), _ => 1 };
  }
  internal static double CookScaleY(double rawScale) => rawScale switch { < 0.8 => double.Lerp(rawScale/0.8,1,0.5), > 1 => double.Lerp(rawScale,1,0.5), _ => 1 };
}