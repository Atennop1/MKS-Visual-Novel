// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Draws a dropdown selection list of the resource paths, which are added via editor managers (aka `EditorResources`).
    /// </summary>
    public class ResourcePopupAttribute : PropertyAttribute
    {
        public const string EmptyValue = "None (disabled)";

        public readonly string Category;
        public readonly string PathPrefix;
        public readonly string EmptyOption;

        /// <param name="category">When specified, will only fetch resources under the category.</param>
        /// <param name="pathPrefix">When specified, will only fetch resources under the path prefix and trim the prefix from the values.</param>
        /// <param name="emptyOption">When specified, will include an additional option with the provided name and <see cref="string.Empty"/> value to the list.</param>
        public ResourcePopupAttribute (string category = null, string pathPrefix = null, string emptyOption = null)
        {
            Category = category;
            PathPrefix = pathPrefix;
            EmptyOption = emptyOption;
        }

        /// <param name="category">Category (usually equal path prefix) of the resources.</param>
        public ResourcePopupAttribute (string category)
            : this(category, category, EmptyValue) { }
    }

    /// <summary>
    /// Draws a dropdown selection list of the actors, which are added via editor managers (aka `EditorResources`).
    /// </summary>
    public class ActorPopupAttribute : ResourcePopupAttribute
    {
        /// <param name="category">Category (usually equal path prefix) of the actors.</param>
        public ActorPopupAttribute (string category)
            : base($"{category}/*", "*", EmptyValue) { }
    }
}
