using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class UpdateNote:UserControl {
  private static UIElement? parent = null;
  private static Action close = MyUtil.MyUtil.nothing;
  internal UpdateNote() {
    InitializeComponent();
    MyInit(this);
    void MyInit(UpdateNote page) {
      CloseButton.Click += (_, _) => close();
      page.SizeChanged += (_, _) => parent?.MyApply(ResizeElem);
      void ResizeElem(UIElement parent) {
        double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize);
        double contentWidth = page.RenderSize.Width / scaleFactor - 15; ContentPanel.Width = contentWidth;
        ContentPanel.Children.ToList().ForEach(child => child.Measure(new(ContentPanel.Width, double.PositiveInfinity)));
        ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor, ScaleY = scaleFactor };
        ContentPanel.Margin = new(0, 0, contentWidth * (scaleFactor - 1), ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
      }
    }
  }
  internal static void InitParentElem(UIElement parentElem) => parent = parentElem;
  internal static void InitCloseAction(Action closeAction) => close = closeAction;
}