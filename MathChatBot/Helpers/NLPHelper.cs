﻿using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.tagger.maxent;
using edu.stanford.nlp.util;
using edu.stanford.nlp.util.logging;
using java.util;
using MathChatBot.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MathChatBot.Helpers
{

    /// <summary>
    /// An object for when tagging the words
    /// </summary>
    public class TaggedWord
    {
        public enum POSTag
        {
            NONE,
            CC,         // Coordinating conjunction
            CD,         // Cardinal number
            DT,         // Determiner
            EX,         // Existential there
            FW,         // Foreign word
            IN,         // Preposition or subordinating conjunction
            JJ,         // Adjective
            JJR,        // Adjective, comparative
            JJS,        // Adjective, superlative
            LS,         // List item marker
            MD,         // Modal
            NN,         // Noun, singular or mass
            NNS,        // Noun, plural
            NNP,        // Proper noun, singular
            NNPS,       // Proper noun, plural
            PDT,        // Predeterminer
            POS,        // Possessive ending
            PRP,        // Personal pronoun
            PRPS,       // Possessive pronoun
            RB,         // Adverb
            RBR,        // Adverb, comparative
            RBS,        // Adverb, superlative
            RP,         // Particle
            SYM,        // Symbol
            TO,         // to
            UH,         // Interjection
            VB,         // Verb, base form
            VBD,        // Verb, past tense
            VBG,        // Verb, gerund or present participle
            VBN,        // Verb, past participle
            VBP,        // Verb, non-3rd person singular present
            VBZ,        // Verb, 3rd person singular present
            WDT,        // Wh-determiner
            WP,         // Wh-pronoun
            WPS,        // Possessive wh-pronoun
            WRB         // Wh-adverb
        }

        public string OriginalText
        {
            get
            {
                if (Original == null || WhiteSpaceCharacterBefore == null)
                    return "";
                else
                    return WhiteSpaceCharacterBefore + Original;
            }
        }
        public string Word { get; set; }
        public string WhiteSpaceCharacterBefore { get; set; }
        public string WhiteSpaceCharacterAfter { get; set; }
        public string Lemma { get; set; }
        public string Original { get; set; }
        public string NERStringIdentifier { get; set; }
        public POSTag POSIdentifier
        {
            get
            {
                if (POSStringIdentifier == null)
                    return POSTag.NONE;
                var tempIdentifier = POSStringIdentifier.Replace("$", "S");
                return Utility.ParseEnum<POSTag>(tempIdentifier);
            }
        }
        public string POSStringIdentifier { get; set; }
        public bool IsNoun
        {
            get
            {
                switch (POSIdentifier)
                {
                    case POSTag.NN:
                    case POSTag.NNP:
                    case POSTag.NNPS:
                    case POSTag.NNS:
                        return true;
                }

                return false;
            }
        }
        public bool IsAdjective
        {
            get
            {
                switch (POSIdentifier)
                {
                    case POSTag.JJ:
                    case POSTag.JJR:
                    case POSTag.JJS:
                    case POSTag.VBG:
                        return true;
                }

                return false;
            }
        }
        public bool IsVerb
        {
            get
            {
                switch (POSIdentifier)
                {
                    case POSTag.VB:
                    case POSTag.VBD:
                    case POSTag.VBG:
                    case POSTag.VBN:
                    case POSTag.VBP:
                    case POSTag.VBZ:
                        return true;
                }

                return false;
            }
        }
        public bool IsWHWord
        {
            get
            {
                switch (POSIdentifier)
                {
                    case POSTag.WDT:
                    case POSTag.WP:
                    case POSTag.WPS:
                    case POSTag.WRB:
                        return true;
                }

                return false;
            }
        }
    }

    public static class NLPUtility
    {
        public static string ListToString(this List<TaggedWord> list)
        {
            return string.Join(string.Empty, list.Select(x => x.OriginalText)).Trim();
        }
    }

    /// <summary>
    /// Interaction logic for the NLPHelper
    /// </summary>
    public class NLPHelper
    {

        //*************************************************/
        // PROPERTIES
        //*************************************************/
        #region Properties

        private StanfordCoreNLP Tagger { get; set; }
        private ArrayList Sentences { get; set; }

        #endregion

        //*************************************************/
        // CONSTRUCTOR
        //*************************************************/
        #region Constructor

        public NLPHelper()
        {
            SetupTagger();
        }

        #endregion

        //*************************************************/
        // METHODS
        //*************************************************/
        #region Methods

        /// <summary>
        /// Setup tagger including POS
        /// </summary>
        private void SetupTagger()
        {
            PerformanceTester.StartMET("NLP");
            // Get path to Stanford NLP models
            var jarRoot = Path.Combine(Utility.GetResourcesFolder(), @"stanford-corenlp-3.9.2-models");
            // Turn off logging
            RedwoodConfiguration.current().clear().apply();
            // Set properties
            var props = new java.util.Properties();
            props.setProperty("annotators", "tokenize, ssplit, pos");
            // Set current directory
            var curDir = Environment.CurrentDirectory;
            var modelsDirectory = curDir + "\\" + jarRoot + @"\edu\stanford\nlp\models";
            Directory.SetCurrentDirectory(jarRoot);
            // Load Stanford NLP
            Tagger = new StanfordCoreNLP(props);
            PerformanceTester.StopMET("NLP");
        }

        /// <summary>
        /// Setup extended tagger that includes POS, lemma and entity analysis
        /// </summary>
        private void SetupExtendedTagger()
        {
            PerformanceTester.StartMET("NLP");
            // Get path to Stanford NLP models
            var jarRoot = Path.Combine(Utility.GetResourcesFolder(), @"stanford-corenlp-3.9.2-models");
            // Turn off logging
            RedwoodConfiguration.current().clear().apply();
            var props = new java.util.Properties();
            props.setProperty("annotators", "tokenize, ssplit, pos, lemma, ner");
            // Makes Named Entity Recognition work in the library
            props.setProperty("ner.useSUTime", "0");
            props.put("ner.applyFineGrained", "0");
            props.put("ner.fine.regexner.mapping", jarRoot + @"\edu\stanford\nlp\models\kbp\english\");
            // Set current directory
            var curDir = Environment.CurrentDirectory;
            var modelsDirectory = curDir + "\\" + jarRoot + @"\edu\stanford\nlp\models";
            Directory.SetCurrentDirectory(jarRoot);

            // Load Stanford NLP
            Tagger = new StanfordCoreNLP(props);
            PerformanceTester.StopMET("NLP");
        }

        /// <summary>
        /// Get sentences from the given message text
        /// </summary>
        /// <param name="text">The message text</param>
        /// <returns>A list with the sentences</returns>
        public List<object> GetSentences(string text)
        {
            PerformanceTester.StartMET("GetSentences");
            var annotation = new Annotation(text);
            Tagger.annotate(annotation);
            Sentences = annotation.get(new CoreAnnotations.SentencesAnnotation().getClass()) as ArrayList;
            PerformanceTester.StopMET("GetSentences");
            return new List<object>(Sentences.toArray());
        }

        /// <summary>
        /// Tag the given message text
        /// </summary>
        /// <param name="text">The message text</param>
        /// <returns>A list with the tagged words</returns>
        public List<TaggedWord> Tag(string text, bool useSavedSentences = false)
        {
            PerformanceTester.StartMET("Tagging");
            var list = new List<TaggedWord>();

            ArrayList sentences = null;
            if (useSavedSentences && Sentences != null)
                sentences = Sentences;
            else
            {
                var annotation = new Annotation(text);
                Tagger.annotate(annotation);
                sentences = annotation.get(new CoreAnnotations.SentencesAnnotation().getClass()) as ArrayList;
            }
            foreach (CoreMap sentence in sentences)
            {
                var tokens = sentence.get(new
                CoreAnnotations.TokensAnnotation().getClass()) as ArrayList;
                foreach (CoreLabel token in tokens)
                {
                    var original = token.get(new CoreAnnotations.OriginalTextAnnotation().getClass());
                    var after = token.get(new CoreAnnotations.AfterAnnotation().getClass());
                    var before = token.get(new CoreAnnotations.BeforeAnnotation().getClass());
                    var word = token.get(new CoreAnnotations.TextAnnotation().getClass());
                    var pos = token.get(new CoreAnnotations.PartOfSpeechAnnotation().getClass());
                    //var ner = token.get(new CoreAnnotations.NamedEntityTagAnnotation().getClass());
                    //var lemma = token.get(new CoreAnnotations.LemmaAnnotation().getClass());

                    var taggedWord = new TaggedWord()
                    {
                        Word = word.ToString(),
                        Original = original.ToString(),
                        WhiteSpaceCharacterAfter = after.ToString(),
                        WhiteSpaceCharacterBefore = before.ToString(),
                        POSStringIdentifier = pos.ToString(),
                        //Lemma = lemma.ToString(),
                        //NERStringIdentifier = ner.ToString()
                    };
                    list.Add(taggedWord);
                }
            }

            PerformanceTester.StopMET("Tagging");

            return list;
        }

        #endregion

    }

}
