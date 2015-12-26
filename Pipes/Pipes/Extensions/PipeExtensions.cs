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
        public static IVertexAndEdgeListGraph<PipeGraphVertex<TMessage>, Edge<PipeGraphVertex<TMessage>>> CreateGraphOfPipeSystem<TMessage>(this IPipe<TMessage> onePipeInTheSystem)
        {
            return CreateGraphOfPipeSystem(PipeGraphVertex<TMessage>.Create(onePipeInTheSystem));
        }

        public static IVertexAndEdgeListGraph<PipeGraphVertex<TMessage>, Edge<PipeGraphVertex<TMessage>>> CreateGraphOfPipeSystem<TMessage>(this IInlet<TMessage> oneInletInThePipeSystem)
        {
            return CreateGraphOfPipeSystem(PipeGraphVertex<TMessage>.Create(oneInletInThePipeSystem));
        }

        public static IVertexAndEdgeListGraph<PipeGraphVertex<TMessage>, Edge<PipeGraphVertex<TMessage>>> CreateGraphOfPipeSystem<TMessage>(this IOutlet<TMessage> oneOutletInThePipeSystem)
        {
            return CreateGraphOfPipeSystem(PipeGraphVertex<TMessage>.Create(oneOutletInThePipeSystem));
        }

        private static IVertexAndEdgeListGraph<PipeGraphVertex<TMessage>, Edge<PipeGraphVertex<TMessage>>> CreateGraphOfPipeSystem<TMessage>(PipeGraphVertex<TMessage> firstVertex)
        {
            var graph = new AdjacencyGraph<PipeGraphVertex<TMessage>, Edge<PipeGraphVertex<TMessage>>>(false);
            var verticesSeen = new HashSet<PipeGraphVertex<TMessage>>();
            var verticesToCheck = new Stack<PipeGraphVertex<TMessage>>();
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

                graph.AddVerticesAndEdgeRange(sendsTo.Select(v => new Edge<PipeGraphVertex<TMessage>>(vertexToCheck, v)));
                graph.AddVerticesAndEdgeRange(receivesFrom.Select(v => new Edge<PipeGraphVertex<TMessage>>(v, vertexToCheck)));

                verticesSeen.Add(vertexToCheck);
            }

            return graph;
        }

        private static IReadOnlyCollection<PipeGraphVertex<TMessage>> GetVerticesYouSendMessagesTo<TMessage>(PipeGraphVertex<TMessage> vertex)
        {
            if (vertex.IsPipe)
            {
                var pipe = vertex.GetPipe();
                return pipe.Outlets.Select(PipeGraphVertex<TMessage>.Create).ToList();
            }

            if (vertex.IsInlet)
            {
                var inlet = vertex.GetInlet();
                return new List<PipeGraphVertex<TMessage>> {PipeGraphVertex<TMessage>.Create(inlet.Pipe)};
            }

            var outlet = vertex.GetOutlet();
            return outlet.ConnectedInlet != null ?
                new List<PipeGraphVertex<TMessage>> {PipeGraphVertex<TMessage>.Create(outlet.ConnectedInlet)} :
                new List<PipeGraphVertex<TMessage>>();
        }

        private static IReadOnlyCollection<PipeGraphVertex<TMessage>> GetVerticesYouReceiveMessagesFrom<TMessage>(PipeGraphVertex<TMessage> vertex)
        {
            if (vertex.IsPipe)
            {
                var pipe = vertex.GetPipe();
                return pipe.Inlets.Select(PipeGraphVertex<TMessage>.Create).ToList();
            }

            if (vertex.IsOutlet)
            {
                var outlet = vertex.GetOutlet();
                return new List<PipeGraphVertex<TMessage>> { PipeGraphVertex<TMessage>.Create(outlet.Pipe) };
            }

            var inlet = vertex.GetInlet();
            return inlet.ConnectedOutlet != null ?
                new List<PipeGraphVertex<TMessage>> { PipeGraphVertex<TMessage>.Create(inlet.ConnectedOutlet) } :
                new List<PipeGraphVertex<TMessage>>();
        }
    }
}
