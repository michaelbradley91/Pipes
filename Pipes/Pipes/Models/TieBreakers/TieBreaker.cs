using Pipes.Constants;

namespace Pipes.Models.TieBreakers
{
    public interface ITieBreaker
    {
        TieResult ResolveTie();
    }
}
