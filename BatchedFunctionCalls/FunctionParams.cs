namespace BatchedFunctionCalls
{
    internal class FunctionParams
    {
        public string Ticker;
        public int Year;


        public override string ToString() => $"FunctionParams {Ticker}-{Year}";
    }
}
