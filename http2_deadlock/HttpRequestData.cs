// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace http2_deadlock
{
    public class HttpRequestData
    {
        public byte[] Body;
        public string Method;
        public string Path;
        public Version Version;
        public List<HttpHeaderData> Headers { get; }
        public int RequestId;       // Generic request ID. Currently only used for HTTP/2 to hold StreamId.

        public HttpRequestData()
        {
            Headers = new List<HttpHeaderData>();
        }

        public static async Task<HttpRequestData> FromHttpRequestMessageAsync(System.Net.Http.HttpRequestMessage request)
        {
            var result = new HttpRequestData();
            result.Method = request.Method.ToString();
            result.Path = request.RequestUri?.AbsolutePath;
            result.Version = request.Version;

            foreach (var header in request.Headers)
            {
                foreach (var value in header.Value)
                {
                    result.Headers.Add(new HttpHeaderData(header.Key, value));
                }
            }

            if (request.Content != null)
            {
                result.Body = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                foreach (var header in request.Content.Headers)
                {
                    foreach (var value in header.Value)
                    {
                        result.Headers.Add(new HttpHeaderData(header.Key, value));
                    }
                }
            }

            return result;
        }

        public string[] GetHeaderValues(string headerName)
        {
            return Headers.Where(h => h.Name.Equals(headerName, StringComparison.OrdinalIgnoreCase))
                    .Select(h => h.Value)
                    .ToArray();
        }

        public string GetSingleHeaderValue(string headerName)
        {
            string[] values = GetHeaderValues(headerName);
            if (values.Length != 1)
            {
                throw new Exception(
                    $"Expected single value for {headerName} header, actual count: {values.Length}{Environment.NewLine}" +
                    $"{"\t"}{string.Join(Environment.NewLine + "\t", values)}");
            }

            return values[0];
        }

        public int GetHeaderValueCount(string headerName)
        {
            return Headers.Where(h => h.Name.Equals(headerName, StringComparison.OrdinalIgnoreCase)).Count();
        }
    }
}
