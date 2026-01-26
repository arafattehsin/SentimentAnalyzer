using BenchmarkDotNet.Running;
using SentimentAnalyzer.Samples.Benchmarks;

// Run all benchmarks
BenchmarkRunner.Run<SentimentBenchmarks>();
