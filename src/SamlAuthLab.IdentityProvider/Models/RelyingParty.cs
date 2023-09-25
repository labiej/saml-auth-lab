namespace SamlAuthLab.IdentityProvider.Models
{
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// The service provider using the identity provider
    /// </summary>
    public class RelyingParty
    {
        public string Metadata { get; set; }
        /// <summary>
        /// A URI identifying the RelyingParty
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// The AssertionConsumer URL
        /// </summary>
        public Uri AcsDestination { get; set; }

        public bool UseAcsArtifact { get; set; } = false;

        public Uri SingleLogoutDestination { get; set; }

        public X509Certificate2 SignatureValidationCertificate { get; set; }

        public X509Certificate2 EncryptionCertificate { get; set; }
    }
}
