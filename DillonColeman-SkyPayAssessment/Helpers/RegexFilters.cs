using System.Text.RegularExpressions;

namespace DillonColeman_SkyPayAssessment.Helpers
{
    public class RegexFilters
    {
        // This regular expression will match phone numbers entered with delimiters (spaces, dots, brackets, etc.)
        private static readonly Regex validatePhoneNumberRegex = new Regex(@"^(\d{3})[-.]?(\d{3})[-.]?(\d{4})$");

        //"^\\+?\\d{1,4}?[-.\\s]?\\(?\\d{1,3}?\\)?[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,9}$"
        /*
            This C# regular expression is compliant to RFC 5322 standard which allows for the most complete validation. 
            Usually, you should not use it because it is an overkill. 
            In most cases apps are not able to handle all emails that this regex allows.                
         */
        private static readonly Regex validateEmailRegex = new Regex("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])");

        /*
            ^ asserts the start of the string.
            [a-zA-Z0-9_] matches any uppercase letter, lowercase letter, digit, or underscore character.
            {3,20} specifies the minimum and maximum length of the user name. In this case, the user name must be between 3 and 20 characters long.
            $ asserts the end of the string.
         */
        private static readonly Regex validateUserNameRegex = new Regex("^[a-zA-Z0-9]{3,20}$");

        /*
            ^ asserts the start of the string.
            (?=.*[a-z]) is a positive lookahead to ensure that there is at least one lowercase letter.
            (?=.*[A-Z]) is a positive lookahead to ensure that there is at least one uppercase letter.
            (?=.*\d) is a positive lookahead to ensure that there is at least one digit.
            .{8,} matches any character (except a newline) at least 8 times.
            $ asserts the end of the string.
         */
        private static readonly Regex validatePasswordRegex = new Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$");

        public static bool IsValidPhoneNum(string number)
        {
            return validatePhoneNumberRegex.IsMatch(number);
        }

        public static bool IsValidEmail(string email)
        {
            return validateEmailRegex.IsMatch(email);
        }

        public static bool IsValidUserName(string userName)
        {
            return validateUserNameRegex.IsMatch(userName);
        }

        public static bool IsValidPassword(string password)
        {
            return validatePasswordRegex.IsMatch(password);
        }
    }
}
