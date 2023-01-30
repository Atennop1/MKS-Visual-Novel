// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Globalization;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable session-specific state of the engine services and related data (aka saved game status).
    /// </summary>
    [Serializable]
    public class GameStateMap : StateMap
    {
        /// <summary>
        /// State of <see cref="IScriptPlayer.PlaybackSpot"/> at the time snapshot was taken;
        /// expected to be set by the player service on serialization.
        /// </summary>
        public PlaybackSpot PlaybackSpot { get => playbackSpot; set => playbackSpot = value; }
        /// <summary>
        /// Date and time when the snapshot was taken.
        /// </summary>
        public DateTime SaveDateTime { get; set; }
        /// <summary>
        /// Preview of the screen when the snapshot was taken.
        /// </summary>
        public Texture2D Thumbnail { get; set; }
        /// <summary>
        /// Whether player is allowed rolling back to this snapshot; see remarks for more info.
        /// </summary>
        /// <remarks>
        /// Player expects rollback to occur between the points where he's interacted with the game to progress it further
        /// (clicked a printer to continue reading, picked up a choice, etc). This flag can be set before mutating game state 
        /// after a meaningful player interaction to indicate that the snapshot can be used when handling "rollback" input.
        /// </remarks>
        public bool PlayerRollbackAllowed { get; set; }
        /// <summary>
        /// JSON representation of the rollback stack when the snapshot was taken.
        /// </summary>
        public string RollbackStackJson { get => rollbackStackJson; set => rollbackStackJson = value; }

        private const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        [SerializeField] private PlaybackSpot playbackSpot;
        [SerializeField] private string saveDateTime;
        [SerializeField] private string thumbnailBase64;
        [SerializeField] private string rollbackStackJson;

        public override void OnBeforeSerialize ()
        {
            base.OnBeforeSerialize();

            saveDateTime = SaveDateTime.ToString(dateTimeFormat, CultureInfo.InvariantCulture);
            thumbnailBase64 = Thumbnail ? Convert.ToBase64String(Thumbnail.EncodeToJPG()) : null;
        }

        public override void OnAfterDeserialize ()
        {
            base.OnAfterDeserialize();

            SaveDateTime = string.IsNullOrEmpty(saveDateTime) ? DateTime.MinValue : DateTime.ParseExact(saveDateTime, dateTimeFormat, CultureInfo.InvariantCulture);
            Thumbnail = string.IsNullOrEmpty(thumbnailBase64) ? null : GetThumbnail();
        }

        /// <summary>
        /// Allows this state snapshot to be used for player-driven state rollback.
        /// </summary>
        public void AllowPlayerRollback () => PlayerRollbackAllowed = true;

        private Texture2D GetThumbnail ()
        {
            var tex = new Texture2D(2, 2);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.LoadImage(Convert.FromBase64String(thumbnailBase64));
            return tex;
        }
    }
}
