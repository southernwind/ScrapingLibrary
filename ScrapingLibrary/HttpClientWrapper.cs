using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapingLibrary
{
    public class HttpClientWrapper
    {
        /// <summary>
        /// HttpClient
        /// </summary>
        private readonly HttpClient _hc;

        /// <summary>
        /// ヘッダ
        /// </summary>
        private readonly Dictionary<string, string> DefaultHeaders = new();

        /// <summary>
        /// Cookie
        /// </summary>
        public CookieContainer CookieContainer
        {
            get;
        }

        public HttpClientWrapper() : this(new HttpClientHandler() { UseCookies = true })
        {
        }

        public HttpClientWrapper(HttpClientHandler handler)
        {
            this.CookieContainer = new CookieContainer();
            handler.CookieContainer = this.CookieContainer;
            this._hc = new HttpClient(handler);
            this.DefaultHeaders.Add("Accept-Encoding", "gzip");
            this.DefaultHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.18363");
            this.DefaultHeaders.Add("Accept-Language", "ja");
            this.DefaultHeaders.Add("Connection", "Keep-Alive");
            this.DefaultHeaders.Add("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, */*; q=0.8");
        }

        /// <summary>
        /// 引数で渡されたURLのHTTPレスポンスを取得する(GET)
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>取得したHTMLDocument</returns>
        public async Task<HttpResponseMessage> GetAsync(string url, Dictionary<string, string>? headers = null)
        {
            return await this.GetAsync(new Uri(url), headers);
        }

        /// <summary>
        /// 引数で渡されたURLのHTTPレスポンスを取得する(GET)
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>取得したHTMLDocument</returns>
        public async Task<HttpResponseMessage> GetAsync(Uri uri, Dictionary<string, string>? headers = null)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri
            };
            this.SetHeaders(request, headers);

            return await this._hc.SendAsync(request);
        }

        /// <summary>
        /// リクエストを行う。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>取得したHTMLDocument</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await this._hc.SendAsync(request);
        }

        /// <summary>
        /// リクエストを行う。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>取得したHTMLDocument</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption)
        {
            return await this._hc.SendAsync(request, httpCompletionOption);
        }

        /// <summary>
        /// リクエストを行う。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>取得したHTMLDocument</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await this._hc.SendAsync(request, cancellationToken);
        }


        /// <summary>
        /// リクエストを行う。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <returns>取得したHTMLDocument</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption httpCompletionOption, CancellationToken cancellationToken)
        {
            return await this._hc.SendAsync(request, httpCompletionOption, cancellationToken);
        }

        /// <summary>
        /// Postする。
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="content">要求本文</param>
        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content, Dictionary<string, string>? headers = null)
        {
            var uri = new Uri(url);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = content
            };
            this.SetHeaders(request, headers);

            return await this._hc.SendAsync(request);
        }

        public void SetHeaders(HttpRequestMessage request, Dictionary<string, string>? headers = null)
        {
            foreach (var header in headers ?? this.DefaultHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var cookieString = this.CookieContainer.GetCookieHeader(request.RequestUri!);
            if (!string.IsNullOrEmpty(cookieString)) {
                request.Headers.Add("Cookie", cookieString);
            }
        }
    }
}
