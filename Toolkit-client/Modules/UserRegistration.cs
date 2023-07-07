using System;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Toolkit_Client.Models;

using static Toolkit_Client.Modules.Database;

namespace Toolkit_Client.Modules
{
    public class UserRegistration
    {
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

        public struct RegisterUserResult
        {
            public User? user;
            public RegisterUserResultType result;
        }

        public static string GetErrorMessageFromRegisterUserResult(RegisterUserResultType result)
        {
            switch (result)
            {
                case RegisterUserResultType.FAIL_EMPTY_LOGIN:            return "Введите логин.";
                case RegisterUserResultType.FAIL_LONG_LOGIN:             return $"Логин должен содержать минимум {User.LOGIN_MAX_LENGTH} символов.";

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

        public static RegisterUserResultType CanRegisterUser(string login, string password, string username, string email, string countryId, string? phone)
        {
            var loginResult = IsValidLogin(login);
            if (loginResult != RegisterUserResultType.SUCCESS)
                return loginResult;

            var passwordResult = IsValidPassword(password);
            if (passwordResult != RegisterUserResultType.SUCCESS)
                return passwordResult;

            var usernameResult = IsValidUsername(username);
            if (usernameResult != RegisterUserResultType.SUCCESS)
                return usernameResult;

            var emailResult = IsValidEmail(email);
            if (emailResult != RegisterUserResultType.SUCCESS)
                return emailResult;

            var countryResult = IsValidCountry(countryId);
            if (countryResult != RegisterUserResultType.SUCCESS)
                return countryResult;

            if (phone != null) {
                var phoneResult = IsValidPhone(phone);
                if (phoneResult != RegisterUserResultType.SUCCESS)
                    return phoneResult;
            }
            
            var registeredResult = IsUserAlreadyRegistered(login);
            if (registeredResult != RegisterUserResultType.SUCCESS)
                return registeredResult;

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

            if (!Validation.IsStrongPassword(password))
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

            if (!Validation.IsCorrectEmail(email))
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

        public static RegisterUserResultType IsUserAlreadyRegistered(string login) 
        {
            using (var db = new ToolkitContext()) {
                foreach (User user in db.Users)
                    if (user.Login == login)
                        return RegisterUserResultType.FAIL_ALREADY_REGISTERED;
            }

            return RegisterUserResultType.SUCCESS;
        }

        public static RegisterUserResultType IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return RegisterUserResultType.FAIL_EMPTY_PHONE;

            if (!Validation.IsCorrectPhone(phone))
                return RegisterUserResultType.FAIL_INCORRECT_PHONE;

            return RegisterUserResultType.SUCCESS;
        }

        public static RegisterUserResult RegisterUser(string login, string password, string username, string email, string countryId, string? phone)
        {
            RegisterUserResult result;
            result.user = null;
            RegisterUserResultType canRegister = CanRegisterUser(login, password, username, email, countryId, phone);
            result.result = canRegister;
            if (canRegister != RegisterUserResultType.SUCCESS)
                return result;

            result = _UncheckedRegisterUser(login, password, username, email, countryId, phone);

            return result;
        }

        public static RegisterUserResult _UncheckedRegisterUser(string login, string password, string username, string email, string countryId, string? phone)
        {
            RegisterUserResult result;
            result.user = null;
            result.result = RegisterUserResultType.SUCCESS;

            using (var db = new ToolkitContext()) {
                foreach (var dbUser in db.Users) {
                    if (dbUser.Login == login) {
                        result.result = RegisterUserResultType.FAIL_ALREADY_REGISTERED;
                        return result;
                    }
                }

                User user = new User()
                {
                    Login     = login,
                    Password  = Encryption.GetHashSHA256(password),
                    Username  = username,
                    Email     = email,
                    CountryId = countryId
                };

                if (phone != null)
                    user.Phone = phone;

                db.Users.Add(user);

                try {
                    result.user = user;
                    db.SaveChanges();
                } catch (DbUpdateException dbUpdateException) {
                    var innerException = dbUpdateException.InnerException;
                    if (innerException == null) {
                        Console.WriteLine("Unknown DB failure: there was no `InnerException` in `DbUpdateException`.");
                        result.result = RegisterUserResultType.FAIL_DATABASE_FAILURE;
                        return result;
                    }

                    if (innerException is SqliteException) {
                        SqliteException sqliteException = (SqliteException)innerException;
                        if (sqliteException.SqliteExtendedErrorCode == (int)SQLiteErrorCode.SQLITE_CONSTRAINT_FOREIGNKEY) {
                            result.result = RegisterUserResultType.FAIL_COUNTRY_NOT_FOUND;
                            return result;
                        }
                    }
                }
            }

            return result;
        }
    }
}
