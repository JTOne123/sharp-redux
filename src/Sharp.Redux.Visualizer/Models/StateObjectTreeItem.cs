﻿using Righthand.Immutable;

namespace Sharp.Redux.Visualizer.Models
{
    public class StateObjectTreeItem : NodeObjectTreeItem
    {
        public StateObjectTreeItem(Sharp.Redux.Visualizer.Models.ObjectTreeItem[] children, string propertyName, string typeName, bool isRoot, Sharp.Redux.Visualizer.Core.DiffType diffType) : base(children, propertyName, typeName, isRoot, diffType)
        {
        }

        public StateObjectTreeItem Clone(Param<Sharp.Redux.Visualizer.Models.ObjectTreeItem[]>? children = null, Param<string>? propertyName = null, Param<string>? typeName = null, Param<bool>? isRoot = null, Param<Sharp.Redux.Visualizer.Core.DiffType>? diffType = null)
        {
            return new StateObjectTreeItem(children.HasValue ? children.Value.Value : Children,
propertyName.HasValue ? propertyName.Value.Value : PropertyName,
typeName.HasValue ? typeName.Value.Value : TypeName,
isRoot.HasValue ? isRoot.Value.Value : IsRoot,
diffType.HasValue ? diffType.Value.Value : DiffType);
        }
    }
}
