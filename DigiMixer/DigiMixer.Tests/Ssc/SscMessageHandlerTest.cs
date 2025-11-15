// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DigiMixer.Ssc;

namespace DigiMixer.Tests.Ssc;

public class SscMessageHandlerTest
{
    [Test]
    public void RequestMessageId()
    {
        var handler = new SscMessageHandler.Builder("id").Build();
        Assert.That(handler.RequestMessage.Id, Is.EqualTo("id"));
    }

    [Test]
    public void HandlerActionCalled()
    {
        string? received = null;
        var handler = new SscMessageHandler.Builder
        {
            { "/abc/def", (string x) => received = x }
        }.Build();

        var response = new SscMessage(new SscProperty("/abc/def", "value"));
        handler.HandleMessage(response);
        Assert.That(received, Is.EqualTo("value"));
    }

    [Test]
    public void DoubleHandlerCalledForLong()
    {
        double received = 0;
        var handler = new SscMessageHandler.Builder
        {
            { "/abc/def", (double x) => received = x }
        }.Build();

        var response = new SscMessage(new SscProperty("/abc/def", 5L));
        handler.HandleMessage(response);
        Assert.That(received, Is.EqualTo(5));
    }

    [Test]
    public void ErrorHandlerCalled()
    {
        string? receivedValue = null;
        SscError? receivedError = null;
        var handler = new SscMessageHandler.Builder
        {
            { "/abc/def", (string x) => receivedValue = x, error => receivedError = error }
        }.Build();

        var error = new SscError("/abc/def", 404, "Not found");
        var response = new SscMessage(SscProperty.FromErrors(error));
        handler.HandleMessage(response);

        Assert.That(receivedValue, Is.Null);
        Assert.That(receivedError, Is.EqualTo(error));
    }
}
