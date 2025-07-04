﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NETCOREAPP
using System.Buffers;
#else
using Microsoft.Testing.Platform.Helpers;
#endif

namespace Microsoft.Testing.Platform.ServerMode;

internal class StreamMessageHandler : IMessageHandler, IDisposable
{
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly IMessageFormatter _formatter;
    private bool _isDisposed;

    public StreamMessageHandler(
        Stream clientToServerStream,
        Stream serverToClientStream,
        IMessageFormatter formatter)
    {
        _reader = new StreamReader(clientToServerStream);
        _writer = new StreamWriter(serverToClientStream)
        {
            // We need to force the NewLine because in Windows and nix different char sequence are used
            // https://learn.microsoft.com/dotnet/api/system.io.textwriter.newline?view=net-7.0
            NewLine = "\r\n",
        };
        _formatter = formatter;
    }

    public virtual async Task<RpcMessage?> ReadAsync(CancellationToken cancellationToken)
    {
        // Reads an RPC message.
        // The message is currently encoded by writing a list of headers
        // and then passing a byte stream with the message.
        // The headers include the size of the byte stream.
        // Content-Length: [content-length]\r\n
        // Content-Type: [mime-type]\r\n
        // \r\n
        // [content]\r\n
        while (true)
        {
            int commandSize = await ReadHeadersAsync(cancellationToken).ConfigureAwait(false);

            // Most probably connection lost
            if (commandSize is -1)
            {
                return null;
            }

#if NETCOREAPP
            char[] commandCharsBuffer = ArrayPool<char>.Shared.Rent(commandSize);
            try
            {
                Memory<char> memoryBuffer = new(commandCharsBuffer, 0, commandSize);
                await _reader.ReadBlockAsync(memoryBuffer, cancellationToken).ConfigureAwait(false);
                return _formatter.Deserialize<RpcMessage>(memoryBuffer);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(commandCharsBuffer);
            }
#else
            char[] commandChars = new char[commandSize];
            await _reader.ReadBlockAsync(commandChars, 0, commandSize).WithCancellationAsync(cancellationToken).ConfigureAwait(false);
            return _formatter.Deserialize<RpcMessage>(new string(commandChars, 0, commandSize));
#endif
        }
    }

    private async Task<int> ReadHeadersAsync(CancellationToken cancellationToken)
    {
        int contentSize = -1;

        while (true)
        {
#if NET7_0_OR_GREATER
            string? line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
#elif NET6_0_OR_GREATER
            string? line = await _reader.ReadLineAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
#else
            string? line = await _reader.ReadLineAsync().WithCancellationAsync(cancellationToken).ConfigureAwait(false);
#endif
            if (line is null || (line.Length == 0 && contentSize != -1))
            {
                break;
            }

            const string ContentLengthHeaderName = "Content-Length:";
            // Content type is not mandatory, and we don't use it.
            if (line.StartsWith(ContentLengthHeaderName, StringComparison.OrdinalIgnoreCase))
            {
#if NETCOREAPP
                _ = int.TryParse(line.AsSpan()[ContentLengthHeaderName.Length..].Trim(), out contentSize);
#else
                _ = int.TryParse(line[ContentLengthHeaderName.Length..].Trim(), out contentSize);
#endif
            }
        }

        return contentSize;
    }

    public async Task WriteRequestAsync(RpcMessage message, CancellationToken cancellationToken)
    {
        string messageStr = await _formatter.SerializeAsync(message).ConfigureAwait(false);
        await _writer.WriteLineAsync($"Content-Length: {Encoding.UTF8.GetByteCount(messageStr)}").ConfigureAwait(false);
        await _writer.WriteLineAsync("Content-Type: application/testingplatform").ConfigureAwait(false);
        await _writer.WriteLineAsync().ConfigureAwait(false);
        await _writer.WriteAsync(messageStr).ConfigureAwait(false);
        await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _reader.Dispose();
                try
                {
                    _writer.Dispose();
                }
                catch (InvalidOperationException)
                {
                    // We can exit the server without wait that the streaming activity is completed.
                    // In that case we can get an InvalidOperationException
                    // (https://learn.microsoft.com/dotnet/api/system.io.streamwriter.writelineasync?view=net-7.0#system-io-streamwriter-writelineasync(system-string)):
                    // The stream writer is currently in use by a previous write operation.
                }
            }

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
