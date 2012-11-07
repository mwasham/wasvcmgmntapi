using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyVMPortal
{


    public class ValidationHelpers
    {
        private static readonly char[] WindowsComputerNameInvalidChars = @"`~!@#$%^&*()=+_[]{}\|;:.'"",<>/?".ToCharArray();
        private static readonly Regex NumericRegex = new Regex(@"^\d+$", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex PasswordHasLowerChar = new Regex(@"[a-z]", RegexOptions.Singleline | RegexOptions.CultureInvariant);
        private static readonly Regex PasswordHasUpperChar = new Regex(@"[A-Z]", RegexOptions.Singleline | RegexOptions.CultureInvariant);
        private static readonly Regex PasswordHasDigitChar = new Regex(@"\d", RegexOptions.Singleline | RegexOptions.CultureInvariant);
        private static readonly Regex PasswordHasSpecialChar = new Regex(@"\W", RegexOptions.Singleline | RegexOptions.CultureInvariant);

        private static readonly Regex[] PasswordCriteria = new Regex[] { PasswordHasLowerChar, PasswordHasUpperChar, PasswordHasDigitChar, PasswordHasSpecialChar };
        private const int PasswordMinComplexity = 3;
        private const int PasswordMinLength = 8;
        private const int PasswordMaxLength = 123;
        private const int LinuxUserNameMinLength = 1;
        private const int LinuxUserNameMaxLength = 64;
        private const int LinuxPasswordMinLength = 6;
        private const int LinuxPasswordMaxLength = 72;

        private const int WindowsComputerNameMaxLength = 15;

        public static bool IsLinuxPasswordValid(string password)
        {
            if (password.Length < LinuxPasswordMinLength || password.Length > LinuxPasswordMaxLength)
            {
                return false;
            }

            // Check complexity
            int complexity = PasswordCriteria.Count(criteria => criteria.IsMatch(password));
            if (complexity < PasswordMinComplexity)
            {
                return false;
            }

            return true;
        }

        public static bool IsWindowsPasswordValid(string password)
        {
            // Check length
            if (password.Length < PasswordMinLength || password.Length > PasswordMaxLength)
            {
                return false;
            }

            // Check complexity
            int complexity = PasswordCriteria.Count(criteria => criteria.IsMatch(password));
            if (complexity < PasswordMinComplexity)
            {
                return false;
            }

            return true;
        }

        public static bool IsLinuxHostNameValid(string hostName)
        {
            if (string.IsNullOrEmpty(hostName))
            {
                return false;
            }

            if (hostName.Length > 64)
            {
                return false;
            }

            return true;
        }

        public static bool IsWindowsComputerNameValid(string computerName)
        {
            if (string.IsNullOrEmpty(computerName))
            {
                return false;
            }

            if (computerName.Length > WindowsComputerNameMaxLength ||
                    computerName.IndexOfAny(WindowsComputerNameInvalidChars) != -1 ||
                    NumericRegex.IsMatch(computerName))
            {
                return false;
            }

            return true;
        }
    }

}