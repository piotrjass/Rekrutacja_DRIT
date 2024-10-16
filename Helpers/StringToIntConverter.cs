using System;


public class StringToIntConverter
{

    public static int Convert(string s)
    {
        int result = 0;
        bool isNegative = false;
        if (s[0] == '-')
        {
            isNegative = true;
            s = s.Substring(1);
        }

        foreach (char c in s)
        {
            if (c < '0' || c > '9')
            {
                throw new FormatException("String contains non-numeric characters.");
            }
            result = result * 10 + (c - '0');
        }

        return isNegative ? -result : result;
    }
}
