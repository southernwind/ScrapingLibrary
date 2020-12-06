using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

using Codeplex.Data;

using HtmlAgilityPack;

namespace ScrapingLibrary {
	public static class HttpResponseMessageExtension {
		/// <summary>
		/// HttpResponseMessageをバイナリ形式に変換
		/// </summary>
		/// <param name="hrm">HttpResponseMessage</param>
		/// <returns>結果</returns>
		public static async Task<byte[]> ToBinaryAsync(this HttpResponseMessage hrm) {
			return await hrm.Content.ReadAsByteArrayAsync();
		}

		/// <summary>
		/// HttpResponseMessageをテキスト形式に変換
		/// </summary>
		/// <param name="hrm">HttpResponseMessage</param>
		/// <returns>結果</returns>
		public static async Task<string> ToTextAsync(this HttpResponseMessage hrm) {
			if (hrm.Content.Headers.ContentEncoding.ToString() != "gzip") {
				return await hrm.Content.ReadAsStringAsync();
			}

			var st = await hrm.Content.ReadAsStreamAsync();
			var gzip = new GZipStream(st, CompressionMode.Decompress);
			var sr = new StreamReader(gzip);
			return await sr.ReadToEndAsync();
		}

		/// <summary>
		/// HttpResponseMessageをHtmlDocument形式に変換
		/// </summary>
		/// <param name="hrm">HttpResponseMessage</param>
		/// <returns>結果</returns>
		public static async Task<HtmlDocument> ToHtmlDocumentAsync(this HttpResponseMessage hrm) {
			var html = await hrm.ToTextAsync();

			var hd = new HtmlDocument();
			hd.LoadHtml(html);
			return hd;
		}

		/// <summary>
		/// 結果をJson形式で取得(GET)
		/// </summary>
		/// <param name="hrm">HttpResponseMessage</param>
		/// <returns>結果</returns>
		public static async Task<dynamic> ToJsonAsync(this HttpResponseMessage hrm) {
			var text = await hrm.ToTextAsync();
			return DynamicJson.Parse(text);
		}
	}
}
