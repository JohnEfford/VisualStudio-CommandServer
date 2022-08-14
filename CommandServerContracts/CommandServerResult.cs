namespace CommandServerContracts
{
    public class CommandServerResult
    {
        public readonly string Message;
        public readonly string Payload;

        /// <summary>
        /// Nothing to return to command server
        /// </summary>
        public const string None = "";

        public CommandServerResult(
            string message,
            string payload)
        {
            Message = message;
            Payload = payload;
        }
    }
}