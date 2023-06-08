using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace BluLib
{
    public class TimeTest : IDisposable
    {
        private static readonly Dictionary<string, TimeTestData> _tests = new();

        #region Desposable Test

        private readonly string _disposableTest;

        public TimeTest(string title, bool useMilliseconds = false)
        {
            _disposableTest = title;
            _tests[_disposableTest] = new(title, useMilliseconds);
        }

        public void Dispose()
        {
            _tests[_disposableTest].EndTest();
            _tests.Remove(_disposableTest);
        }

        #endregion


        #region Static Test

        private static string _lastStaticTest = string.Empty;

        public static void Start(string title, bool useMilliseconds = false)
        {
            if (_tests.TryGetValue(title, out TimeTestData test))
            {
                test.Timer.Start();
            }
            else
            {
                _lastStaticTest = title;
                _tests[_lastStaticTest] = new(title, useMilliseconds);
            }
        }
        
        
        public static void Pause()
        {
            if (!_tests.ContainsKey(_lastStaticTest))
            {
                Debug.LogWarning("TimeTest caused: TimeTest.Pause() call. There was no TimeTest.Start()");
                return;
            }

            _tests[_lastStaticTest].Timer.Stop();
        }

        public static void Pause(string title)
        {
            if (!_tests.ContainsKey(title))
            {
                Debug.LogWarning("TimeTest caused: Test Paused but not Started — " + title);
                return;
            }

            _tests[title].Timer.Stop();
        }
        

        public static void End()
        {
            if (!_tests.ContainsKey(_lastStaticTest))
            {
                Debug.LogWarning("TimeTest caused: TimeTest.End() call. There was no TimeTest.Start()");
                return;
            }

            _tests[_lastStaticTest].EndTest();
            _tests.Remove(_lastStaticTest);
        }

        public static void End(string title)
        {
            if (!_tests.ContainsKey(title))
            {
                Debug.LogWarning("TimeTest caused: Test not found — " + title);
                return;
            }

            _tests[title].EndTest();
            _tests.Remove(title);
            _lastStaticTest = string.Empty;
        }

        #endregion


        private struct TimeTestData
        {
            private readonly string _testTitle;
            private readonly bool _precise;
            public readonly Stopwatch Timer;


            private static readonly StringBuilder StringBuilder = new();

            public TimeTestData(string testTitle, bool precise)
            {
                _testTitle = testTitle;
                _precise = precise;
                Timer = Stopwatch.StartNew();
            }


            public void EndTest()
            {
                long ms = Timer.ElapsedMilliseconds;
                float elapsedVal = _precise ? ms : ms / 1000f;
                string valMark = _precise ? "ms" : "s";

                StringBuilder.Length = 0;
                StringBuilder.Append("Time Test <color=brown>")
                    .Append(_testTitle)
                    .Append("</color>: ")
                    .Append(elapsedVal)
                    .Append(valMark);

                Debug.LogWarning(StringBuilder);
            }
        }
    }
}