﻿using NUnit.Framework;
using Sharp.Redux.Visualizer.Core;
using Sharp.Redux.Visualizer.Models;
using Sharp.Redux.Visualizer.Services.Implementation;
using System.Collections.Immutable;
using System.Linq;

namespace Sharp.Redux.Visualizer.Test.Services.Implementation
{
    public class TreeComparerTest
    {
        public static ObjectTreeItem ToTreeItem(object state)
        {
            var currentData = PropertiesCollector.Collect(state);
            return StateFormatter.ToTreeHierarchy(currentData);
        }
        [TestFixture]
        public class CreateDifferenceTree: TreeComparerTest
        {
            [Test]
            public void WhenSimpleState_IsEqual_ReturnsNull()
            {
                var objectData = new StateObjectData("tn", ImmutableDictionary<string, ObjectData>.Empty, hasKey: false, key: null);
                var current = new StateObjectTreeItem(null, "pn", objectData, depth: 0);
                var next = new StateObjectTreeItem(null, "pn", objectData, depth: 0);

                var actual = TreeComparer.CreateDifferenceTree(current, next);

                Assert.That(actual, Is.Null);
            }
            [Test]
            public void WhenBothTreeItemsAreNull_ReturnsNull()
            {
                var actual = TreeComparer.CreateDifferenceTree(null, null);

                Assert.That(actual, Is.Null);
            }
            [Test]
            public void WhenCurrentIsPresentButNextIsNull_ReturnsRemovedDifferenceItem()
            {
                var current = CreatePrimitiveTreeItem(1);

                var actual = TreeComparer.CreateDifferenceTree(current, null);

                Assert.That(actual, Is.Not.Null);
                Assert.That(actual.DiffType, Is.EqualTo(DiffType.Removed));
                Assert.That(actual.Current, Is.SameAs(current));
                Assert.That(actual.Next, Is.Null);
            }
            [Test]
            public void WhenCurrentIsNullButNextIsValid_ReturnsAddedDifferenceItem()
            {
                var next = CreatePrimitiveTreeItem(1);

                var actual = TreeComparer.CreateDifferenceTree(null, next);

                Assert.That(actual, Is.Not.Null);
                Assert.That(actual.DiffType, Is.EqualTo(DiffType.Added));
                Assert.That(actual.Current, Is.Null);
                Assert.That(actual.Next, Is.SameAs(next));
            }
            [Test]
            public void WhenCurrentAndNextAreDifferentTypes_ReturnsModified()
            {
                var current = CreateListTreeItem();
                var next = CreatePrimitiveTreeItem(1);

                var actual = TreeComparer.CreateDifferenceTree(current, next);

                Assert.That(actual.DiffType, Is.EqualTo(DiffType.Modified));
            }
            [Test]
            public void WhenLeafChangesAndNotReferenceEquals_OnlyLeafIsCollected()
            {
                var current = new Root { First = new FirstBranch { Alpha = "a", Omega = 1 } };
                var next = new Root { First = new FirstBranch { Alpha = "ab", Omega = 1 } };

                var actual = (DifferenceItemContainer)TreeComparer.CreateDifferenceTree(ToTreeItem(current), ToTreeItem(next));

                Assert.That(actual.DiffType, Is.EqualTo(DiffType.Modified));
                var first = (DifferenceItemContainer)actual.Children[0];
                Assert.That(first.Children.Length, Is.EqualTo(1));
                var alpha = first.Children[0];
                Assert.That(alpha.DescriptionHeader, Is.EqualTo("Alpha"));
                Assert.That(alpha.DiffType, Is.EqualTo(DiffType.Modified));
            }
            [Test]
            public void WhenReferencesChangeButContentIsSame_NullIsReturned()
            {
                var current = new Root { First = new FirstBranch { Alpha = "a", Omega = 1 } };
                var next = new Root { First = new FirstBranch { Alpha = "a", Omega = 1 } };

                var actual = (DifferenceItemContainer)TreeComparer.CreateDifferenceTree(ToTreeItem(current), ToTreeItem(next));

                Assert.That(actual, Is.Null);
            }
            [Test]
            public void WhenCurrentIsNull_ButNextIsNot_ReturnsWholeNextBranchAsAdded()
            {
                var next = new Root { First = new FirstBranch { Alpha = "a", Omega = 1 } };

                var actual = (DifferenceItemContainer)TreeComparer.CreateDifferenceTree(null, ToTreeItem(next));

                Assert.That(actual.DiffType, Is.EqualTo(DiffType.Added));
                Assert.That(actual.Children.Length, Is.EqualTo(2));
                var first = actual.Children[0];
                Assert.That(first.DiffType, Is.EqualTo(DiffType.Added));
                var second = actual.Children[1];
                Assert.That(second.DiffType, Is.EqualTo(DiffType.Added));
            }
        }

        [TestFixture]
        public class FromPrimitive: TreeComparerTest
        {
            [Test]
            public void WhenSameSourceSamePropertySameValue_ReturnsNull()
            {
                var source = new PrimitiveData("tn", 1);
                var current = new PrimitiveObjectTreeItem(source.Value, null, "pn", source);
                var next = new PrimitiveObjectTreeItem(source.Value, null, "pn", source);

                var actual = TreeComparer.FromPrimitive(current, next);

                Assert.That(actual, Is.Null);
            }
            [Test]
            public void WhenDifferentSourceSamePropertySameValue_ReturnsNull()
            {
                var currentSource = new PrimitiveData("tn", 1);
                var nextSource = new PrimitiveData("tn", 1);
                var current = new PrimitiveObjectTreeItem(currentSource.Value, null, "pn", currentSource);
                var next = new PrimitiveObjectTreeItem(nextSource.Value, null, "pn", nextSource);

                var actual = TreeComparer.FromPrimitive(current, next);

                Assert.That(actual, Is.Null);
            }
            [Test]
            public void WhenDifferentSourceSamePropertyDifferentValue_ReturnsDifferenceItem()
            {
                var currentSource = new PrimitiveData("tn", 1);
                var nextSource = new PrimitiveData("tn", 2);
                var current = new PrimitiveObjectTreeItem(currentSource.Value, null, "pn", currentSource);
                var next = new PrimitiveObjectTreeItem(nextSource.Value, null, "pn", nextSource);

                var actual = TreeComparer.FromPrimitive(current, next);

                Assert.That(actual, Is.Not.Null);
            }
            [Test]
            public void WhenDifferentSourceSamePropertyDifferentValue_DifferenceItemContainsBothItems()
            {
                var currentSource = new PrimitiveData("tn", 1);
                var nextSource = new PrimitiveData("tn", 2);
                var current = new PrimitiveObjectTreeItem(currentSource.Value, null, "pn", currentSource);
                var next = new PrimitiveObjectTreeItem(nextSource.Value, null, "pn", nextSource);

                var actual = TreeComparer.FromPrimitive(current, next);

                Assert.That(actual.Current, Is.SameAs(current));
                Assert.That(actual.Next, Is.SameAs(next));
            }
            [Test]
            public void WhenDifferentSourceSamePropertyDifferentValue_DifferenceItemIsMarkedAsModified()
            {
                var currentSource = new PrimitiveData("tn", 1);
                var nextSource = new PrimitiveData("tn", 2);
                var current = new PrimitiveObjectTreeItem(currentSource.Value, null, "pn", currentSource);
                var next = new PrimitiveObjectTreeItem(nextSource.Value, null, "pn", nextSource);

                var actual = TreeComparer.FromPrimitive(current, next);

                Assert.That(actual.DiffType, Is.EqualTo(DiffType.Modified));
            }
        }

        [TestFixture]
        public class FromNamedProperties: TreeComparerTest
        {
            [Test]
            public void WhenBothHaveNoChildren_ReturnsNull()
            {
                var current = CreateStateObjectTreeItem();
                var next = CreateStateObjectTreeItem();

                var actual = TreeComparer.FromNamedProperties(current, next);

                Assert.That(actual, Is.Null);
            }
        }

        [TestFixture]
        public class FromList: TreeComparerTest
        {
            [Test]
            public void WhenBothHaveNoChildren_ReturnsNull()
            {
                var current = CreateListTreeItem();
                var next = CreateListTreeItem();

                var actual = TreeComparer.FromList(current, next);

                Assert.That(actual, Is.Null);
            }
            [Test]
            public void WhenCurrentHasChildAndNextHasNone_ReturnsModified()
            {
                var current = CreateListTreeItem(CreatePrimitiveTreeItem(5));
                var next = CreateListTreeItem();

                var actual = TreeComparer.FromList(current, next);

                Assert.That(actual, Is.InstanceOf<DifferenceItemContainer>());
                Assert.That(actual.DiffType, Is.EqualTo(DiffType.Modified));
                Assert.That(actual.Current, Is.SameAs(current));
                Assert.That(actual.Next, Is.SameAs(next));
            }
            [Test]
            public void WhenCurrentHasChildAndNextHasNone_ContainsProperChildren()
            {
                var current = CreateListTreeItem(CreatePrimitiveTreeItem(5));
                var next = CreateListTreeItem();

                var actual = TreeComparer.FromList(current, next);

                Assert.That(actual.Children.Length, Is.EqualTo(1));
                Assert.That(actual.Children[0].Current, Is.SameAs(current.Children[0]));
                Assert.That(actual.Children[0].Next, Is.Null);
                Assert.That(actual.Children[0].DiffType, Is.EqualTo(DiffType.Removed));
            }
            [Test]
            public void WhenKeyItemChanges_MarkItAsModified()
            {
                var first = CreateStateObjectTreeItem(CreateNamedPrimitiveTreeItem("Text", "a"));
                var modified = CreateStateObjectTreeItem(CreateNamedPrimitiveTreeItem("Text", "b"));
                var current = CreateListTreeItem(first);
                var next = CreateListTreeItem(modified);

                var actual = TreeComparer.FromList(current, next);

                Assert.That(actual.Children.Length, Is.EqualTo(1));
                Assert.That(actual.DiffType, Is.EqualTo(DiffType.Modified));
            }
        }

        [TestFixture]
        public class FromBranchAdded: TreeComparerTest
        {
            [Test]
            public void WhenSourceIsPrimitive_ReturnsDifferenceItemWithAdded()
            {
                var source = CreatePrimitiveTreeItem(1);

                var actual = TreeComparer.FromBranchAdded(source);

                Assert.That(actual, Is.Not.Null);
                Assert.That(actual, Is.Not.InstanceOf<DifferenceItemContainer>());
                Assert.That(actual.DiffType, Is.EqualTo(DiffType.Added));
            }
            [Test]
            public void WhenSourceIsNode_ReturnsDifferenceItemContainerWithAdded()
            {
                var source = CreateListTreeItem();

                var actual = TreeComparer.FromBranchAdded(source);

                Assert.That(actual, Is.InstanceOf<DifferenceItemContainer>());
                Assert.That(actual.DiffType, Is.EqualTo(DiffType.Added));
            }
            [Test]
            public void WhenSourceIsEmptyNode_ResultHasNoChildren()
            {
                var source = CreateListTreeItem();

                var actual = (DifferenceItemContainer)TreeComparer.FromBranchAdded(source);

                Assert.That(actual.Children.Length, Is.Zero);
            }
            [Test]
            public void WhenSourceIsNonEmptyNode_ResultHasSameNumberOfChildren()
            {
                var source = CreateListTreeItem(CreatePrimitiveTreeItem(1), CreatePrimitiveTreeItem(2));

                var actual = (DifferenceItemContainer)TreeComparer.FromBranchAdded(source);

                Assert.That(actual.Children.Length, Is.EqualTo(2));
            }
            [Test]
            public void WhenSourceIsNode_DifferenceItemContainsCorrectSources()
            {
                var source = CreateListTreeItem();

                var actual = TreeComparer.FromBranchAdded(source);

                Assert.That(actual.Current, Is.Null);
                Assert.That(actual.Next, Is.SameAs(source));
            }
        }

        [TestFixture]
        public class FromBranchModified : TreeComparerTest
        {
            [Test]
            public void WhenSourceIsPrimitive_ReturnsDifferenceItemWithNone()
            {
                var current = CreatePrimitiveTreeItem(1);
                var next = CreatePrimitiveTreeItem(2);

                var actual = TreeComparer.FromBranchModified(current, next);

                Assert.That(actual, Is.Not.Null);
                Assert.That(actual, Is.Not.InstanceOf<DifferenceItemContainer>());
                Assert.That(actual.DiffType, Is.EqualTo(DiffType.None));
            }
            [Test]
            public void WhenSourceIsNode_ReturnsDifferenceItemContainerWithNone()
            {
                var current = CreateListTreeItem();
                var next = CreateListTreeItem();

                var actual = TreeComparer.FromBranchModified(current, next);

                Assert.That(actual, Is.InstanceOf<DifferenceItemContainer>());
                Assert.That(actual.DiffType, Is.EqualTo(DiffType.None));
            }
            [Test]
            public void WhenSourceIsEmptyNode_ResultHasNoChildren()
            {
                var current = CreateListTreeItem();
                var next = CreateListTreeItem();

                var actual = (DifferenceItemContainer)TreeComparer.FromBranchModified(current, next);

                Assert.That(actual.Children.Length, Is.Zero);
            }
            [Test]
            public void WhenSourceIsNonEmptyNode_ResultHasSameNumberOfChildren()
            {
                var current = CreateListTreeItem();
                var next = CreateListTreeItem(CreatePrimitiveTreeItem(5), CreatePrimitiveTreeItem("alfa"));

                var actual = (DifferenceItemContainer)TreeComparer.FromBranchModified(current, next);

                Assert.That(actual.Children.Length, Is.EqualTo(2));
            }
            [Test]
            public void WhenSourceIsNode_DifferenceItemContainsCorrectSources()
            {
                var current = CreateListTreeItem();
                var next = CreateListTreeItem(CreatePrimitiveTreeItem(5), CreatePrimitiveTreeItem("alfa"));

                var actual = TreeComparer.FromBranchModified(current, next);

                Assert.That(actual.Current, Is.SameAs(current));
                Assert.That(actual.Next, Is.SameAs(next));
            }
        }

        public PrimitiveObjectTreeItem CreatePrimitiveTreeItem(object value)
        {
            var data = new PrimitiveData("tn", value);
            return new PrimitiveObjectTreeItem(data.Value, null, $"pn{value}", data);
        }
        public PrimitiveObjectTreeItem CreateNamedPrimitiveTreeItem(string propertyName, object value)
        {
            var data = new PrimitiveData("tn", value);
            return new PrimitiveObjectTreeItem(data.Value, null, propertyName, data);
        }

        public ListObjectTreeItem CreateListTreeItem(params ObjectTreeItem[] children)
        {
            var data = new ListData("tn", children.Select(c => c.Source).ToArray());
            return new ListObjectTreeItem(null, "pn", data, depth: 0);
        }

        public StateObjectTreeItem CreateStateObjectTreeItem(params PrimitiveObjectTreeItem[] children)
        {
            var data = new StateObjectData("tn", children.ToImmutableDictionary(c => c.PropertyName, c => c.Source), hasKey: false, key: null);
            return new StateObjectTreeItem(null, "sn", data, depth: 0);
        }

        public StateObjectTreeItem CreateKeyedStateObjectTreeItem(object key, params PrimitiveObjectTreeItem[] children)
        {
            var data = new StateObjectData("tn", children.ToImmutableDictionary(c => c.PropertyName, c => c.Source), hasKey: true, key: key);
            return new StateObjectTreeItem(null, "sn", data, depth: 0);
        }
    }

    public class Root
    {
        public FirstBranch First { get; set; }
        public SecondBranch Second { get; set; }
    }

    public class FirstBranch
    {
        public string Alpha { get; set; }
        public int Omega { get; set; }
    }

    public class SecondBranch
    {
        public string Delta { get; set; }
    }
}
