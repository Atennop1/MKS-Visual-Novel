// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    [Serializable]
    public abstract class ActorMetadataMap
    {
        public abstract int Length { get; }

        public abstract ActorMetadata GetMetadata (string id);
        public abstract List<string> GetAllIds ();
        public abstract int GetMetaIndex (string id);
        public abstract bool ContainsId (string id);
        public abstract void RemoveRecord (string id);
        public abstract void RemoveAllRecords ();
        public abstract void MoveRecord (string sourceId, string destinationId);
    }

    /// <summary>
    /// Represents a serializable actor ID (string) to <see cref="ActorMetadata"/> map.
    /// </summary>
    /// <typeparam name="TMeta">Type of the actor metadata.</typeparam>
    [Serializable]
    public abstract class ActorMetadataMap<TMeta> : ActorMetadataMap
        where TMeta : ActorMetadata
    {
        public override int Length => ids.Length;

        [SerializeField] private string[] ids = new string[0];
        [SerializeField] private TMeta[] metas = new TMeta[0];

        protected ActorMetadataMap () { }

        protected ActorMetadataMap (IDictionary<string, TMeta> dictionary)
        {
            ids = new string[dictionary.Count];
            metas = new TMeta[dictionary.Count];

            foreach (var kv in dictionary)
                AddRecord(kv.Key, kv.Value);
        }

        public TMeta this [string id] { get => GetMetaById(id); set => AddRecord(id, value); }

        public Dictionary<string, TMeta> ToDictionary ()
        {
            var dictionary = new Dictionary<string, TMeta>();
            for (int i = 0; i < ids.Length; i++)
                dictionary[ids[i]] = metas.ElementAtOrDefault(i);
            return dictionary;
        }

        public override ActorMetadata GetMetadata (string id) => GetMetaById(id);

        public override List<string> GetAllIds () => new List<string>(ids);

        public List<TMeta> GetAllMetas () => new List<TMeta>(metas);

        public override int GetMetaIndex (string id) => ids.IndexOf(id);

        public override bool ContainsId (string id) => ArrayUtils.Contains(ids, id);

        public TMeta GetMetaById (string id)
        {
            if (!ContainsId(id)) return null;

            var index = ArrayUtils.IndexOf(ids, id);
            return metas.ElementAtOrDefault(index);
        }

        public void AddRecord (string id, TMeta meta)
        {
            if (ContainsId(id))
            {
                var index = ArrayUtils.IndexOf(ids, id);
                metas[index] = meta;
            }
            else
            {
                ArrayUtils.Add(ref ids, id);
                ArrayUtils.Add(ref metas, meta);
            }
        }

        public override void RemoveRecord (string id)
        {
            if (!ContainsId(id)) return;

            var index = ArrayUtils.IndexOf(ids, id);
            ArrayUtils.RemoveAt(ref ids, index);
            ArrayUtils.RemoveAt(ref metas, index);
        }

        public override void RemoveAllRecords ()
        {
            ArrayUtils.ClearAndResize(ref ids);
            ArrayUtils.ClearAndResize(ref metas);
        }

        public override void MoveRecord (string sourceId, string destinationId)
        {
            if (!ContainsId(sourceId)) throw new InvalidOperationException($"Failed to move `{sourceId}` actor record: actor with ID not found.");
            if (ContainsId(destinationId)) throw new InvalidOperationException($"Failed to move `{sourceId}` actor record to `{destinationId}`: another actor with same ID exists.");
            var meta = GetMetaById(sourceId);
            RemoveRecord(sourceId);
            AddRecord(destinationId, meta);
        }
    }
}
