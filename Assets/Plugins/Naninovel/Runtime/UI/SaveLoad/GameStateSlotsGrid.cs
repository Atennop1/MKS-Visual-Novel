// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Linq;

namespace Naninovel.UI
{
    public class GameStateSlotsGrid : ScriptableGrid<GameStateSlot>
    {
        private Action<int> onSlotClicked, onDeleteClicked;
        private Func<int, UniTask<GameStateMap>> loadStateAt;

        public void Initialize (int itemsCount, Action<int> onSlotClicked,
            Action<int> onDeleteClicked, Func<int, UniTask<GameStateMap>> loadStateAt)
        {
            this.onSlotClicked = onSlotClicked;
            this.onDeleteClicked = onDeleteClicked;
            this.loadStateAt = loadStateAt;
            Initialize(itemsCount);
        }

        public virtual void BindSlot (int slotNumber, GameStateMap state)
        {
            Slots.FirstOrDefault(s => s.SlotNumber == slotNumber)?.Bind(slotNumber, state);
        }

        protected new void Initialize (int itemsCount) => base.Initialize(itemsCount);

        protected override void InitializeSlot (GameStateSlot slot)
        {
            slot.Initialize(onSlotClicked, onDeleteClicked);
        }

        protected override async void BindSlot (GameStateSlot slot, int itemIndex)
        {
            var slotNumber = itemIndex + 1;
            var state = await loadStateAt(slotNumber);
            if (!slot) return; // Don't use Engine.Initialized here, as it may be false.
            slot.Bind(slotNumber, state);
        }
    }
}
