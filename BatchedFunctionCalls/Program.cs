
using BatchedFunctionCalls;
using ExcelDna.Integration;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

public static class BatchedFunctions
{
    private static Channel<FunctionParams> c = Channel.CreateUnbounded<FunctionParams>();
    private static readonly int MaxBatchSize = 2;

    static BatchedFunctions()
    {
        Task.Run(async delegate
        {
            while (true)
            {
                // https://makolyte.com/csharp-how-to-batch-read-with-threading-channelreader/
                await c.Reader.WaitToReadAsync();
                List<FunctionParams> batch = new();
                while (batch.Count < MaxBatchSize && c.Reader.TryRead(out FunctionParams p))
                {
                    batch.Add(p);
                }
                Thread.Sleep(1000);
                Debug.WriteLine(string.Join(",", batch));
                foreach(FunctionParams p in batch)
                {
                    p.result.SetResult($"done getting {p}");
                }
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
