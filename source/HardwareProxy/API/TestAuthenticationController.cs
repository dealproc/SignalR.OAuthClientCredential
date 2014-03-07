using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;


namespace HardwareProxy.API {
	[Authorize]
	public class TestAuthenticationController : ApiController {
		public bool Get() {
			var principal = Request.GetRequestContext().Principal as ClaimsPrincipal;
			return principal.Identity.IsAuthenticated;
			//return User.Identity.IsAuthenticated;
		}
	}
}