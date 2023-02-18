using Naninovel;
using UnityEngine;

namespace VisualNovel
{
    [CommandAlias("playerSenatorVideo")]
    public class PlaySenatorVideoCommand : Command
    {
        private const string SENATOR_VIDEO_URL = "https://www.youtube.com/watch?v=4ZIR1nd60To&ab_channel=SpammingTowers";
        
        public override UniTask ExecuteAsync(AsyncToken asyncToken = default)
        {
            Application.OpenURL(SENATOR_VIDEO_URL);
            return UniTask.CompletedTask;
        }
    }
}