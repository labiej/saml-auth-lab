namespace SamlAuthLab.IdentityProvider.Cache;

using ITfoxtec.Identity.Saml2;

public class SamlRequestStore
{
    private readonly Dictionary<string, Saml2AuthnRequest> requests = new();
    public bool TryGetRequestForSessionId(string sessionId, out Saml2AuthnRequest? request)
    {
        return requests.TryGetValue(sessionId, out request);
    }

    public void StoreRequest(string sessionId, Saml2AuthnRequest request)
    {
        requests[sessionId] = request;
    }

}
