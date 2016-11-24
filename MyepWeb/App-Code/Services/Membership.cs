using System.Web.Security;

namespace Site
{
    public class SiteMembership : MembershipBase
    {
        public override bool ValidateUser(string email, string password)
        {
            var repo = Ioc.Get<Users>();
            var user = repo.Load(email);
            return user != null && user.Active && user.Password == password;
        }
    };

    public abstract class MembershipBase : MembershipProvider
    {
        public override bool ValidateUser(string username, string password)
        {
            return false;
        }

        public override MembershipUser GetUser(string name, bool userIsOnline)
        {
            return null;
        }

        public override string GetUserNameByEmail(string email)
        {
            return null;
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion,
                                                  string passwordAnswer, bool isApproved, object providerUserKey,
                                                  out MembershipCreateStatus status)
        {
            status = MembershipCreateStatus.ProviderError;
            return null;
        }

        public override string GetPassword(string userid, string answer)
        {
            return null;
        }

        public override bool ChangePassword(string userid, string oldPwd, string newPwd)
        {
            return false;
        }

        public override MembershipUser GetUser(object name, bool userIsOnline)
        {
            return GetUser(name.ToString(), userIsOnline);
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override bool EnablePasswordReset
        {
            get { return false; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override string ApplicationName
        {
            get { return ""; }
            set { }
        }

        public override bool ChangePasswordQuestionAndAnswer(string name, string password, string newPwdQuestion,
                                                             string newPwdAnswer)
        {
            return false;
        }

        public override string ResetPassword(string name, string answer)
        {
            return "";
        }

        public override void UpdateUser(MembershipUser user)
        {
        }

        public override bool DeleteUser(string name, bool deleteAllRelatedData)
        {
            return false;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = -1;
            return null;
        }

        public override int GetNumberOfUsersOnline()
        {
            return -1;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize,
                                                                 out int totalRecords)
        {
            totalRecords = -1;
            return new MembershipUserCollection();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize,
                                                                  out int totalRecords)
        {
            totalRecords = -1;
            return new MembershipUserCollection();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return 3; }
        }

        public override int PasswordAttemptWindow
        {
            get { return 5; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return true; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Clear; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 1; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return 0; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return null; }
        }

        public override bool UnlockUser(string userName)
        {
            return true;
        }
    };
}
