
using BatchedFunctionCalls;
using ExcelDna.Integration;
using Open.ChannelExtensions;
using System.Diagnostics;
using System.Threading.Channels;

public static class BatchedFunctions
{
    private static readonly Channel<FunctionParams> c = Channel.CreateUnbounded<FunctionParams>();
    private static readonly int MaxBatchSize = 2;

    static BatchedFunctions()
    {
        c.Reader.Batch(MaxBatchSize, singleReader: true).WithTimeout(1).ReadAllAsync(async batch =>
        {
            foreach (var item in batch)
            {
                item.result.SetResult($"done getting {item}");
            }
        });
    }


    [ExcelFunction(Description = "Function that will be batched")]
    public static async void BatchedCall(string ticker, int year, ExcelAsyncHandle asyncHandle)
    {
        var writer = c.Writer;
        var param = new FunctionParams() { Ticker = ticker, Year = year };
        writer.TryWrite(param);
        Task<object> t = param.result.Task;
        await t;

        asyncHandle.SetResult(t.Result);
    }
}
