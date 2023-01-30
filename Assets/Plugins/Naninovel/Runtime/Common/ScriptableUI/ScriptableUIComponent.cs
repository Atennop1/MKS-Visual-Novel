// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine.EventSystems;

namespace Naninovel
{
    public abstract class ScriptableUIComponent<T> : ScriptableUIBehaviour where T : UIBehaviour
    {
        public virtual T UIComponent => uiComponent ? uiComponent : uiComponent = GetComponent<T>();

        private T uiComponent;
    }
}
