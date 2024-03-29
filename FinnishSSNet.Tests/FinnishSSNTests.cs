using System.Globalization;

namespace FinnishSSNet.Tests;

public class FinnishSSNTests
{
	[Fact]
	public void Parse_ShouldThrow_WhenNullStringIsGiven()
	{
		_ = Assert.Throws<ArgumentNullException>(() => FinnishSSN.Parse(null!));
	}

	[Theory]
	[InlineData("")]
	[InlineData("Lorem ipsum dolor sit amet")]
	[InlineData("010106A933")]
	[InlineData("010106A93333")]
	public void Parse_ShouldThrow_WhenSsnIsNotCorrectLength(string input)
	{
		FormatException ex = Assert.Throws<FormatException>(() => FinnishSSN.Parse(input));

		Assert.Equal($"Given SSN \"{input}\" was not in correct format (ddmmyysrrrc)", ex.Message);
	}

	[Theory]
	[InlineData("170665+989H")]
	[InlineData("190544-988J")]
	[InlineData("010905A9639")]
	[InlineData("010875A970A")]
	public void Parse_ShouldThrow_WhenSsnDoesNotPassChecksumCheck(string input)
	{
		FormatException ex = Assert.Throws<FormatException>(() => FinnishSSN.Parse(input));

		Assert.Equal("SSN did not pass the checksum check", ex.Message);
	}

	[Theory]
	[InlineData("290298-930L")]
	[InlineData("400298-930P")]
	public void Parse_ShouldThrow_WhenSsnBirthDateHasOutOfBoundsDay(string input)
	{
		FormatException ex = Assert.Throws<FormatException>(() => FinnishSSN.Parse(input));

		Assert.Equal("SSN contains no date of birth or it's invalid", ex.Message);
	}

	[Theory]
	[InlineData("281398-930U")]
	[InlineData("072055A964X")]
#pragma warning disable S4144 // Methods should not have identical implementations
	public void Parse_ShouldThrow_WhenSsnBirthDateHasOutOfBoundsMonth(string input)
#pragma warning restore S4144 // Methods should not have identical implementations
	{
		FormatException ex = Assert.Throws<FormatException>(() => FinnishSSN.Parse(input));

		Assert.Equal("SSN contains no date of birth or it's invalid", ex.Message);
	}

	[Theory]
	[InlineData("290224A975Y")]
	[InlineData("290296-982S")]
	public void Parse_ShouldParseCorrectly_WhenSsnBirthDateIsLeapYearDay(string input)
	{
		FinnishSSN ssn = FinnishSSN.Parse(input);
		Assert.Equal(ssn, input);
	}

	[Theory]
	[InlineData("081176+9177", "1876-11-08")]
	[InlineData("290220-994J", "1920-02-29")]
	[InlineData("010514A981X", "2014-05-01")]
	public void Parse_ShouldParseBirthDateCorrectly_WhenSsnIsValid(string input, string expected)
	{
		FinnishSSN ssn = FinnishSSN.Parse(input);
		Assert.Equal(ssn.DateOfBirth, DateOnly.Parse(expected, CultureInfo.InvariantCulture));
	}

	[Theory]
	[InlineData("160901+9350", Gender.Male)]
	[InlineData("030199+9265", Gender.Female)]
	[InlineData("111046-9035", Gender.Male)]
	[InlineData("311000-970B", Gender.Female)]
	[InlineData("010100A935L", Gender.Male)]
	[InlineData("311216A9061", Gender.Female)]
	public void Parse_ShouldParseGenderCorrectly(string input, Gender expected)
	{
		FinnishSSN ssn = FinnishSSN.Parse(input);
		Assert.Equal(ssn.Gender, expected);
	}

	[Theory]
	[InlineData("010100+928C", 1800)]
	[InlineData("311299+9927", 1899)]
	[InlineData("010100-907P", 1900)]
	[InlineData("081154X924S", 1954)]
	[InlineData("050602W963M", 1902)]
	[InlineData("311299V969F", 1999)]
	[InlineData("050892U941S", 1992)]
	[InlineData("010100A941T", 2000)]
	[InlineData("290204B918Y", 2004)]
	[InlineData("030912C9254", 2012)]
	[InlineData("080475D967X", 2075)]
	[InlineData("091091E937K", 2091)]
	[InlineData("011299F9107", 2099)]
	public void Parse_ShouldParseTheCenturyCorrectly(string input, int expectedYear)
	{
		FinnishSSN ssn = FinnishSSN.Parse(input);
		Assert.Equal(ssn.DateOfBirth.Year, expectedYear);
	}

	[Theory]
	[InlineData(null, false)]
	[InlineData("", false)]
	[InlineData("           ", false)]
	[InlineData("Lorem ipsum dolor sit amet", false)]
	[InlineData("123456ä123å", false)]
	[InlineData("1008911-945V", false)]
	[InlineData("181078+948B", false)]
	[InlineData("041275A956☢️", false)]
	[InlineData("290298-930L", false)]
	[InlineData("090561+936S", true)]
	[InlineData("230345-0051", true)]
	[InlineData("280207A661T", true)]
	[InlineData("290224A975Y", true)]
	public void IsValidFinnishSSN_ShouldValidateSSNCorrectly(string input, bool expected)
	{
		Assert.Equal(FinnishSSN.IsValidFinnishSSN(input), expected);
	}

	[Theory]
	[InlineData(null, false)]
	[InlineData("", false)]
	[InlineData("           ", false)]
	[InlineData("Lorem ipsum dolor sit amet", false)]
	[InlineData("123456ä123å", false)]
	[InlineData("1008911-945V", false)]
	[InlineData("181078+948B", false)]
	[InlineData("041275A956☢️", false)]
	[InlineData("290298-930L", false)]
	[InlineData("090561+936S", true)]
	[InlineData("230345-0051", true)]
	[InlineData("280207A661T", true)]
	[InlineData("290224A975Y", true)]
	public void TryParse_ShouldParseCorrectly(string input, bool expected)
	{
		bool result = FinnishSSN.TryParse(input, out FinnishSSN? ssn);

		Assert.Equal(result, expected);

		if (result)
		{
			Assert.Equal(ssn, input);
		}
	}
}