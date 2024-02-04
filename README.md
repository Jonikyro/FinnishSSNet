# FinnishSSNet

Finnish social security number (SSN) parser for .NET 8.

## Installation

TODO

## Usage

Parsing finnish SSN

```c#
FinnishSSN ssn = FinnishSSN.Parse("310885-903H");

// ssn.DateOfBirth -> 31.08.1985 (DateOnly)
// ssn.Gender -> Gender.Male (enum)
// ssn.ToString() -> "310885-903H"
// ssn.IsValid -> true
```

TryParsing finnish SSN

```c#
if (FinnishSSN.TryParse("310885-903H", out FinnishSSN ssn)) 
{
   ...
}
```

Implicit conversion (uses `FinnishSSN.Parse`)

```c#
void DoSomething(FinnishSSN ssn)
{
   ...
}

DoSomething("310885-903H");
```

Checking if valid

```c#
bool isValid = FinnishSSN.IsValidFinnishSSN("310885-903H");
```

## Performance

```
BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.3930/22H2/2022Update)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
Frequency: 14318180 Hz, Resolution: 69.8413 ns, Timer: HPET
.NET SDK 8.0.101
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2


| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Parse  | 60.19 ns | 0.345 ns | 0.323 ns |         - |
```

## Spec

TODO