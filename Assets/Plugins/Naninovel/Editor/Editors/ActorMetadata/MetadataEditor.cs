// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public abstract class MetadataEditor
    {
        public abstract void Draw (SerializedProperty serializedProperty, ActorMetadata metadata);
    }

    public abstract class MetadataEditor<TActor, TMeta> : MetadataEditor
        where TActor : IActor
        where TMeta : ActorMetadata
    {
        protected TMeta Metadata { get; private set; }
        protected bool HasResources { get; private set; }
        protected bool IsGeneric { get; private set; }

        private const string customDataPropertyName = "customData";
        private static readonly string[] implementations;
        private static readonly string[] implementationLabels;
        private static readonly Action<SerializedProperty> defaultDrawer = p => EditorGUILayout.PropertyField(p);
        private static readonly Action<SerializedProperty> noneDrawer = _ => { };

        static MetadataEditor ()
        {
            implementations = ActorImplementations.GetImplementations<TActor>().Select(t => t.AssemblyQualifiedName).ToArray();
            implementationLabels = implementations.Select(s => s.GetBefore(",")).ToArray();
        }

        public override void Draw (SerializedProperty serializedProperty, ActorMetadata metadata)
        {
            Draw(serializedProperty, metadata as TMeta);
        }

        public void Draw (SerializedProperty serializedProperty, TMeta metadata)
        {
            SetEditedMetadata(metadata);

            var propertyCopy = serializedProperty.Copy();
            var endProperty = propertyCopy.GetEndProperty();
            var implementationProperty = propertyCopy.FindPropertyRelative(nameof(ActorMetadata.Implementation));

            propertyCopy.NextVisible(true);
            do
            {
                if (SerializedProperty.EqualContents(propertyCopy, endProperty)) break;
                if (TryDrawCustomProperty(propertyCopy)) continue;
                if (TryDrawImplementationPopup(propertyCopy)) continue;
                if (TryDrawCustomData(propertyCopy)) continue;
                EditorGUILayout.PropertyField(propertyCopy, true);
            } while (propertyCopy.NextVisible(false));
        }

        protected virtual Action<SerializedProperty> GetCustomDrawer (string propertyName)
        {
            switch (propertyName)
            {
                case nameof(ActorMetadata.Loader): return DrawWhen(HasResources);
            }
            return null;
        }

        protected Action<SerializedProperty> DrawWhen (bool condition)
        {
            return DrawWhen(condition, defaultDrawer);
        }

        protected Action<SerializedProperty> DrawWhen (bool condition, Action<SerializedProperty> drawer)
        {
            return condition ? drawer : noneDrawer;
        }

        protected Action<SerializedProperty> DrawNothing ()
        {
            return noneDrawer;
        }

        private void SetEditedMetadata (TMeta metadata)
        {
            Metadata = metadata;
            var implementation = Metadata.Implementation;
            HasResources = ActorImplementations.TryGetResourcesAttribute(implementation, out var a) && a.TypeConstraint != null;
            IsGeneric = HasResources && typeof(GenericActorBehaviour).IsAssignableFrom(a.TypeConstraint);
        }

        private bool TryDrawCustomProperty (SerializedProperty property)
        {
            var customDrawer = GetCustomDrawer(property.name);
            if (customDrawer is null) return false;
            customDrawer.Invoke(property);
            return true;
        }

        private bool TryDrawImplementationPopup (SerializedProperty property)
        {
            if (!property.propertyPath.EndsWithFast(nameof(ActorMetadata.Implementation))) return false;
            var label = EditorGUI.BeginProperty(Rect.zero, null, property);
            var curIndex = implementations.IndexOf(property.stringValue ?? string.Empty);
            var newIndex = EditorGUILayout.Popup(label, curIndex, implementationLabels);
            property.stringValue = implementations.IsIndexValid(newIndex) ? implementations[newIndex] : string.Empty;
            return true;
        }

        private bool TryDrawCustomData (SerializedProperty propertyCopy)
        {
            if (!propertyCopy.propertyPath.EndsWithFast(customDataPropertyName)) return false;
            if (!ActorImplementations.TryGetCustomDataType(Metadata.Implementation, out var dataType)) return true;

            if (!propertyCopy.hasVisibleChildren || propertyCopy.managedReferenceFullTypename?.GetAfter(" ")?.Replace("/", "+") != dataType.FullName)
                propertyCopy.managedReferenceValue = Activator.CreateInstance(dataType);

            var customMetaProperty = propertyCopy.Copy();
            var endCustomMetaProperty = customMetaProperty.GetEndProperty();
            customMetaProperty.NextVisible(true);
            do
            {
                if (SerializedProperty.EqualContents(customMetaProperty, endCustomMetaProperty)) break;
                EditorGUILayout.PropertyField(customMetaProperty, true);
            } while (customMetaProperty.NextVisible(false));

            return true;
        }
    }
}
