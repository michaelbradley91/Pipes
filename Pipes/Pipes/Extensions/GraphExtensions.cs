using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;

namespace Pipes.Extensions
{
    internal static class GraphExtensions
    {
        public static bool ContainsCycle<TVertex>(this IVertexAndEdgeListGraph<TVertex, Edge<TVertex>> graph)
        {
            IDictionary<TVertex, int> components;
            graph.StronglyConnectedComponents(out components);
            return components.Select(c => c.Value).Distinct().Count() != graph.Vertices.Count();
        }
    }
}
