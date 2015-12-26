using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Search;

namespace Pipes.Extensions
{
    internal static class GraphExtensions
    {
        public static bool ContainsCycle<TVertex>(this IVertexAndEdgeListGraph<TVertex, Edge<TVertex>> graph)
        {
            if (graph.Edges.Any(e => e.IsSelfEdge<TVertex, Edge<TVertex>>())) return true;
            IDictionary<TVertex, int> components;
            graph.StronglyConnectedComponents(out components);
            return components.Select(c => c.Value).Distinct().Count() != graph.Vertices.Count();
        }

        public static bool IsTree<TVertex>(this IVertexAndEdgeListGraph<TVertex, Edge<TVertex>> graph)
        {
            if (graph.Edges.Any(e => e.IsSelfEdge<TVertex, Edge<TVertex>>())) return false;
            if (graph.VertexCount == 0) return true;

            var undirectedGraph = new UndirectedGraph<TVertex, Edge<TVertex>>(true);
            graph.Clone(v => v, (e, src, tgt) => e, undirectedGraph);

            var dfs = new UndirectedDepthFirstSearchAlgorithm<TVertex, Edge<TVertex>>(undirectedGraph);
            var foundBackEdge = false;
            dfs.BackEdge += delegate { foundBackEdge = true; };
            dfs.Compute();
            return !foundBackEdge;
        }
    }
}
