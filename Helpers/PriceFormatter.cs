using System.Globalization;

namespace RealEstateWeb.Helpers;

/// <summary>Display Vietnamese dong in a short form without changing its stored value.</summary>
public static class PriceFormatter
{
    private static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");
    private const decimal Billion = 1_000_000_000m;
    private const decimal Million = 1_000_000m;

    public static string Short(decimal amount)
    {
        if (amount < 0)
        {
            return "-" + Short(decimal.Negate(amount));
        }

        if (amount >= Billion)
        {
            // Exact multiples of one million need at most three fractional billion digits.
            // Keep leading zeros: 10.02 billion -> "10 tỷ 02", not "10 tỷ 2".
            if (amount % Million == 0)
            {
                var parts = (amount / Billion).ToString("0.###", CultureInfo.InvariantCulture).Split('.');
                return parts.Length == 1 ? parts[0] + " tỷ" : parts[0] + " tỷ " + parts[1];
            }

            // Preserve smaller amounts exactly rather than rounding a listing price.
            var wholeBillions = decimal.Truncate(amount / Billion);
            var remainderInMillions = (amount % Billion) / Million;
            return wholeBillions.ToString("0", Vietnamese) + " tỷ "
                + remainderInMillions.ToString("0.########", Vietnamese) + " triệu";
        }

        if (amount >= Million)
        {
            return (amount / Million).ToString("0.########", Vietnamese) + " triệu";
        }

        return Full(amount);
    }

    public static string Full(decimal amount)
    {
        var format = amount == decimal.Truncate(amount) ? "N0" : "N2";
        return amount.ToString(format, Vietnamese) + " đ";
    }
}
