/*--------------------------------------------------------------------------
* DynamicJson
* ver 1.2.0.0 (May. 21th, 2010)
*
* created and maintained by neuecc <ils@neue.cc>
* licensed under Microsoft Public License(Ms-PL)
* http://neue.cc/
* http://dynamicjson.codeplex.com/
*--------------------------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;
// ReSharper disable All
#nullable disable
namespace Codeplex.Data; 
public class DynamicJson : DynamicObject {
	private enum JsonType {
		@string, number, boolean, @object, array, @null
	}

	// public static methods

	/// <summary>from JsonSring to DynamicJson</summary>
	public static dynamic Parse(string json) {
		return Parse(json, Encoding.Unicode);
	}

	/// <summary>from JsonSring to DynamicJson</summary>
	public static dynamic Parse(string json, Encoding encoding) {
		using var reader = JsonReaderWriterFactory.CreateJsonReader(encoding.GetBytes(json), XmlDictionaryReaderQuotas.Max);
		return ToValue(XElement.Load(reader));
	}

	/// <summary>from JsonSringStream to DynamicJson</summary>
	public static dynamic Parse(Stream stream) {
		using var reader = JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max);
		return ToValue(XElement.Load(reader));
	}

	/// <summary>from JsonSringStream to DynamicJson</summary>
	public static dynamic Parse(Stream stream, Encoding encoding) {
		using var reader = JsonReaderWriterFactory.CreateJsonReader(stream, encoding, XmlDictionaryReaderQuotas.Max, _ => { });
		return ToValue(XElement.Load(reader));
	}

	/// <summary>create JsonSring from primitive or IEnumerable or Object({public property name:property value})</summary>
	public static string Serialize(object obj) {
		return CreateJsonString(new XStreamingElement("root", CreateTypeAttr(GetJsonType(obj)), CreateJsonNode(obj)));
	}

	// private static methods

	private static dynamic ToValue(XElement element) {
		var type = (JsonType)Enum.Parse(typeof(JsonType), element.Attribute("type").Value);
		return type switch {
			JsonType.boolean => (bool)element,
			JsonType.number => (double)element,
			JsonType.@string => (string)element,
			JsonType.@object or JsonType.array => new DynamicJson(element, type),
			_ => null,
		};
	}

	private static JsonType GetJsonType(object obj) {
		if (obj == null) {
			return JsonType.@null;
		}

		return (Type.GetTypeCode(obj.GetType())) switch {
			TypeCode.Boolean => JsonType.boolean,
			TypeCode.String or TypeCode.Char or TypeCode.DateTime => JsonType.@string,
			TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal or TypeCode.SByte or TypeCode.Byte => JsonType.number,
			TypeCode.Object => (obj is IEnumerable) ? JsonType.array : JsonType.@object,
			_ => JsonType.@null,
		};
	}

	private static XAttribute CreateTypeAttr(JsonType type) {
		return new XAttribute("type", type.ToString());
	}

	private static object CreateJsonNode(object obj) {
		var type = GetJsonType(obj);
		return type switch {
			JsonType.@string or JsonType.number => obj,
			JsonType.boolean => obj.ToString().ToLower(),
			JsonType.@object => CreateXObject(obj),
			JsonType.array => CreateXArray(obj as IEnumerable),
			_ => null,
		};
	}

	private static IEnumerable<XStreamingElement> CreateXArray<T>(T obj) where T : IEnumerable {
		return obj.Cast<object>()
			.Select(o => new XStreamingElement("item", CreateTypeAttr(GetJsonType(o)), CreateJsonNode(o)));
	}

	private static IEnumerable<XStreamingElement> CreateXObject(object obj) {
		return obj.GetType()
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Select(pi => new { pi.Name, Value = pi.GetValue(obj, null) })
			.Select(a => new XStreamingElement(a.Name, CreateTypeAttr(GetJsonType(a.Value)), CreateJsonNode(a.Value)));
	}

	private static string CreateJsonString(XStreamingElement element) {
		using var ms = new MemoryStream();
		using var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.Unicode);
		element.WriteTo(writer);
		writer.Flush();
		return Encoding.Unicode.GetString(ms.ToArray());
	}

	// dynamic structure represents JavaScript Object/Array

	private readonly XElement xml;
	private readonly JsonType jsonType;

	/// <summary>create blank JSObject</summary>
	public DynamicJson() {
		this.xml = new XElement("root", CreateTypeAttr(JsonType.@object));
		this.jsonType = JsonType.@object;
	}

	private DynamicJson(XElement element, JsonType type) {
		Debug.Assert(type == JsonType.array || type == JsonType.@object);

		this.xml = element;
		this.jsonType = type;
	}

	public bool IsObject {
		get {
			return this.jsonType == JsonType.@object;
		}
	}

	public bool IsArray {
		get {
			return this.jsonType == JsonType.array;
		}
	}

	/// <summary>has property or not</summary>
	public bool IsDefined(string name) {
		return this.IsObject && (this.xml.Element(name) != null);
	}

	/// <summary>has property or not</summary>
	public bool IsDefined(int index) {
		return this.IsArray && (this.xml.Elements().ElementAtOrDefault(index) != null);
	}

	/// <summary>delete property</summary>
	public bool Delete(string name) {
		var elem = this.xml.Element(name);
		if (elem != null) {
			elem.Remove();
			return true;
		} else {
			return false;
		}
	}

	/// <summary>delete property</summary>
	public bool Delete(int index) {
		var elem = this.xml.Elements().ElementAtOrDefault(index);
		if (elem != null) {
			elem.Remove();
			return true;
		} else {
			return false;
		}
	}

	/// <summary>mapping to Array or Class by Public PropertyName</summary>
	public T Deserialize<T>() {
		return (T)this.Deserialize(typeof(T));
	}

	private object Deserialize(Type type) {
		return this.IsArray ? this.DeserializeArray(type) : this.DeserializeObject(type);
	}

	private static dynamic DeserializeValue(XElement element, Type elementType) {
		var value = ToValue(element);
		if (value is DynamicJson json) {
			value = json.Deserialize(elementType);
		}
		return Convert.ChangeType(value, elementType);
	}

	private object DeserializeObject(Type targetType) {
		var result = Activator.CreateInstance(targetType);
		var dict = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(p => p.CanWrite)
			.ToDictionary(pi => pi.Name, pi => pi);
		foreach (var item in this.xml.Elements()) {
			if (!dict.TryGetValue(item.Name.LocalName, out var propertyInfo)) {
				continue;
			}

			var value = DynamicJson.DeserializeValue(item, propertyInfo.PropertyType);
			propertyInfo.SetValue(result, value, null);
		}
		return result;
	}

	private object DeserializeArray(Type targetType) {
		if (targetType.IsArray) // Foo[]
		{
			var elemType = targetType.GetElementType();
			dynamic array = Array.CreateInstance(elemType, this.xml.Elements().Count());
			var index = 0;
			foreach (var item in this.xml.Elements()) {
				array[index++] = DynamicJson.DeserializeValue(item, elemType);
			}
			return array;
		} else // List<Foo>
		  {
			var elemType = targetType.GetGenericArguments()[0];
			dynamic list = Activator.CreateInstance(targetType);
			foreach (var item in this.xml.Elements()) {
				list.Add(DynamicJson.DeserializeValue(item, elemType));
			}
			return list;
		}
	}

	// Delete
	public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
		result = this.IsArray
			? this.Delete((int)args[0])
			: this.Delete((string)args[0]);
		return true;
	}

	// IsDefined, if has args then TryGetMember
	public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
		if (args.Length > 0) {
			result = null;
			return false;
		}

		result = this.IsDefined(binder.Name);
		return true;
	}

	// Deserialize or foreach(IEnumerable)
	public override bool TryConvert(ConvertBinder binder, out object result) {
		if (binder.Type == typeof(IEnumerable) || binder.Type == typeof(object[])) {
			var ie = this.IsArray
				? this.xml.Elements().Select(x => ToValue(x))
				: this.xml.Elements().Select(x => (dynamic)new KeyValuePair<string, object>(x.Name.LocalName, ToValue(x)));
			result = (binder.Type == typeof(object[])) ? ie.ToArray() : ie;
		} else {
			result = this.Deserialize(binder.Type);
		}
		return true;
	}

	private static bool TryGet(XElement element, out object result) {
		if (element == null) {
			result = null;
			return false;
		}

		result = ToValue(element);
		return true;
	}

	public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
		return this.IsArray
			? TryGet(this.xml.Elements().ElementAtOrDefault((int)indexes[0]), out result)
			: TryGet(this.xml.Element((string)indexes[0]), out result);
	}

	public override bool TryGetMember(GetMemberBinder binder, out object result) {
		return this.IsArray
			? TryGet(this.xml.Elements().ElementAtOrDefault(int.Parse(binder.Name)), out result)
			: TryGet(this.xml.Element(binder.Name), out result);
	}

	private bool TrySet(string name, object value) {
		var type = GetJsonType(value);
		var element = this.xml.Element(name);
		if (element == null) {
			this.xml.Add(new XElement(name, CreateTypeAttr(type), CreateJsonNode(value)));
		} else {
			element.Attribute("type").Value = type.ToString();
			element.ReplaceNodes(CreateJsonNode(value));
		}

		return true;
	}

	private bool TrySet(int index, object value) {
		var type = GetJsonType(value);
		var e = this.xml.Elements().ElementAtOrDefault(index);
		if (e == null) {
			this.xml.Add(new XElement("item", CreateTypeAttr(type), CreateJsonNode(value)));
		} else {
			e.Attribute("type").Value = type.ToString();
			e.ReplaceNodes(CreateJsonNode(value));
		}

		return true;
	}

	public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
		return this.IsArray
			? this.TrySet((int)indexes[0], value)
			: this.TrySet((string)indexes[0], value);
	}

	public override bool TrySetMember(SetMemberBinder binder, object value) {
		return this.IsArray
			? this.TrySet(int.Parse(binder.Name), value)
			: this.TrySet(binder.Name, value);
	}

	public override IEnumerable<string> GetDynamicMemberNames() {
		return this.IsArray
			? this.xml.Elements().Select((x, i) => i.ToString())
			: this.xml.Elements().Select(x => x.Name.LocalName);
	}

	/// <summary>Serialize to JsonString</summary>
	public override string ToString() {
		// <foo type="null"></foo> is can't serialize. replace to <foo type="null" />
		foreach (var elem in this.xml.Descendants().Where(x => x.Attribute("type").Value == "null")) {
			elem.RemoveNodes();
		}
		return CreateJsonString(new XStreamingElement("root", CreateTypeAttr(this.jsonType), this.xml.Elements()));
	}
}