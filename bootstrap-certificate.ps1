$params = @{
    Type = 'Custom'
    Subject = 'CN=saml-auth-lab-idp,OU=UserAccounts,DC=corp,DC=contoso,DC=com'
    KeyUsage = 'DigitalSignature'
    KeyAlgorithm = 'RSA'
    KeyLength = 2048
    CertStoreLocation = 'Cert:\CurrentUser\My'
}

# First generate the certificate 
$cert = New-SelfSignedCertificate @params

# Choose a password for the exported pfx file
$pwd = ConvertTo-SecureString -String '1234' -Force -AsPlainText

# Export the generated certificate as pfx
Export-PfxCertificate -Cert $cert -FilePath .\saml-auth-lab.pfx -Password $pwd

# Delete the certificate from the store
Remove-Item $cert.PSPath 
