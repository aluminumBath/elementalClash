namespace Elementborn.Core.Social
{
    /// <summary>
    /// The owner / developer admin allow-list. Any Gmail address that normalises to the owner's
    /// (<see cref="SignatureCharacter.AdminEmail"/>) is treated as a global <see cref="UserRole.Admin"/>, so the
    /// owner's account moderates everywhere and can reach admin tooling. This is a personal owner allow-list; if
    /// the game ever needs more than one admin, move the list to server config.
    /// </summary>
    public static class AdminAccounts
    {
        /// <summary>True if <paramref name="email"/> resolves to the owner's address (Gmail dots and +suffixes ignored).</summary>
        public static bool IsAdminEmail(string email) =>
            !string.IsNullOrWhiteSpace(email) &&
            NormalizeGmail(email) == NormalizeGmail(SignatureCharacter.AdminEmail);

        /// <summary>The owner's directory entry, as a global Admin.</summary>
        public static UserRef OwnerUser(string id) =>
            new UserRef(string.IsNullOrWhiteSpace(id) ? SignatureCharacter.AdminEmail : id,
                        SignatureCharacter.Name, UserRole.Admin);

        /// <summary>Register (or elevate) the owner in a directory as a global Admin; returns the entry.</summary>
        public static UserRef ProvisionOwner(IUserDirectory directory, string id)
        {
            var owner = OwnerUser(id);
            directory?.Register(owner);
            return owner;
        }

        /// <summary>
        /// Gmail-normalise an address: trim, lowercase, and for gmail.com / googlemail.com strip dots and any
        /// "+suffix" from the local part (those all deliver to the same inbox). Other domains are left as-is
        /// apart from trim + lowercase.
        /// </summary>
        public static string NormalizeGmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";
            email = email.Trim().ToLowerInvariant();
            int at = email.IndexOf('@');
            if (at <= 0 || at == email.Length - 1) return email;

            string local = email.Substring(0, at);
            string domain = email.Substring(at + 1);
            if (domain == "gmail.com" || domain == "googlemail.com")
            {
                int plus = local.IndexOf('+');
                if (plus >= 0) local = local.Substring(0, plus);
                local = local.Replace(".", "");
                domain = "gmail.com";
            }
            return local + "@" + domain;
        }
    }
}
