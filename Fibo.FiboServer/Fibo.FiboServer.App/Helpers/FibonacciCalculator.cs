namespace Fibo.FiboServer.App.Helpers
{
    public static class FibonacciCalculator
    {
        /// <summary>
        /// Calculates N+1 Fibonacci value
        /// </summary>
        // <param name="previous">(N-1) value</param>
        // <param name="current">(N) value</param>
        /// <exception cref="OverflowException">Arithmetic operation resulted in an overflow</exception>
        /// <returns></returns>
        public static long GetNext(long previous, long current)
        {
            checked
            {
                return current > 0
                    ? previous + current
                    : 1;
            }
        }
    }
}
