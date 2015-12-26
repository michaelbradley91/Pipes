using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Extensions;
using Pipes.Models.Pipes;

namespace Pipes.Tests.UnitTests.Extensions
{
    [TestFixture]
    public class PipeExtensionsTests
    {
        [Test]
        public void CreateGraphOfPipeSystem_GivenASinglePipe_ReturnsAGraphWithOneVertex()
        {
            // Arrange
            var basicPipe = PipeBuilder.New.BasicPipe<int>().Build();

            // Act
            var graph = basicPipe.CreateGraphOfPipeSystem();

            // Assert
            graph.EdgeCount.Should().Be(0);
            graph.VertexCount.Should().Be(1);
            graph.Vertices.First().Should().Be(basicPipe);
        }

        [Test]
        public void CreateGraphOfPipeSystem_GivenPipesConnectedInALine_ReturnsAGraphWithVerticesConnectedInALine()
        {
            // Arrange
            var basicPipe1 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe3 = PipeBuilder.New.BasicPipe<int>().Build();

            basicPipe1.Outlet.ConnectTo(basicPipe2.Inlet);
            basicPipe2.Outlet.ConnectTo(basicPipe3.Inlet);

            // Act
            var graph = basicPipe1.CreateGraphOfPipeSystem();

            // Assert
            graph.Vertices.Count().Should().Be(3);
            graph.Vertices.Should().Contain(new[] {basicPipe1, basicPipe2, basicPipe3});
            graph.EdgeCount.Should().Be(2);
            graph.Edges.Count(e => e.Source.Equals(basicPipe1) && e.Target.Equals(basicPipe2)).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(basicPipe2) && e.Target.Equals(basicPipe3)).Should().Be(1);
        }

        [Test]
        public void CreateGraphOfPipeSystem_OfPipesFormingABinaryTree_ReturnsAGraphOfABinaryTree()
        {
            // Arrange
            var twoOutletPipe1 = PipeBuilder.New.EitherOutletPipe<int>().Build();
            var twoOutletPipe2 = PipeBuilder.New.EitherOutletPipe<int>().Build();
            var twoOutletPipe3 = PipeBuilder.New.EitherOutletPipe<int>().Build();

            twoOutletPipe1.LeftOutlet.ConnectTo(twoOutletPipe2.Inlet);
            twoOutletPipe1.RightOutlet.ConnectTo(twoOutletPipe3.Inlet);

            // Act
            var graph = twoOutletPipe1.CreateGraphOfPipeSystem();

            // Assert
            graph.Vertices.Count().Should().Be(3);
            graph.Vertices.Should().Contain(new[] { twoOutletPipe1, twoOutletPipe2, twoOutletPipe3 });
            graph.EdgeCount.Should().Be(2);
            graph.Edges.Count(e => e.Source.Equals(twoOutletPipe1) && e.Target.Equals(twoOutletPipe2)).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(twoOutletPipe1) && e.Target.Equals(twoOutletPipe3)).Should().Be(1);
        }

        [Test]
        public void CreateGraphOfPipeSystem_GivenAPipeConnectedToItself_ReturnsAGraphWithASelfLoop()
        {
            // Arrange
            var basicPipe = PipeBuilder.New.BasicPipe<int>().Build();

            basicPipe.Inlet.ConnectedOutlet = basicPipe.Outlet;
            basicPipe.Outlet.ConnectedInlet = basicPipe.Inlet;

            // Act
            var graph = basicPipe.CreateGraphOfPipeSystem();

            // Assert
            graph.Vertices.Count().Should().Be(1);
            graph.Vertices.Should().Contain(new IPipe<int>[] { basicPipe });
            graph.EdgeCount.Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(basicPipe) && e.Target.Equals(basicPipe)).Should().Be(1);
        }

        [Test]
        public void CreateGraphOfPipeSystem_GivenPipesFormingExactlyACycle_ReturnsAGraphWithACycle()
        {
            // Arrange
            var basicPipe1 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe3 = PipeBuilder.New.BasicPipe<int>().Build();

            basicPipe1.Outlet.ConnectTo(basicPipe2.Inlet);
            basicPipe2.Outlet.ConnectTo(basicPipe3.Inlet);
            basicPipe3.Outlet.ConnectedInlet = basicPipe1.Inlet;
            basicPipe1.Inlet.ConnectedOutlet = basicPipe3.Outlet;

            // Act
            var graph = basicPipe1.CreateGraphOfPipeSystem();

            // Assert
            graph.Vertices.Count().Should().Be(3);
            graph.Vertices.Should().Contain(new[] { basicPipe1, basicPipe2, basicPipe3 });
            graph.EdgeCount.Should().Be(3);
            graph.Edges.Count(e => e.Source.Equals(basicPipe1) && e.Target.Equals(basicPipe2)).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(basicPipe2) && e.Target.Equals(basicPipe3)).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(basicPipe3) && e.Target.Equals(basicPipe1)).Should().Be(1);
        }

        [Test]
        public void CreateGraphOfPipeSystem_GivenPipesIncludingACycle_ReturnsAGraphWithACycle()
        {
            // Arrange
            var basicPipe1 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe2 = PipeBuilder.New.BasicPipe<int>().Build();
            var basicPipe3 = PipeBuilder.New.BasicPipe<int>().Build();
            var twoOutletPipe = PipeBuilder.New.EitherOutletPipe<int>().Build();

            basicPipe1.Outlet.ConnectTo(basicPipe2.Inlet);
            basicPipe2.Outlet.ConnectTo(twoOutletPipe.Inlet);
            twoOutletPipe.RightOutlet.ConnectTo(basicPipe3.Inlet);
            twoOutletPipe.LeftOutlet.ConnectedInlet = basicPipe1.Inlet;
            basicPipe1.Inlet.ConnectedOutlet = twoOutletPipe.LeftOutlet;

            // Act
            var graph = basicPipe1.CreateGraphOfPipeSystem();

            // Assert
            graph.Vertices.Count().Should().Be(4);
            graph.Vertices.Should().Contain(new IPipe<int>[] { basicPipe1, basicPipe2, basicPipe3, twoOutletPipe });
            graph.EdgeCount.Should().Be(4);
            graph.Edges.Count(e => e.Source.Equals(basicPipe1) && e.Target.Equals(basicPipe2)).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(basicPipe2) && e.Target.Equals(twoOutletPipe)).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(twoOutletPipe) && e.Target.Equals(basicPipe1)).Should().Be(1);
            graph.Edges.Count(e => e.Source.Equals(twoOutletPipe) && e.Target.Equals(basicPipe3)).Should().Be(1);
        }
    }
}
