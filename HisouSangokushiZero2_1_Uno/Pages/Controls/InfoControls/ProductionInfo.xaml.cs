using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;

internal sealed partial class ProductionInfo : UserControl {
  private enum ProductionInfoState { UpdateNote, AdditionalNote };
  private static readonly Dictionary<ProductionInfoState, UserControl> productionInfoStateMap = [];
  private static UIElement? parent = null;
  internal ProductionInfo() {
    InitializeComponent();
    MyInit(this);
    void MyInit(ProductionInfo page) {
      SetChildCloseAction();
      AttachEvents(page);
      void SetChildCloseAction() {
        void close() => page.ProductionInfoContentPanel.MySetChildren([]);
        AdditionalNote.InitCloseAction(close);
        UpdateNote.InitCloseAction(close);
      }
      void AttachEvents(ProductionInfo page) {
        page.SizeChanged += (_, _) => parent?.MyApply(ResizeElem);
        UpdateNoteButton.Click += (_, _) => ShowProductionInfoButton(ProductionInfoState.UpdateNote);
        AdditionalNoteButton.Click += (_, _) => ShowProductionInfoButton(ProductionInfoState.AdditionalNote);
        void ResizeElem(UIElement parent) {
          double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize);
          double contentWidth = RenderSize.Width / scaleFactor - 5;
          ContentPanel.Width = contentWidth;
          ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor, ScaleY = scaleFactor };
          ContentPanel.Margin = new(0, 0, contentWidth * (scaleFactor - 1), ContentPanel.Children.Sum(v => v.RenderSize.Height) * (scaleFactor - 1));
        }
        void ShowProductionInfoButton(ProductionInfoState newState) {
          if (productionInfoStateMap.GetValueOrDefault(newState) is UserControl elem) {
            ProductionInfoContentPanel.MySetChildren([elem]);
          } else {
            UserControl createdControl = CreateProductionInfoPanel(newState);
            productionInfoStateMap.TryAdd(newState, createdControl);
            ProductionInfoContentPanel.MySetChildren([createdControl]);
          }
          static UserControl CreateProductionInfoPanel(ProductionInfoState state) => state switch {
            ProductionInfoState.UpdateNote => new UpdateNote(),
            ProductionInfoState.AdditionalNote => new AdditionalNote()
          };
        }
      }
    }
  }
  internal static void Init(UIElement parentElem) {
    UpdateNote.InitParentElem(parentElem);
    AdditionalNote.InitParentElem(parentElem);
    parent = parentElem;
  }
}