// Copyright 2016 Jon Skeet.
// Licensed under the Apache License Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using static Shed.Controllers.Factory;

namespace Shed.Uwp
{
    // This just makes the initialization of MainPage.handlers simpler.
    internal static class DictionaryExtensions
    {
        internal static Dictionary<string, TValue> WithKeyPrefix<TValue>(
            this Dictionary<string, TValue> source,
            string prefix)
            => source.ToDictionary(pair => prefix + pair.Key, pair => pair.Value);
    }

    public sealed partial class MainPage : Page
    {
        private const double ConfidenceThreshold = 0.6;
        private const string Prefix = "shed ";

        private static readonly Dictionary<string, Action> handlers = new Dictionary<string, Action>
        {
            { "lights on", Lighting.On },
            { "lights off", Lighting.Off },
            { "music play", Sonos.Play },
            { "music pause", Sonos.Pause },
            { "music mute", () => Sonos.SetVolume(0) },
            { "music quiet", () => Sonos.SetVolume(30) },
            { "music medium", () => Sonos.SetVolume(60) },
            { "music loud", () => Sonos.SetVolume(90) },
            { "music next", Sonos.Next },
            { "music previous", Sonos.Previous },
            { "music restart", Sonos.Restart },
            { "amplifier on", Amplifier.On },
            { "amplifier off", Amplifier.Off },
            { "amplifier mute", () => Amplifier.SetVolume(0) },
            { "amplifier quiet", () => Amplifier.SetVolume(30) },
            { "amplifier medium", () => Amplifier.SetVolume(50) },
            { "amplifier loud", () => Amplifier.SetVolume(60) },
            { "amplifier input dock", () => Amplifier.Source("dock") },
            { "amplifier input pie", () => Amplifier.Source("pi") },
            { "amplifier input sonos", () => Amplifier.Source("sonos") },
            { "amplifier input playstation", () => Amplifier.Source("ps4") }
        }.WithKeyPrefix(Prefix);

        // Unclear whether we really need an instance variable here. If we just
        // used a local variable in RegisterVoiceActivation, would the instance be
        // garbage collected?
        private SpeechRecognizer recognizer;

        public MainPage()
        {
            this.InitializeComponent();
        }
        
        private void TurnOnLights(object sender, RoutedEventArgs e)
        {
            Lighting.On();
        }

        private void TurnOffLights(object sender, RoutedEventArgs e)
        {
            Lighting.Off();
        }

        private async void RegisterVoiceActivation(object sender, RoutedEventArgs e)
        {
            recognizer = new SpeechRecognizer
            {
                Constraints = { new SpeechRecognitionListConstraint(handlers.Keys) }
            };
            recognizer.ContinuousRecognitionSession.ResultGenerated += HandleVoiceCommand;
            recognizer.ContinuousRecognitionSession.AutoStopSilenceTimeout = TimeSpan.FromDays(1000);
            recognizer.StateChanged += HandleStateChange;

            SpeechRecognitionCompilationResult compilationResult = await recognizer.CompileConstraintsAsync();
            if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
            {
                await recognizer.ContinuousRecognitionSession.StartAsync();
            }
            else
            {
                await Dispatcher.RunIdleAsync(_ => lastState.Text = $"Compilation failed: {compilationResult.Status}");
            }
        }

        private async void HandleStateChange(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            await Dispatcher.RunIdleAsync(_ => lastState.Text = args.State.ToString());
        }

        private async void HandleVoiceCommand(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            string text = args.Result.Text;
            // TODO: Clear this 5 seconds later, or add a timestamp.
            await Dispatcher.RunIdleAsync(_ =>
            {
                lastText.Text = text;
                lastConfidence.Text = $"{args.Result.Confidence} ({args.Result.RawConfidence})";
            });
            if (args.Result.RawConfidence >= ConfidenceThreshold)
            {
                Action handler;
                if (handlers.TryGetValue(text, out handler))
                {
                    // Asynchronously call the handler; we don't care about the result.
                    // (We might want ContinueWith on error at some point...)
#pragma warning disable CS4014
                    Task.Run(handler);
#pragma warning restore
                }
            }
        }
    }
}
