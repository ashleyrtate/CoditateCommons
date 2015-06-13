using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Utility methods for working with email addresses.
    /// </summary>
    public static class EmailUtils
    {
        /// <summary>
        /// Default separator strings checked when parsing a list
        /// of email addresses stored as a single string.
        /// </summary>
        /// <remarks>
        /// The default separators are comma, semi-colon, and <see cref="Environment.NewLine"/>.
        /// </remarks>
        public static readonly string[] DefaultSeparators = {",", ";", "\n"};

        /// <summary>
        /// Parses a list of unique email addresses from a string.
        /// </summary>
        /// <param name="emailAddresses">The email addresses to parse.</param>
        /// <param name="separators">The email separators.</param>
        /// <param name="invalidEmail">The first invalid email found.</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public static List<MailAddress> GetAddresses(string emailAddresses, string[] separators, out string invalidEmail)
        {
            Arg.CheckNull("emailAddresses", emailAddresses);
            Arg.CheckNull("separators", separators);

            invalidEmail = null;
            var mailAddresses = new Dictionary<string, MailAddress>();
            emailAddresses = emailAddresses.Trim();
            string[] addresses = emailAddresses.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string address in addresses)
            {
                string trimmedAddress = address.Trim();
                if (trimmedAddress.Length == 0)
                {
                    continue;
                }
                try
                {
                    var mailAddress = new MailAddress(trimmedAddress);
                    mailAddresses[mailAddress.Address.ToLower()] = mailAddress;
                }
                catch
                {
                    invalidEmail = address;
                    break;
                }
            }

            return mailAddresses.Values.ToList();
        }
    }
}