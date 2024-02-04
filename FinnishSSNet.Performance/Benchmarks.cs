using BenchmarkDotNet.Attributes;

namespace FinnishSSNet.Performance;

[MemoryDiagnoser]
public class Benchmarks
{
	private readonly string _ssn = "010106A981M";

	[Benchmark]
	public void Parse()
	{
		FinnishSSN.Parse(this._ssn);
	}
}