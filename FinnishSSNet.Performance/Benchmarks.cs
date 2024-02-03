using BenchmarkDotNet.Attributes;

namespace FinnishSSNet.Performance;

[MemoryDiagnoser]
public class Benchmarks
{
	private readonly string _pin = "010106A981M";


	[Benchmark(Description = "Parse_Pin")]
	public FinnishSSN Parse_Pin()
	{
		return FinnishSSN.Parse(this._pin);
	}
}