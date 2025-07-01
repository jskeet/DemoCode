using DigiMixer.Diagnostics;
using DigiMixer.AllenAndHeath.Core;

namespace DigiMixer.SqSeries.Tools;

internal static class AHRawMessageExtensions
{
    internal static void DisplayStructure(this AnnotatedMessage<AHRawMessage> annotatedMessage, TextWriter writer)
    {
        var message = annotatedMessage.Message;
        string directionIndicator = annotatedMessage.Direction == MessageDirection.ClientToMixer ? "=>" : "<=";
        var sqMessage = SqMessage.FromRawMessage(message);
        writer.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff}: {directionIndicator} 0x{annotatedMessage.StreamOffset:x8} {sqMessage}");
    }
}
