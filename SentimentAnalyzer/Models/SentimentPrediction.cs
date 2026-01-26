
using Microsoft.ML.Data;

namespace SentimentAnalyzer.Models
{
    public class SentimentPrediction
    {
        // ColumnName attribute is used to change the column name from
        // its default value, which is the name of the field.
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        /// <summary>
        /// Raw score from the model. May be outside 0-1 range.
        /// Use Probability for a normalized 0-1 value.
        /// </summary>
        public float Score { get; set; }

        /// <summary>
        /// Probability value (0-1) derived from the score.
        /// </summary>
        public float Probability { get; set; }
    }
}