using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Codeplex.Data;

using CsvHelper;
using CsvHelper.Configuration;

using HtmlAgilityPack;

namespace ScrapingLibrary;
public static class HttpResponseMessageExtension {

	static HttpResponseMessageExtension() {
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
	}

	/// <summary>
	/// HttpResponseMessageをバイナリ形式に変換
	/// </summary>
	/// <param name="hrm">HttpResponseMessage</param>
	/// <returns>結果</returns>
	public static async Task<byte[]> ToBinaryAsync(this Task<HttpResponseMessage> hrm) {
		return await (await hrm).ToBinaryAsync();
	}

	/// <summary>
	/// HttpResponseMessageをテキスト形式に変換
	/// </summary>
	/// <param name="hrm">HttpResponseMessage</param>
	/// <returns>結果</returns>
	public static async Task<string> ToTextAsync(this Task<HttpResponseMessage> hrm) {
		return await (await hrm).ToTextAsync();
	}

	/// <summary>
	/// HttpResponseMessageをHtmlDocument形式に変換
	/// </summary>
	/// <param name="hrm">HttpResponseMessage</param>
	/// <returns>結果</returns>
	public static async Task<HtmlDocument> ToHtmlDocumentAsync(this Task<HttpResponseMessage> hrm) {
		return await (await hrm).ToHtmlDocumentAsync();
	}

	/// <summary>
	/// 結果をJson形式で取得
	/// </summary>
	/// <param name="hrm">HttpResponseMessage</param>
	/// <returns>結果</returns>
	public static async Task<dynamic> ToJsonAsync(this Task<HttpResponseMessage> hrm) {
		return await (await hrm).ToJsonAsync();
	}

	/// <summary>
	/// 結果をCSVレコード形式で取得
	/// </summary>
	/// <param name="hrm">HttpResponseMessage</param>
	/// <param name="csvConfiguration">CsvConfiguration</param>
	/// <returns>結果</returns>
	public static async Task<List<T>> ToCsvRecordAsync<T>(this Task<HttpResponseMessage> hrm, CsvConfiguration? csvConfiguration = null) {
		return await (await hrm).ToCsvRecordAsync<T>(csvConfiguration);
	}

        /// <summary>
        /// HttpResponseMessageをStream形式に変換
        /// </summary>
        /// <param name="hrm">HttpResponseMessage</param>
        /// <returns>結果</returns>
        public static async Task<Stream> ToStreamAsync(this HttpResponseMessage hrm)
        {
            var st = await hrm.Content.ReadAsStreamAsync();
            if (hrm.Content.Headers.ContentEncoding.ToString() != "gzip") {
				return st;
            }

            return new GZipStream(st, CompressionMode.Decompress);
        }

        /// <summary>
        /// HttpResponseMessageをバイナリ形式に変換
        /// </summary>
        /// <param name="hrm">HttpResponseMessage</param>
        /// <returns>結果</returns>
        public static async Task<byte[]> ToBinaryAsync(this HttpResponseMessage hrm) {
			using var stream = await hrm.ToStreamAsync();
			using var memory = new MemoryStream();
            
            var count = 0;
            const int size = 4096;
            var buffer = new byte[size];
            do {
				count = stream.Read(buffer, 0, size);
                if (count > 0)
                {
				memory.Write(buffer, 0, count);
                }
            }
            while (count > 0);
            return memory.ToArray();
        }

	/// <summary>
	/// HttpResponseMessageをテキスト形式に変換
	/// </summary>
	/// <param name="hrm">HttpResponseMessage</param>
	/// <returns>結果</returns>
	public static async Task<string> ToTextAsync(this HttpResponseMessage hrm) {
		var charset = hrm.Content.Headers.ContentType?.CharSet;
		if (charset?.ToLower() == "windows-31j") {
			charset = "shift_jis";
		}

		if (hrm.Content.Headers.ContentEncoding.ToString() != "gzip") {
			if (hrm.Content.Headers.ContentType != null) {
				hrm.Content.Headers.ContentType.CharSet = charset;
			}
			return await hrm.Content.ReadAsStringAsync();
		}

		var encoding = charset == null || charset.ToLower() == "utf8" ? Encoding.UTF8 : Encoding.GetEncoding(charset);
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
	/// 結果をJson形式で取得
	/// </summary>
	/// <param name="hrm">HttpResponseMessage</param>
	/// <returns>結果</returns>
	public static async Task<dynamic> ToJsonAsync(this HttpResponseMessage hrm) {
		var text = await hrm.ToTextAsync();
		return DynamicJson.Parse(text);
	}

	/// <summary>
	/// 結果をCSVレコード形式で取得
	/// </summary>
	/// <param name="hrm">HttpResponseMessage</param>
	/// <param name="csvConfiguration">csvConfiguration</param>
	/// <returns>結果</returns>
	public static async Task<List<T>> ToCsvRecordAsync<T>(this HttpResponseMessage hrm, CsvConfiguration? csvConfiguration = null) {
		csvConfiguration ??= new(CultureInfo.CurrentCulture);
		var text = await hrm.ToTextAsync();
		while (true) {
			var before = text;
			text = Regex.Replace(text, "^null,|,null$", ",,", RegexOptions.Multiline).Replace(",null,", ",,");
			if (before == text) {
				break;
			}
		}

		var encoding = Encoding.UTF8;
		await using var memoryStream = new MemoryStream(encoding.GetBytes(text));
		using var streamReader = new StreamReader(memoryStream, encoding);
		using var csvReader = new CsvReader(streamReader, csvConfiguration);

		return csvReader.GetRecords<T>().ToList();

	}
}
