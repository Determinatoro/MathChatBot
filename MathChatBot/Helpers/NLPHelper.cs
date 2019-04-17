using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.tagger.maxent;
using edu.stanford.nlp.util;
using java.util;
using MathChatBot.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MathChatBot.Helpers
{
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

        private string _stringIdentifier;

        public string OriginalText
        {
            get
            {
                if (Original == null || WhiteSpaceCharacterBefore == null)
                    return null;
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
        public POSTag POSIdentifier { get; set; }
        public string POSStringIdentifier
        {
            get
            {
                return _stringIdentifier;
            }
            set
            {
                _stringIdentifier = value;

                if (_stringIdentifier == null)
                    POSIdentifier = POSTag.NONE;

                var tempIdentifier = _stringIdentifier.Replace("$", "S");

                POSIdentifier = Utility.ParseEnum<POSTag>(tempIdentifier);
            }
        }


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

        public static TaggedWord ParseFromNLP(string nlp)
        {
            var splitted = Regex.Split(nlp, "/");
            if (splitted.Length != 2)
                return null;

            return new TaggedWord()
            {
                Word = splitted[0],
                POSStringIdentifier = splitted[1]
            };
        }
    }

    public class NLPHelper
    {
        //private MaxentTagger POSTagger { get; set; }
        private StanfordCoreNLP ExtendedTagger { get; set; }

        private static string AppFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string ResourcesFolderPath = Path.Combine(Directory.GetParent(AppFolderPath).Parent.FullName, "Resources\\{0}");

        public NLPHelper()
        {
            //var taggerPath = string.Format(ResourcesFolderPath, @"stanford-corenlp-3.9.2-models\edu\stanford\nlp\models\pos-tagger\english-left3words\english-left3words-distsim.tagger");
            //POSTagger = new MaxentTagger(taggerPath);

            var jarRoot = string.Format(ResourcesFolderPath, @"stanford-corenlp-3.9.2-models");
            var props = new java.util.Properties();
            props.setProperty("annotators", "tokenize, ssplit, pos, lemma, ner");
            props.setProperty("ner.useSUTime", "0");
            props.put("ner.applyFineGrained", "0");
            props.put("ner.fine.regexner.mapping", jarRoot + @"\edu\stanford\nlp\models\kbp\english\");
            var curDir = Environment.CurrentDirectory;
            var modelsDirectory = curDir + "\\" + jarRoot + @"\edu\stanford\nlp\models";
            Directory.SetCurrentDirectory(jarRoot);

            ExtendedTagger = new StanfordCoreNLP(props);
        }

        public List<object> GetSentences(string text)
        {
            var sentences = MaxentTagger.tokenizeText(new java.io.StringReader(text)).toArray();

            return new List<object>(sentences);
        }

        /*public List<TaggedWord> POSTag(string text)
        {
            var list = new List<TaggedWord>();
            
            var sentences = MaxentTagger.tokenizeText(new java.io.StringReader(text)).toArray();
            foreach (ArrayList sentence in sentences)
            {
                var taggedSentence = POSTagger.tagSentence(sentence);
                var array = taggedSentence.toArray();

                foreach (var word in array)
                {
                    var strWord = word.ToString();
                    var nlpWord = TaggedWord.ParseFromNLP(strWord);
                    list.Add(nlpWord);
                }

                //System.Console.WriteLine(edu.stanford.nlp.ling.SentenceUtils.listToString(taggedSentence, false));
            }

            return list;
        }*/

        public List<TaggedWord> Tag(string text)
        {
            var list = new List<TaggedWord>();

            var annotation = new Annotation(text);

            ExtendedTagger.annotate(annotation);

            var sentences = annotation.get(new CoreAnnotations.SentencesAnnotation().getClass()) as ArrayList;
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
                    var pos = token.get(new
                    CoreAnnotations.PartOfSpeechAnnotation().getClass());
                    var ner = token.get(new
                    CoreAnnotations.NamedEntityTagAnnotation().getClass());
                    var lemma = token.get(new
                    CoreAnnotations.LemmaAnnotation().getClass());

                    var taggedWord = new TaggedWord()
                    {
                        Word = word.ToString(),
                        Original = original.ToString(),
                        WhiteSpaceCharacterAfter = after.ToString(),
                        WhiteSpaceCharacterBefore = before.ToString(),
                        POSStringIdentifier = pos.ToString(),
                        Lemma = lemma.ToString(),
                        NERStringIdentifier = ner.ToString()
                    };
                    list.Add(taggedWord);
                }
            }

            return list;
        }
    }
}
