using Toolkit_Client;
using Toolkit_Client.Models;

namespace Toolkit_Client_Tests
{
    public class UserTests
    {
        [Theory]
        [InlineData(null, null, null, null, null)]
        [InlineData("", "", "", "", "")]
        [InlineData("", "QWErty123", "TestUsername", "test@gmail.com", "ru")]
        [InlineData("TestLogin", "", "TestUsername", "test@gmail.com", "ru")]
        [InlineData("TestLogin", "QWErty123", "", "test@gmail.com", "ru")]
        [InlineData("TestLogin", "QWErty123", "TestUsername", "", "ru")]
        [InlineData("TestLogin", "QWErty123", "TestUsername", "test@gmail.com", "")]
        public void RegisterEmptyDataTest(string login, string password, string username, string email, string countryId)
        {
            ToolkitApp.InitDbConnection();
            var result = ToolkitApp.CanRegisterUser(
                login, 
                password, 
                username, 
                email, 
                countryId
            );
            Assert.True(result != RegisterUserResultType.SUCCESS);
        }

        [Theory]
        [InlineData("TestLogin", "1", "TestUsername", "test@gmail.com", "ru")]
        [InlineData("TestLogin", "12345678", "TestUsername", "test@gmail.com", "ru")]
        [InlineData("TestLogin", "abcdefgh", "TestUsername", "test@gmail.com", "ru")]
        [InlineData("TestLogin", "ABCDEFGH", "TestUsername", "test@gmail.com", "ru")]
        [InlineData("TestLogin", "ABCDefgh", "TestUsername", "test@gmail.com", "ru")]
        [InlineData("TestLogin", "!@#$%^&*", "TestUsername", "test@gmail.com", "ru")]
        public void RegisterBadPasswordDataTest(string login, string password, string username, string email, string countryId)
        {
            ToolkitApp.InitDbConnection();
            var result = ToolkitApp.CanRegisterUser(
                login, 
                password, 
                username, 
                email, 
                countryId
            );
            Assert.True(result != RegisterUserResultType.SUCCESS);
        }

        [Theory]
        [InlineData("TestLogin", "123456Qq", "TestUsername", "123", "ru")]
        [InlineData("TestLogin", "123456Qq", "TestUsername", "@", "ru")]
        [InlineData("TestLogin", "123456Qq", "TestUsername", "123@", "ru")]
        [InlineData("TestLogin", "123456Qq", "TestUsername", "gmail.com", "ru")]
        [InlineData("TestLogin", "123456Qq", "TestUsername", "@gmail.com", "ru")]
        [InlineData("TestLogin", "123456Qq", "TestUsername", "@gmail.com123", "ru")]
        [InlineData("TestLogin", "123456Qq", "TestUsername", "1@1", "ru")]
        [InlineData("TestLogin", "123456Qq", "TestUsername", "1@1.1", "ru")]
        public void RegisterBadEmailDataTest(string login, string password, string username, string email, string countryId)
        {
            ToolkitApp.InitDbConnection();
            var result = ToolkitApp.CanRegisterUser(
                login, 
                password, 
                username, 
                email, 
                countryId
            );
            Assert.True(result != RegisterUserResultType.SUCCESS);
        }

        [Fact]
        public void RegisterExistingDataTest()
        {
            ToolkitApp.InitDbConnection();
            User dbUser;
            using (var db = new ToolkitContext()) {
                dbUser = db.Users.FirstOrDefault();
            }
            var result = ToolkitApp.CanRegisterUser(
                dbUser.Login, 
                dbUser.Password, 
                dbUser.Username, 
                dbUser.Email, 
                dbUser.CountryId
            );
            Assert.True(result != RegisterUserResultType.SUCCESS);
        }

        [Theory]
        [InlineData("TestLogin", "QWErty123", "TestUsername", "test@gmail.com", "ru")]
        public void RegisterCorrectDataTest(string login, string password, string username, string email, string countryId)
        {
            ToolkitApp.InitDbConnection();
            var result = ToolkitApp.CanRegisterUser(
                login, 
                password, 
                username, 
                email, 
                countryId
            );
            Assert.True(result == RegisterUserResultType.SUCCESS);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("user", "")]
        [InlineData("", "password")]
        public void AuthEmptyDataTest(string login, string password)
        {
            ToolkitApp.InitDbConnection();
            var result = ToolkitApp.AuthUser(
                login, 
                password
            );
            Assert.True(result.result != AuthUserResultType.SUCCESS);
        }

        [Theory]
        [InlineData("DoNotExist", "DoNotExist")]
        public void AuthUnregisteredDataTest(string login, string password)
        {
            ToolkitApp.InitDbConnection();
            var result = ToolkitApp.AuthUser(
                login, 
                password
            );
            Assert.True(result.result != AuthUserResultType.SUCCESS);
        }

        [Theory]
        [InlineData("Usertest1", "Usertest1")]
        public void AuthCorrectDataTest(string login, string password)
        {
            ToolkitApp.InitDbConnection();
            var result = ToolkitApp.AuthUser(
                login, 
                password
            );
            Assert.True(result.result == AuthUserResultType.SUCCESS && result.user != null);
        }
    }
}