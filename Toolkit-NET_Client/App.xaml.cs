using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Security.Cryptography;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Toolkit_NET_Client.Models;

namespace Toolkit_NET_Client
{
    public enum SQLiteErrorCode : int
    {
        SQLITE_CONSTRAINT_FOREIGNKEY = 787,
        SQLITE_CONSTRAINT_UNIQUE = 2067
    }

    public enum RegisterUserResultType
    {
        SUCCESS = 0,

        FAIL_EMPTY_LOGIN = 1,
        FAIL_LONG_LOGIN = 2,

        FAIL_EMPTY_PASSWORD = 3,
        FAIL_SHORT_PASSWORD = 4,
        FAIL_LONG_PASSWORD = 5,
        FAIL_WEAK_PASSWORD = 6,

        FAIL_EMPTY_USERNAME = 7,
        FAIL_LONG_USERNAME = 8,

        FAIL_EMPTY_EMAIL = 9,
        FAIL_LONG_EMAIL = 10,
        FAIL_INCORRECT_EMAIL = 11,

        FAIL_EMPTY_COUNTRY = 12,
        FAIL_INCORRECT_COUNTRY_CODE = 13,
        FAIL_COUNTRY_NOT_FOUND = 14,

        FAIL_EMPTY_PHONE = 15,
        FAIL_INCORRECT_PHONE = 16,

        FAIL_ALREADY_REGISTERED = 17,
        FAIL_DATABASE_FAILURE = 18
    }

    public enum AuthUserResultType
    {
        SUCCESS = 0,

        FAIL_EMPTY_LOGIN = 1,
        FAIL_EMPTY_PASSWORD = 2,

        FAIL_INCORRECT_CREDENTIALS = 3
    }

    public struct RegisterUserResult
    {
        public User? user;
        public RegisterUserResultType result;
    }

    public struct AuthUserResult
    {
        public User? user;
        public AuthUserResultType result;
    }

    /// <summary>
    /// Interaction logic for ToolkitApp.xaml
    /// </summary>
    public partial class ToolkitApp : Application
    {
        public static Color Color_Error   = (Color)ColorConverter.ConvertFromString("#d1473d");
        public static Color Color_Warning = (Color)ColorConverter.ConvertFromString("#cc8635");
        public static Color Color_Success = (Color)ColorConverter.ConvertFromString("#2f9e2b");
        public static Color Color_DefaultBorderBrush = (Color)ColorConverter.ConvertFromString("#59afff");

        public static SolidColorBrush SolidColorBrush_Error   = new SolidColorBrush(Color_Error);
        public static SolidColorBrush SolidColorBrush_Warning = new SolidColorBrush(Color_Warning);
        public static SolidColorBrush SolidColorBrush_Success = new SolidColorBrush(Color_Success);
        public static SolidColorBrush SolidColorBrush_DefaultBorderBrush = new SolidColorBrush(Color_DefaultBorderBrush);

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

        public static string GetHashSHA256(string input)
        {
            string result = null;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytesInput = Encoding.UTF8.GetBytes(input);
                byte[] bytesOutput = new byte[32];
                int bytesWritten;
            
                bool computed = sha256.TryComputeHash(bytesInput, bytesOutput, out bytesWritten);
                if (computed && bytesWritten == 32)
                {
                    StringBuilder hashBuilder = new StringBuilder();

                    for (int i = 0; i < bytesWritten; i++)
                        hashBuilder.Append(bytesOutput[i].ToString("x2"));

                    result = hashBuilder.ToString();
                }
            }

            return result;
        }

        public static string GetErrorMessageFromRegisterResult(RegisterUserResultType result)
        {
            switch (result)
            {
                case RegisterUserResultType.FAIL_EMPTY_LOGIN:            return "Введите логин.";
                case RegisterUserResultType.FAIL_LONG_LOGIN:             return "Логин превышает максимальное количество допустимых символов.";

                case RegisterUserResultType.FAIL_EMPTY_PASSWORD:         return "Введите пароль.";
                case RegisterUserResultType.FAIL_SHORT_PASSWORD:         return $"Пароль должен содержать минимум {User.PASSWORD_MIN_LENGTH} символов.";
                case RegisterUserResultType.FAIL_LONG_PASSWORD:          return $"Пароль должен содержать максимум {User.PASSWORD_MAX_LENGTH} символов.";
                case RegisterUserResultType.FAIL_WEAK_PASSWORD:          return "Данный пароль ненадежный. Используйте как минимум 1 строчную, 1 заглавную и 1 цифру.";

                case RegisterUserResultType.FAIL_EMPTY_USERNAME:         return "Введите отображаемое имя.";
                case RegisterUserResultType.FAIL_LONG_USERNAME:          return $"Отображаемое имя должно содержать максимум {User.USERNAME_MAX_LENGTH} символов.";

                case RegisterUserResultType.FAIL_EMPTY_EMAIL:            return "Введите эл. почту.";
                case RegisterUserResultType.FAIL_LONG_EMAIL:             return $"Эл. почта должна содержать максимум {User.EMAIL_MAX_LENGTH} символов.";
                case RegisterUserResultType.FAIL_INCORRECT_EMAIL:        return "Данная эл. почта некорректна. Пожалуйста, введите корректную эл. почту.";

                case RegisterUserResultType.FAIL_EMPTY_COUNTRY:          return "Выберите страну.";
                case RegisterUserResultType.FAIL_INCORRECT_COUNTRY_CODE: return "Данный код страны некорректный. Пожалуйста, выберите корректную страну.";
                case RegisterUserResultType.FAIL_COUNTRY_NOT_FOUND:      return "Данная страна не поддерживается. Пожалуйста, выберите поддерживаемую страну.";

                case RegisterUserResultType.FAIL_EMPTY_PHONE:            return "Введите номер телефона.";
                case RegisterUserResultType.FAIL_INCORRECT_PHONE:        return "Данный номер телефона некорректный. Пожалуйста, введите корректный номер телефона.";

                case RegisterUserResultType.FAIL_ALREADY_REGISTERED:     return "Данный пользователь уже зарегистрирован. Пожалуйста, введите другие данные.";
                case RegisterUserResultType.FAIL_DATABASE_FAILURE:       return "Произошла неизвестная ошибка при регистрации пользователя. Пожалуйста, попробуйте еще раз или обратитесь в поддержку.";
                
                default:                                                 return null;
            }
        }

        public static string GetErrorMessageFromAuthResult(AuthUserResultType result)
        {
            switch (result)
            {
                case AuthUserResultType.FAIL_EMPTY_LOGIN:           return "Введите логин.";
                case AuthUserResultType.FAIL_EMPTY_PASSWORD:        return "Введите пароль.";

                case AuthUserResultType.FAIL_INCORRECT_CREDENTIALS: return "Неверный логин или пароль.";
                
                default:                                            return null;
            }
        }

        public static RegisterUserResultType CanRegisterUser(string login, string password, string username, string email, string countryId)
        {
            if (string.IsNullOrWhiteSpace(login))
                return RegisterUserResultType.FAIL_EMPTY_LOGIN;

            if (login.Length > User.LOGIN_MAX_LENGTH)
                return RegisterUserResultType.FAIL_LONG_LOGIN;

            if (string.IsNullOrWhiteSpace(password))
                return RegisterUserResultType.FAIL_EMPTY_PASSWORD;

            if (password.Length < User.PASSWORD_MIN_LENGTH)
                return RegisterUserResultType.FAIL_SHORT_PASSWORD;

            if (password.Length > User.PASSWORD_MAX_LENGTH)
                return RegisterUserResultType.FAIL_LONG_PASSWORD;

            if (!IsStrongPassword(password))
                return RegisterUserResultType.FAIL_WEAK_PASSWORD;

            if (string.IsNullOrWhiteSpace(username))
                return RegisterUserResultType.FAIL_EMPTY_USERNAME;

            if (username.Length > 64)
                return RegisterUserResultType.FAIL_LONG_USERNAME;

            if (string.IsNullOrWhiteSpace(email))
                return RegisterUserResultType.FAIL_EMPTY_EMAIL;

            if (email.Length > User.EMAIL_MAX_LENGTH)
                return RegisterUserResultType.FAIL_LONG_EMAIL;

            if (!IsCorrectEmail(email))
                return RegisterUserResultType.FAIL_INCORRECT_EMAIL;

            if (string.IsNullOrWhiteSpace(countryId))
                return RegisterUserResultType.FAIL_EMPTY_COUNTRY;

            if (countryId.Length != Country.TWO_LETTER_ISO_CODE_LENGTH)
                return RegisterUserResultType.FAIL_INCORRECT_COUNTRY_CODE;

            return RegisterUserResultType.SUCCESS;
        }

        public static RegisterUserResultType IsValidLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return RegisterUserResultType.FAIL_EMPTY_LOGIN;

            if (login.Length > User.LOGIN_MAX_LENGTH)
                return RegisterUserResultType.FAIL_LONG_LOGIN;

            return RegisterUserResultType.SUCCESS;
        }

        public static RegisterUserResultType IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return RegisterUserResultType.FAIL_EMPTY_PASSWORD;

            if (password.Length < User.PASSWORD_MIN_LENGTH)
                return RegisterUserResultType.FAIL_SHORT_PASSWORD;

            if (password.Length > User.PASSWORD_MAX_LENGTH)
                return RegisterUserResultType.FAIL_LONG_PASSWORD;

            if (!IsStrongPassword(password))
                return RegisterUserResultType.FAIL_WEAK_PASSWORD;

            return RegisterUserResultType.SUCCESS;
        }

        public static RegisterUserResultType IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return RegisterUserResultType.FAIL_EMPTY_USERNAME;

            if (username.Length > 64)
                return RegisterUserResultType.FAIL_LONG_USERNAME;

            return RegisterUserResultType.SUCCESS;
        }

        public static RegisterUserResultType IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RegisterUserResultType.FAIL_EMPTY_EMAIL;

            if (email.Length > User.EMAIL_MAX_LENGTH)
                return RegisterUserResultType.FAIL_LONG_EMAIL;

            if (!IsCorrectEmail(email))
                return RegisterUserResultType.FAIL_INCORRECT_EMAIL;

            return RegisterUserResultType.SUCCESS;
        }

        public static RegisterUserResultType IsValidCountry(string countryId)
        {
            if (string.IsNullOrWhiteSpace(countryId))
                return RegisterUserResultType.FAIL_EMPTY_COUNTRY;

            if (countryId.Length != Country.TWO_LETTER_ISO_CODE_LENGTH)
                return RegisterUserResultType.FAIL_INCORRECT_COUNTRY_CODE;

            return RegisterUserResultType.SUCCESS;
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

        public static RegisterUserResultType IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return RegisterUserResultType.FAIL_EMPTY_PHONE;

            if (!IsCorrectPhone(phone))
                return RegisterUserResultType.FAIL_INCORRECT_PHONE;

            return RegisterUserResultType.SUCCESS;
        }

        public static RegisterUserResult RegisterUser(string login, string password, string username, string email, string countryId)
        {
            RegisterUserResult result;
            result.user = null;
            RegisterUserResultType canRegister = CanRegisterUser(login, password, username, email, countryId);
            result.result = canRegister;
            if (canRegister != RegisterUserResultType.SUCCESS)
                return result;

            result = _UncheckedRegisterUser(login, password, username, email, countryId);

            return result;
        }

        public static RegisterUserResult _UncheckedRegisterUser(string login, string password, string username, string email, string countryId)
        {
            RegisterUserResult result;
            result.user = null;
            result.result = RegisterUserResultType.SUCCESS;

            using (var db = new ToolkitContext())
            {
                User user = new User()
                {
                    Login = login,
                    Password = GetHashSHA256(password),
                    Username = username,
                    Email = email,
                    CountryId = countryId
                };

                db.Users.Add(user);

                try
                {
                    db.SaveChanges();
                    result.user = user;
                }
                catch (DbUpdateException dbUpdateException)
                {
                    var innerException = dbUpdateException.InnerException;
                    if (innerException == null)
                    {
                        Debug.WriteLine("Unknown DB failure: there was no `InnerException` in `DbUpdateException`.");
                        result.result = RegisterUserResultType.FAIL_DATABASE_FAILURE;
                        return result;
                    }

                    if (innerException is SqliteException)
                    {
                        SqliteException sqliteException = (SqliteException)innerException;
                        if (sqliteException.SqliteExtendedErrorCode == (int)SQLiteErrorCode.SQLITE_CONSTRAINT_FOREIGNKEY)
                        {
                            result.result = RegisterUserResultType.FAIL_COUNTRY_NOT_FOUND;
                            return result;
                        }
                    }
                }
            }

            return result;
        }

        public static AuthUserResult AuthUser(string login, string password)
        {
            AuthUserResult result;
            result.user = null;

            if (string.IsNullOrWhiteSpace(login))
            {
                result.result = AuthUserResultType.FAIL_EMPTY_LOGIN;
                return result;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                result.result = AuthUserResultType.FAIL_EMPTY_PASSWORD;
                return result;
            }

            using (var db = new ToolkitContext())
            {
                string hashPassword = GetHashSHA256(password);
                foreach (var user in db.Users)
                {
                    if (user.Login == login && user.Password == hashPassword)
                    {
                        result.user = user;
                        result.result = AuthUserResultType.SUCCESS;
                        return result;
                    }
                }
            }

            result.result = AuthUserResultType.FAIL_INCORRECT_CREDENTIALS;
            return result;
        }

        public static void SetStatusError(TextBlock statusTextBlock, string? error)
        {
            if (string.IsNullOrEmpty(error))
            {
                statusTextBlock.Visibility = Visibility.Collapsed;
                return;
            }

            if (statusTextBlock.Visibility == Visibility.Collapsed)
                statusTextBlock.Visibility = Visibility.Visible;

            statusTextBlock.Text = error;
            statusTextBlock.Foreground = SolidColorBrush_Error;
        }

        public static void SetStatusWarning(TextBlock statusTextBlock, string? warning)
        {
            if (string.IsNullOrEmpty(warning))
            {
                statusTextBlock.Visibility = Visibility.Collapsed;
                return;
            }

            if (statusTextBlock.Visibility == Visibility.Collapsed)
                statusTextBlock.Visibility = Visibility.Visible;

            statusTextBlock.Text = warning;
            statusTextBlock.Foreground = SolidColorBrush_Warning;
        }

        public static void SetStatusSuccess(TextBlock statusTextBlock, string? success)
        {
            if (string.IsNullOrEmpty(success))
            {
                statusTextBlock.Visibility = Visibility.Collapsed;
                return;
            }

            if (statusTextBlock.Visibility == Visibility.Collapsed)
                statusTextBlock.Visibility = Visibility.Visible;

            statusTextBlock.Text = success;
            statusTextBlock.Foreground = SolidColorBrush_Success;
        }

        public static void ClearStatus(TextBlock statusTextBlock)
        {
            statusTextBlock.Visibility = Visibility.Collapsed;
        }

        public static User? FindUser(long id, ToolkitContext? context)
        {
            User? result = null;
            bool contextPassed = (context != null);
            var db = (contextPassed) ? context : new ToolkitContext();
            foreach (var user in db.Users)
            {
                if (user.Id == id)
                {
                    result = user;
                    break;
                }
            }

            if (!contextPassed)
                db.Dispose();

            return result;
        }

        public static string GetDatetimeMonthName(int month)
        {
            switch (month)
            {
                case 1:  return "Января";
                case 2:  return "Февраля";
                case 3:  return "Марта";
                case 4:  return "Апреля";
                case 5:  return "Мая";
                case 6:  return "Июня";
                case 7:  return "Июля";
                case 8:  return "Августа";
                case 9:  return "Сентября";
                case 10: return "Октября";
                case 11: return "Ноября";
                case 12: return "Декабря";
                default: return null;
            }
        }
    }
}
