using System.Collections.Generic;
using System.Linq;
using Pipes.Models;
using Pipes.Models.Pipes;
using QuickGraph;

namespace Pipes.Extensions
{
    internal static class PipeExtensions
    {
        public static IVertexAndEdgeListGraph<IPipe<TMessage>, Edge<IPipe<TMessage>>> CreateGraphOfPipeSystem<TMessage>(this IPipe<TMessage> onePipeInTheSystem)
        {
            var graph = new AdjacencyGraph<IPipe<TMessage>, Edge<IPipe<TMessage>>>(false);
            var pipesSeen = new HashSet<IPipe<TMessage>>();
            var pipesToCheck = new Stack<IPipe<TMessage>>();
            pipesToCheck.Push(onePipeInTheSystem);

            while (pipesToCheck.Any())
            {
                var pipeToCheck = pipesToCheck.Pop();
                if (pipesSeen.Contains(pipeToCheck)) continue;

                graph.AddVertex(pipeToCheck);

                foreach (var otherPipe in GetPipesYouSendMessagesTo(pipeToCheck))
                {
                    graph.AddVertex(otherPipe);
                    graph.AddEdge(new Edge<IPipe<TMessage>>(pipeToCheck, otherPipe));
                }

                foreach (var otherPipe in GetPipesYouReceiveMessagesFrom(pipeToCheck))
                {
                    graph.AddVertex(otherPipe);
                    graph.AddEdge(new Edge<IPipe<TMessage>>(otherPipe, pipeToCheck));
                }
                pipesSeen.Add(pipeToCheck);
            }

            return graph;
        }

        private static IEnumerable<IPipe<TMessage>> GetPipesYouSendMessagesTo<TMessage>(IPipe<TMessage> pipe)
        {
            return pipe.Outlets.Where(outlet => outlet.ConnectedInlet != null).Select(outlet => outlet.ConnectedInlet.Pipe);
        }

        private static IEnumerable<IPipe<TMessage>> GetPipesYouReceiveMessagesFrom<TMessage>(IPipe<TMessage> pipe)
        {
            return pipe.Inlets.Where(inlet => inlet.ConnectedOutlet != null).Select(inlet => inlet.ConnectedOutlet.Pipe);
        }
    }
}
