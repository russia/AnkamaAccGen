using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace AccountCreatorV2.Helper
{
    public static class StringHelper
    {
        internal static void SafeAddEntry(this Dictionary<string, string> list, string key, string value)
        {
            if (list != null && !list.ContainsKey(key))
            {
                list.Add(key, value);
            }
        }

        internal static void SafeAddEntry(this NameValueCollection list, string key, string value)
        {
            if (list != null && list[key] == null)
            {
                list.Add(key, value);
            }
        }
        internal static string GetUniqueStringUpper()
        {
            return GetFixedLengthString(2) + GetFixedLengthAlphaNumericString(10);
        }
        internal static string GetFixedLengthString(int len)
        {
            return GetRandomString("abcdefghijkmnopqrstuvwxyz", len);
        }

        internal static string GetFixedLengthAlphaNumericString(int len)
        {
            return GetRandomString("abcdefghijkmnopqrstuvwxyz0123456789", len);
        }

        internal static string GetTimeStamp(DateTime dataTime)
        {
            return new DateTimeOffset(dataTime).ToUnixTimeMilliseconds().ToString();
        }

        internal static int RandomNumBetween(int smallNum, int bigNum)
        {
            var rnd = new Random();
            var randomValue = rnd.Next(smallNum, bigNum + 1);
            return randomValue;
        }

        internal static bool IsValidJson(this string stringValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return false;
            }

            var value = stringValue.Trim();

            if ((!value.StartsWith("{") || !value.EndsWith("}")) && (!value.StartsWith("[") || !value.EndsWith("]")))
                return false;
            try
            {
                var obj = JToken.Parse(value);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        internal static string SafeTrim(this string value)
        {
            if (value == null) return string.Empty;

            return value.Trim();
        }

        internal static string GetPassword(int maxCharacters = 12, int minNumeric = 1)
        {
            bool includeLowercase = true;
            bool includeUppercase = true;
            bool includeNumeric = true;
            bool includeSpecial = false;
            bool includeSpaces = false;
            int lengthOfPassword = 12;

            string password = GeneratePassword(includeLowercase, includeUppercase, includeNumeric, includeSpecial, includeSpaces, lengthOfPassword);

            while (!PasswordIsValid(includeLowercase, includeUppercase, includeNumeric, includeSpecial, includeSpaces, password))
            {
                password = GeneratePassword(includeLowercase, includeUppercase, includeNumeric, includeSpecial, includeSpaces, lengthOfPassword);
            }

            return password;

            //return Membership.GeneratePassword(maxCharacters, minNumeric);
        }

        internal static string GetRandomString(string allowedCharacters, int maxCharacters)
        {
            var sb = new StringBuilder();
            var randomNumber = new Random();

            for (var i = 0; i < maxCharacters; i++)
            {
                sb.Append(allowedCharacters[randomNumber.Next(0, allowedCharacters.Length)]);
            }

            return sb.ToString();
        }

        internal static int ToInt(this string text, bool positive = true)
        {
            if (int.TryParse(text, out int _value))
            {
                return positive && _value < 1 ? 0 : _value;
            }

            return 0;
        }

        /// <summary>
        /// Generates a random password based on the rules passed in the parameters
        /// </summary>
        /// <param name="includeLowercase">Bool to say if lowercase are required</param>
        /// <param name="includeUppercase">Bool to say if uppercase are required</param>
        /// <param name="includeNumeric">Bool to say if numerics are required</param>
        /// <param name="includeSpecial">Bool to say if special characters are required</param>
        /// <param name="includeSpaces">Bool to say if spaces are required</param>
        /// <param name="lengthOfPassword">Length of password required. Should be between 8 and 128</param>
        /// <returns></returns>
        public static string GeneratePassword(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial, bool includeSpaces, int lengthOfPassword)
        {
            const int MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS = 2;
            const string LOWERCASE_CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
            const string UPPERCASE_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string NUMERIC_CHARACTERS = "0123456789";
            const string SPECIAL_CHARACTERS = @"!#$%&*@\";
            const string SPACE_CHARACTER = " ";
            const int PASSWORD_LENGTH_MIN = 8;
            const int PASSWORD_LENGTH_MAX = 128;

            if (lengthOfPassword < PASSWORD_LENGTH_MIN || lengthOfPassword > PASSWORD_LENGTH_MAX)
            {
                return "Password length must be between 8 and 128.";
            }

            string characterSet = "";

            if (includeLowercase)
            {
                characterSet += LOWERCASE_CHARACTERS;
            }

            if (includeUppercase)
            {
                characterSet += UPPERCASE_CHARACTERS;
            }

            if (includeNumeric)
            {
                characterSet += NUMERIC_CHARACTERS;
            }

            if (includeSpecial)
            {
                characterSet += SPECIAL_CHARACTERS;
            }

            if (includeSpaces)
            {
                characterSet += SPACE_CHARACTER;
            }

            char[] password = new char[lengthOfPassword];
            int characterSetLength = characterSet.Length;

            System.Random random = new System.Random();
            for (int characterPosition = 0; characterPosition < lengthOfPassword; characterPosition++)
            {
                password[characterPosition] = characterSet[random.Next(characterSetLength - 1)];

                bool moreThanTwoIdenticalInARow =
                    characterPosition > MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS
                    && password[characterPosition] == password[characterPosition - 1]
                    && password[characterPosition - 1] == password[characterPosition - 2];

                if (moreThanTwoIdenticalInARow)
                {
                    characterPosition--;
                }
            }

            return string.Join(null, password);
        }
        public static string ParseVerificationLinkFromBody(string body)
        {
            var match = Regex.Match(body, @":\/\/.*?guid=.* ");
            if (match.Success)
                return "https" + match.Value.Remove(match.Value.Length - 1);
            else
            {
                Console.WriteLine("Regex coudn't find any validation link");
                return null;
            }
                
        }
        /// <summary>
        /// Checks if the password created is valid
        /// </summary>
        /// <param name="includeLowercase">Bool to say if lowercase are required</param>
        /// <param name="includeUppercase">Bool to say if uppercase are required</param>
        /// <param name="includeNumeric">Bool to say if numerics are required</param>
        /// <param name="includeSpecial">Bool to say if special characters are required</param>
        /// <param name="includeSpaces">Bool to say if spaces are required</param>
        /// <param name="password">Generated password</param>
        /// <returns>True or False to say if the password is valid or not</returns>
        public static bool PasswordIsValid(bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial, bool includeSpaces, string password)
        {
            const string REGEX_LOWERCASE = @"[a-z]";
            const string REGEX_UPPERCASE = @"[A-Z]";
            const string REGEX_NUMERIC = @"[\d]";
            const string REGEX_SPECIAL = @"([!#$%&*@\\])+";
            const string REGEX_SPACE = @"([ ])+";

            bool lowerCaseIsValid = !includeLowercase || (includeLowercase && Regex.IsMatch(password, REGEX_LOWERCASE));
            bool upperCaseIsValid = !includeUppercase || (includeUppercase && Regex.IsMatch(password, REGEX_UPPERCASE));
            bool numericIsValid = !includeNumeric || (includeNumeric && Regex.IsMatch(password, REGEX_NUMERIC));
            bool symbolsAreValid = !includeSpecial || (includeSpecial && Regex.IsMatch(password, REGEX_SPECIAL));
            bool spacesAreValid = !includeSpaces || (includeSpaces && Regex.IsMatch(password, REGEX_SPACE));

            return lowerCaseIsValid && upperCaseIsValid && numericIsValid && symbolsAreValid && spacesAreValid;
        }
    }
}