# saml-auth-lab

Single sign on providers are useful and all around us these days.
When using SAML2 for SSO we need to take some precautions to avoid implementing ineffective authentication mechanisms.

Some mistakes that can be made include not validating the signatures (SAML2 messages should always be signed) of the responses.
We can also process messages which we didn't ask for. While a case can be made for this it means that the Identity provider can decide when a message is sent.

This repository contains an Identity provider where a user authenticates and a service provider which uses the Identity provider to authenticate their users.
There are 3 consumer URLs which provide a different number of checks.

Due to the use of `ITfoxtec.Identity.Saml2.MvcCore` we cannot simulate all vulnerabilities that are possible.
Some of these include

* Processing generic XML as a SAML2 message
* Not handling expired messages (there is a timeframe when the message is valid)


