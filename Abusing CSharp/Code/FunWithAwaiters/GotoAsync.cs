                                // Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
                                using System;
                                using System.ComponentModel;

                                namespace FunWithAwaiters
                                {
                                    [Description("Goto (async style)")]
                                    class GotoAsync
                                    {
                                        private static void Main()
                                        {
                                            var executioner = new GotoExecutioner(Entry);
                                            executioner.Start();
                                        }

                                        private static async void Entry(LineAction _, GotoAction @goto)                                                                   
                                        {
await _();                                    Console.WriteLine("Hello");
await _();                                    Console.Write("Keep going? ");
await _();                                    string line = Console.ReadLine();
await _();                                    if (line == "y") {
await _();                                        await @goto(19);
await _();                                    }
await _();                                    Console.WriteLine("Finished!");
                                        }
                                    }
                                }
