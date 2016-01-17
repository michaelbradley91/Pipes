using System;

namespace Pipes.Builders
{
    public interface IBigValvedPipeBuilder<TReceive, TSend>
    {
        ISizedBigValvedPipeBuilder<TReceive, TSend> WithSize(int numberOfInlets, int numberOfOutlets);
    }

    public class BigValvedPipeBuilder<TReceive, TSend> : IBigValvedPipeBuilder<TReceive, TSend>
    {
        public ISizedBigValvedPipeBuilder<TReceive, TSend> WithSize(int numberOfInlets, int numberOfOutlets)
        {
            if (numberOfInlets < 1) throw new ArgumentOutOfRangeException(nameof(numberOfInlets), "A big valved pipe must have at least one inlet");
            if (numberOfOutlets < 1) throw new ArgumentOutOfRangeException(nameof(numberOfOutlets), "A big valved pipe must have at least one outlet");

            return new SizedBigValvedPipeBuilder<TReceive, TSend>(numberOfInlets, numberOfOutlets);
        }
    }
}