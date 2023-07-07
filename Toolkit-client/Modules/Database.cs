using System;
using Toolkit_Client.Models;

namespace Toolkit_Client.Modules
{
    public class Database
    {
        private static string DbFilePath;

        public static void InitDbConnection()
        {
            string executablePath = Environment.CurrentDirectory;
            string dbFilePath = executablePath + "\\Toolkit.db";
            DbFilePath = dbFilePath;
        }

        public static string GetDbFilePath()
        {
            return DbFilePath;
        }

        public enum SQLiteErrorCode : int
        {
            SQLITE_CONSTRAINT_FOREIGNKEY = 787,
            SQLITE_CONSTRAINT_UNIQUE = 2067
        }

        public static User? FindUserById(long id, ToolkitContext? context)
        {
            User? result = null;
            bool contextPassed = (context != null);
            var db = (contextPassed) ? context : new ToolkitContext();
            foreach (var user in db.Users) {
                if (user.Id == id) {
                    result = user;
                    break;
                }
            }

            if (!contextPassed)
                db.Dispose();

            return result;
        }

        public static Category? FindCategoryById(long id, ToolkitContext? context)
        {
            Category? result = null;
            bool contextPassed = (context != null);
            var db = (contextPassed) ? context : new ToolkitContext();
            foreach (var category in db.Categories) {
                if (category.Id == id) {
                    result = category;
                    break;
                }
            }

            if (!contextPassed)
                db.Dispose();

            return result;
        }

        public static Tag? FindTagById(long id, ToolkitContext? context)
        {
            Tag? result = null;
            bool contextPassed = (context != null);
            var db = (contextPassed) ? context : new ToolkitContext();
            foreach (var tag in db.Tags) {
                if (tag.Id == id) {
                    result = tag;
                    break;
                }
            }

            if (!contextPassed)
                db.Dispose();

            return result;
        }

        public static Company? FindCompanyByOwnerUserId(long id, ToolkitContext? context) {
            Company? result = null;
            bool contextPassed = (context != null);
            var db = (contextPassed) ? context : new ToolkitContext();
            foreach (var company in db.Companies) {
                if (company.OwnerUserId == id) {
                    result = company;
                    break;
                }
            }

            if (!contextPassed)
                db.Dispose();

            return result;
        }

        public static App? FindAppById(long id, ToolkitContext? context) {
            App? result = null;
            bool contextPassed = (context != null);
            var db = (contextPassed) ? context : new ToolkitContext();
            foreach (var app in db.Apps) {
                if (app.Id == id) {
                    result = app;
                    break;
                }
            }

            if (!contextPassed)
                db.Dispose();

            return result;
        }

        public static AppStorePage? FindAppStorePageByAppId(long id, ToolkitContext? context) {
            AppStorePage? result = null;
            bool contextPassed = (context != null);
            var db = (contextPassed) ? context : new ToolkitContext();
            foreach (var page in db.AppStorePages) {
                if (page.Id == id) {
                    result = page;
                    break;
                }
            }

            if (!contextPassed)
                db.Dispose();

            return result;
        }
    }
}
