
using BatchedFunctionCalls;
using ExcelDna.Integration;
using Microsoft.Office.Interop.Excel;
using Open.ChannelExtensions;
using System.Threading.Channels;
using Application = Microsoft.Office.Interop.Excel.Application;
using Range = Microsoft.Office.Interop.Excel.Range;
using ExcelDna.Registration;
using System.Diagnostics;

public class BatchedFunctions : IExcelAddIn
{
    private static readonly Channel<FunctionParams> c = Channel.CreateUnbounded<FunctionParams>();
    private static readonly int MaxBatchSize = 200;
    private static readonly int InitialThreadID = Environment.CurrentManagedThreadId;


    private static int NumberOfBatches = 0;
    static BatchedFunctions()
    {
        c.Reader.Batch(MaxBatchSize, singleReader: true).WithTimeout(1).ReadAllAsync(async batch =>
        {
            var requestTime = 1000 + (batch.Count * 10);// Simulate calling a remote server to get data.
            await Task.Delay(requestTime);
            foreach (var item in batch)
            {
                item.result.SetResult(item.Year);
            }
            Debug.WriteLine($"Number of batches {++NumberOfBatches}");
        });
    }


    private static int NumberOfCalls = 0;
    [ExcelFunction(Name = "BatchedCall", Description = "Function that will be batched")]
    public static async Task<object> BatchedCall(
        [ExcelArgument(Name = "ticker")] string ticker,
        [ExcelArgument(Name = "year")] int year)
    {
        Debug.WriteLine($"Pushing {year} to queue number of calls {++NumberOfCalls}");

        var writer = c.Writer;
        var param = new FunctionParams() { Ticker = ticker, Year = year };
        writer.TryWrite(param);
        Task<object> t = param.result.Task;
        await t;

        return t.Result;
    }

    private static void SetCellFormatting(string numberFormat, ExcelReference cell)
    {
        var cellRange = ToRange(cell);
        cellRange.NumberFormat = numberFormat;
    }

    /// <summary>
    /// https://groups.google.com/g/exceldna/c/4FwjwuPTYO0/m/_OFRuLb0AwAJ.
    /// </summary>
    /// <param name="reference"></param>
    /// <returns></returns>
    private static Range ToRange(ExcelReference reference)
    {
        var xlApp = ExcelDnaUtil.Application as Application;
        var item = XlCall.Excel(XlCall.xlSheetNm, reference) as string;
        int index = item.LastIndexOf(']');
        item = item.Substring(index + 1);
        var ws = xlApp.Sheets[item] as Worksheet;
        var target = xlApp.Range[
            ws.Cells[reference.RowFirst + 1, reference.ColumnFirst + 1],
            ws.Cells[reference.RowLast + 1, reference.ColumnLast + 1]] as Range;

        return target;
    }


    public void AutoOpen()
    {
        var functions = ExcelRegistration.GetExcelFunctions().ToList();
        functions.ProcessAsyncRegistrations(nativeAsyncIfAvailable: false).RegisterFunctions();
    }

    public void AutoClose()
    {

    }

}
