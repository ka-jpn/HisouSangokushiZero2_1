using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class ProductionInfo : UserControl {
  private enum ProductionInfoState { UpdateNote, AdditionalNote };
  private static readonly Dictionary<ProductionInfoState, UserControl> productionInfoStateMap = [];
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
        page.SizeChanged += (_, _) => ResizeElem();
        UpdateNoteButton.Click += (_, _) => ShowProductionInfoButton(ProductionInfoState.UpdateNote);
        AdditionalNoteButton.Click += (_, _) => ShowProductionInfoButton(ProductionInfoState.AdditionalNote);
        void ResizeElem() {
          double contentWidth = RenderSize.Width - 5;
          ContentPanel.Width = contentWidth;
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
}