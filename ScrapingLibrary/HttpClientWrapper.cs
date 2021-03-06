using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ScrapingLibrary {
	public class HttpClientWrapper {
		/// <summary>
		/// HttpClient
		/// </summary>
		private readonly HttpClient _hc;

		/// <summary>
		/// ヘッダ
		/// </summary>
		public readonly Dictionary<string, string> Headers = new();

		/// <summary>
		/// Cookie
		/// </summary>
		public CookieContainer CookieContainer {
			get;
		}

		public HttpClientWrapper() {
			this.CookieContainer = new CookieContainer();
			this._hc = new HttpClient(new HttpClientHandler {
				CookieContainer = this.CookieContainer
			});
			this.Headers.Add("Accept-Encoding", "gzip, deflate, br");
			this.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.18363");
			this.Headers.Add("Accept-Language", "ja");
			this.Headers.Add("Connection", "Keep-Alive");
			this.Headers.Add("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, */*; q=0.8");
		}

		public HttpClientWrapper(HttpClientHandler handler) {
			this.CookieContainer = new CookieContainer();
			handler.CookieContainer = this.CookieContainer;
			this._hc = new HttpClient(handler);
			this.Headers.Add("Accept-Encoding", "gzip, deflate, br");
			this.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.18363");
			this.Headers.Add("Accept-Language", "ja");
			this.Headers.Add("Connection", "Keep-Alive");
			this.Headers.Add("Accept", "text/html, application/xhtml+xml, application/xml; q=0.9, */*; q=0.8");
		}

		/// <summary>
		/// 引数で渡されたURLのHTTPレスポンスを取得する(GET)
		/// </summary>
		/// <param name="url">URL</param>
		/// <returns>取得したHTMLDocument</returns>
		public async Task<HttpResponseMessage> GetAsync(string url) {
			return await this.GetAsync(new Uri(url));
		}

		/// <summary>
		/// 引数で渡されたURLのHTTPレスポンスを取得する(GET)
		/// </summary>
		/// <param name="uri">URI</param>
		/// <returns>取得したHTMLDocument</returns>
		public async Task<HttpResponseMessage> GetAsync(Uri uri) {
			var request = new HttpRequestMessage {
				Method = HttpMethod.Get,
				RequestUri = uri
			};
			this.SetHeaders(request);

			return await this._hc.SendAsync(request);
		}

		/// <summary>
		/// Postする。
		/// </summary>
		/// <param name="url">URL</param>
		/// <param name="content">要求本文</param>
		public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content) {
			var uri = new Uri(url);
			var request = new HttpRequestMessage {
				Method = HttpMethod.Post,
				RequestUri = uri,
				Content = content
			};
			this.SetHeaders(request);

			return await this._hc.SendAsync(request);

		}

		private void SetHeaders(HttpRequestMessage request) {
			foreach (var header in this.Headers) {
				request.Headers.Add(header.Key, header.Value);
			}
		}
	}
}
