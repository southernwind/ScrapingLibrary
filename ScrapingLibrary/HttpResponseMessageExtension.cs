using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Codeplex.Data;

using HtmlAgilityPack;

namespace ScrapingLibrary {
	public static class HttpResponseMessageExtension {

		static HttpResponseMessageExtension() {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

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
			var charset = hrm.Content.Headers.ContentType?.CharSet;
			if (charset == "windows-31j") {
				charset = "shift_jis";
			}

			if (hrm.Content.Headers.ContentEncoding.ToString() != "gzip") {
				if (hrm.Content.Headers.ContentType != null) {
					hrm.Content.Headers.ContentType.CharSet = charset;
				}
				return await hrm.Content.ReadAsStringAsync();
			}

			var encoding = charset == null ? Encoding.UTF8 : Encoding.GetEncoding(charset);
			var st = await hrm.Content.ReadAsStreamAsync();
			var gzip = new GZipStream(st, CompressionMode.Decompress);
			var sr = new StreamReader(gzip, encoding);
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
