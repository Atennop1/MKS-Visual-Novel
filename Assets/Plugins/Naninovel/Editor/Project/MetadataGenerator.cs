// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Naninovel.Bridging;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Naninovel
{
    public static class MetadataGenerator
    {
        public static ProjectMetadata GenerateProjectMetadata ()
        {
            try
            {
                var meta = new ProjectMetadata();
                DisplayProgress("Processing commands...", 0);
                var customCommands = Command.CommandTypes.Values.Where(t => t.Namespace != Command.DefaultNamespace).ToList();
                meta.Commands = GenerateCommandsMetadata(customCommands, ResolveCustomCommandDocs, ResolveCustomParameterDocs);
                DisplayProgress("Processing resources...", .25f);
                meta.Resources = GenerateResourcesMetadata();
                DisplayProgress("Processing actors...", .50f);
                meta.Actors = GenerateActorsMetadata();
                DisplayProgress("Processing variables...", .75f);
                meta.Variables = GenerateVariablesMetadata();
                DisplayProgress("Processing functions...", .95f);
                meta.Functions = GenerateFunctionsMetadata();
                DisplayProgress("Processing constants...", .99f);
                meta.Constants = GenerateConstantsMetadata(customCommands);
                return meta;
            }
            finally { EditorUtility.ClearProgressBar(); }

            void DisplayProgress (string info, float progress)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Generating Metadata", info, progress))
                    throw new OperationCanceledException("Metadata generation cancelled by the user.");
            }
        }

        public static string SerializeMetadata (ProjectMetadata meta)
        {
            var message = new UpdateMetadata { Metadata = meta };
            return Serializer.Serialize(message);
        }

        public static ProjectMetadata DeserializeMetadata (string xml)
        {
            if (!Serializer.TryDeserialize<UpdateMetadata>(xml, out var message))
                throw new FormatException("Provided metadata string is not a correct format.");
            return message.Metadata;
        }

        public static Bridging.Command[] GenerateCommandsMetadata ()
        {
            return GenerateCommandsMetadata(Command.CommandTypes.Values, _ => (null, null, null), _ => null);
        }

        public static Bridging.Command[] GenerateCommandsMetadata (IReadOnlyCollection<Type> commands,
            Func<Type, (string summary, string remarks, string examples)> getCommandDoc, Func<FieldInfo, string> getParamDoc)
        {
            var commandsMeta = new List<Bridging.Command>();
            foreach (var commandType in commands)
            {
                (string summary, string remarks, string examples) = getCommandDoc(commandType);
                var metadata = new Bridging.Command {
                    Id = commandType.Name,
                    Alias = GetData<Command.CommandAliasAttribute>(commandType, 0) as string,
                    Localizable = typeof(Command.ILocalizable).IsAssignableFrom(commandType),
                    Summary = summary,
                    Remarks = remarks,
                    Examples = examples,
                    Parameters = ExtractParametersMetadata(commandType, getParamDoc)
                };
                commandsMeta.Add(metadata);
            }
            return commandsMeta.OrderBy(c => string.IsNullOrEmpty(c.Alias) ? c.Id : c.Alias).ToArray();
        }

        public static Constant[] GenerateConstantsMetadata () => GenerateConstantsMetadata(Command.CommandTypes.Values);

        public static Constant[] GenerateConstantsMetadata (IEnumerable<Type> commands)
        {
            var enumType = new HashSet<Type>();
            foreach (var field in commands.SelectMany(GetParameterFields))
                if (field.GetCustomAttribute<ConstantContextAttribute>() is ConstantContextAttribute attribute)
                    enumType.Add(attribute.EnumType);
            var constants = new List<Constant>();
            foreach (var type in enumType)
                constants.Add(new Constant { Name = type.Name, Values = Enum.GetNames(type) });
            return constants.ToArray();
        }

        public static Bridging.Resource[] GenerateResourcesMetadata ()
        {
            var resources = new List<Bridging.Resource>();
            var editorResources = EditorResources.LoadOrDefault();
            var records = editorResources.GetAllRecords();
            foreach (var kv in records)
            {
                var record = editorResources.GetRecordByGuid(kv.Value);
                if (!record.HasValue) continue;
                var resource = new Bridging.Resource {
                    Type = record.Value.PathPrefix,
                    Path = record.Value.Name
                };
                resources.Add(resource);
            }
            return resources.ToArray();
        }

        public static Actor[] GenerateActorsMetadata ()
        {
            var actors = new List<Actor>();
            var editorResources = EditorResources.LoadOrDefault();
            var allResources = editorResources.GetAllRecords().Keys.ToArray();
            var chars = ProjectConfigurationProvider.LoadOrDefault<CharactersConfiguration>().Metadata.ToDictionary();
            foreach (var kv in chars)
            {
                var charActor = new Actor {
                    Id = kv.Key,
                    Description = kv.Value.DisplayName,
                    Type = kv.Value.Loader.PathPrefix,
                    Appearances = FindAppearances(kv.Key, kv.Value.Loader.PathPrefix, kv.Value.Implementation)
                };
                actors.Add(charActor);
            }
            var backs = ProjectConfigurationProvider.LoadOrDefault<BackgroundsConfiguration>().Metadata.ToDictionary();
            foreach (var kv in backs)
            {
                var backActor = new Actor {
                    Id = kv.Key,
                    Type = kv.Value.Loader.PathPrefix,
                    Appearances = FindAppearances(kv.Key, kv.Value.Loader.PathPrefix, kv.Value.Implementation)
                };
                actors.Add(backActor);
            }
            var choiceHandlers = ProjectConfigurationProvider.LoadOrDefault<ChoiceHandlersConfiguration>().Metadata.ToDictionary();
            foreach (var kv in choiceHandlers)
            {
                var choiceHandlerActor = new Actor {
                    Id = kv.Key,
                    Type = kv.Value.Loader.PathPrefix
                };
                actors.Add(choiceHandlerActor);
            }
            var printers = ProjectConfigurationProvider.LoadOrDefault<TextPrintersConfiguration>().Metadata.ToDictionary();
            foreach (var kv in printers)
            {
                var printerActor = new Actor {
                    Id = kv.Key,
                    Type = kv.Value.Loader.PathPrefix
                };
                actors.Add(printerActor);
            }
            return actors.ToArray();

            string[] FindAppearances (string actorId, string pathPrefix, string actorImplementation)
            {
                var prefabPath = allResources.FirstOrDefault(p => p.EndsWithFast($"{pathPrefix}/{actorId}"));
                var assetGUID = prefabPath != null ? editorResources.GetGuidByPath(prefabPath) : null;
                var assetPath = assetGUID != null ? AssetDatabase.GUIDToAssetPath(assetGUID) : null;
                var prefabAsset = assetPath != null ? AssetDatabase.LoadMainAssetAtPath(assetPath) : null;
                if (prefabAsset != null && actorImplementation.Contains("Layered"))
                {
                    var layeredBehaviour = (prefabAsset as GameObject)?.GetComponent<LayeredActorBehaviour>();
                    return layeredBehaviour != null ? layeredBehaviour.GetCompositionMap().Keys.ToArray() : Array.Empty<string>();
                }
                else if (prefabAsset != null && (actorImplementation.Contains("Generic") || actorImplementation.Contains("Live2D")))
                {
                    var animator = (prefabAsset as GameObject)?.GetComponent<Animator>();
                    var controller = animator != null ? animator.runtimeAnimatorController as AnimatorController : null;
                    return controller != null
                        ? controller.parameters.Where(p => p.type == AnimatorControllerParameterType.Trigger).Select(p => p.name).ToArray()
                        : Array.Empty<string>();
                }
                #if SPRITE_DICING_AVAILABLE
                else if (prefabAsset != null && actorImplementation.Contains("Diced"))
                {
                    return (prefabAsset as SpriteDicing.DicedSpriteAtlas)?.Sprites.Select(s => s.name).ToArray() ?? Array.Empty<string>();
                }
                #endif
                else
                {
                    var multiplePrefix = $"{pathPrefix}/{actorId}/";
                    return allResources.Where(p => p.Contains(multiplePrefix)).Select(p => p.GetAfter(multiplePrefix)).ToArray();
                }
            }
        }

        public static string[] GenerateVariablesMetadata ()
        {
            var config = ProjectConfigurationProvider.LoadOrDefault<CustomVariablesConfiguration>();
            return config.PredefinedVariables.Select(p => p.Name).ToArray();
        }

        public static string[] GenerateFunctionsMetadata ()
        {
            return Engine.Types
                .Where(t => t.Namespace != typeof(ExpressionFunctions).Namespace && t.IsDefined(typeof(ExpressionFunctionsAttribute)))
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static)).Select(m => m.Name).Distinct().ToArray();
        }

        private static CustomAttributeData GetData<T> (MemberInfo info)
        {
            return info.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(T));
        }

        private static object GetData<T> (MemberInfo info, int arg)
        {
            return GetData<T>(info)?.ConstructorArguments[arg].Value;
        }

        private static (string, string, string) ResolveCustomCommandDocs (Type type)
        {
            var summary = GetData<DocumentationAttribute>(type, 0) as string;
            var remarks = GetData<DocumentationAttribute>(type, 1) as string;
            return (summary, remarks, null);
        }

        private static string ResolveCustomParameterDocs (FieldInfo field)
        {
            return GetData<DocumentationAttribute>(field, 0) as string;
        }

        private static Parameter[] ExtractParametersMetadata (Type commandType, Func<FieldInfo, string> summaryResolver)
        {
            var result = new List<Parameter>();
            foreach (var fieldInfo in GetParameterFields(commandType))
                result.Add(ExtractParameterMetadata(fieldInfo, summaryResolver));
            return result.ToArray();
        }

        private static FieldInfo[] GetParameterFields (Type commandType)
        {
            return commandType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => !x.IsSpecialName && !x.GetCustomAttributes<ObsoleteAttribute>().Any())
                .Where(f => f.FieldType.GetInterface(nameof(ICommandParameter)) != null).ToArray();
        }

        private static Parameter ExtractParameterMetadata (FieldInfo field, Func<FieldInfo, string> summaryResolver)
        {
            var nullableName = typeof(INullable<>).Name;
            var namedName = typeof(INamed<>).Name;
            var meta = new Parameter {
                Id = field.Name,
                Alias = GetData<Command.ParameterAliasAttribute>(field, 0) as string,
                Required = GetData<Command.RequiredParameterAttribute>(field) != null,
                Localizable = GetData<Command.LocalizableParameterAttribute>(field) != null,
                DefaultValue = GetData<Command.ParameterDefaultValueAttribute>(field, 0) as string,
                ValueContext = GetValueContext(field, false),
                NamedValueContext = GetValueContext(field, true),
                Summary = summaryResolver(field)
            };
            meta.Nameless = meta.Alias == Command.NamelessParameterAlias;
            if (TryResolveValueType(field.FieldType, out var valueType))
                meta.ValueContainerType = ValueContainerType.Single;
            else if (GetInterface(field, nameof(IEnumerable)) != null) SetListValue();
            else SetNamedValue();
            meta.ValueType = valueType;
            return meta;

            Type GetInterface (MemberInfo info, string name) => field.FieldType.GetInterface(name);

            Type GetNullableType (MemberInfo info) => GetInterface(info, nullableName).GetGenericArguments()[0];

            void SetListValue ()
            {
                var elementType = GetNullableType(field).GetGenericArguments()[0];
                var namedElementType = elementType.BaseType?.GetGenericArguments()[0];
                if (namedElementType?.GetInterface(nameof(INamedValue)) != null)
                {
                    meta.ValueContainerType = ValueContainerType.NamedList;
                    var namedType = namedElementType.GetInterface(namedName).GetGenericArguments()[0];
                    TryResolveValueType(namedType, out valueType);
                }
                else
                {
                    meta.ValueContainerType = ValueContainerType.List;
                    TryResolveValueType(elementType, out valueType);
                }
            }

            void SetNamedValue ()
            {
                meta.ValueContainerType = ValueContainerType.Named;
                var namedType = GetNullableType(field).GetInterface(namedName).GetGenericArguments()[0];
                TryResolveValueType(namedType, out valueType);
            }
        }

        private static ValueContext GetValueContext (FieldInfo field, bool named)
        {
            var context = field.GetCustomAttributes<ParameterContextAttribute>()
                .FirstOrDefault(a => a.NamedIndex < 0 || a.NamedIndex == (named ? 1 : 0));
            if (context is null) return null;
            return new ValueContext {
                Type = context is ActorContextAttribute ? ValueContextType.Actor :
                    context is AppearanceContextAttribute ? ValueContextType.Appearance :
                    context is ConstantContextAttribute ? ValueContextType.Constant :
                    context is ExpressionContextAttribute ? ValueContextType.Expression :
                    context is ResourceContextAttribute ? ValueContextType.Resource :
                    throw new FormatException($"Unknown IDE attribute: `{context.GetType().FullName}`."),
                SubType = context.SubType
            };
        }

        private static bool TryResolveValueType (Type type, out Bridging.ValueType result)
        {
            var nullableName = typeof(INullable<>).Name;
            var valueTypeName = type.GetInterface(nullableName)?.GetGenericArguments()[0].Name;
            switch (valueTypeName)
            {
                case nameof(String):
                case nameof(NullableString):
                    result = Bridging.ValueType.String;
                    return true;
                case nameof(Int32):
                case nameof(NullableInteger):
                    result = Bridging.ValueType.Integer;
                    return true;
                case nameof(Single):
                case nameof(NullableFloat):
                    result = Bridging.ValueType.Decimal;
                    return true;
                case nameof(Boolean):
                case nameof(NullableBoolean):
                    result = Bridging.ValueType.Boolean;
                    return true;
            }
            result = default;
            return false;
        }
    }
}
