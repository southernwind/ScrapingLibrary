using System.Net;

namespace ScrapingLibrary {
	public class ResponseObject<T> : ResponseObject {

		public T Result {
			get;
			init;
		}

		internal ResponseObject(T result, HttpStatusCode statusCode) {
			this.Result = result;
			this.StatusCode = statusCode;
		}

		internal ResponseObject(T result, ResponseObject responseObject) : base(responseObject) {
			this.Result = result;
		}
	}

	public abstract class ResponseObject {
		public HttpStatusCode StatusCode {
			get;
			init;
		}

		protected ResponseObject() {

		}

		protected ResponseObject(ResponseObject responseObject) {
			this.StatusCode = responseObject.StatusCode;
		}
	}
}
