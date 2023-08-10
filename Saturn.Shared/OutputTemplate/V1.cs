namespace Saturn.Shared.OutputTemplate
{
    public static class V1
    {
        public static string ToString(string content)
        {
            //text	....
            return content;
        }

        public static string ToDialog(string title, string content)
        {
            // dialog/r[title]/r[content]
            return $"dialog\t{title ?? string.Empty}\t{content ?? string.Empty}";
        }

        public static bool IsDialog(string content)
        {
            if(!string.IsNullOrWhiteSpace(content))
            {
                var tokens = content.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                if(tokens.Length == 3)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
