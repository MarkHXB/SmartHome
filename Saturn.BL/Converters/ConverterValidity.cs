using Saturn.Shared;

namespace Saturn.BL.Converters
{
    public static class ConverterValidity
    {
        public static bool IsFeatureDescriptionFile(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            string fileName = Path.GetFileName(path);

            if (!fileName.ToLower().Contains(AppInfo.FeatureDescriptionIdentifier)) return false;

            return true;
        }
    }
}
