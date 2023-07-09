using System.Runtime.Serialization;

namespace Saturn.BL.FeatureUtils
{
    [Serializable]
    internal class FeatureAlreadyDisabledException : Exception
    {
        public FeatureAlreadyDisabledException()
        {
        }

        public FeatureAlreadyDisabledException(string? message) : base(message)
        {
        }

        public FeatureAlreadyDisabledException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected FeatureAlreadyDisabledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}