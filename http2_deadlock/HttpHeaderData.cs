// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace http2_deadlock
{
    public struct HttpHeaderData
    {
        // http://httpwg.org/specs/rfc7541.html#rfc.section.4.1
        public const int RfcOverhead = 32;

        public string Name { get; }
        public string Value { get; }
        public bool HuffmanEncoded { get; }
        public byte[] Raw { get; }
        public Encoding ValueEncoding { get; }

        public HttpHeaderData(string name, string value, bool huffmanEncoded = false, byte[] raw = null, Encoding valueEncoding = null)
        {
            Name = name;
            Value = value;
            HuffmanEncoded = huffmanEncoded;
            Raw = raw;
            ValueEncoding = valueEncoding;
        }

        public override string ToString() => Name == null ? "<empty>" : (Name + ": " + (Value ?? string.Empty));
    }
}
