
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
                await c.Reader.WaitToReadAsync();
                List<FunctionParams> batch = new();
                while (batch.Count < MaxBatchSize && c.Reader.TryRead(out FunctionParams p))
                {
                    batch.Add(p);
                }
                Thread.Sleep(1000);
                Debug.WriteLine(string.Join(",", batch));
            }
        });
    }


    [ExcelFunction(Description = "Function that will be batched")]
    public static string BatchedCall(string ticker, int year)
    {
        var writer = c.Writer;
        var param = new FunctionParams() { Ticker = ticker, Year = year };
        writer.TryWrite(param);
        return "true";
    }
}
