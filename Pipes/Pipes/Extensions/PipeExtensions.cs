using System.Collections.Generic;
using System.Linq;
using Pipes.Models.Pipes;
using QuickGraph;

namespace Pipes.Extensions
{
    public static class PipeExtensions
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

                var sendsTo = GetPipesYouSendMessagesTo(pipeToCheck);
                var receivesFrom = GetPipesYouReceiveMessagesFrom(pipeToCheck);

                foreach (var pipe in sendsTo) pipesToCheck.Push(pipe);
                foreach (var pipe in receivesFrom) pipesToCheck.Push(pipe);

                graph.AddVertex(pipeToCheck);
                graph.AddVerticesAndEdgeRange(sendsTo.Select(p => new Edge<IPipe<TMessage>>(pipeToCheck, p)));
                graph.AddVerticesAndEdgeRange(receivesFrom.Select(p => new Edge<IPipe<TMessage>>(p, pipeToCheck)));

                pipesSeen.Add(pipeToCheck);
            }

            return graph;
        }

        private static IReadOnlyCollection<IPipe<TMessage>> GetPipesYouSendMessagesTo<TMessage>(IPipe<TMessage> pipe)
        {
            return pipe.Outlets.Where(outlet => outlet.ConnectedInlet != null).Select(outlet => outlet.ConnectedInlet.Pipe).ToList();
        }

        private static IReadOnlyCollection<IPipe<TMessage>> GetPipesYouReceiveMessagesFrom<TMessage>(IPipe<TMessage> pipe)
        {
            return pipe.Inlets.Where(inlet => inlet.ConnectedOutlet != null).Select(inlet => inlet.ConnectedOutlet.Pipe).ToList();
        }
    }
}
