using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MathChatBot.Utilities
{
    public class PerformanceTester
    {
        private static Dictionary<string, Stopwatch> stopwatches = new Dictionary<string, Stopwatch>();

        private static bool RunPerformanceTesting()
        {
            return Properties.Settings.Default.PerformanceTesting;
        }

        /// <summary>
        /// Start measure execution time
        /// </summary>
        /// <param name="key">Key for the stopwatch</param>
        public static void StartMET(string key)
        {
            if (!RunPerformanceTesting())
                return;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            stopwatches[key] = stopwatch;
            Console.WriteLine("{0}: Start stopwatch", key);
        }

        /// <summary>
        /// Stop measure execution time
        /// </summary>
        /// <param name="key">Key for the stopwatch</param>
        public static void StopMET(string key)
        {
            if (!RunPerformanceTesting())
                return;

            if (!stopwatches.Keys.Any(x => x == key))
                return;
            var stopwatch = stopwatches[key];
            stopwatch.Stop();

            long ticks = stopwatch.ElapsedTicks;
            double ns = 1000000000.0 * ticks / Stopwatch.Frequency;
            double ms = ns / 1000000.0;

            Console.WriteLine("{0}: {1}ms, {2}ns, {3} ticks", key, Math.Round(ms).ToString().Replace(",","."), Math.Round(ns).ToString().Replace(",", "."), ticks.ToString());
            stopwatches.Remove(key);
        }

    }
}
