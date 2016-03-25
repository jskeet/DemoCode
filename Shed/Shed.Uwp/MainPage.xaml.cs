// Copyright 2016 Jon Skeet.
// Licensed under the Apache License Version 2.0.

using Shed.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            { "lights on", Factory.Lighting.On },
            { "lights off", Factory.Lighting.Off },
            { "music play", Factory.Sonos.Play },
            { "music pause", Factory.Sonos.Pause },
            { "music mute", () => Factory.Sonos.SetVolume(0) },
            { "music quiet", () => Factory.Sonos.SetVolume(30) },
            { "music medium", () => Factory.Sonos.SetVolume(60) },
            { "music loud", () => Factory.Sonos.SetVolume(90) },
            { "music next", Factory.Sonos.Next },
            { "music previous", Factory.Sonos.Previous },
            { "music restart", Factory.Sonos.Restart },
            { "amplifier on", Factory.Amplifier.On },
            { "amplifier off", Factory.Amplifier.Off },
            { "amplifier mute", () => Factory.Amplifier.SetVolume(0) },
            { "amplifier quiet", () => Factory.Amplifier.SetVolume(30) },
            { "amplifier medium", () => Factory.Amplifier.SetVolume(50) },
            { "amplifier loud", () => Factory.Amplifier.SetVolume(60) },
            { "amplifier source pie", () => Factory.Amplifier.Source("pi") },
            { "amplifier source sonos", () => Factory.Amplifier.Source("sonos") },
            { "amplifier source playstation", () => Factory.Amplifier.Source("ps4") }
        }.WithKeyPrefix(Prefix);

        private SpeechRecognizer recognizer;

        public MainPage()
        {
            this.InitializeComponent();
        }
        
        private void TurnOnLights(object sender, RoutedEventArgs e)
        {
            Factory.Lighting.On();
        }

        private void TurnOffLights(object sender, RoutedEventArgs e)
        {
            Factory.Lighting.Off();
        }

        private async void RegisterVoiceActivation(object sender, RoutedEventArgs e)
        {
            recognizer = new SpeechRecognizer
            {
                Constraints = { new SpeechRecognitionListConstraint(handlers.Keys) }
            };
            recognizer.ContinuousRecognitionSession.ResultGenerated += HandleVoiceCommand;
            recognizer.StateChanged += HandleStateChange;

            // Compile grammar
            SpeechRecognitionCompilationResult compilationResult = await recognizer.CompileConstraintsAsync();

            if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
            {
                await recognizer.ContinuousRecognitionSession.StartAsync();
            }
            else
            {
                await Dispatcher.RunIdleAsync(_ => lastState.Text = $"Compilation failed: $compilationResult.Status");
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
