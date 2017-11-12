﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using VGPrompter;
using System.Collections.Generic;
using System.Linq;

namespace Tests {

    [TestClass]
    public class UnitTest1 {

        public static string GetResourcePath(string filename) {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\" + filename);
        }

        public static readonly string TEST_SCRIPT_1 = "Data/insc_test1.rpy";
        public static readonly string TEST_SCRIPT_1_TAB = "Data/insc_test1_tab.rpy";
        public static readonly string DEMO = "Data/demo.rpy";
        public static readonly string NEW_DEMO = "Data/new_demo.rpy";

        public Script LoadScript(string fp, Script.Parser.IndentChar indent = Script.Parser.IndentChar.Auto) {

            Assert.IsTrue(File.Exists(fp));

            var actions = new Dictionary<string, Action>() {
                { "Nothing", () => { } }
            };

            var conditions = new Dictionary<string, Func<bool>>() {
                { "True", () => true },
                { "False", () => false }
            };

            Script.Parser.Logger = new VGPrompter.Logger();

            var script = Script.FromSource(fp, indent: indent);
            script.Conditions = conditions;

            script.Logger = new VGPrompter.Logger();

            return script;

        }

        [TestMethod]
        public void TestScriptPriming() {

            var script = LoadScript(GetResourcePath(TEST_SCRIPT_1));
            script.Prime();
            script.Validate();

        }

        [TestMethod]
        public void TestScriptEnumerator() {
            var script = LoadScript(GetResourcePath(TEST_SCRIPT_1_TAB));
            script.Prime();
            script.Validate();
            foreach (var x in script) {
                if (x is Script.Menu)
                    script.CurrentChoiceIndex = (uint)SelectChoice(x as Script.Menu);
                Console.WriteLine(x.ToString());
            }
        }

        void SomeOtherDelegate(string y) {
            Console.WriteLine(y);
        }

        delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7);

        void TestFunc(bool a, bool b, string c, string d, double e, int f, float g) { }

        [TestMethod]
        public void TestDemoScriptEnumerator() {
            var script = LoadScript(GetResourcePath(DEMO));
            var conditions = new Dictionary<string, Func<bool>>() {
                { "True", () => true },
                { "False", () => false },
                { "CurrentColorNotGreen", () => false }
            };

            var actions = new Dictionary<string, Action>() {
                { "DoNothing", () => { } },
                { "TurnCubeGreen", () => { } },
                { "TurnCubeBlue", () => { } }
            };

            script.SetDelegates(conditions, actions);
            script.Functions = new Dictionary<string, Delegate>() {
                { "SomeDelegate", (Action<Script, int,int>)((s, a, b) => {
                    Console.WriteLine(s);
                    Console.WriteLine(a + b);
                }) },
                { "SomeOtherDelegate", (Action<string>)SomeOtherDelegate },
                { "Test", (Action<bool, bool, string, string, double, int, float>)TestFunc }
            };

            script.Prime();
            script.Validate();

            script.RunFromBeginning(
                OnMenu: m => {
                    var x = m;
                    var idx = x.TrueChoices.Last().Index;
                    return idx;
                },
                OnLine: l => { Console.WriteLine(l); });
        }

        [TestMethod]
        public void TestScriptEnumeratorWhile() {
            var script = LoadScript(GetResourcePath(TEST_SCRIPT_1_TAB));
            script.Prime();
            script.Validate();

            var i = 0;
            foreach (var line in script) {
                if (i++ > 2) {
                    break;
                }
                Console.WriteLine(line.ToString());
            }

        }

        [TestMethod]
        public void TestScriptSerialization() {

            var fn = GetResourcePath("serialized.bin");
            var script = LoadScript(GetResourcePath(TEST_SCRIPT_1_TAB));

            script.Prime();
            script.Validate();
            script.RepeatLastLineOnRecover = false;

            PlayTest(script, 2);

            var bytes = script.ToBinary();
            File.WriteAllBytes(fn, bytes);

            var dscript = Utils.LoadSerialized<Script>(fn);
            dscript.Validate();

            PlayTest(dscript);


            /*var enum1 = script.GetEnumeratorStrings();

            for (int i = 0; i < 2; i++)
                enum1.GetEnumerator().MoveNext();

            var enum2 = dscript.GetEnumeratorStrings();

            enum2.GetEnumerator().MoveNext();

            var a = enum1.GetEnumerator().Current;
            var b = enum2.GetEnumerator().Current;

            Console.WriteLine(a);
            Console.WriteLine(b);

            Assert.AreEqual(a, b);*/


            //PlayTest(dscript);

        }

        /*public void PlayTest(Script script, int? n = null) {
            Console.WriteLine("Playtest");
            var rnd = new Random();
            script.RunFromBeginning(
                (menu) => script.CurrentChoiceIndex = (uint)rnd.Next(menu.Count - 1),
                (line) => Console.WriteLine(line.ToString()),
                n: n
            );
        }*/

        public void PlayTest(Script script, int? n = null) {
            Console.WriteLine("Playtest");
            script.RunFromCurrentLine(
                OnMenu: menu => (new Random()).Next(menu.Count - 1),
                OnLine: line => Console.WriteLine(line.ToString()),
                Take: n
            );
        }

        int SelectChoice(Script.Menu menu) {
            return (new Random()).Next(menu.Count - 1);
        }

        [TestMethod]
        public void TestNewParser() {

            var rm = new VGPrompter.Script.ResourceManager();
            List<Script.Parser.RawLine> lines;
            List<string> labels;
            Script.Parser.ParseVGPScriptFile(NEW_DEMO, rm, out lines, out labels);
            foreach (var line in lines) {
                Console.WriteLine(line.Text);
            }
            foreach (var label in labels) {
                Console.WriteLine(label);
            }

        }

    }

}
