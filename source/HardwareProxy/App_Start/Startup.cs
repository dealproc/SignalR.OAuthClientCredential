using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Thinktecture.IdentityModel.Owin;
using Thinktecture.IdentityModel.Tokens;


[assembly: OwinStartup(typeof(HardwareProxy.Startup))]

namespace HardwareProxy {
	public class Startup {
		public void Configuration(IAppBuilder app) {

			JwtSecurityTokenHandler.InboundClaimTypeMap = ClaimMappings.None; //new Dictionary<string, string>();

			app.UseJsonWebToken(
				issuer: ConfigurationManager.AppSettings["apiauth:authsvr"],
				audience: ConfigurationManager.AppSettings["apiauth:audience"],
				signingKey: ConfigurationManager.AppSettings["apiauth:signingkey"],
				location: TokenLocation.QueryString("access_token")
			);

			app.MapSignalR();
			app.UseWebApi(WebApiConfig.Register());
		}

		private Task<ClaimsPrincipal> TransformClaims(ClaimsPrincipal incoming) {
			// here for plumbing.  We would enable our claims transformation bits here.
			if (!incoming.Identity.IsAuthenticated) {
				return Task.FromResult<ClaimsPrincipal>(incoming);
			}

			incoming.Identities.First().AddClaim(new Claim("localclaim", "localvalue"));
			return Task.FromResult(incoming);
		}
	}
}