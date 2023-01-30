// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine.EventSystems;

namespace Naninovel
{
    public abstract class ScriptableUIControl<T> : ScriptableUIComponent<T> where T : UIBehaviour
    {
        protected override void OnEnable ()
        {
            base.OnEnable();
            BindUIEvents();
        }

        protected override void OnDisable ()
        {
            base.OnDisable();
            UnbindUIEvents();
        }

        protected abstract void BindUIEvents ();
        protected abstract void UnbindUIEvents ();
    }
}
