// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System.Net.Quic.Implementations;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Quic.Public
{
    public sealed class QuicListener : IDisposable
    {
        private readonly QuicListenerProvider _provider;

        /// <summary>
        /// Create a QUIC listener on the specified local endpoint and start listening.
        /// </summary>
        /// <param name="listenEndPoint">The local endpoint to listen on.</param>
        /// <param name="sslServerAuthenticationOptions">TLS options for the listener.</param>
        public QuicListener(IPEndPoint listenEndPoint, SslServerAuthenticationOptions sslServerAuthenticationOptions)
            : this(QuicImplementationProviders.Managed, listenEndPoint, sslServerAuthenticationOptions)
        {
        }

        public QuicListener(QuicListenerOptions options)
            : this(QuicImplementationProviders.Managed, options)
        {
        }

        private QuicListener(QuicImplementationProvider implementationProvider, IPEndPoint listenEndPoint, SslServerAuthenticationOptions sslServerAuthenticationOptions)
            : this(implementationProvider,  new QuicListenerOptions() { ListenEndPoint = listenEndPoint, ServerAuthenticationOptions = sslServerAuthenticationOptions })
        {
        }

        private QuicListener(QuicImplementationProvider implementationProvider, QuicListenerOptions options)
        {
            _provider = implementationProvider.CreateListener(new Quic.QuicListenerOptions()
            {
                IdleTimeout = options.IdleTimeout,
                ListenBacklog = options.ListenBacklog,
                CertificateFilePath = options.CertificateFilePath,
                ListenEndPoint = options.ListenEndPoint,
                MaxBidirectionalStreams = options.MaxBidirectionalStreams,
                MaxUnidirectionalStreams = options.MaxUnidirectionalStreams,
                ServerAuthenticationOptions = options.ServerAuthenticationOptions,
                PrivateKeyFilePath = options.PrivateKeyFilePath
            });
        }

        public IPEndPoint ListenEndPoint => _provider.ListenEndPoint;

        /// <summary>
        /// Accept a connection.
        /// </summary>
        /// <returns></returns>
        public async ValueTask<QuicConnection> AcceptConnectionAsync(CancellationToken cancellationToken = default) =>
            new QuicConnection(await _provider.AcceptConnectionAsync(cancellationToken).ConfigureAwait(false));

        public void Start() => _provider.Start();

        /// <summary>
        /// Stop listening and close the listener.
        /// </summary>
        public void Close() => _provider.Close();

        public void Dispose() => _provider.Dispose();
    }
}
