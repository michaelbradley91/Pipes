using System.Collections.Generic;
using System.Linq;
using Pipes.Models.Graphs;
using Pipes.Models.Lets;
using Pipes.Models.Pipes;
using QuickGraph;

namespace Pipes.Extensions
{
    public static class PipeExtensions
    {
        public static IVertexAndEdgeListGraph<PipeGraphVertex, Edge<PipeGraphVertex>> CreateGraphOfPipeSystem(this IPipe onePipeInTheSystem)
        {
            return CreateGraphOfPipeSystem(PipeGraphVertex.Create(onePipeInTheSystem));
        }

        public static IVertexAndEdgeListGraph<PipeGraphVertex, Edge<PipeGraphVertex>> CreateGraphOfPipeSystem(this IInlet oneInletInThePipeSystem)
        {
            return CreateGraphOfPipeSystem(PipeGraphVertex.Create(oneInletInThePipeSystem));
        }

        public static IVertexAndEdgeListGraph<PipeGraphVertex, Edge<PipeGraphVertex>> CreateGraphOfPipeSystem(this IOutlet oneOutletInThePipeSystem)
        {
            return CreateGraphOfPipeSystem(PipeGraphVertex.Create(oneOutletInThePipeSystem));
        }

        private static IVertexAndEdgeListGraph<PipeGraphVertex, Edge<PipeGraphVertex>> CreateGraphOfPipeSystem(PipeGraphVertex firstVertex)
        {
            var graph = new AdjacencyGraph<PipeGraphVertex, Edge<PipeGraphVertex>>(false);
            var verticesSeen = new HashSet<PipeGraphVertex>();
            var verticesToCheck = new Stack<PipeGraphVertex>();
            verticesToCheck.Push(firstVertex);

            while (verticesToCheck.Any())
            {
                var vertexToCheck = verticesToCheck.Pop();
                if (verticesSeen.Contains(vertexToCheck)) continue;

                var sendsTo = GetVerticesYouSendMessagesTo(vertexToCheck);
                var receivesFrom = GetVerticesYouReceiveMessagesFrom(vertexToCheck);

                foreach (var vertex in sendsTo) verticesToCheck.Push(vertex);
                foreach (var vertex in receivesFrom) verticesToCheck.Push(vertex);

                graph.AddVertex(vertexToCheck);

                graph.AddVerticesAndEdgeRange(sendsTo.Select(v => new Edge<PipeGraphVertex>(vertexToCheck, v)));
                graph.AddVerticesAndEdgeRange(receivesFrom.Select(v => new Edge<PipeGraphVertex>(v, vertexToCheck)));

                verticesSeen.Add(vertexToCheck);
            }

            return graph;
        }

        private static IReadOnlyCollection<PipeGraphVertex> GetVerticesYouSendMessagesTo(PipeGraphVertex vertex)
        {
            if (vertex.IsPipe)
            {
                var pipe = vertex.GetPipe();
                return pipe.AllOutlets.Select(PipeGraphVertex.Create).ToList();
            }

            if (vertex.IsInlet)
            {
                var inlet = vertex.GetInlet();
                return new List<PipeGraphVertex> {PipeGraphVertex.Create(inlet.Pipe)};
            }

            var outlet = vertex.GetOutlet();
            return outlet.TypelessConnectedInlet != null ?
                new List<PipeGraphVertex> {PipeGraphVertex.Create(outlet.TypelessConnectedInlet)} :
                new List<PipeGraphVertex>();
        }

        private static IReadOnlyCollection<PipeGraphVertex> GetVerticesYouReceiveMessagesFrom(PipeGraphVertex vertex)
        {
            if (vertex.IsPipe)
            {
                var pipe = vertex.GetPipe();
                return pipe.AllInlets.Select(PipeGraphVertex.Create).ToList();
            }

            if (vertex.IsOutlet)
            {
                var outlet = vertex.GetOutlet();
                return new List<PipeGraphVertex> { PipeGraphVertex.Create(outlet.Pipe) };
            }

            var inlet = vertex.GetInlet();
            return inlet.TypelessConnectedOutlet != null ?
                new List<PipeGraphVertex> { PipeGraphVertex.Create(inlet.TypelessConnectedOutlet) } :
                new List<PipeGraphVertex>();
        }
    }
}
