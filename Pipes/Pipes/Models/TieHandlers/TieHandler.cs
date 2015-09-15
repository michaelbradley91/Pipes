using Pipes.Constants;

namespace Pipes.Models.TieHandlers
{
    internal interface ITieHandler
    {
        TieResult ResolveTie();
    }
}
