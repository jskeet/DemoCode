// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VDrumExplorer.InstrumentParser
{
    /// <summary>
    /// This is just a simple program to take an instrument list (stored as a text file, copy/pasted from the data list PDF)
    /// and spit out an instrument groups JSON file.
    /// Extra data is hard-coded in the program for simplicity - we only need this one time per module.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: <input-file> <output-file>");
                return;
            }

            var categories = new Category[]
            {
                new("OFF", "Off", "Off"),
                new("KICK A", "Kick A", "KickA"),
                new("KICK B", "Kick B", "KickB"),
                new("KICK PROC", "Kick Proc", "Other"),
                new("KICK ELEC", "Kick Elec", "Other"),
                new("SNARE", "Snare", "Snare"),
                new("CROSS STICK", "Cross stick", "XStick"),
                new("SNARE PROC", "Snare Proc", "Other"),
                new("CROSS STICK PROC", "Cross stick proc", "Other"),
                new("SNARE ELEC", "Snare Elec", "Other"),
                new("TOM", "Tom", "Tom"),
                new("TOM PROC", "Tom Proc", "Other"),
                new("TOM ELEC", "Tom Elec", "Other"),
                new("HI-HAT", "Hi-Hat", "HiHat"),
                new("HI-HAT PROC", "Hi-Hat Proc", "HiHat"),
                new("HI-HAT ELEC", "Hi-Hat Elec", "HiHat"),
                new("HI-HAT FIXED ELEC", "Hi-Hat Fixed Elec", "HiHat"),
                new("RIDE", "Ride", "Ride"),
                new("CRASH", "Crash", "CrashChinaSplashStackedCymbal"),
                new("CHINA", "China", "CrashChinaSplashStackedCymbal"),
                new("SPLASH", "Splash", "CrashChinaSplashStackedCymbal"),
                new("STACKED CYMBAL", "Stacked Cymbal", "CrashChinaSplashStackedCymbal"),
                new("CYMBAL OTHERS", "Cymbal Others", "Other"),
                new("CYMBAL PROC", "Cymbal Proc", "Other"),
                new("CYMBAL ELEC", "Cymbal Elec", "Other"),
                new("BELL/CHIME/GONG", "Bell/Chime/Gong", "Other"),
                new("BLOCK/COWBELL", "Block/Cowbell", "Other"),
                new("PERCUSSION", "Percussion", "Other"),
                new("PERC ELEC", "Perc Elec", "Other"),
                new("CLAP", "Clap", "Other"),
                new("SOUND FX", "Sound FX", "Other"),
                new("ELEMENTS", "Elements", "Other"),
                new("SNARE BRUSH", "Snare Brush", "SnareBrush"),
                new("TOM BRUSH", "Tom Brush", "TomBrush"),
            }.OrderByDescending(cat => cat.prefix.Length).ToList();

            var lines = File.ReadAllLines(args[0]);
            var groups = lines
                .Select(ParseInstrument)
                .GroupBy(inst => inst.category)
                .OrderBy(g => g.First().number)
                .Select(g => new InstrumentGroup
                {
                    Description = g.Key.description,
                    VEditCategory = g.Key.veditCategory,
                    Instruments = new SortedDictionary<int, string>(g.ToDictionary(inst => inst.number, inst => inst.name))
                })
                .ToList();
            var json = JsonConvert.SerializeObject(groups, Formatting.Indented);
            File.WriteAllText(args[1], json);

            Instrument ParseInstrument(string line)
            {
                string[] bits = line.Split(' ', 2);
                int number = int.Parse(bits[0]);
                string categoryNameRemarks = bits[1];
                string categoryName = categoryNameRemarks.Split('*')[0];
                Category category = categories.FirstOrDefault(cat => categoryName.StartsWith(cat.prefix));
                if (category is null)
                {
                    throw new Exception($"Can't determine category for '{line}'");
                }
                string name = categoryName[category.prefix.Length..].Trim();
                return new Instrument(number, name, category);
            }
        }
    }

    record Category(string prefix, string description, string veditCategory);
    record Instrument(int number, string name, Category category);
}
