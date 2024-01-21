namespace SamlAuthLab.ServiceProvider.Data;

public class UserStoreOptions
{
    public const string Name = "UserStore";

    public List<User> Users { get; set; } = new();
}
