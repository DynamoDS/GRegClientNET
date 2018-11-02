﻿using System.Net;
using Greg.Requests;
using Greg.Responses;
using RestSharp;

namespace Greg
{
    public class GregClient : IGregClient
    {
        private readonly RestClient _client;

        public string BaseUrl { get { return _client.BaseUrl.ToString(); } }

        public readonly IAuthProvider _authProvider;
        public IAuthProvider AuthProvider
        {
            get { return _authProvider; }
        }

        public GregClient(IAuthProvider provider, string packageManagerUrl)
        {
            // added to enable TLS1.2 compatability - this is required as we target .net 4.5.
            // TODO can be removed when we upgrade to .net4.7
            // documented here: https://medium.com/@kyle.gagnet/your-net-code-could-stop-working-in-june-afb35fbf29ca

            //https://stackoverflow.com/questions/2819934/detect-windows-version-in-net
            // if the current OS is windows 7 or lower
            // set TLS to 1.2.
            // else do nothing and let the OS decide the version of TLS to support. (.net 4.7 required)
            if (System.Environment.OSVersion.Version.Major <= 6 && System.Environment.OSVersion.Version.Minor <= 1)
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            _authProvider = provider;
            _client = new RestClient(packageManagerUrl);
        }

        private IRestResponse ExecuteInternal(Request m)
        {
            var req = new RestRequest(m.Path, m.HttpMethod);
            m.Build(ref req);

            if (m.RequiresAuthorization)
            {
                AuthProvider.SignRequest(ref req, _client);
            }
            return _client.Execute(req);
        }

        public Response Execute(Request m)
        {
            return new Response(ExecuteInternal(m));
        }

        public ResponseBody ExecuteAndDeserialize(Request m)
        {
            return Execute(m).Deserialize();
        }

        /// <summary>
        /// Execute the request and deserialize the content.
        /// </summary>
        /// <typeparam name="T">The Type of content</typeparam>
        /// <param name="m">The request.</param>
        /// <returns>A <see cref="ResponseWithContent{T}"/> or null if there was an error
        /// in executing the message.</returns>
        public ResponseWithContentBody<T> ExecuteAndDeserializeWithContent<T>(Request m)
        {
            var response = this.ExecuteInternal(m);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return new ResponseWithContent<T>(response).DeserializeWithContent();
        }
    }
}


