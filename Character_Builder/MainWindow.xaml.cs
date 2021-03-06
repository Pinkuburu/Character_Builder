﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using DnDSupportTypes;

namespace Character_Builder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BlockingCollection<DnDCharacter> party = new BlockingCollection<DnDCharacter>(100);
        Task[] producers;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void main_click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();

            blog($"starting: {sw.ElapsedMilliseconds} ms");

            if (!DnDCore.hasLoadSuccess)
            {
                log("Core data not loaded, try again later");
                return;
            }

            int numChars = nud_numCharacters.Value.Value, current = 0;

            while (current < numChars)
            {
                DnDCharacter temp;
                if (party.TryTake(out temp))
                {
                    log($"{temp.ToString()}\n");
                    current++;
                }
            }


            sw.Stop();
            blog($"complate: {sw.ElapsedMilliseconds} ms");
        }

        private void log(string message)
        {
            output_1.Text += $"{message}\n";
        }

        private void blog(string message)
        {
            output_2.Text += $"{message}\n";
        }

        private void clear_click(object sender, RoutedEventArgs e)
        {
            output_1.Text = "";
            output_2.Text = "";
        }

        private void exit_click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // if the dndcore hasn't loaded properly, this outputs the exceptions, warns the user, and stops the program dead.
            if (!DnDCore.hasLoadSuccess)
            {
                foreach (var ex in DnDCore.BOOM)
                {
                    log($"\n\n{ex.Message}\n{ex.StackTrace}");
                }
                log($"\nAny further attempt to interact with this program will fail.");
                return;
            }

            // spin up multiple producer threads
            producers = new Task[5];
            for (int i = 0; i < producers.Length; i++)
            {
                producers[i] = Task.Run(() =>
                {
                    while (!party.IsAddingCompleted)
                    {
                        try
                        {
                            party.Add(new DnDCharacter());
                        }
                        catch (InvalidOperationException)
                        {
                            break;
                        }
                    }
                });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            party.CompleteAdding();
            if (producers != null)
            {
                Task.WaitAll(producers);
                foreach (var a in producers)
                {
                    a.Dispose();
                }
            }
            party.Dispose();
        }

        private void button_2_Click(object sender, RoutedEventArgs e)
        {
            string a = "+++ Races +++\n\n";
            foreach (var r in DnDCore.Races)
            {
                a += $"{r.ToString()}\n";
            }
            a += $"+++ Classes +++\n\n";
            foreach (var r in DnDCore.Classes)
            {
                a += $"{r.ToString()}\n";
            }
            a += $"+++ Subclasses +++\n\n";
            foreach (var r in DnDCore.SubClasses)
            {
                a += $"{r.ToString()}\n";
            }
            a += $"+++ Features +++\n";
            foreach (var r in DnDCore.Features)
            {
                a += $"\n{r.ToString()}\n";
            }
            blog(a);
        }

        private void verbosity_Checked(object sender, RoutedEventArgs e)
        {
            DnDCore.verbose = true;
        }

        private void verbosity_Unchecked(object sender, RoutedEventArgs e)
        {
            DnDCore.verbose = false;
        }
    }
}
