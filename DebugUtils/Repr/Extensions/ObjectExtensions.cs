using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DebugUtils.Repr.Extensions;

internal static class ObjectExtensions
{
    /// <summary>
    /// Executes a property getter with optional timeout protection.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="obj">The object to get the property value from.</param>
    /// <param name="property">The property to access.</param>
    /// <param name="timeoutMs">Timeout in milliseconds. Negative values disable timeout protection.</param>
    /// <param name="onSuccess">Function to execute when property access succeeds.</param>
    /// <param name="onTimeout">Function to execute when property access times out.</param>
    /// <param name="onError">Function to execute when property access throws an exception.</param>
    /// <returns>The result from the appropriate handler function.</returns>
    private static T ExecutePropertyWithTimeout<T>(
        object obj,
        PropertyInfo property,
        int timeoutMs,
        Func<object?, T> onSuccess,
        Func<T> onTimeout,
        Func<Exception, T> onError)
    {
        if (timeoutMs < 0)
        {
            // Timeout disabled - call directly without protection
            try
            {
                var value = property.GetValue(obj);
                return onSuccess(value);
            }
            catch (Exception e)
            {
                var actualException = e.GetRootException();
                return onError(actualException);
            }
        }

        // Use timeout protection
        try
        {
            var task = Task.Run(() => property.GetValue(obj));
            if (task.Wait(timeoutMs))
            {
                var value = task.Result;
                return onSuccess(value);
            }

            return onTimeout();
        }
        catch (Exception e)
        {
            var actualException = e.GetRootException();
            return onError(actualException);
        }
    }

    public static bool TryGetAutoPropInfo(this FieldInfo f, out string propName)
    {
        propName = null!;
        var n = f.Name; // "<PropName>k__BackingField"
        if (n.Length < 5 || n[index: 0] != '<')
        {
            return false;
        }

        var close = n.IndexOf(value: '>');
        if (close <= 1)
        {
            return false;
        }

        if (!n.EndsWith(value: "k__BackingField", comparisonType: StringComparison.Ordinal))
        {
            return false;
        }

        propName = n.Substring(startIndex: 1, length: close - 1);
        return true;
    }
    public static bool TryGetAnonymousInfo(this FieldInfo f, out string propName)
    {
        propName = null!;
        var n = f.Name; // "<PropName>i__Field"
        if (n.Length < 5 || n[index: 0] != '<')
        {
            return false;
        }

        var close = n.IndexOf(value: '>');
        if (close <= 1)
        {
            return false;
        }

        if (!n.EndsWith(value: "i__Field", comparisonType: StringComparison.Ordinal))
        {
            return false;
        }

        propName = n.Substring(startIndex: 1, length: close - 1);
        return true;
    }
    public static bool IsCompilerGeneratedName(this string fieldName)
    {
        return fieldName.Contains(value: "k__BackingField") || // Auto-property backing fields
               fieldName.Contains(value: "i__Field") || // Auto-property backing fields
               fieldName.StartsWith(value: "<") || // Most compiler-generated fields
               fieldName.Contains(value: "__") || // Various compiler patterns
               fieldName.Contains(value: "DisplayClass") || // Closure fields
               fieldName.EndsWith(value: "__0") || // State machine fields
               fieldName.Contains(value: "c__Iterator") || // Iterator fields
               fieldName == "EqualityContract"; // Record EqualityContract
    }

    public static string ToReprParts(this object obj, FieldInfo f, ReprContext context)
    {
        var value = f.GetValue(obj: obj);
        return $"{f.Name}: {value.Repr(context: context.WithIncrementedDepth())}";
    }
    public static string ToReprParts(this object obj, (PropertyInfo p, FieldInfo f) pair,
        ReprContext context)
    {
        var value = pair.f.GetValue(obj: obj);
        return $"{pair.p.Name}: {value.Repr(context: context.WithIncrementedDepth())}";
    }
    public static string ToReprParts(this object obj, PropertyInfo p, ReprContext context)
    {
        return ExecutePropertyWithTimeout(
            obj: obj,
            property: p,
            timeoutMs: context.Config.MaxMemberTimeMs,
            onSuccess: value => $"{p.Name}: {value.Repr(context: context.WithIncrementedDepth())}",
            onTimeout: () => $"{p.Name}: [Timed Out]",
            onError: ex => $"{p.Name}: [{ex.GetType().Name}: {ex.Message}]"
        );
    }

    public static KeyValuePair<string, JsonNode?> ToReprTreeParts(this object obj, FieldInfo f,
        ReprContext context)
    {
        var value = f.GetValue(obj: obj);
        return new KeyValuePair<string, JsonNode?>(key: f.Name,
            value: value.FormatAsJsonNode(context: context.WithIncrementedDepth()));
    }
    public static KeyValuePair<string, JsonNode?> ToReprTreeParts(this object obj,
        (PropertyInfo p, FieldInfo f) pair, ReprContext context)
    {
        var value = pair.f.GetValue(obj: obj);
        return new KeyValuePair<string, JsonNode?>(key: pair.p.Name,
            value: value.FormatAsJsonNode(context: context.WithIncrementedDepth()));
    }
    public static KeyValuePair<string, JsonNode?> ToReprTreeParts(this object obj, PropertyInfo p,
        ReprContext context)
    {
        return ExecutePropertyWithTimeout(
            obj: obj,
            property: p,
            timeoutMs: context.Config.MaxMemberTimeMs,
            onSuccess: value => new KeyValuePair<string, JsonNode?>(key: p.Name,
                value: value.FormatAsJsonNode(context: context.WithIncrementedDepth())),
            onTimeout: () => new KeyValuePair<string, JsonNode?>(key: p.Name, value: "[Timed Out]"),
            onError: ex => new KeyValuePair<string, JsonNode?>(key: p.Name,
                value: $"[{ex.GetType().Name}: {ex.Message}]")
        );
    }


    public static string ToPrivateReprParts(this object obj, FieldInfo f, ReprContext context)
    {
        var value = f.GetValue(obj: obj);
        return $"private_{f.Name}: {value.Repr(context: context.WithIncrementedDepth())}";
    }
    public static string ToPrivateReprParts(this object obj, (PropertyInfo p, FieldInfo f) pair,
        ReprContext context)
    {
        var value = pair.f.GetValue(obj: obj);
        return $"private_{pair.p.Name}: {value.Repr(context: context.WithIncrementedDepth())}";
    }
    public static string ToPrivateReprParts(this object obj, PropertyInfo p, ReprContext context)
    {
        return ExecutePropertyWithTimeout(
            obj: obj,
            property: p,
            timeoutMs: context.Config.MaxMemberTimeMs,
            onSuccess: value => $"private_{p.Name}: {value.Repr(context: context.WithIncrementedDepth())}",
            onTimeout: () => $"private_{p.Name}: [Timed Out]",
            onError: ex => $"private_{p.Name}: [{ex.GetType().Name}: {ex.Message}]"
        );
    }
    public static KeyValuePair<string, JsonNode?> ToPrivateReprTreeParts(this object obj,
        FieldInfo f, ReprContext context)
    {
        var value = f.GetValue(obj: obj);
        return new KeyValuePair<string, JsonNode?>(key: $"private_{f.Name}",
            value: value.FormatAsJsonNode(context: context.WithIncrementedDepth()));
    }
    public static KeyValuePair<string, JsonNode?> ToPrivateReprTreeParts(this object obj,
        (PropertyInfo p, FieldInfo f) pair, ReprContext context)
    {
        var value = pair.f.GetValue(obj: obj);
        return new KeyValuePair<string, JsonNode?>(key: $"private_{pair.p.Name}",
            value: value.FormatAsJsonNode(context: context.WithIncrementedDepth()));
    }
    public static KeyValuePair<string, JsonNode?> ToPrivateReprTreeParts(this object obj,
        PropertyInfo p, ReprContext context)
    {
        return ExecutePropertyWithTimeout(
            obj: obj,
            property: p,
            timeoutMs: context.Config.MaxMemberTimeMs,
            onSuccess: value => new KeyValuePair<string, JsonNode?>(key: $"private_{p.Name}",
                value: value.FormatAsJsonNode(context: context.WithIncrementedDepth())),
            onTimeout: () => new KeyValuePair<string, JsonNode?>(key: $"private_{p.Name}",
                value: "[Timed Out]"),
            onError: ex => new KeyValuePair<string, JsonNode?>(key: $"private_{p.Name}",
                value: $"[{ex.GetType().Name}: {ex.Message}]")
        );
    }

    public static ObjectMembers GetObjectMembers(this object obj, ReprContext context)
    {
        var type = obj.GetType();
        var publicFields = new List<FieldInfo>();
        var publicAutoProps = new List<(PropertyInfo prop, FieldInfo backing)>();
        var publicProperties = new List<PropertyInfo>();
        var privateFields = new List<FieldInfo>();
        var privateAutoProps = new List<(PropertyInfo prop, FieldInfo backing)>();
        var privateProperties = new List<PropertyInfo>();
        var shown = 0;
        var truncated = false;

        // Get public fields based on ViewMode
        if (ShouldIncludePublicFields(mode: context.Config.ViewMode))
        {
            foreach (var field in type
                                 .GetFields(bindingAttr: BindingFlags.Public |
                                                         BindingFlags.Instance)
                                 .OrderBy(keySelector: f => f.Name))
            {
                if (IsLimitReached(shown: shown, context: context))
                {
                    truncated = true;
                    break;
                }

                publicFields.Add(item: field);
                shown += 1;
            }
        }

        // Get public auto-properties based on ViewMode
        if (ShouldIncludePublicAutoProps(mode: context.Config.ViewMode))
        {
            var publicProps = GetPublicAutoProps(type: type);
            var nonPubFields =
                type.GetFields(bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance);
            var publicPairs = GetAutoPropPairs(fields: nonPubFields, properties: publicProps);

            foreach (var pair in publicPairs.OrderBy(keySelector: p => p.prop.Name))
            {
                if (IsLimitReached(shown: shown, context: context))
                {
                    truncated = true;
                    break;
                }

                publicAutoProps.Add(item: pair);
                shown += 1;
            }
        }

        if (ShouldIncludePublicProperties(mode: context.Config.ViewMode))
        {
            var publicProps = GetPublicProperties(type: type, publicAutoProps: publicAutoProps
               .Select(selector: p => p.prop.Name)
               .ToHashSet());

            foreach (var pair in publicProps)
            {
                if (IsLimitReached(shown: shown, context: context))
                {
                    truncated = true;
                    break;
                }

                publicProperties.Add(item: pair);
                shown += 1;
            }
        }

        // Get private members based on ViewMode
        if (ShouldIncludePrivateAutoProps(mode: context.Config.ViewMode))
        {
            var nonPubFields =
                type.GetFields(bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance);
            var usedBackers =
                new HashSet<FieldInfo>(
                    collection: publicAutoProps.Select(selector: p => p.backing));
            var privateProps = GetPrivateAutoProps(type: type);
            var privatePairs = GetAutoPropPairs(fields: nonPubFields, properties: privateProps);
            usedBackers.UnionWith(other: privatePairs.Select(selector: p => p.backing));

            // Add remaining private fields (not auto-property backing fields)
            foreach (var field in nonPubFields.OrderBy(keySelector: f => f.Name))
            {
                if (usedBackers.Contains(item: field) || field.Name.IsCompilerGeneratedName() ||
                    field.Name == "EqualityContract")
                {
                    continue;
                }

                if (IsLimitReached(shown: shown, context: context))
                {
                    truncated = true;
                    break;
                }

                privateFields.Add(item: field);
                shown += 1;
            }

            // Add private auto-properties
            foreach (var pair in privatePairs.OrderBy(keySelector: p => p.prop.Name))
            {
                if (IsLimitReached(shown: shown, context: context))
                {
                    truncated = true;
                    break;
                }

                privateAutoProps.Add(item: pair);
                shown += 1;
            }
        }

        if (ShouldIncludePrivateProperties(mode: context.Config.ViewMode))
        {
            var privateProps = GetPrivateProperties(type: type, privateAutoProps: privateAutoProps
               .Select(selector: p => p.prop.Name)
               .ToHashSet());

            foreach (var pair in privateProps)
            {
                if (IsLimitReached(shown: shown, context: context))
                {
                    truncated = true;
                    break;
                }

                privateProperties.Add(item: pair);
                shown += 1;
            }
        }

        return new ObjectMembers(publicFields, publicAutoProps, publicProperties, privateFields,
            privateAutoProps, privateProperties, truncated);
    }

    public static bool ShouldIncludePublicFields(MemberReprMode mode)
    {
        return mode == MemberReprMode.PublicFieldAutoProperty ||
               mode == MemberReprMode.AllPublic || mode == MemberReprMode.AllFieldAutoProperty ||
               mode == MemberReprMode.Everything;
    }
    public static bool ShouldIncludePublicAutoProps(MemberReprMode mode)
    {
        return mode == MemberReprMode.PublicFieldAutoProperty ||
               mode == MemberReprMode.AllPublic || mode == MemberReprMode.AllFieldAutoProperty ||
               mode == MemberReprMode.Everything;
    }
    public static bool ShouldIncludePublicProperties(MemberReprMode mode)
    {
        return mode == MemberReprMode.AllPublic || mode == MemberReprMode.Everything;
    }
    public static bool ShouldIncludePrivateAutoProps(MemberReprMode mode)
    {
        return mode == MemberReprMode.AllFieldAutoProperty || mode == MemberReprMode.Everything;
    }
    public static bool ShouldIncludePrivateProperties(MemberReprMode mode)
    {
        return mode == MemberReprMode.Everything;
    }

    public static bool IsLimitReached(int shown, ReprContext context)
    {
        return context.Config.MaxItemsPerContainer >= 0 &&
               shown >= context.Config.MaxItemsPerContainer;
    }

    public static Dictionary<string, PropertyInfo> GetPublicAutoProps(Type type)
    {
        return type.GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                   .Where(predicate: p => p.CanRead && p.GetIndexParameters()
                                                        .Length == 0 &&
                                          !p.Name.IsCompilerGeneratedName())
                   .ToDictionary(keySelector: p => p.Name, elementSelector: p => p);
    }

    public static IEnumerable<PropertyInfo> GetPublicProperties(Type type,
        HashSet<string> publicAutoProps)
    {
        return type.GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
                   .Where(predicate: p => p.CanRead && p.GetIndexParameters()
                                                        .Length == 0 &&
                                          !p.Name.IsCompilerGeneratedName() &&
                                          !publicAutoProps.Contains(item: p.Name))
                   .OrderBy(keySelector: f => f.Name);
    }

    public static Dictionary<string, PropertyInfo> GetPrivateAutoProps(Type type)
    {
        return type.GetProperties(bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance)
                   .Where(predicate: p => p.CanRead && p.GetIndexParameters()
                                                        .Length == 0 &&
                                          !p.Name.IsCompilerGeneratedName())
                   .ToDictionary(keySelector: p => p.Name, elementSelector: p => p);
    }
    public static IEnumerable<PropertyInfo> GetPrivateProperties(Type type,
        HashSet<string> privateAutoProps)
    {
        return type.GetProperties(bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance)
                   .Where(predicate: p => p.CanRead && p.GetIndexParameters()
                                                        .Length == 0 &&
                                          !p.Name.IsCompilerGeneratedName() &&
                                          !privateAutoProps.Contains(item: p.Name))
                   .OrderBy(keySelector: f => f.Name);
    }

    public static List<(PropertyInfo prop, FieldInfo backing)> GetAutoPropPairs(FieldInfo[] fields,
        Dictionary<string, PropertyInfo> properties)
    {
        var pairs = new List<(PropertyInfo prop, FieldInfo backing)>();
        foreach (var field in fields)
        {
            if ((field.TryGetAutoPropInfo(propName: out var propName) ||
                 field.TryGetAnonymousInfo(propName: out propName)) &&
                properties.TryGetValue(key: propName, value: out var prop))
            {
                pairs.Add(item: (prop, field));

            }
        }

        return pairs;
    }

    private static Exception GetRootException(this Exception exception)
    {
        return exception switch
        {
            AggregateException aggEx when aggEx.InnerExceptions.Count == 1 => aggEx.InnerException!
               .GetRootException(),
            TargetInvocationException { InnerException: not null } targetEx => targetEx
               .InnerException!.GetRootException(),
            _ when exception.InnerException != null => exception.InnerException.GetRootException(),
            _ => exception
        };
    }

    public readonly struct ObjectMembers
    {
        public List<FieldInfo> PublicFields { get; }
        public List<(PropertyInfo prop, FieldInfo backing)> PublicAutoProps { get; }
        public List<PropertyInfo> PublicProperties { get; }
        public List<FieldInfo> PrivateFields { get; }
        public List<(PropertyInfo prop, FieldInfo backing)> PrivateAutoProps { get; }
        public List<PropertyInfo> PrivateProperties { get; }
        public bool Truncated { get; }

        public ObjectMembers(List<FieldInfo> publicFields,
            List<(PropertyInfo prop, FieldInfo backing)> publicAutoProps,
            List<PropertyInfo> publicProperties, List<FieldInfo> privateFields,
            List<(PropertyInfo prop, FieldInfo backing)> privateAutoProps,
            List<PropertyInfo> privateProperties, bool truncated)
        {
            PublicFields = publicFields;
            PublicAutoProps = publicAutoProps;
            PublicProperties = publicProperties;
            PrivateFields = privateFields;
            PrivateAutoProps = privateAutoProps;
            PrivateProperties = privateProperties;
            Truncated = truncated;
        }
    }
}