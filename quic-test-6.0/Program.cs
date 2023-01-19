using System;
using System.Net;
using System.Net.Quic;
using System.Net.Security;

// See https://aka.ms/new-console-template for more information
Console.WriteLine(QuicImplementationProviders.MsQuic.IsSupported);
Console.WriteLine(Environment.ProcessId);
Console.WriteLine("Hello, World!");