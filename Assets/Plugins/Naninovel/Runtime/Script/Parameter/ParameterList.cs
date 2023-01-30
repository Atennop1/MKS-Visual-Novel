// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Naninovel
{
    /// <summary>
    /// Represents a <see cref="Command"/> parameter with a collection of nullable <typeparamref name="TItem"/> values.
    /// </summary>
    public abstract class ParameterList<TItem> : CommandParameter<List<TItem>>, IEnumerable<TItem>
        where TItem : class, new()
    {
        private static readonly Regex unescapedComma = new Regex(@"(?<!\\),", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Number of items in the value collection; returns 0 when the value is not assigned.
        /// </summary>
        public int Length => HasValue ? Value.Count : 0;

        public TItem this [int index]
        {
            get => HasValue ? Value[index] : null;
            set
            {
                if (HasValue) Value[index] = value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator () => Value?.GetEnumerator();

        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator () => Value?.GetEnumerator();

        public override string ToString ()
        {
            if (!HasValue || DynamicValue) return base.ToString();
            return string.Join(",", Value.Select(ItemToString));
        }

        /// <summary>
        /// Returns element at the provided index or null, in case the index is not valid 
        /// or element at that index is not assigned (skipped in naninovel script).
        /// </summary>
        public TItem ElementAtOrNull (int index)
        {
            if (!HasValue || !Value.IsIndexValid(index)) return null;
            var value = Value[index];
            if (value is INullableValue nullable)
                return nullable.HasValue ? value : null;
            return value;
        }

        protected override List<TItem> ParseValueText (string valueText, out string errors)
        {
            errors = null;
            var items = unescapedComma.Split(valueText);
            // Restore escaped commas.
            for (int i = 0; i < items.Length; i++)
                items[i] = items[i].Replace("\\,", ",");
            var list = new List<TItem>(items.Length);
            foreach (var item in items)
                list.Add(string.IsNullOrEmpty(item) ? null : ParseItemValueText(item, out errors));
            return list;
        }

        protected abstract TItem ParseItemValueText (string valueText, out string errors);

        protected virtual string ItemToString (TItem item)
        {
            if (item is INullableValue nullableItem && !nullableItem.HasValue)
                return string.Empty;
            return item?.ToString() ?? string.Empty;
        }
    }
}
