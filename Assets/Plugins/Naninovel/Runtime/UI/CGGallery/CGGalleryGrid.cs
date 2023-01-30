// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using Naninovel.Runtime.UI;
using Naninovel.UI;

namespace Naninovel
{
    public class CGGalleryGrid : ScriptableGrid<CGGalleryGridSlot>
    {
        protected virtual List<CGSlotData> SlotData { get; private set; }

        private CGViewerPanel viewerPanel;

        public void Initialize (CGViewerPanel viewerPanel, List<CGSlotData> slotData)
        {
            this.viewerPanel = viewerPanel;
            SlotData = slotData;
            Initialize(slotData.Count);
        }

        protected new void Initialize (int itemsCount) => base.Initialize(itemsCount);

        protected override void InitializeSlot (CGGalleryGridSlot slot)
        {
            slot.Initialize(viewerPanel);
        }

        protected override void BindSlot (CGGalleryGridSlot slot, int itemIndex)
        {
            var slotData = SlotData[itemIndex];
            slot.Bind(slotData);
        }
    }
}
