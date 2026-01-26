using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SentimentAnalyzer.Onnx.Multilingual
{
    /// <summary>
    /// Interface for text tokenization.
    /// </summary>
    public interface ITokenizer
    {
        /// <summary>
        /// Encodes text into token IDs and attention mask.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="maxLength">Maximum sequence length.</param>
        /// <returns>Tuple of (input_ids, attention_mask).</returns>
        (int[] InputIds, int[] AttentionMask) Encode(string text, int maxLength = 512);
    }

    /// <summary>
    /// Simple WordPiece tokenizer for BERT-based models.
    /// This is a simplified implementation - for production, consider using a full tokenizer library.
    /// </summary>
    public sealed class WordPieceTokenizer : ITokenizer
    {
        // Special tokens
        private const int ClsTokenId = 101;  // [CLS]
        private const int SepTokenId = 102;  // [SEP]
        private const int PadTokenId = 0;    // [PAD]
        private const int UnkTokenId = 100;  // [UNK]

        private readonly Dictionary<string, int> _vocab;

        /// <summary>
        /// Creates a new WordPiece tokenizer with basic vocabulary.
        /// </summary>
        public WordPieceTokenizer()
        {
            // Initialize with a basic vocabulary mapping
            // In production, this would load from a vocab.txt file
            _vocab = InitializeBasicVocab();
        }

        /// <summary>
        /// Creates a new WordPiece tokenizer with a vocabulary file.
        /// </summary>
        /// <param name="vocabPath">Path to the vocab.txt file.</param>
        public WordPieceTokenizer(string vocabPath)
        {
            if (!File.Exists(vocabPath))
                throw new FileNotFoundException($"Vocabulary file not found: {vocabPath}", vocabPath);

            _vocab = LoadVocab(vocabPath);
        }

        /// <inheritdoc/>
        public (int[] InputIds, int[] AttentionMask) Encode(string text, int maxLength = 512)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            // Tokenize
            var tokens = Tokenize(text);

            // Truncate if necessary (leave room for [CLS] and [SEP])
            if (tokens.Count > maxLength - 2)
            {
                tokens = tokens.Take(maxLength - 2).ToList();
            }

            // Add special tokens
            var inputIds = new List<int> { ClsTokenId };
            inputIds.AddRange(tokens);
            inputIds.Add(SepTokenId);

            // Create attention mask (1 for real tokens, 0 for padding)
            var attentionMask = Enumerable.Repeat(1, inputIds.Count).ToList();

            // Pad to maxLength if needed
            while (inputIds.Count < maxLength)
            {
                inputIds.Add(PadTokenId);
                attentionMask.Add(0);
            }

            return (inputIds.ToArray(), attentionMask.ToArray());
        }

        private List<int> Tokenize(string text)
        {
            var tokens = new List<int>();

            // Basic tokenization: split on whitespace/punctuation
            // NOTE: do_lower_case=false in tokenizer_config.json - model is case-sensitive!
            var words = text
                .Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}' },
                       StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                // Try to find the word in vocabulary
                if (_vocab.TryGetValue(word, out var tokenId))
                {
                    tokens.Add(tokenId);
                }
                else
                {
                    // WordPiece: try to break into subwords
                    var subTokens = WordPieceTokenize(word);
                    tokens.AddRange(subTokens);
                }
            }

            return tokens;
        }

        private List<int> WordPieceTokenize(string word)
        {
            var tokens = new List<int>();
            var start = 0;

            while (start < word.Length)
            {
                var end = word.Length;
                var found = false;
                string? subWord = null;

                while (start < end)
                {
                    subWord = word.Substring(start, end - start);
                    if (start > 0)
                    {
                        subWord = "##" + subWord;
                    }

                    if (_vocab.TryGetValue(subWord, out var tokenId))
                    {
                        tokens.Add(tokenId);
                        found = true;
                        break;
                    }

                    end--;
                }

                if (!found)
                {
                    // Unknown token
                    tokens.Add(UnkTokenId);
                    start++;
                }
                else
                {
                    start = start > 0 ? start + (end - start) : end;
                }
            }

            return tokens;
        }

        private static Dictionary<string, int> InitializeBasicVocab()
        {
            // This is a minimal vocabulary for basic functionality
            // A full implementation would load the complete BERT vocabulary
            var vocab = new Dictionary<string, int>
            {
                ["[PAD]"] = 0,
                ["[UNK]"] = 100,
                ["[CLS]"] = 101,
                ["[SEP]"] = 102,
                ["[MASK]"] = 103,

                // Common words (this would be much larger in production)
                ["the"] = 1996,
                ["a"] = 1037,
                ["is"] = 2003,
                ["was"] = 2001,
                ["are"] = 2024,
                ["were"] = 2020,
                ["it"] = 2009,
                ["this"] = 2023,
                ["that"] = 2008,
                ["i"] = 1045,
                ["you"] = 2017,
                ["he"] = 2002,
                ["she"] = 2016,
                ["we"] = 2057,
                ["they"] = 2027,

                // Sentiment words
                ["good"] = 2204,
                ["great"] = 2307,
                ["excellent"] = 6581,
                ["amazing"] = 6429,
                ["wonderful"] = 6919,
                ["fantastic"] = 10392,
                ["love"] = 2293,
                ["loved"] = 3866,
                ["like"] = 2066,
                ["best"] = 2190,
                ["awesome"] = 12476,
                ["perfect"] = 3819,
                ["beautiful"] = 3376,
                ["happy"] = 3407,

                ["bad"] = 2919,
                ["terrible"] = 6659,
                ["awful"] = 9643,
                ["horrible"] = 9202,
                ["worst"] = 5765,
                ["hate"] = 5765,
                ["hated"] = 6283,
                ["poor"] = 3532,
                ["disappointing"] = 13318,
                ["disappointed"] = 9364,
                ["boring"] = 11771,
                ["waste"] = 5765,
                ["sad"] = 6874,
                ["angry"] = 4954,

                ["ok"] = 7929,
                ["okay"] = 4957,
                ["fine"] = 2986,
                ["average"] = 3903,
                ["mediocre"] = 24168,
                ["neutral"] = 8699,

                // Common subwords
                ["##s"] = 2015,
                ["##ed"] = 2098,
                ["##ing"] = 2075,
                ["##ly"] = 2135,
                ["##er"] = 2099,
                ["##est"] = 4355,
                ["##tion"] = 3508,
                ["##ment"] = 3672,
                ["##ful"] = 3993,
                ["##less"] = 3238,
                ["##able"] = 3085,
                ["##ive"] = 3512,
            };

            return vocab;
        }

        private static Dictionary<string, int> LoadVocab(string path)
        {
            var vocab = new Dictionary<string, int>();
            var lines = File.ReadAllLines(path);

            for (int i = 0; i < lines.Length; i++)
            {
                var token = lines[i].Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    vocab[token] = i;
                }
            }

            return vocab;
        }
    }
}
