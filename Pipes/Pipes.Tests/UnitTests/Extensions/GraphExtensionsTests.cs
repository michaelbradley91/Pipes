using FluentAssertions;
using NUnit.Framework;
using Pipes.Extensions;
using QuickGraph;

namespace Pipes.Tests.UnitTests.Extensions
{
    [TestFixture]
    public class GraphExtensionsTests
    {
        private AdjacencyGraph<string, Edge<string>> graph;

        [SetUp]
        public void SetUp()
        {
            graph = new AdjacencyGraph<string, Edge<string>>();
        }

        [Test]
        public void ContainsCycle_GivenAnEmptyGraph_ReturnsFalse()
        {
            // Act
            var containsCycle = graph.ContainsCycle();

            // Assert
            containsCycle.Should().BeFalse();
        }

        [Test]
        public void ContainsCycle_GivenAGraphWithASingleVertex_ReturnsFalse()
        {
            // Arrange
            graph.AddVertex("A");

            // Act
            var containsCycle = graph.ContainsCycle();

            // Assert
            containsCycle.Should().BeFalse();
        }

        [Test]
        public void ContainsCycle_GivenAGraphWithALineOfVertices_ReturnsFalse()
        {
            // Arrange
            graph.AddVerticesAndEdgeRange(new []
            {
                new Edge<string>("A", "B"),
                new Edge<string>("B", "C"), 
                new Edge<string>("D", "C")
            });

            // Act
            var containsCycle = graph.ContainsCycle();

            // Assert
            containsCycle.Should().BeFalse();
        }

        [Test]
        public void ContainsCycle_GivenAGraphWhoseVerticesDoNotFormATreeButIsAcyclic_ReturnsFalse()
        {
            // Arrange
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<string>("A", "B"),
                new Edge<string>("B", "C"), 
                new Edge<string>("A", "C")
            });

            // Act
            var containsCycle = graph.ContainsCycle();

            // Assert
            containsCycle.Should().BeFalse();
        }

        [Test]
        public void ContainsCycle_GivenAGraphWhichIsExactlyACycle_ReturnsTrue()
        {
            // Arrange
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<string>("A", "B"),
                new Edge<string>("B", "C"), 
                new Edge<string>("C", "A")
            });

            // Act
            var containsCycle = graph.ContainsCycle();

            // Assert
            containsCycle.Should().BeTrue();
        }

        [Test]
        public void ContainsCycle_GivenAGraphWhichHasASelfLoop_ReturnsTrue()
        {
            // Arrange
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<string>("A", "A")
            });

            // Act
            var containsCycle = graph.ContainsCycle();

            // Assert
            containsCycle.Should().BeTrue();
        }

        [Test]
        public void ContainsCycle_GivenAGraphWhichContainsACycle_ReturnsTrue()
        {
            // Arrange
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<string>("A", "B"),
                new Edge<string>("B", "C"), 
                new Edge<string>("C", "A"),
                new Edge<string>("D", "E") 
            });

            // Act
            var containsCycle = graph.ContainsCycle();

            // Assert
            containsCycle.Should().BeTrue();
        }

        [Test]
        public void IsTree_GivenAnEmptyGraph_ReturnsTrue()
        {
            // Act
            var isTree = graph.IsTree();

            // Assert
            isTree.Should().BeTrue();
        }

        [Test]
        public void IsTree_GivenAGraphWhichIsATree_ReturnsTrue()
        {
            // Arrange
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<string>("A", "B"),
                new Edge<string>("B", "C"),
                new Edge<string>("A", "D"),
                new Edge<string>("D", "E")
            });

            // Act
            var isTree = graph.IsTree();

            // Assert
            isTree.Should().BeTrue();
        }

        [Test]
        public void IsTree_GivenAGraphContainingADirectedCycle_ReturnsFalse()
        {
            // Arrange
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<string>("A", "B"),
                new Edge<string>("B", "C"),
                new Edge<string>("C", "A"),
                new Edge<string>("D", "E")
            });

            // Act
            var isTree = graph.IsTree();

            // Assert
            isTree.Should().BeFalse();
        }

        [Test]
        public void IsTree_GivenAGraphContainingAnUndirectedCycle_ReturnsFalse()
        {
            // Arrange
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<string>("A", "B"),
                new Edge<string>("B", "C"),
                new Edge<string>("A", "C")
            });

            // Act
            var isTree = graph.IsTree();

            // Assert
            isTree.Should().BeFalse();
        }

        [Test]
        public void IsTree_GivenAGraphContainingASelfLoop_ReturnsFalse()
        {
            // Arrange
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<string>("A", "A")
            });

            // Act
            var isTree = graph.IsTree();

            // Assert
            isTree.Should().BeFalse();
        }

        [Test]
        public void IsTree_GivenAGraphContainingTwoVerticesInACycle_ReturnsFalse()
        {
            // Arrange
            graph.AddVerticesAndEdgeRange(new[]
            {
                new Edge<string>("A", "B"),
                new Edge<string>("B", "A")
            });

            // Act
            var isTree = graph.IsTree();

            // Assert
            isTree.Should().BeFalse();
        }
    }
}
