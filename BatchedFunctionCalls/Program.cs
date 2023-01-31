
using BatchedFunctionCalls;
using ExcelDna.Integration;
using ExcelDna.Registration;
using System.Diagnostics;
using System.Threading.Channels;

public class BatchedFunctions : IExcelAddIn
{
    private static Channel<FunctionParams> c = Channel.CreateUnbounded<FunctionParams>();
    private static readonly int MaxBatchSize = 20;

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
                await Task.Delay(1000);

                foreach (FunctionParams p in batch)
                {
                    p.result.SetResult($"done getting {p} {Environment.CurrentManagedThreadId}");
                }
            }
        });
    }
    private static int numberOfCalls = 0;

    [ExcelFunction(Description = "Function that will be batched")]
    public static async Task<object> BatchedCall(string ticker, int year)
    {
        Debug.WriteLine($"writing year {year} number of calls {numberOfCalls++}");
        var writer = c.Writer;
        var param = new FunctionParams() { Ticker = ticker, Year = year };
        writer.TryWrite(param);
        Task<object> t = param.result.Task;
        await t;
        return t.Result;

    }

    public void AutoOpen()
    {
        ExcelRegistration.GetExcelFunctions().ProcessAsyncRegistrations(nativeAsyncIfAvailable: false).RegisterFunctions();
    }

    public void AutoClose()
    {

    }
}
