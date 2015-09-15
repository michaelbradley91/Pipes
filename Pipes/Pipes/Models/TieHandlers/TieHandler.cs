using Pipes.Constants;

namespace Pipes.Models.TieHandlers
{
    public interface ITieHandler
    {
        TieResult ResolveTie();
        ITieHandler DeepCopy();
    }
}
