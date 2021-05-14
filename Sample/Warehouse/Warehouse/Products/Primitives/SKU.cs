using System;
using System.Text.RegularExpressions;

namespace Warehouse.Products.Primitives
{
    public record SKU
    {
        public string Value { get; init; }

        private SKU(string value)
        {
            Value = value;
        }

        public static SKU Create(string? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(SKU));
            if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, "[A-Z]{2,4}[0-9]{4,18}"))
                throw new ArgumentOutOfRangeException(nameof(SKU));

            return new SKU(value);
        }
    }
}
