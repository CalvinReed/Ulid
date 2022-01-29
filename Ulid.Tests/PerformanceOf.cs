using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace CalvinReed.Tests;

public class PerformanceOf
{
    private readonly ITestOutputHelper output;

    public PerformanceOf(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void MonotonicGeneration()
    {
        const int count = 10_000_000;
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < count; i++)
        {
            Ulid.Create();
        }

        stopwatch.Stop();
        output.WriteLine($"{count / stopwatch.ElapsedMilliseconds} per ms");
    }

    [Fact]
    public void RandomGeneration()
    {
        const int count = 10_000_000;
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < count; i++)
        {
            Ulid.Create(DateTime.UtcNow);
        }

        stopwatch.Stop();
        output.WriteLine($"{count / stopwatch.ElapsedMilliseconds} per ms");
    }

    [Fact]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public void Parsing()
    {
        const int count = 1_000_000;
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < count; i++)
        {
            Ulid.Parse("01ARZ3NDEKTSV4RRFFQ69G5FAV");
        }

        stopwatch.Stop();
        output.WriteLine($"{count / stopwatch.ElapsedMilliseconds} per ms");
    }

    [Fact]
    public void Hashing()
    {
        var ids = new Ulid[10_000_000];
        for (var i = 0; i < ids.Length; i++)
        {
            ids[i] = Ulid.Create();
        }

        var hashes = new int[ids.Length];
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < ids.Length; i++)
        {
            hashes[i] = ids[i].GetHashCode();
        }

        stopwatch.Stop();
        output.WriteLine($"{ids.Length / stopwatch.ElapsedMilliseconds} per ms");
        output.WriteLine($"{hashes.Distinct().Count()} / {ids.Length} distinct hashes");
    }
}
