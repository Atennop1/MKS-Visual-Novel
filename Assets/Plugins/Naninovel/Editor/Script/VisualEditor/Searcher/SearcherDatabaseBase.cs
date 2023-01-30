// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Naninovel.Searcher
{
    [PublicAPI]
    public abstract class SearcherDatabaseBase
    {
        protected const string k_SerializedJsonFile = "/SerializedDatabase.json";
        public string DatabaseDirectory { get; set; }

        public IList<SearcherItem> ItemList => m_ItemList;

        // ReSharper disable once Unity.RedundantSerializeFieldAttribute
        [SerializeField]
        protected List<SearcherItem> m_ItemList;

        protected SearcherDatabaseBase(string databaseDirectory)
        {
            DatabaseDirectory = databaseDirectory;
        }

        public abstract List<SearcherItem> Search(string query, out float localMaxScore);

        internal void OverwriteId(int newId)
        {
            Id = newId;
        }

        internal int Id { get; private set; }

        protected void LoadFromFile()
        {
            var reader = new StreamReader(DatabaseDirectory + k_SerializedJsonFile);
            var serializedData = reader.ReadToEnd();
            reader.Close();

            EditorJsonUtility.FromJsonOverwrite(serializedData, this);

            foreach (var item in m_ItemList)
            {
                item.OverwriteDatabase(this);
                item.ReInitAfterLoadFromFile();
            }
        }

        protected void SerializeToFile()
        {
            return;
        }

        protected void AddItemToIndex(SearcherItem item, ref int lastId, Action<SearcherItem> action)
        {
            m_ItemList.Insert(lastId, item);

            // We can only set the id here as we only know the final index of the item here.
            item.OverwriteId(lastId);
            item.GeneratePath();

            action?.Invoke(item);

            lastId++;

            // This is used for sorting results between databases.
            item.OverwriteDatabase(this);

            if (!item.HasChildren)
                return;

            var childrenIds = new List<int>();
            foreach (SearcherItem child in item.Children)
            {
                AddItemToIndex(child, ref lastId, action);
                childrenIds.Add(child.Id);
            }

            item.OverwriteChildrenIds(childrenIds);
        }
    }
}
