﻿using System;

namespace Core.ADAL
{
    using Nancy.Security;

    /// <summary>
    /// This static class is used to separate Active Directory Authentication Library (ADAL) operations against Azure Active Directory (AAD) required for user authentication.
    /// </summary>
    public static class ActiveDirectoryAuthenticationHelper
    {
        /// <summary>
        /// Return the URL to use when redirecting an incoming client to authenticate via Azure Active Directory
        /// </summary>
        /// <returns>a string URL representing an authentication endpoint for AAD</returns>
        public static string GetAuthorizationURL()
        {
            // compose the URL that will redirect the incoming client to be authenticated 
            // this url contains various arguments Azure Active Directory consumes via oauth2 in order to determine a client is a person who has access to the Azure Active Directory
            // (and therefore applications configured within it)
            string authorizationUrl = string.Format("https://login.windows.net/{0}/oauth2/authorize?api-version=1.0&response_type=code&client_id={1}&resource={2}&redirect_uri={3}",
                                             AAD.TENANT_ID,
                                             AAD.CLIENT_ID,
                                             AAD.APP_ID_URI,
                                             AAD.REPLY_URL);     
            return authorizationUrl;
        }

        /// <summary>
        /// Acquires an IUserIdentity from Azure Active Directory using the argument authorizationCode.
        /// </summary>
        /// <param name="authorizationCode">An authorization code provided by Azure Active Directory used to retrieve an IUserIdentity</param>
        /// <returns>Returns an IUserIdentity representing a successfully authenticated Azure Active Directory user who has privileges for this configured application</returns>
        public static IUserIdentity GetAuthenticatedUserIDentity(string authorizationCode)
        {
            var authenticationContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(string.Format("https://login.windows.net/{0}", AAD.TENANT_ID));
            var clientCredential      = new Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential(AAD.CLIENT_ID, AAD.CLIENT_KEY);
            var authenticationResult  = authenticationContext.AcquireTokenByAuthorizationCode(authorizationCode, new Uri(AAD.REPLY_URL), clientCredential);
            return new UserIdentity(authenticationResult.UserInfo);
        }

        /// <summary>
        /// Variables and configured settings within Azure Active Directory ('AAD') that are required by Azure Active Directory Authentication Library 
        /// ('ADAL'/ Microsoft.IdentityModel.Clients.ActiveDirectory http://goo.gl/EzRE6d) in order to consume authentication service from AAD from this 
        /// client application.
        /// </summary>
        /// <remarks>
        /// Grouped, documented here for simplicity
        /// </remarks>
        public struct AAD
        {
            /// <summary>
            /// an Azure Active Directory "tenant" (aka Domain) which is identified by us as a key (which can be found via) :
            ///
            /// Azure Portal -> 
            ///    Active Directory (pick the one containing the api/resource app and the client app) ->
            ///           Applications ->
            ///              View Endpoints (at the bottom, center of the screen)
            ///
            /// the TENANT_ID appears sandwiched within the various end point urls
            /// </summary>
            public static readonly string TENANT_ID = "f721817b-99eb-4505-b220-850208ab5dd7";
            /// <summary>
            ///  this is the 'clientID' associated with THIS application (which in our case is a web app client, not a native client), 
            ///  as configured within Azure Active Directory, and associated as a client for the resource (we want to access).
            ///  AAD uses this to identify the client application.
            /// </summary>
            public static readonly string CLIENT_ID = "17c9a991-4954-48e4-9cf9-39b126ba975c";
            /// <summary>
            /// 'APP ID URI' of resource we want to access, as configured within Azure Active Directory (and associated as a resource for THIS application (the client application))
            /// AAD uses this to identify the resource application.
            /// </summary>
            public static readonly string APP_ID_URI = "https://SalesApplication.onmicrosoft.com/WebAPIDemo";
            /// <summary>
            /// reply url to send the authorization code  :
            ///
            ///   (1) the URL needs to be : this app's host url. 
            ///   (2) the URL needs to be : configured within AAD to match our AAD client application's 'REPLY URL' 
            ///   (3) the URL *should be* : ssl
            ///   
            /// </summary>
            /// <remarks>
            /// notice how the callback URI (aka redirect -- the URI Azure Active Directory will return its authentication token/"code" to)
            /// is https, not http. This necessitates that our Nancy web client app (READ:this application) handle reception over https/SSL.
            /// Configuring Nancy for SSL is much easier to accomplish when hosted via IIS/SystemWeb. Self-hosted Nancy
            /// requires a lot of overhead to configure SSL that IIS handles implicitly.
            /// </remarks>
            /// <seealso cref="http://goo.gl/VXRR4c"/>
            /// <seealso cref="http://goo.gl/4zDuD"/>
            /// <seealso cref="http://goo.gl/Psep0"/>
            /// <seealso cref="http://goo.gl/5t6fg8"/>
            public static readonly string REPLY_URL  = "https://localhost:44308/";//"https://localhost:44308/Home/CatchCode";
            /// <summary>
            /// This is the 'secret' configured within AAD to associate the calling code with the configured application within AAD
            /// </summary>
            public static readonly string CLIENT_KEY = "wTB9UZQGx+1dVcKPlsdGbVcHNnaiH6PQgKDdSvf0bak=";
        }
    }
}
