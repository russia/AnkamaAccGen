using Newtonsoft.Json;
using System.Globalization;

namespace AnkamaAccGen.Helpers
{
    public class JsonHelper
    {
        public static string ExtractStr(string task, dynamic json, string firstLevel, string secondLevel = null, bool silent = false)
        {
            var path = firstLevel + (secondLevel == null ? "" : "=>" + secondLevel);

            try
            {
                object result = json[firstLevel];

                if (result != null && secondLevel != null)
                    result = json[firstLevel][secondLevel];

                if (result == null)

                    return null;

                return result.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static string AsString(string task, dynamic json)
        {
            return JsonConvert.SerializeObject(json, Formatting.Indented);
        }

        public static double? ExtractDouble(string task, dynamic json, string firstLevel, string secondLevel = null)
        {
            double outDouble;
            string numberAsStr = ExtractStr(task, json, firstLevel, secondLevel);

            if (numberAsStr == null ||
                !double.TryParse(numberAsStr.Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture,
                    out outDouble))
            {
                var path = firstLevel + (secondLevel == null ? "" : "=>" + secondLevel);

                return null;
            }

            return outDouble;
        }

        public static int? ExtractInt(string task, dynamic json, string firstLevel, string secondLevel = null, bool silent = false)
        {
            int outInt;
            string numberAsStr = JsonHelper.ExtractStr(task, json, firstLevel, secondLevel, silent);

            if (!int.TryParse(numberAsStr, out outInt))
            {
                var path = firstLevel + (secondLevel == null ? "" : "=>" + secondLevel);

                return null;
            }

            return outInt;
        }
    }
}