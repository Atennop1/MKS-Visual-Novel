// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.UI
{
    public class RollbackUI : CustomUI, IRollbackUI
    {
        protected float HideTime => hideTime;

        [SerializeField] private float hideTime = 1f;

        private IStateManager stateManager;
        private Timer hideTimer;

        protected override void Awake ()
        {
            base.Awake();

            stateManager = Engine.GetService<IStateManager>();
            hideTimer = new Timer(onCompleted: Hide);
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            stateManager.OnRollbackStarted += HandleRollbackStarted;
            stateManager.OnRollbackFinished += HandleRollbackFinished;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            stateManager.OnRollbackStarted -= HandleRollbackStarted;
            stateManager.OnRollbackFinished -= HandleRollbackFinished;
        }

        protected virtual void HandleRollbackStarted ()
        {
            if (hideTimer.Running)
                hideTimer.Stop();

            Show();
        }

        protected virtual void HandleRollbackFinished ()
        {
            if (!stateManager.RollbackInProgress)
                hideTimer.Run(hideTime);
        }
    }
}
