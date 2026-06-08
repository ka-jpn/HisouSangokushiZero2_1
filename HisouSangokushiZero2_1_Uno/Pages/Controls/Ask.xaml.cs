using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using HisouSangokushiZero2_1_Uno.Pages.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Ask:UserControl {
  private const double padding = 5;
  private const double maxWidth = 600;
  private const double titleTextScaleFactor = 1.25;
  private static Action OkButtonAction = () => { };
  private static UIElement? parent = null;
  internal Ask() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      OkButton.Click += (_,_) => ExeOkButtonAction();
      CloseButton.Click += (_,_) => UIUtil.SetVisibility(this,false);
      Page.SizeChanged += (_,_) => parent?.MyApply(parent => ResizeElem(parent));
      static void ExeOkButtonAction() => OkButtonAction();
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
  internal static void SetElems(Ask page,string titleText,List<TextBlock> contents,string okButtonText,Action? okButtonAction,bool isOkButtonClickClose) {
    page.TitleTextBlock.Text = titleText;
    page.DetailPanel.MySetChildren([.. contents]);
    OkButtonAction = () => { okButtonAction?.Invoke(); UIUtil.SetVisibility(page,!isOkButtonClickClose); };
    page.OkButtonText.Text = okButtonText;
    page.OkButton.IsEnabled = okButtonAction is {};
    UIUtil.SetVisibility(page, true);
    parent?.MyApply(page.ResizeElem);
  }
  internal void ResizeElem(UIElement parent) {
    double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize);
    double sideMargin = UIUtil.infoFrameWidth * scaleFactor;
    double contentWidth = Page.RenderSize.Width - sideMargin * 2 - padding * 2;
    Content.Margin = new(sideMargin,sideMargin,sideMargin,0);
    TitleTextBlock.Margin = new(0,padding * scaleFactor,0,BasicStyle.textHeight * titleTextScaleFactor * (scaleFactor - 1));
    TitleTextBlock.RenderTransform = new ScaleTransform { ScaleX = scaleFactor * titleTextScaleFactor,ScaleY = scaleFactor * titleTextScaleFactor,CenterX = UIUtil.CalcTextWidthForFullWidth(TitleTextBlock.Text) * BasicStyle.fontsize * titleTextScaleFactor / 2 };
    DetailView.Width = Math.Min(contentWidth,(maxWidth + padding * 2 / scaleFactor) * scaleFactor);
    DetailView.Height = Math.Min(Page.RenderSize.Height * 0.6,Page.RenderSize.Height - (UIUtil.infoFrameWidth * 2 + TitleTextBlock.RenderSize.Height + OkButton.RenderSize.Height + padding * 2) * scaleFactor - padding * 4);
    double detailWidth = DetailView.Width / scaleFactor - 15;
    double detailHeight = DetailPanel.Children.MyApply(v => v.ToList().ForEach(child => child.Measure(new(detailWidth,double.PositiveInfinity)))).Sum(v => Math.Max(BasicStyle.textHeight,v.DesiredSize.Height));
    DetailPanel.Margin = new(detailWidth * (scaleFactor - 1) / 2,0, detailWidth * (scaleFactor - 1) / 2, detailHeight * (scaleFactor - 1));
    DetailPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = detailWidth / 2 };
    ButtonGrid.Height = 45 * scaleFactor;
    OkButtonText.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = OkButtonText.RenderSize.Width / 2,CenterY = OkButtonText.RenderSize.Height / 2 };
    CloseButtonText.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = CloseButtonText.RenderSize.Width / 2,CenterY = CloseButtonText.RenderSize.Height / 2 };
    Content.Padding = new(padding * scaleFactor);
    TitleDetailMargin.Height = padding * scaleFactor;
    DetailButtonMargin.Height = padding * scaleFactor;
    ButtonMargin.Width = padding * scaleFactor;
  }
}