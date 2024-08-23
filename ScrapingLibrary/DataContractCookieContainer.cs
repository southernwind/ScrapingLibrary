using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace ScrapingLibrary;

[DataContract]
public class DataContractCookieContainer {
	private DataContractCookieContainer() {
		this.CookieContainer = new CookieContainer();
	}

	public DataContractCookieContainer(CookieContainer cookieContainer) {
		this.CookieContainer = cookieContainer;
	}

	[IgnoreDataMember]
	public CookieContainer CookieContainer {
		get; set;
	}
	[DataMember(Order = 1)]
	private List<DataContractCookie> Cookies {
		get {
			var result = new List<DataContractCookie>();
			var collection = this.CookieContainer.GetAllCookies();
			foreach (Cookie c in collection) {
				result.Add(new DataContractCookie(c));
			}
			return result;
		}
		set {
			this.CookieContainer = new CookieContainer();
			var collection = new CookieCollection();
			foreach (var cookie in value) {
				collection.Add(cookie.ToCookie());
			}
			this.CookieContainer.Add(collection);
		}
	}

	[DataContract]
	private sealed class DataContractCookie {
		[Obsolete("for serialization only", true)]
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。
		private DataContractCookie() {
		}
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。

		public DataContractCookie(Cookie cookie) {
			this.Comment = cookie.Comment;
			this.CommentUri = cookie.CommentUri;
			this.Discard = cookie.Discard;
			this.Domain = cookie.Domain;
			this.Expired = cookie.Expired;
			this.Expires = cookie.Expires;
			this.HttpOnly = cookie.HttpOnly;
			this.Name = cookie.Name;
			this.Path = cookie.Path;
			this.Port = cookie.Port;
			this.Secure = cookie.Secure;
			this.Value = cookie.Value;
			this.Version = cookie.Version;
		}

		public Cookie ToCookie() {
			var cookie = new Cookie() {
				Name = this.Name,
				Value = this.Value,
				Path = this.Path,
				Domain = this.Domain,
				Comment = this.Comment,
				CommentUri = this.CommentUri,
				Discard = this.Discard,
				Expired = this.Expired,
				Expires = this.Expires,
				HttpOnly = this.HttpOnly,
				Port = this.Port,
				Secure = this.Secure,
				Version = this.Version,
			};
			return cookie;
		}

		[DataMember]
		public string Comment {
			get;
			set;
		}
		[DataMember]
		public string? CommentUrl {
			get;
			set;
		}

		[IgnoreDataMember]
		public Uri? CommentUri {
			get {
				return string.IsNullOrWhiteSpace(this.CommentUrl) ? null : new Uri(this.CommentUrl);
			}
			set {
				this.CommentUrl = (value == null) ? "" : value.ToString();
			}
		}
		[DataMember]
		public bool Discard {
			get;
			set;
		}
		[DataMember]
		public string Domain {
			get;
			set;
		}
		[DataMember]
		public bool Expired {
			get;
			set;
		}
		[DataMember]
		public DateTime Expires {
			get;
			set;
		}
		[DataMember]
		public bool HttpOnly {
			get;
			set;
		}
		[DataMember]
		public string Name {
			get;
			set;
		}
		[DataMember]
		public string Path {
			get;
			set;
		}
		[DataMember]
		public string Port {
			get;
			set;
		}
		[DataMember]
		public bool Secure {
			get;
			set;
		}
		[DataMember]
		public string Value {
			get;
			set;
		}
		[DataMember]
		public int Version {
			get;
			set;
		}
	}
}