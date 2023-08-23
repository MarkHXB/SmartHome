using System.Runtime.Serialization;

namespace Saturn.Shared
{
    [Serializable]
    internal class FeatureAlreadyEnabledException : Exception
    {
        public FeatureAlreadyEnabledException()
        {
        }

        public FeatureAlreadyEnabledException(string? message) : base(message)
        {
        }

        public FeatureAlreadyEnabledException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected FeatureAlreadyEnabledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}