﻿using Sharp.Redux.Visualizer.Core;
using Sharp.Redux.Visualizer.States;
using System;
using System.Collections.Generic;

namespace Sharp.Redux.Visualizer.Services.Implementation
{
    public static class StateFormatter
    {
        public const string DefaultPropertyName = "State";
        public static ObjectTreeItem ToTreeHierarchy(ObjectData source, string propertyName = DefaultPropertyName)
        {
            return ToTreeHierarchy(source, 0, propertyName);
        }
        public static ObjectTreeItem ToTreeHierarchy(ObjectData source, int depth, string propertyName)
        {
            switch (source)
            {
                case StateObjectData state:
                    return FormatState(depth, propertyName, state);
                case ListData list:
                    return FormatList(depth, propertyName, list);
                case DictionaryData dictionary:
                    return FormatDictionary(depth, propertyName, dictionary);
                case PrimitiveData primitive:
                    return FormatPrimitive(depth, propertyName, primitive);
                default:
                    throw new Exception($"Unknown ObjectData {source.GetType()}");
            }
        }
        public static ObjectTreeItem FormatPrimitive(int depth, string propertyName, PrimitiveData source)
        {
           return new ObjectTreeItem(propertyName, source.TypeName, Convert.ToString(source.Value), null, isRoot: depth==0);
        }
        public static ObjectTreeItem FormatList(int depth, string propertyName, ListData source)
        {
            var builder = new List<ObjectTreeItem>(source.List.Length);
            foreach (var item in source.List)
            {
                builder.Add(ToTreeHierarchy(item, depth+1, null));
            }
            return new ObjectTreeItem(propertyName, source.TypeName, null, builder.ToArray(), isRoot: depth == 0);
        }
        public static ObjectTreeItem FormatDictionary(int depth, string propertyName, DictionaryData source)
        {
            var builder = new List<ObjectTreeItem>(source.Dictionary.Count);
            foreach (var item in source.Dictionary)
            {
                builder.Add(ToTreeHierarchy(item.Value, depth+1, Convert.ToString(item.Key)));
            }
            return new ObjectTreeItem(propertyName, source.TypeName, null, builder.ToArray(), isRoot: depth == 0);
        }
        public static ObjectTreeItem FormatState(int depth, string propertyName, StateObjectData source)
        {
            var builder = new List<ObjectTreeItem>(source.Properties.Count);
            foreach (var item in source.Properties)
            {
                builder.Add(ToTreeHierarchy(item.Value, depth+1, item.Key));
            }
            return new ObjectTreeItem(propertyName, source.TypeName, null, builder.ToArray(), isRoot: depth == 0);
        }

        public static string GetActionName(ReduxAction action)
        {
            const string Suffix = "Action";
            var name = action.GetType().FullName;
            foreach (string prefix in ReduxVisualizer.IgnoredNamespacePrefixes)
            {
                if (name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    name = name.Substring(prefix.Length + 1);
                }
            }
            if (name.EndsWith(Suffix))
            {
                name = name.Substring(0, name.Length - Suffix.Length);
            }
            return name;
        }
    }
}