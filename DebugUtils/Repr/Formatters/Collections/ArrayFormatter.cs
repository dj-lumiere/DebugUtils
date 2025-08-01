﻿using DebugUtils.Repr.Formatters.Attributes;
using DebugUtils.Repr.Interfaces;
using DebugUtils.Repr.Records;

namespace DebugUtils.Repr.Formatters.Collections;

[ReprOptions(needsPrefix: true)]
internal class ArrayFormatter : IReprFormatter
{
    public string ToRepr(object obj, ReprConfig config, HashSet<int>? visited)
    {
        var array = (Array)obj;
        // Apply container defaults if configured
        config = config.GetContainerConfig();

        var rank = array.Rank;
        var content = array.ArrayToReprRecursive(indices: new int[rank], dimension: 0,
            config: config, visited: visited);
        return content;
    }
}