namespace Fibo.Shared
{
    public class FiboMessage
    {
        public FiboMessage()
        {
        }

        /// <summary>
        /// Calculation Id
        /// </summary>
        public string KeyId { get; set; }

        public long PrevValue { get; set; }

        public long Value { get; set; }

        public int IterationIndex { get; set; }

        public override string ToString()
        {
            return $"KeyId: {KeyId}, PrevValue: {PrevValue}, Value: {Value}, IterationIndex: {IterationIndex}";
        }
    }
}