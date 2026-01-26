using System;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace SentimentAnalyzer.Tests
{
    public class Benchmarks
    {
        private readonly ITestOutputHelper output;

        // Inline test data - no external file needed
        private static readonly (string Text, bool IsPositive)[] TestReviews = new[]
        {
            // Positive reviews
            ("This movie was absolutely fantastic! The acting was superb and the story kept me engaged throughout.", true),
            ("I loved every minute of this film. Highly recommended for everyone!", true),
            ("An incredible cinematic experience. The director did an amazing job.", true),
            ("Best movie I've seen this year. The performances were outstanding.", true),
            ("A masterpiece of storytelling. I was moved to tears by the ending.", true),
            ("Brilliant acting and a compelling narrative. Five stars!", true),
            ("This exceeded all my expectations. Truly wonderful entertainment.", true),
            ("A delightful film with great characters and beautiful cinematography.", true),
            ("I can't say enough good things about this movie. It's perfect.", true),
            ("Exceptional in every way. The cast delivered phenomenal performances.", true),
            ("A heartwarming story that will stay with you long after watching.", true),
            ("One of the best films I've ever seen. Absolutely stunning.", true),
            ("The perfect blend of action, drama, and humor. Loved it!", true),
            ("A triumph of filmmaking. Every scene was beautifully crafted.", true),
            ("I was completely captivated from start to finish. Amazing!", true),
            ("This movie restored my faith in cinema. Truly excellent work.", true),
            ("A must-watch for any film lover. Incredible storytelling.", true),
            ("The performances were so authentic and moving. Wonderful film.", true),
            ("I laughed, I cried, I was thoroughly entertained. Great movie!", true),
            ("A beautiful and touching story told with skill and heart.", true),
            ("Outstanding direction and brilliant screenplay. Highly recommend.", true),
            ("This film is a gem. Every aspect was perfectly executed.", true),
            ("An unforgettable experience. One of the year's best films.", true),
            ("The chemistry between the leads was electric. Fantastic movie!", true),
            ("A powerful and inspiring film that everyone should see.", true),
            
            // Negative reviews
            ("This was a complete waste of time. Terrible acting and boring plot.", false),
            ("I couldn't even finish watching this disaster of a movie.", false),
            ("Absolutely awful. The worst film I've seen in years.", false),
            ("Save your money and skip this one. It's really bad.", false),
            ("The acting was wooden and the story made no sense at all.", false),
            ("A disappointing mess from start to finish. Very poor quality.", false),
            ("I want my two hours back. This movie was painfully bad.", false),
            ("Boring, predictable, and poorly executed. Do not recommend.", false),
            ("The dialogue was cringeworthy and the plot was ridiculous.", false),
            ("A total failure in every aspect. Avoid at all costs.", false),
            ("I've never been so bored watching a movie. Terrible.", false),
            ("The worst movie I've ever had the misfortune of watching.", false),
            ("Completely unwatchable. How did this even get made?", false),
            ("A tedious and uninteresting film with no redeeming qualities.", false),
            ("The plot holes were enormous and the acting was atrocious.", false),
            ("I regret wasting my evening on this garbage.", false),
            ("Painfully slow and utterly forgettable. A real letdown.", false),
            ("The script was terrible and the direction was even worse.", false),
            ("An insult to the audience's intelligence. Truly awful.", false),
            ("I fell asleep halfway through. That says it all.", false),
            ("Dreadful performances and a nonsensical storyline. Horrible.", false),
            ("This movie is an embarrassment to the film industry.", false),
            ("A colossal waste of talent and resources. Very disappointing.", false),
            ("Nothing worked in this film. It was a complete disaster.", false),
            ("One of the worst movies ever made. Absolutely terrible.", false)
        };

        public Benchmarks(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Benchmark with sample movie reviews.
        /// Tests 50 reviews (25 positive, 25 negative).
        /// </summary>
        [Fact]
        public void Benchmark_Sample_Reviews()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            int correct = 0;
            foreach (var (text, isPositive) in TestReviews)
            {
                var prediction = Sentiments.Predict(text);
                if (prediction.Prediction == isPositive)
                    correct++;
            }

            stopWatch.Stop();

            var accuracy = (double)correct / TestReviews.Length * 100;
            output.WriteLine($"Reviews analyzed: {TestReviews.Length}");
            output.WriteLine($"Duration: {stopWatch.ElapsedMilliseconds}ms");
            output.WriteLine($"Accuracy: {accuracy:F1}%");
            output.WriteLine($"Avg per review: {(double)stopWatch.ElapsedMilliseconds / TestReviews.Length:F2}ms");

            Assert.True(accuracy > 70, $"Accuracy {accuracy}% is below 70% threshold");
        }

        /// <summary>
        /// Benchmark with 1000 iterations for performance testing.
        /// </summary>
        [Fact]
        public void Benchmark_1000_Iterations()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // Run 20 iterations of all 50 reviews = 1000 predictions
            for (int iter = 0; iter < 20; iter++)
            {
                foreach (var (text, _) in TestReviews)
                {
                    Sentiments.Predict(text);
                }
            }

            stopWatch.Stop();

            var totalPredictions = 20 * TestReviews.Length;
            output.WriteLine($"Total predictions: {totalPredictions}");
            output.WriteLine($"Duration: {stopWatch.ElapsedMilliseconds}ms");
            output.WriteLine($"Avg per prediction: {(double)stopWatch.ElapsedMilliseconds / totalPredictions:F3}ms");

            Assert.True(stopWatch.ElapsedMilliseconds > 0);
        }
    }
}
