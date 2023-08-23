using System.Runtime.Serialization;

namespace Saturn.Shared
{
    [Serializable]
    internal class DllFolderNotFoundException : Exception
    {
        public DllFolderNotFoundException()
        {
        }

        public DllFolderNotFoundException(string? message) : base(message)
        {
        }

        public DllFolderNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DllFolderNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}