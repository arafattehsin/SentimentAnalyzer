using System;

namespace SentimentAnalyzer.Models
{
    /// <summary>
    /// Enhanced sentiment analysis result with confidence scores and classification.
    /// </summary>
    public class SentimentResult
    {
        /// <summary>
        /// The analyzed text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The sentiment classification label (Positive, Neutral, or Negative).
        /// </summary>
        public SentimentLabel Label { get; }

        /// <summary>
        /// Raw score from the model (0.0 to 1.0). 
        /// Higher values indicate positive sentiment.
        /// </summary>
        public float Score { get; }

        /// <summary>
        /// Confidence level of the prediction (0.0 to 1.0).
        /// Calculated as distance from the neutral zone.
        /// </summary>
        public float Confidence { get; }

        /// <summary>
        /// Whether the sentiment is positive.
        /// </summary>
        public bool IsPositive => Label == SentimentLabel.Positive;

        /// <summary>
        /// Whether the sentiment is negative.
        /// </summary>
        public bool IsNegative => Label == SentimentLabel.Negative;

        /// <summary>
        /// Whether the sentiment is neutral.
        /// </summary>
        public bool IsNeutral => Label == SentimentLabel.Neutral;

        /// <summary>
        /// Legacy compatibility: Returns true for Positive, false for Negative/Neutral.
        /// </summary>
        [Obsolete("Use Label property instead for more accurate classification.")]
        public bool Prediction => Label == SentimentLabel.Positive;

        internal SentimentResult(string text, float score, SentimentLabel label, float confidence)
        {
            Text = text;
            Score = score;
            Label = label;
            Confidence = confidence;
        }

        /// <summary>
        /// Returns a string representation of the sentiment result.
        /// </summary>
        public override string ToString()
        {
            return $"{Label} (Score: {Score:F3}, Confidence: {Confidence:P1})";
        }
    }
}
