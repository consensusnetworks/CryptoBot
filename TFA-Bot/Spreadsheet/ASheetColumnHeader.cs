using System;
namespace TFABot
{
    public class ASheetColumnHeader : Attribute
    {
        string[] HeaderMatch;
        public bool IsIndex { get; private set; }

        public ASheetColumnHeader(params string[] headerMatch)
        {
            HeaderMatch = headerMatch;
        }

        public ASheetColumnHeader(bool index, params string[] headerMatch)
        {
            HeaderMatch = headerMatch;
            IsIndex = index;
        }

        public bool IsMatch(string text)
        {
            text = text.ToLower();
            foreach (var item in HeaderMatch)
            {
                if (text.Contains(item)) return true;
            }
            return false;
        }
    }
}
