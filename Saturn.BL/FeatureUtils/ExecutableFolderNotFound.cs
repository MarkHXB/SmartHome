using System.Runtime.Serialization;

namespace Saturn.BL.FeatureUtils
{
    [Serializable]
    internal class ExecutableFolderNotFound : Exception
    {
        public ExecutableFolderNotFound()
        {
        }

        public ExecutableFolderNotFound(string? message) : base(message)
        {
        }

        public ExecutableFolderNotFound(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ExecutableFolderNotFound(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}