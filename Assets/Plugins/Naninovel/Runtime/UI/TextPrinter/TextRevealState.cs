// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    /// <summary>
    /// Represents text reveal progress state.
    /// </summary>
    public class TextRevealState
    {
        public bool InProgress { get; private set; }
        public int CharactersToReveal { get; private set; }
        public float RevealDuration { get; private set; }
        public AsyncToken AsyncToken { get; private set; }
        public int CharactersRevealed { get; set; }

        public virtual void Start (int count, float duration, AsyncToken asyncToken)
        {
            InProgress = true;
            CharactersRevealed = 0;
            CharactersToReveal = count;
            RevealDuration = duration;
            AsyncToken = asyncToken;
        }

        public virtual void Reset ()
        {
            InProgress = false;
            CharactersToReveal = CharactersRevealed = 0;
            RevealDuration = 0f;
            AsyncToken = default;
        }
    }
}
