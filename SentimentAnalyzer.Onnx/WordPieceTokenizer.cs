using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SentimentAnalyzer.Onnx
{
    /// <summary>
    /// WordPiece tokenizer for TinyBERT model.
    /// </summary>
    internal sealed class WordPieceTokenizer
    {
        private readonly Dictionary<string, int> _vocab;
        private readonly int _unkTokenId;
        private readonly int _clsTokenId;
        private readonly int _sepTokenId;
        private readonly int _padTokenId;

        public WordPieceTokenizer()
        {
            _vocab = LoadVocabulary();
            _unkTokenId = _vocab.TryGetValue("[UNK]", out var unk) ? unk : 100;
            _clsTokenId = _vocab.TryGetValue("[CLS]", out var cls) ? cls : 101;
            _sepTokenId = _vocab.TryGetValue("[SEP]", out var sep) ? sep : 102;
            _padTokenId = _vocab.TryGetValue("[PAD]", out var pad) ? pad : 0;
        }

        private Dictionary<string, int> LoadVocabulary()
        {
            var vocab = new Dictionary<string, int>();

            // Try to load from extracted file first
            var vocabPath = Path.Combine(Path.GetTempPath(), "SentimentAnalyzer", "TinyBert", "vocab.txt");

            if (File.Exists(vocabPath))
            {
                var lines = File.ReadAllLines(vocabPath);
                for (int i = 0; i < lines.Length; i++)
                {
                    vocab[lines[i]] = i;
                }
            }
            else
            {
                // Fall back to embedded resource
                var assembly = typeof(WordPieceTokenizer).Assembly;
                using var stream = assembly.GetManifestResourceStream("vocab.txt");

                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    int index = 0;
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        vocab[line] = index++;
                    }
                }
                else
                {
                    // Create minimal vocab for basic functionality
                    vocab["[PAD]"] = 0;
                    vocab["[UNK]"] = 100;
                    vocab["[CLS]"] = 101;
                    vocab["[SEP]"] = 102;
                }
            }

            return vocab;
        }

        /// <summary>
        /// Encode text into token IDs and attention mask.
        /// </summary>
        public (int[] inputIds, int[] attentionMask) Encode(string text, int maxLength = 512)
        {
            var tokens = Tokenize(text);

            // Add special tokens [CLS] ... [SEP]
            var inputIds = new List<int> { _clsTokenId };

            foreach (var token in tokens)
            {
                if (inputIds.Count >= maxLength - 1)
                    break;

                inputIds.Add(_vocab.TryGetValue(token, out var id) ? id : _unkTokenId);
            }

            inputIds.Add(_sepTokenId);

            // Create attention mask (1 for real tokens, 0 for padding)
            var attentionMask = Enumerable.Repeat(1, inputIds.Count).ToList();

            // Pad to max length
            while (inputIds.Count < maxLength)
            {
                inputIds.Add(_padTokenId);
                attentionMask.Add(0);
            }

            return (inputIds.ToArray(), attentionMask.ToArray());
        }

        private List<string> Tokenize(string text)
        {
            var tokens = new List<string>();
            var words = text.ToLowerInvariant()
                .Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                // Simple WordPiece: try whole word first, then subwords
                var cleanWord = CleanWord(word);
                if (string.IsNullOrEmpty(cleanWord))
                    continue;

                if (_vocab.ContainsKey(cleanWord))
                {
                    tokens.Add(cleanWord);
                }
                else
                {
                    // Try to break into subwords
                    var subTokens = TokenizeWord(cleanWord);
                    tokens.AddRange(subTokens);
                }
            }

            return tokens;
        }

        private string CleanWord(string word)
        {
            // Remove punctuation from edges, keep alphanumeric
            var chars = new List<char>();
            foreach (var c in word)
            {
                if (char.IsLetterOrDigit(c))
                    chars.Add(c);
            }
            return new string(chars.ToArray());
        }

        private List<string> TokenizeWord(string word)
        {
            var tokens = new List<string>();
            int start = 0;

            while (start < word.Length)
            {
                int end = word.Length;
                string? currentToken = null;

                while (start < end)
                {
                    var substr = word.Substring(start, end - start);
                    var candidate = start == 0 ? substr : "##" + substr;

                    if (_vocab.ContainsKey(candidate))
                    {
                        currentToken = candidate;
                        break;
                    }
                    end--;
                }

                if (currentToken == null)
                {
                    tokens.Add("[UNK]");
                    start++;
                }
                else
                {
                    tokens.Add(currentToken);
                    start = end;
                }
            }

            return tokens;
        }
    }
}
