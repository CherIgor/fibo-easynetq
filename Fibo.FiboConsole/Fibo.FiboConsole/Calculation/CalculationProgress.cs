namespace Fibo.FiboConsole.Calculation
{
    public class CalculationProgress
    {
        public CalculationProgress(
            Guid calculationId,
            bool isFinished,
            bool isCancelled,
            IEnumerable<long> calculatedFibonacciValues
        )
        {
            CalculationId = calculationId;
            IsFinished = isFinished;
            IsCancelled = isCancelled;
            CalculatedFibonacciValues = calculatedFibonacciValues.ToList();
        }

        public Guid CalculationId { get; }

        public bool IsFinished { get; }

        public bool IsCancelled { get; }

        public IList<long> CalculatedFibonacciValues { get; }

        public override string ToString()
        {
            return $"CalculationId: {CalculationId}, IsFinished: {IsFinished}, IsCancelled: {IsCancelled}, calculated Fibonacci values ({CalculatedFibonacciValues.Count} items) : {string.Join(", ", CalculatedFibonacciValues)}.";
        }
    }
}
