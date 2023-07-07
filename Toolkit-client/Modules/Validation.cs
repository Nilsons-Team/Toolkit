using Toolkit_Client.Models;

namespace Toolkit_Client.Modules
{
    public class Validation
    {
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            bool hasLowercaseLetter = false;
            bool hasUppercaseLetter = false;
            bool hasDigit = false;
            int i = 0;
            while (!hasLowercaseLetter || !hasUppercaseLetter || !hasDigit)
            {
                char c = password[i];

                hasLowercaseLetter |= char.IsLower(c);
                hasUppercaseLetter |= char.IsUpper(c);
                hasDigit |= char.IsDigit(c);

                i++;
                if (i >= password.Length)
                    break;
            }

            if (hasLowercaseLetter && hasUppercaseLetter && hasDigit)
                return true;

            return false;
        }

        public static bool IsCorrectEmail(string email)
        {
            var emailTrimmed = email.Trim();
            var atIndex = emailTrimmed.IndexOf('@');
            if (atIndex < 1)
                return false; // @xxxx.yy - `@` is not found.

            var stringCursor = atIndex;
            do {
                stringCursor++;
                if (stringCursor >= emailTrimmed.Length)
                    return false; // @xxxx.yy - `.` is not found.
            } while (email[stringCursor] != '.');

            if (stringCursor == atIndex + 1)
                return false; // @xxxx.yy - `xxxx` can not be 0 characters.

            var maxCursor = emailTrimmed.Length - 1;
            if (stringCursor + 2 > maxCursor)
                return false; // @xxxx.yy - `yy` must be at least 2 characters.

            return true;
        }

        public static bool IsCorrectPhone(string phone)
        {
            phone = phone.Trim();

            if (phone.Length < User.PHONE_MIN_LENGTH)
                return false;

            if (phone.Length > User.PHONE_MAX_LENGTH)
                return false;

            foreach (char c in phone)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }
    }
}
