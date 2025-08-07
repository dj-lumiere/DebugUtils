using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using DebugUtils.Repr.Attributes;
using DebugUtils.Repr.Formatters.Generic;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;
using DebugUtils.Repr.TypeHelpers;

namespace DebugUtils.Repr.Formatters.Standard;

[ReprOptions(needsPrefix: false)]
internal class TypeFormatter : IReprFormatter, IReprTreeFormatter
{
    public string ToRepr(object obj, ReprContext context)
    {
        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return "<Max Depth Reached>";
        }

        var typeObject = (Type)obj;

        if (typeObject.IsGenericTypeDefinition)
        {
            return $"Type<{typeObject.FullName}> (generic definition)";
        }

        if (typeObject.IsConstructedGenericType)
        {
            return $"Type<{typeObject.GetReprTypeName()}> (constructed)";
        }

        return $"Type<{typeObject.FullName}>";
    }
    public JsonNode ToReprTree(object obj, ReprContext context)
    {
        var typeObject = (Type)obj;
        var type = typeObject.GetType();

        if (context.Config.MaxDepth >= 0 && context.Depth >= context.Config.MaxDepth)
        {
            return new JsonObject
            {
                [propertyName: "type"] = type.GetReprTypeName(),
                [propertyName: "kind"] = type.GetTypeKind(),
                [propertyName: "maxDepthReached"] = "true",
                [propertyName: "depth"] = context.Depth
            };
        }

        var result = new JsonObject();
        result.Add(propertyName: "type", value: type.GetReprTypeName());
        result.Add(propertyName: "kind", value: type.GetTypeKind());
        result.Add(propertyName: "hashCode", value: $"0x{RuntimeHelpers.GetHashCode(o: obj):X8}");
        result.Add(propertyName: "namespace", value: typeObject.Namespace);
        result.Add(propertyName: "name", value: typeObject.Name);
        result.Add(propertyName: "fullName", value: typeObject.FullName);
        var assemblyInfo = new JsonObject();
        assemblyInfo.Add(propertyName: "name", value: typeObject.Assembly.GetName()
                                                                .Name);
        assemblyInfo.Add(propertyName: "version", value: typeObject.Assembly
           .GetName()
           .Version
          ?.ToString());
        assemblyInfo.Add(propertyName: "publicKeyToken", value: typeObject.Assembly
           .GetName()
           .GetPublicKeyToken()
          ?.ToHexString() ?? "null");
        assemblyInfo.Add(propertyName: "culture", value: typeObject.Assembly.GetName()
           .CultureName ?? "neutral");
        result.Add(propertyName: "assembly", value: assemblyInfo);
        result.Add(propertyName: "guid", value: typeObject.GUID.ToString());
        result.Add(propertyName: "typeHandle",
            value: typeObject.TypeHandle.Value.ToRepr(context: context));
        result.Add(propertyName: "baseType", value: typeObject.BaseType?.GetReprTypeName());
        var propertiesStartsWithIs = type
                                    .GetProperties(bindingAttr: BindingFlags.Public |
                                                                BindingFlags.Instance)
                                    .Where(predicate: p =>
                                         p.CanRead &&
                                         p.PropertyType == typeof(bool) &&
                                         !p.Name.IsCompilerGeneratedName() &&
                                         p.Name.StartsWith(value: "Is"))
                                    .OrderByDescending(keySelector: p =>
                                         (bool)p.GetValue(obj: obj)!)
                                    .ThenBy(keySelector: p => p.Name);
        var properties = new JsonArray();
        var availableProperties = new JsonArray();
        var propertyCount = 0;
        foreach (var property in propertiesStartsWithIs)
        {
            if (context.Config.MaxPropertiesPerObject >= 0 &&
                propertyCount >= context.Config.MaxPropertiesPerObject)
            {
                if (availableProperties.Count != 0)
                {
                    availableProperties.Add(item: "...");
                }

                if (properties.Count != 0)
                {
                    properties.Add(item: "...");
                }

                break;
            }

            var propertyValue = property.GetValue(obj: obj);
            if (propertyValue is true)
            {
                properties.Add(value: property.Name[2..]
                                              .ToLowerInvariant());
            }

            availableProperties.Add(value: property.Name[2..]
                                                   .ToLowerInvariant());

            propertyCount += 1;
        }

        result.Add(propertyName: "properties", value: properties);
        result.Add(propertyName: "availableProperties", value: availableProperties);

        return result;
    }
}

internal static class AssemblyExtensions
{
    public static string ToHexString(this byte[]? bytes)
    {
        return bytes == null
            ? "null"
            : BitConverter.ToString(value: bytes)
                          .Replace(oldValue: "-", newValue: "")
                          .ToLowerInvariant();
    }
}