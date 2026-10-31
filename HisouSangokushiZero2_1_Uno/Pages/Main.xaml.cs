using System;
using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml.Controls;
namespace HisouSangokushiZero2_1_Uno.Pages;
public sealed partial class Main:Page {
  public Main() {
    InitializeComponent();
    ToTitlePage();
    Title.ToGamePage = ToGamePage;
    Option.ToTitlePage = ToTitlePage;
    UIUtil.headerPadding = HeaderPadding;
    UIUtil.SwitchViewModeActions.Add(RefreshHeaderHeight);
    RefreshHeaderHeight();
  }
  private void ToTitlePage() => Content.MySetChildren([new Title()]);
  private void ToGamePage() => Content.MySetChildren([new Game()]);
  private void RefreshHeaderHeight() {
    HeaderPadding.Height = UIUtil.viewMode == UIUtil.ViewMode.fix ? 100 : 0;
    ((Action)(UIUtil.viewMode == UIUtil.ViewMode.fix ? JsInterop.FixBody : JsInterop.FitBody)).Invoke();
  }
}