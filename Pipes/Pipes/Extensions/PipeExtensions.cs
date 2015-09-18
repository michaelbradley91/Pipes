using System.Collections.Generic;
using System.Linq;
using Pipes.Models;
using Pipes.Models.Pipes;
using QuickGraph;

namespace Pipes.Extensions
{
    internal static class PipeExtensions
    {
        public static IVertexAndEdgeListGraph<IPipe<TMessageType>, Edge<IPipe<TMessageType>>> CreateGraphOfPipeSystem<TMessageType>(this IPipe<TMessageType> onePipeInTheSystem)
        {
            var graph = new AdjacencyGraph<IPipe<TMessageType>, Edge<IPipe<TMessageType>>>(false);
            var pipesSeen = new HashSet<IPipe<TMessageType>>();
            var pipesToCheck = new Stack<IPipe<TMessageType>>();
            pipesToCheck.Push(onePipeInTheSystem);

            while (pipesToCheck.Any())
            {
                var pipeToCheck = pipesToCheck.Pop();
                if (pipesSeen.Contains(pipeToCheck)) continue;

                graph.AddVertex(pipeToCheck);

                foreach (var otherPipe in GetPipesYouSendMessagesTo(pipeToCheck))
                {
                    graph.AddVertex(otherPipe);
                    graph.AddEdge(new Edge<IPipe<TMessageType>>(pipeToCheck, otherPipe));
                }

                foreach (var otherPipe in GetPipesYouReceiveMessagesFrom(pipeToCheck))
                {
                    graph.AddVertex(otherPipe);
                    graph.AddEdge(new Edge<IPipe<TMessageType>>(otherPipe, pipeToCheck));
                }
                pipesSeen.Add(pipeToCheck);
            }

            return graph;
        }

        private static IEnumerable<IPipe<TMessageType>> GetPipesYouSendMessagesTo<TMessageType>(IPipe<TMessageType> pipe)
        {
            return pipe.Outlets.Where(outlet => outlet.ConnectedInlet != null).Select(outlet => outlet.ConnectedInlet.Pipe);
        }

        private static IEnumerable<IPipe<TMessageType>> GetPipesYouReceiveMessagesFrom<TMessageType>(IPipe<TMessageType> pipe)
        {
            return pipe.Inlets.Where(inlet => inlet.ConnectedOutlet != null).Select(inlet => inlet.ConnectedOutlet.Pipe);
        }
    }
}
