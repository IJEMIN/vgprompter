﻿using System;

namespace VGPrompter {

    public partial class Script {

        [Serializable]
        internal class VGPDialogueLine : Line, IWrappable, ITextual {

            public bool ToInterpolate { get; private set; }
            public string Tag { get; private set; }
            public string ParentLabel { get; private set; }
            public string TextHash { get; private set; }

            public VGPDialogueLine() { }

            public VGPDialogueLine(string label, string hash, string tag, bool to_interpolate) {
                Tag = tag;
                ParentLabel = label;
                TextHash = hash;
                ToInterpolate = to_interpolate;
            }

            public override bool IsValid() { return !string.IsNullOrEmpty(TextHash); }

            public new string ToString() {
                return (string.IsNullOrEmpty(Tag) ? "<Anonymous>" : Tag) + ": " + TextHash;
            }

            public IScriptLine ToWrapper(Script script) {
                //var text = script._text_manager.GetText(ParentLabel, TextHash, ToInterpolate);
                var text = GetInterpolatedText(script);
                return new DialogueLine(text, Tag);
            }

            public string GetInterpolatedText(Script script) {
                return script._text_manager.GetText(ParentLabel, TextHash, ToInterpolate);
            }

        }

    }

}