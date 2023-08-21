using UnityEngine;

public static class LongExtenions
{
    public static string FormatBytes(this long bytes)
    {
        const int scale = 1024;
        string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
        long max = (long)Mathf.Pow(scale, orders.Length - 1);

        foreach (string order in orders)
        {
            if (bytes > max)
                return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

            max /= scale;
        }

        return "0 Bytes";
    }
}
