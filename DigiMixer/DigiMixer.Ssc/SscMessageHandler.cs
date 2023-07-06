// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections;

namespace DigiMixer.Ssc;

/// <summary>
/// Helper class for SSC messages, allowing for responses to messages to be handled by
/// type of value, and for easy creation of request messages.
/// </summary>
public sealed class SscMessageHandler
{
    private readonly Dictionary<string, Func<object?, bool>> actions;
    private readonly Dictionary<string, Action<SscError>> errorActions;

    /// <summary>
    /// A request message which will fetch all the addresses handled by this message handler.
    /// </summary>
    public SscMessage RequestMessage { get; }

    /// <summary>
    /// Calls <see cref="HandleMessage(SscMessage)"/> if the request message has an ID,
    /// and the response message has an ID matching it.
    /// </summary>
    public void MaybeHandleMessage(SscMessage message)
    {
        if (message.Id is not null || message.Id == RequestMessage.Id)
        {
            HandleMessage(message);
        }
    }

    public void HandleMessage(SscMessage message)
    {
        foreach (var property in message.Properties)
        {
            if (actions.TryGetValue(property.Address, out var action))
            {
                if (!action(property.Value))
                {
                    // TODO: Log? Call a backstop?
                }
            }
        }
        // While we could add a handler for /osc/error in the builder, we might as well
        // use the Errors property to avoid reparsing.
        foreach (var error in message.Errors)
        {
            if (errorActions.TryGetValue(error.Address, out var action))
            {
                action(error);
            }
        }
    }

    private SscMessageHandler(string? id, Dictionary<string, Func<object?, bool>> actions, Dictionary<string, Action<SscError>> errorActions)
    {
        this.actions = new(actions);
        this.errorActions = new(errorActions);
        RequestMessage = new SscMessage(actions.Keys.OrderBy(addr => addr, StringComparer.Ordinal)).WithId(id);
    }

    public sealed class Builder : IEnumerable
    {
        private readonly string? id;
        private readonly Dictionary<string, Func<object?, bool>> actions = new();
        private readonly Dictionary<string, Action<SscError>> errorActions = new();

        /// <summary>
        /// Creates a builder for a handler with the given <see cref="SscAddresses.Xid"/>.
        /// </summary>
        /// <param name="id">The ID for the handler. This is included in <see cref="RequestMessage"/>,
        /// and can be used to correlate requests with handlers.</param>
        public Builder(string? id)
        {
            this.id = id;
        }

        public Builder() : this(null)
        {
        }

        public IEnumerator GetEnumerator() =>
            throw new NotSupportedException("This type onlyimplements IEnumerable to support collection initializers");

        public void Add(string address, Action<double> handler)
        {
            var doubleHandler = MaybeHandle(handler);
            var longHandler = MaybeHandle((long x) => handler(x));
            actions[address] = obj => doubleHandler(obj) || longHandler(obj);
        }

        public void Add(string address, Action<double> handler, Action<SscError> errorHandler)
        {
            Add(address, handler);
            errorActions[address] = errorHandler;
        }

        public void Add<T>(string address, Action<T> handler) =>
            actions[address] = MaybeHandle(handler);

        public void Add<T>(string address, Action<T> handler, Action<SscError> errorHandler)
        {
            actions[address] = MaybeHandle(handler);
            errorActions[address] = errorHandler;
        }

        public SscMessageHandler Build() => new SscMessageHandler(id, actions, errorActions);

        private Func<object?, bool> MaybeHandle<T>(Action<T> handler) =>
            obj =>
            {
                if (obj is T value)
                {
                    handler(value);
                    return true;
                }
                return false;
            };
    }
}

