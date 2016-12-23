using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Site
{
    public class Users
    {
        private readonly IDb _db;

        public Users(IDb db)
        {
            _db = db;
        }

        public List<User> Query()
        {
            return _db
                .Query<User>()
                .OrderBy(x => x.Email)
                .ToList();
        }

        public User Load(int? id)
        {
            if (!id.HasValue)
                return null;

            return _db
                .Query<User>()
                .SingleOrDefault(x => x.Id == id);
        }

        public User Load(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return _db
                .Query<User>()
                .SingleOrDefault(x => x.Email == email);
        }

        public User LoadByCode(string resetCode)
        {
            if (string.IsNullOrWhiteSpace(resetCode))
                return null;

            return _db
                .Query<User>()
                .SingleOrDefault(x => x.ResetCode == resetCode);
        }

        public User Create()
        {
            return new User
            {
                Password = Guid.NewGuid().ToString("N").Substring(0, 8),
            };
        }

        public void Save(User model)
        {
            _db.Save(model, model.Id == 0);
            _db.SaveChanges();
        }

        public bool Login(string email, string password, bool remember)
        {
            if (Membership.ValidateUser(email, password))
            {
                FormsAuthentication.SetAuthCookie(email, remember);
                return true;
            }
            return false;
        }

        public void Logout()
        {
            HttpContext.Current.Session.Abandon();
            FormsAuthentication.SignOut();
        }

        public void EmailLoginInfo(User model)
        {
            model.ResetCode = Guid.NewGuid().ToString("N");
            Save(model);

            try
            {
                var email = new System.Net.Mail.MailMessage();
                email.To.Add(model.Email);
                email.Subject = "MYEP Reset Password";
                email.Body = "Url: " + Web.ServerPath + "/home/reset/" + model.ResetCode;

                new System.Net.Mail.SmtpClient().Send(email);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error sending email to '" + model.Email + "'", ex);
            }
        }

        public void ResetPassword(User model, string password)
        {
            model.Password = password;
            model.ResetCode = null;
            Save(model);
        }
    };
}
