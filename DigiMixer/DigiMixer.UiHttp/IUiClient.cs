namespace DigiMixer.UiHttp;

internal interface IUiClient : IDisposable
{
    Task StartReading();
    event EventHandler<UiMessage>? MessageReceived;
    Task Send(UiMessage message, CancellationToken cancellationToken = default);

    public class Fake : IUiClient
    {
        internal static Fake Instance { get; } = new Fake();
        private Fake() { }

        public event EventHandler<UiMessage>? MessageReceived
        {
            add { }
            remove { }
        }

        public void Dispose() { }

        public Task Send(UiMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task StartReading() => Task.CompletedTask;
    }
}
