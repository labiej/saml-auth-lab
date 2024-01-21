namespace SamlAuthLab.ServiceProvider.Data;

public class User
{
    /// <summary>
    /// The shared identifier which allows us to use the IdentityProvider's response to
    /// authenticate a local user.
    /// </summary>
    public string SharedIdentifier { get; set; } = string.Empty;

    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<UserRole> Roles { get; set; } = new();
}
