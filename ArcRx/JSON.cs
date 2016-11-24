using System;
using System.IO;
using System.Web;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ArcRx
{
    [RecognizedMimeType("application/json")]
    public sealed class JSON : MediaType<JSON>
    {
        public new abstract class Representation : MediaType<JSON>.Representation
        {
            protected override string ContentType => "application/json";

            readonly List<JSONProperty> properties = new List<JSONProperty>();

            struct JSONProperty
            {
                readonly JSONIdentifier identifier;
                readonly JSONValue value;

                public JSONProperty(JSONIdentifier name, JSONValue value)
                {
                    identifier = name;
                    this.value = value;
                }

                public void Write(int n, TextWriter tw)
                {
                    tw.Write(new string('\t', n));
                    tw.Write(identifier);
                    tw.Write(" : ");

                    value.Write(tw, n);
                }

                public override string ToString() => $"{{{identifier} : {value}}}";
            }

            protected void Add<T>(string name, T value) where T : struct => properties.Add(new JSONProperty(name, new SimpleJSONValue<T>(value)));
            protected void Add(string name, string value) => properties.Add(new JSONProperty(name, value));
            protected void Add(string name, IReadOnlyList<JSONValue> xs) => properties.Add(new JSONProperty(name, new JSONArrayObject(xs)));
            protected void Add(string name, JSON.Representation obj) => properties.Add(new JSONProperty(name, new JSONObjectValue(obj)));

            struct JSONIdentifier
            {
                static readonly Regex JSKeyWords = new Regex("^(do|if|in|for|let|new|try|var|case|else|enum|eval|false|null|this|true|void|with|break|catch|class|const|super|throw|while|yield|delete|export|import|public|return|static|switch|typeof|default|extends|finally|package|private|continue|debugger|function|arguments|interface|protected|implements|instanceof)$", RegexOptions.Compiled);

                readonly string identifier;

                public JSONIdentifier(string identifier)
                {
                    if (string.IsNullOrWhiteSpace(identifier))
                        throw new Exception("JSON Identifier cannot  be null");

                    if(IsValidIdentifier(identifier))
                        this.identifier = identifier;
                    else
                        throw new Exception($"{identifier} is not a valid JSON identifier.");

                    if (JSKeyWords.IsMatch(identifier))
                        this.identifier = "$" + identifier;
                }

                public static implicit operator JSONIdentifier (string name) => new JSONIdentifier(name);

                static bool IsValidIdentifier(string identifier) => IsvalidStart(identifier[0]) && HasVAllalidNonStartChars(identifier);

                static bool IsvalidStart(char c)
                {
                    var result = (c == '$' || c == '_' || char.IsLetter(c));

                    var category = char.GetUnicodeCategory(c);

                    return result || category == UnicodeCategory.TitlecaseLetter || category == UnicodeCategory.ModifierLetter || category == UnicodeCategory.OtherLetter || category == UnicodeCategory.LetterNumber;                    
                }

                static bool HasVAllalidNonStartChars(string identifier)
                {
                    for (var i = 1; i < identifier.Length; i++)
                        if (IsvalidStart(identifier[i]))
                            return true;
                        else
                        {
                            var c = identifier[i];
                            var category = char.GetUnicodeCategory(c);

                            if (!(category == UnicodeCategory.SpacingCombiningMark || category == UnicodeCategory.NonSpacingMark || category == UnicodeCategory.DecimalDigitNumber || category == UnicodeCategory.ConnectorPunctuation))
                                return false;
                        }

                    return true;
                }

                public override string ToString() => $"\"{identifier}\"";
            }

            public abstract class JSONValue
            {
                public abstract void Write(TextWriter tw, int n);

                public static implicit operator JSONValue(int value) => new SimpleJSONValue<int>(value);
                public static implicit operator JSONValue(byte value) => new SimpleJSONValue<byte>(value);
                public static implicit operator JSONValue(bool value) => new SimpleJSONValue<bool>(value);
                public static implicit operator JSONValue(long value) => new SimpleJSONValue<long>(value);
                public static implicit operator JSONValue(Guid value) => new SimpleJSONValue<Guid>(value);
                public static implicit operator JSONValue(short value) => new SimpleJSONValue<short>(value);
                public static implicit operator JSONValue(ulong value) => new SimpleJSONValue<ulong>(value);
                public static implicit operator JSONValue(float value) => new SimpleJSONValue<float>(value);
                public static implicit operator JSONValue(ushort value) => new SimpleJSONValue<ushort>(value);
                public static implicit operator JSONValue(decimal value) => new SimpleJSONValue<decimal>(value);
                public static implicit operator JSONValue(DateTime value) => new SimpleJSONValue<DateTime>(value);

                public static implicit operator JSONValue(string s) => new StringJSONValue(s);
            }

            abstract class JSONValue<T> : JSONValue
            {
                protected T value;

                public JSONValue(T value)
                {
                    this.value = value;
                }

                public abstract override string ToString();
            }

            sealed class SimpleJSONValue<T> : JSONValue<T>
                where T : struct
            {
                public SimpleJSONValue(T value) : base(value) { }

                public override string ToString() => $"\"{value.ToString().ToLower()}\"";

                public override void Write(TextWriter tw, int n)
                {
                    tw.Write(ToString());
                }
            }

            sealed class StringJSONValue :  JSONValue<string>
            {
                static readonly Regex escape_chars = new Regex(@"",  RegexOptions.Compiled);

                static string MatchEval(Match m)
                {
                    throw new NotImplementedException();
                }

                public StringJSONValue(string s)  : base(s) { }

                public override string ToString() => value != null ? $"\"{value}\"" : "null";

                public override void Write(TextWriter tw, int n)
                {
                    tw.Write(ToString());
                }
            }

            sealed class JSONArrayObject : JSONValue<IReadOnlyList<JSONValue>>
            {
                public JSONArrayObject(IReadOnlyList<JSONValue> values) : base(values) { }

                public override void Write(TextWriter tw, int n)
                {
                    tw.Write("[");

                    var i = 0;

                    foreach (var value in this.value)
                    {
                        value.Write(tw, n);

                        if (++i < this.value.Count)
                        {
                            tw.Write(",");
                            tw.WriteLine();
                            tw.Write(new string('\t', n));
                        }
                    }

                    tw.WriteLine("]");
                }

                public override string ToString()
                {
                    var ms = new MemoryStream();

                    using (var sw = new StreamWriter(ms))
                        Write(sw, 0);

                    using (var sr = new StreamReader(ms))
                        return sr.ReadToEnd();
                }
            }

            sealed class JSONObjectValue : JSONValue<JSON.Representation>
            {
                public JSONObjectValue(JSON.Representation json) : base(json) { }

                public override void Write(TextWriter tw, int n)
                {
                    value.Write(tw, n);
                }

                public override string ToString()
                {
                    var ms = new MemoryStream();

                    using (var sw = new StreamWriter(ms))
                        Write(sw, 0);

                    using (var sr = new StreamReader(ms))
                        return sr.ReadToEnd();
                }
            }

            public void Write(TextWriter tw, int n)
            {
                tw.Write("{");
                tw.WriteLine();

                var i = 0;

                foreach (var property in properties)
                {
                    property.Write(n +  1, tw);

                    if (++i < properties.Count)
                    {
                        tw.Write(",");

                        tw.Write("\r\n");
                    }
                }

                tw.Write(" }");
            }

            protected override void _ProcessRequest(HttpContext context)
            {
                Write(context.Response.Output, 0);
            }
        }

        protected override MediaType<JSON>.Representation _Convert(ArcRx.Representation rep)
        {
            return (rep as JSON.Compatible)?.Convert(this) as JSON.Representation;
        }
    }
}