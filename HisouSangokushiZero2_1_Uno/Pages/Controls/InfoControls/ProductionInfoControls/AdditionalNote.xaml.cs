using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class AdditionalNote:UserControl {
  private static UIElement? parent = null;
  private static Action close = MyUtil.MyUtil.nothing;
  internal AdditionalNote() {
    InitializeComponent();
    MyInit(this);
    void MyInit(AdditionalNote page) {
      CloseButton.Click += (_, _) => close();
      page.SizeChanged += (_, _) => ResizeElem();
      void ResizeElem() {
        double contentWidth = page.RenderSize.Width - 15;
        ContentPanel.Width = contentWidth;
      }
    }
  }
  internal static void InitCloseAction(Action closeAction) => close = closeAction;
}