using ExcelDna.Integration;

namespace BatchedFunctionCalls
{
    internal class FunctionParams
    {
        public string Ticker;
        public int Year;

     //   public ExcelReference CellReference = XlCall.Excel(XlCall.xlfCaller) as ExcelReference;

        public readonly TaskCompletionSource<object> result = new();

        public override string ToString() => $"FunctionParams {Ticker}-{Year}";
    }
}
