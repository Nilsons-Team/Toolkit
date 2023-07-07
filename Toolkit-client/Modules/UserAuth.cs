using Toolkit_Client.Models;

namespace Toolkit_Client.Modules
{
    public class UserAuth
    {
        public enum AuthUserResultType
        {
            SUCCESS = 0,

            FAIL_EMPTY_LOGIN = 1,
            FAIL_LONG_LOGIN = 2,

            FAIL_EMPTY_PASSWORD = 3,
            FAIL_SHORT_PASSWORD = 4,
            FAIL_LONG_PASSWORD = 5,

            FAIL_INCORRECT_CREDENTIALS = 6,
            FAIL_DATABASE_FAILURE = 7
        }

        public struct AuthUserResult
        {
            public User? user;
            public AuthUserResultType result;
        }

        public static string GetErrorMessageFromAuthResult(AuthUserResultType result)
        {
            switch (result)
            {
                case AuthUserResultType.FAIL_EMPTY_LOGIN:           return "Введите логин.";
                case AuthUserResultType.FAIL_LONG_LOGIN:            return $"Логин должен содержать максимум {User.LOGIN_MAX_LENGTH} символов.";

                case AuthUserResultType.FAIL_EMPTY_PASSWORD:        return "Введите пароль.";
                case AuthUserResultType.FAIL_SHORT_PASSWORD:        return $"Пароль должен содержать минимум {User.PASSWORD_MIN_LENGTH} символов.";
                case AuthUserResultType.FAIL_LONG_PASSWORD:         return $"Пароль должен содержать максимум {User.PASSWORD_MAX_LENGTH} символов.";

                case AuthUserResultType.FAIL_INCORRECT_CREDENTIALS: return "Неверный логин или пароль.";
                case AuthUserResultType.FAIL_DATABASE_FAILURE:      return "Произошла неизвестная ошибка при авторизации пользователя. Пожалуйста, попробуйте еще раз или обратитесь в поддержку.";
                
                default:                                            return null;
            }
        }

        public static AuthUserResultType IsValidLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return AuthUserResultType.FAIL_EMPTY_LOGIN;

            if (login.Length > User.LOGIN_MAX_LENGTH)
                return AuthUserResultType.FAIL_LONG_LOGIN;

            return AuthUserResultType.SUCCESS;
        }

        public static AuthUserResultType IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return AuthUserResultType.FAIL_EMPTY_PASSWORD;

            if (password.Length < User.PASSWORD_MIN_LENGTH)
                return AuthUserResultType.FAIL_SHORT_PASSWORD;

            if (password.Length > User.PASSWORD_MAX_LENGTH)
                return AuthUserResultType.FAIL_LONG_PASSWORD;

            return AuthUserResultType.SUCCESS;
        }

        public static AuthUserResult AuthUser(string login, string password)
        {
            AuthUserResult result;
            result.user = null;

            if (string.IsNullOrWhiteSpace(login)) {
                result.result = AuthUserResultType.FAIL_EMPTY_LOGIN;
                return result;
            }

            if (string.IsNullOrWhiteSpace(password)) {
                result.result = AuthUserResultType.FAIL_EMPTY_PASSWORD;
                return result;
            }

            using (var db = new ToolkitContext())
            {
                string hashPassword = Encryption.GetHashSHA256(password);
                foreach (var user in db.Users) {
                    if (user.Login == login && user.Password == hashPassword) {
                        result.user = user;
                        result.result = AuthUserResultType.SUCCESS;
                        return result;
                    }
                }
            }

            result.result = AuthUserResultType.FAIL_INCORRECT_CREDENTIALS;
            return result;
        }
    }
}
