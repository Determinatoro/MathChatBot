using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using java.util;
using java.io;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.tagger.maxent;
using edu.stanford.nlp.ie.crf;
using edu.stanford.nlp.coref.data;
using edu.stanford.nlp.simple;

namespace MathChatBot.Utilities
{
    public class NLPUtility
    {
        public static void ProcessText()
        {
            /*var jarRoot = @"stanford-corenlp-3.9.2-models\";
            var props = new java.util.Properties();
            props.setProperty("annotators", "tokenize, ssplit, pos, lemma, parse, ner,dcoref");
            props.setProperty("ner.useSUTime", "0");
            var curDir = Environment.CurrentDirectory;
            var modelsDirectory = curDir + "\\" + jarRoot + @"\edu\stanford\nlp\models";
            Directory.SetCurrentDirectory(jarRoot);

            // Loading POS Tagger
            var tagger = new MaxentTagger(modelsDirectory + @"\pos-tagger\english-left3words\english-left3words-distsim.tagger");

            var pipeline = new StanfordCoreNLP(props);

            var text = "Kosgi Santosh sent an email to Stanford University. He didn't get a reply.";

            // Annotation
            var annotation = new Annotation(text);
            pipeline.annotate(annotation);

            // Result - Pretty Print
            using (var stream = new ByteArrayOutputStream())
            {
                pipeline.prettyPrint(annotation, new PrintWriter(stream));
                System.Console.WriteLine(stream.toString());
                stream.close();
            }*/

            var jarRoot = @"C:\Users\japr\Desktop\stanford-corenlp-full-2018-10-05\stanford-corenlp-full-2018-10-05\stanford-corenlp-3.9.2-models\edu\stanford\nlp\models\ner";

            var classifiersDirecrory = jarRoot;

            // Loading 3 class classifier model
            var classifier = CRFClassifier.getClassifierNoExceptions(
                classifiersDirecrory + @"\english.all.3class.distsim.crf.ser.gz");

            var s1 = "Good afternoon Rajat Raina, how are you Functions?";
            System.Console.WriteLine("{0}\n", classifier.classifyToString(s1));

            var s2 = "I go to school at Stanford University, which is located in Denmark.";
            System.Console.WriteLine("{0}\n", classifier.classifyWithInlineXML(s2));

            System.Console.WriteLine("{0}\n", classifier.classifyToString(s2, "xml", true));

            GetNouns(s2);

            // Text for processing


            /*// Annotation pipeline configuration
            var props = new java.util.Properties();
            props.setProperty("annotators", "tokenize, ssplit, pos, lemma, ner, parse, dcoref");
            props.setProperty("ner.useSUTime", "0");

            // We should change current directory, so StanfordCoreNLP could find all the model files automatically
            var curDir = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(jarRoot);
            var pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(curDir);*/


        }

        public static HashSet<String> nounPhrases = new HashSet<String>();

        public static void GetNouns(string str)
        {
            var model = @"C:\Users\japr\Desktop\stanford-corenlp-full-2018-10-05\stanford-corenlp-full-2018-10-05\stanford-corenlp-3.9.2-models\edu\stanford\nlp\models\pos-tagger\english-left3words\english-left3words-distsim.tagger";

            // Loading POS Tagger
            var tagger = new MaxentTagger(model);

            // Text for tagging
            var text = "Why is a Part-Of-Speech Tagger (POS Tagger) a piece of software that reads text"
                       + "in some language and assigns parts of speech to each word (and other token),"
                       + " such as noun, verb, adjective, etc., although generally computational "
                       + "applications use more fine-grained POS tags like 'noun-plural'.";

            var sentences = MaxentTagger.tokenizeText(new java.io.StringReader(text)).toArray();
            foreach (ArrayList sentence in sentences)
            {
                var taggedSentence = tagger.tagSentence(sentence);
                System.Console.WriteLine(edu.stanford.nlp.ling.SentenceUtils.listToString(taggedSentence, false));
            }
        }

    }
}
