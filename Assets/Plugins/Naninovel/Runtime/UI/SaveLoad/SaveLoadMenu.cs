// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SaveLoadMenu : CustomUI, ISaveLoadUI
    {
        [Serializable]
        private class GlobalState
        {
            public bool LastSaveWasQuick;
        }

        public SaveLoadUIPresentationMode PresentationMode { get => presentationMode; set => SetPresentationMode(value); }

        [ManagedText("DefaultUI")]
        protected static string OverwriteSaveSlotMessage = "Are you sure you want to overwrite save slot?";
        [ManagedText("DefaultUI")]
        protected static string DeleteSaveSlotMessage = "Are you sure you want to delete save slot?";

        protected virtual bool LastSaveWasQuick
        {
            get => stateManager.GlobalState.GetState<GlobalState>()?.LastSaveWasQuick ?? false;
            set => stateManager.GlobalState.SetState<GlobalState>(new GlobalState { LastSaveWasQuick = value });
        }
        protected virtual Toggle QuickLoadToggle => quickLoadToggle;
        protected virtual Toggle SaveToggle => saveToggle;
        protected virtual Toggle LoadToggle => loadToggle;
        protected virtual GameStateSlotsGrid QuickLoadGrid => quickLoadGrid;
        protected virtual GameStateSlotsGrid SaveGrid => saveGrid;
        protected virtual GameStateSlotsGrid LoadGrid => loadGrid;

        [Header("Tabs")]
        [SerializeField] private Toggle quickLoadToggle = null;
        [SerializeField] private Toggle saveToggle = null;
        [SerializeField] private Toggle loadToggle = null;

        [Header("Grids")]
        [SerializeField] private GameStateSlotsGrid quickLoadGrid = null;
        [SerializeField] private GameStateSlotsGrid saveGrid = null;
        [SerializeField] private GameStateSlotsGrid loadGrid = null;

        private const string titleLabel = "OnLoad";

        private string titleScriptName;
        private IStateManager stateManager;
        private IScriptPlayer scriptPlayer;
        private IScriptManager scriptManager;
        private IConfirmationUI confirmationUI;
        private SaveLoadUIPresentationMode presentationMode;
        private ISaveSlotManager<GameStateMap> slotManager => stateManager?.GameSlotManager;

        public override UniTask InitializeAsync ()
        {
            stateManager = Engine.GetService<IStateManager>();
            scriptManager = Engine.GetService<IScriptManager>();
            titleScriptName = scriptManager.Configuration.TitleScript;
            scriptPlayer = Engine.GetService<IScriptPlayer>();
            confirmationUI = Engine.GetService<IUIManager>().GetUI<IConfirmationUI>();
            if (confirmationUI is null) throw new Exception("Confirmation UI is missing.");

            stateManager.OnGameSaveStarted += HandleGameSaveStarted;
            stateManager.OnGameSaveFinished += HandleGameSaveFinished;

            quickLoadGrid.Initialize(stateManager.Configuration.QuickSaveSlotLimit,
                HandleQuickLoadSlotClicked, HandleDeleteQuickLoadSlotClicked, LoadQuickSaveSlotAsync);
            saveGrid.Initialize(stateManager.Configuration.SaveSlotLimit,
                HandleSaveSlotClicked, HandleDeleteSlotClicked, LoadSaveSlotAsync);
            loadGrid.Initialize(stateManager.Configuration.SaveSlotLimit,
                HandleLoadSlotClicked, HandleDeleteSlotClicked, LoadSaveSlotAsync);
            return UniTask.CompletedTask;
        }

        public virtual SaveLoadUIPresentationMode GetLastLoadMode ()
        {
            return LastSaveWasQuick ? SaveLoadUIPresentationMode.QuickLoad : SaveLoadUIPresentationMode.Load;
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(QuickLoadToggle, SaveToggle, LoadToggle, QuickLoadGrid, SaveGrid, LoadGrid);
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy();

            if (stateManager != null)
            {
                stateManager.OnGameSaveStarted -= HandleGameSaveStarted;
                stateManager.OnGameSaveFinished -= HandleGameSaveFinished;
            }
        }

        protected virtual void SetPresentationMode (SaveLoadUIPresentationMode value)
        {
            presentationMode = value;
            switch (value)
            {
                case SaveLoadUIPresentationMode.QuickLoad:
                    LoadToggle.gameObject.SetActive(true);
                    QuickLoadToggle.gameObject.SetActive(true);
                    QuickLoadToggle.isOn = true;
                    SaveToggle.gameObject.SetActive(false);
                    break;
                case SaveLoadUIPresentationMode.Load:
                    LoadToggle.gameObject.SetActive(true);
                    QuickLoadToggle.gameObject.SetActive(true);
                    LoadToggle.isOn = true;
                    SaveToggle.gameObject.SetActive(false);
                    break;
                case SaveLoadUIPresentationMode.Save:
                    SaveToggle.gameObject.SetActive(true);
                    SaveToggle.isOn = true;
                    LoadToggle.gameObject.SetActive(false);
                    QuickLoadToggle.gameObject.SetActive(false);
                    break;
            }
        }

        protected virtual void HandleLoadSlotClicked (int slotNumber)
        {
            var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
            HandleLoadSlotClicked(slotId);
        }

        protected virtual void HandleQuickLoadSlotClicked (int slotNumber)
        {
            var slotId = stateManager.Configuration.IndexToQuickSaveSlotId(slotNumber);
            HandleLoadSlotClicked(slotId);
        }

        protected virtual async void HandleLoadSlotClicked (string slotId)
        {
            if (!slotManager.SaveSlotExists(slotId)) return;

            if (!string.IsNullOrEmpty(titleScriptName) &&
                await scriptManager.LoadScriptAsync(titleScriptName) is Script titleScript &&
                titleScript.LabelExists(titleLabel))
            {
                scriptPlayer.ResetService();
                await scriptPlayer.PreloadAndPlayAsync(titleScript, label: titleLabel);
                await UniTask.WaitWhile(() => scriptPlayer.Playing);
            }

            Hide();
            Engine.GetService<IUIManager>()?.GetUI<ITitleUI>()?.Hide();
            await stateManager.LoadGameAsync(slotId);
        }

        protected virtual void HandleSaveSlotClicked (int slotNumber)
        {
            var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
            HandleSaveSlotClicked(slotId, slotNumber);
        }

        protected virtual void HandleQuickSaveSlotClicked (int slotNumber)
        {
            var slotId = stateManager.Configuration.IndexToQuickSaveSlotId(slotNumber);
            HandleSaveSlotClicked(slotId, slotNumber);
        }

        protected virtual async void HandleSaveSlotClicked (string slotId, int slotNumber)
        {
            SetInteractable(false);

            if (slotManager.SaveSlotExists(slotId))
            {
                var confirmed = await confirmationUI.ConfirmAsync(OverwriteSaveSlotMessage);
                if (!confirmed)
                {
                    SetInteractable(true);
                    return;
                }
            }

            var state = await stateManager.SaveGameAsync(slotId);
            SaveGrid.BindSlot(slotNumber, state);
            LoadGrid.BindSlot(slotNumber, state);

            SetInteractable(true);
        }

        protected virtual async void HandleDeleteSlotClicked (int slotNumber)
        {
            var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
            if (!slotManager.SaveSlotExists(slotId)) return;

            if (!await confirmationUI.ConfirmAsync(DeleteSaveSlotMessage)) return;

            slotManager.DeleteSaveSlot(slotId);
            SaveGrid.BindSlot(slotNumber, null);
            LoadGrid.BindSlot(slotNumber, null);
        }

        protected virtual async void HandleDeleteQuickLoadSlotClicked (int slotNumber)
        {
            var slotId = stateManager.Configuration.IndexToQuickSaveSlotId(slotNumber);
            if (!slotManager.SaveSlotExists(slotId)) return;

            if (!await confirmationUI.ConfirmAsync(DeleteSaveSlotMessage)) return;

            slotManager.DeleteSaveSlot(slotId);
            QuickLoadGrid.BindSlot(slotNumber, null);
        }

        protected virtual void HandleGameSaveStarted (GameSaveLoadArgs args)
        {
            LastSaveWasQuick = args.Quick;
        }

        protected virtual async void HandleGameSaveFinished (GameSaveLoadArgs args)
        {
            if (!args.Quick) return;

            // Shifting quick save slots by one to free the first slot.
            for (int i = QuickLoadGrid.Slots.Count - 2; i >= 0; i--)
            {
                var currSlot = QuickLoadGrid.Slots[i];
                var prevSlot = QuickLoadGrid.Slots[i + 1];
                prevSlot.Bind(prevSlot.SlotNumber, currSlot.State);
            }

            // Setting the new quick save to the first slot.
            var slotState = await stateManager.GameSlotManager.LoadAsync(args.SlotId);
            QuickLoadGrid.BindSlot(1, slotState);
        }

        protected virtual async UniTask<GameStateMap> LoadSaveSlotAsync (int slotNumber)
        {
            var slotId = stateManager.Configuration.IndexToSaveSlotId(slotNumber);
            var state = slotManager.SaveSlotExists(slotId) ? await slotManager.LoadAsync(slotId) : null;
            return state;
        }

        protected virtual async UniTask<GameStateMap> LoadQuickSaveSlotAsync (int slotNumber)
        {
            var slotId = stateManager.Configuration.IndexToQuickSaveSlotId(slotNumber);
            var state = slotManager.SaveSlotExists(slotId) ? await slotManager.LoadAsync(slotId) : null;
            return state;
        }
    }
}
