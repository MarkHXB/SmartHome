namespace Saturn.Shared.OutputTemplate
{
    public static class V1
    {
        public static string ToString(string content)
        {
            return content;
        }

        public static string ToDialog(string title, string content)
        {
            return $"dialog\t{title ?? string.Empty}\t{content ?? string.Empty}";
        }

        public static bool IsDialog(string content)
        {
            if(!string.IsNullOrWhiteSpace(content))
            {
                var tokens = content.Split('\t', StringSplitOptions.None);
                if(tokens.Length > 0)
                {
                    if (tokens[0].Equals("dialog"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
