// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

// Very simplist functionality to proxy access between JavaScript and Blazor.
// No, JavaScript isn't my strong suit.
var midi = function () {
    var access = null;

    function initialize(handler) {
        success = function (midiAccess) {
            access = midiAccess;
            handler.invokeMethodAsync("Success");
        };
        failure = message => handler.invokeMethodAsync("Failure", message);
        navigator.requestMIDIAccess({ sysex: 5 })
            .then(success, failure);
    }

    function getInputPorts() {
        var ret = [];
        access.inputs.forEach(input => ret.push({ id: input.id, name: input.name, manufacturer: input.manufacturer }));
        return ret;
    }

    function getOutputPorts() {
        var ret = [];
        access.outputs.forEach(output => ret.push({ id: output.id, name: output.name, manufacturer: output.manufacturer }));
        return ret;
    }

    function addMessageHandler(portId, handler) {
        access.inputs.get(portId).onmidimessage = function (message) {
            // We need to base64-encode the data explicitly, so let's create a new object.
            var jsonMessage = { data: bytesToBase64(message.data), timestamp: message.timestamp };
            handler.invokeMethodAsync("OnMessageReceived", jsonMessage);
        };
    }

    function sendMessage(portId, data) {
        var binary = base64ToBytes(data);
        access.outputs.get(portId).send(binary);
    }

    return {
        initialize: initialize,
        getInputPorts: getInputPorts,
        getOutputPorts: getOutputPorts,
        addMessageHandler: addMessageHandler,
        sendMessage: sendMessage
    };
}();
