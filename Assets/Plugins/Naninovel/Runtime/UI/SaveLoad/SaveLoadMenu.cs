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
        protected virtual Toggle SaveToggle => saveToggle;
        protected virtual Toggle LoadToggle => loadToggle;
        protected virtual GameStateSlotsGrid SaveGrid => saveGrid;
        protected virtual GameStateSlotsGrid LoadGrid => loadGrid;

        [Header("Tabs")]
        [SerializeField] private Toggle saveToggle;
        [SerializeField] private Toggle loadToggle;

        [Header("Grids")]
        [SerializeField] private GameStateSlotsGrid saveGrid;
        [SerializeField] private GameStateSlotsGrid loadGrid;

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
            this.AssertRequiredObjects(SaveToggle, LoadToggle, SaveGrid, LoadGrid);
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy();

            if (stateManager != null)
                stateManager.OnGameSaveStarted -= HandleGameSaveStarted;
        }

        protected virtual void SetPresentationMode (SaveLoadUIPresentationMode value)
        {
            presentationMode = value;
            switch (value)
            {
                case SaveLoadUIPresentationMode.QuickLoad or SaveLoadUIPresentationMode.Load:
                    LoadToggle.gameObject.SetActive(true);
                    LoadToggle.isOn = true;
                    LoadToggle.onValueChanged.Invoke(true);
                    SaveToggle.gameObject.SetActive(false);
                    break;
                case SaveLoadUIPresentationMode.Save:
                    SaveToggle.gameObject.SetActive(true);
                    SaveToggle.isOn = true;
                    LoadToggle.gameObject.SetActive(false);
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
        protected virtual void HandleGameSaveStarted (GameSaveLoadArgs args)
        {
            LastSaveWasQuick = args.Quick;
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
