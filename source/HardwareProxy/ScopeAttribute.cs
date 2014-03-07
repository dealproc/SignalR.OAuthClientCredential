using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Microsoft.AspNet.SignalR {
	public class ScopeAttribute : AuthorizeAttribute {
		string[] _RequiredScopes;
		public ScopeAttribute(params string[] requiredScopes) {
			_RequiredScopes = requiredScopes;
		}
		protected override bool UserAuthorized(IPrincipal user) {

			if (user == null) {
				throw new ArgumentNullException("user");
			}

			var principal = (ClaimsPrincipal)user;
			if (principal != null) {

				var scopes = principal.Claims.Where(x => x.Type == "scope");

				var scopeClaims = string.Join(",", scopes.Select(x => x.Value));

				Trace.WriteLine("User Authenticated: " + principal.Identity.IsAuthenticated.ToString());
				Trace.WriteLine("User's Name: " + principal.Identity.Name);
				Trace.WriteLine("Authentication Type: " + principal.Identity.AuthenticationType);
				Trace.WriteLine("User's scopes: " + scopeClaims);

				return _RequiredScopes.All(rs => scopes.Any(userScope => userScope.Value == rs));
			}

			return false;
		}
	}
}