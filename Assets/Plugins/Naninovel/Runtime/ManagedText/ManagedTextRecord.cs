// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Represents a managed text document record.
    /// </summary>
    public readonly struct ManagedTextRecord : IEquatable<ManagedTextRecord>
    {
        /// <summary>
        /// Name of the category (managed text document name) to use when
        /// no category is specified in <see cref="ManagedTextAttribute"/>.
        /// </summary>
        public const string DefaultCategoryName = "Uncategorized";

        /// <summary>
        /// Category (managed text document name) for which the record belongs to.
        /// </summary>
        public readonly string Category;
        /// <summary>
        /// Unique (inside category) key of the record.
        /// </summary>
        public readonly string Key;
        /// <summary>
        /// Value of the record.
        /// </summary>
        public readonly string Value;

        public ManagedTextRecord (string key, string value, string category)
        {
            Key = key;
            Value = value?.Replace("\\n", "\n");
            Category = category;
        }

        public override bool Equals (object obj)
        {
            return obj is ManagedTextRecord text && Equals(text);
        }

        public bool Equals (ManagedTextRecord other)
        {
            return Key == other.Key &&
                   Category == other.Category;
        }

        public override int GetHashCode ()
        {
            var hashCode = 363954443;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Category);
            return hashCode;
        }

        public static bool operator == (ManagedTextRecord left, ManagedTextRecord right)
        {
            return left.Equals(right);
        }

        public static bool operator != (ManagedTextRecord left, ManagedTextRecord right)
        {
            return !(left == right);
        }

        public string ToDocumentTextLine () => $"{Key}{ManagedTextUtils.RecordIdLiteral}{Value?.Replace("\n", "\\n") ?? string.Empty}";
    }
}
