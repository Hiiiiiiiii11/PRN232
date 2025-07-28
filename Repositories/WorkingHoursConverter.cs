using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Repositories
{
    public class WorkingHoursConverter : ValueConverter<List<DateTime>, string>
    {
        public WorkingHoursConverter() : base(
            v => ConvertToString(v),
            v => ConvertFromString(v))
        {
        }

        private static string ConvertToString(List<DateTime> value)
        {
            return value == null || value.Count == 0 ? "[]" : JsonSerializer.Serialize(value);
        }

        private static List<DateTime> ConvertFromString(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "[]")
                return new List<DateTime>();

            try
            {
                return JsonSerializer.Deserialize<List<DateTime>>(value) ?? new List<DateTime>();
            }
            catch
            {
                // If JSON parsing fails, try to parse as simple string array and convert to DateTime
                try
                {
                    var stringArray = JsonSerializer.Deserialize<List<string>>(value);
                    if (stringArray != null)
                    {
                        var dateList = new List<DateTime>();
                        foreach (var str in stringArray)
                        {
                            if (DateTime.TryParse(str, out DateTime date))
                                dateList.Add(date);
                        }
                        return dateList;
                    }
                }
                catch { }

                return new List<DateTime>(); // Fallback to empty list
            }
        }
    }
} 