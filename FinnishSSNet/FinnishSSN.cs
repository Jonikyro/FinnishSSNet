using System.Buffers;
using System.Diagnostics;

namespace FinnishSSNet;

public enum Gender
{
	Male,
	Female
}

[Serializable]
[DebuggerDisplay("{ToString(),nq}")]
#pragma warning disable S101 // Types should be named in PascalCase
public struct FinnishSSN : IEquatable<FinnishSSN>, IComparable<FinnishSSN>
#pragma warning restore S101 // Types should be named in PascalCase
{
	// ddmmyysrrrc
	private const int EXPECTED_LENGTH = 11;
	// ddmmyy
	private const int DATEPART_LENGTH = 6;
	// s
	private const int SEPARATOR_LENGTH = 1;
	// rrr
	private const int ROLLING_NUMBER_LENGTH = 3;
	// c
	private const int CHECKSUM_LENGTH = 1;

	private const string CHECKSUM_CHARS = "0123456789ABCDEFHJKLMNPRSTUVWXY";

	private static readonly SearchValues<char> s_digits = SearchValues.Create("0123456789");
	private static readonly SearchValues<char> s_separators = SearchValues.Create("+-YXWVUABCDEF");
	private static readonly SearchValues<char> s_checksumChars = SearchValues.Create(CHECKSUM_CHARS);
	private static readonly IDictionary<char, string> s_centuryMap = new Dictionary<char, string>
	 {
		  { '+', "18" },
		  { '-', "19" },
		  { 'Y', "19" },
		  { 'X', "19" },
		  { 'W', "19" },
		  { 'V', "19" },
		  { 'U', "19" },
		  { 'A', "20" },
		  { 'B', "20" },
		  { 'C', "20" },
		  { 'D', "20" },
		  { 'E', "20" },
		  { 'F', "20" }
	 };

	private readonly string _ssn;
	public readonly DateOnly DateOfBirth;
	public readonly Gender Gender;
	public readonly bool IsValid;

	private FinnishSSN(string ssn, Gender gender, DateOnly dateOfBirth)
	{
		this._ssn = ssn;
		this.DateOfBirth = dateOfBirth;
		this.Gender = gender;
		this.IsValid = true;
	}

	/// <summary>
	/// Parses a finnish social security number from a string.
	/// </summary>
	/// <exception cref="FormatException">When given SSN is not in correct format</exception>
	/// <exception cref="ArgumentNullException">When given SSN is null</exception>
	public static FinnishSSN Parse(string? ssn)
	{
		ArgumentNullException.ThrowIfNull(ssn);

		if (!IsCorrectFormat(ssn))
		{
			throw new FormatException($"Given SSN \"{ssn}\" was not in correct format (ddmmyysrrrc)");
		}

		if (!PassesChecksumCheck(ssn))
		{
			throw new FormatException("SSN did not pass the checksum check");
		}

		if (!TryParseDateOfBirth(ssn, out DateOnly dateOfBirth))
		{
			throw new FormatException("SSN contains no date of birth or it's invalid");
		}

		Gender gender = ParseGender(ssn);

		return new FinnishSSN(ssn, gender, dateOfBirth);
	}

	/// <summary>
	/// Tries to parse given finnish <paramref name="ssn"/>.
	/// </summary>
	public static bool TryParse(string? ssn, out FinnishSSN result)
	{
		result = default;

		if (
			!IsCorrectFormat(ssn)
			|| !PassesChecksumCheck(ssn)
			|| !TryParseDateOfBirth(ssn, out DateOnly dateOfBirth))
		{
			return false;
		}

		Gender gender = ParseGender(ssn);
		result = new FinnishSSN(ssn!, gender, dateOfBirth);
		return true;
	}

	/// <summary>
	/// Checks if given <paramref name="ssn"/> is valid finnish social security number.
	/// </summary>
	public static bool IsValidFinnishSSN(ReadOnlySpan<char> ssn)
	{
		return IsCorrectFormat(ssn) && PassesChecksumCheck(ssn) && TryParseDateOfBirth(ssn, out _);
	}

	private static bool IsCorrectFormat(ReadOnlySpan<char> ssn)
	{
		if (ssn.IsEmpty || ssn.Length != EXPECTED_LENGTH)
		{
			return false;
		}

		if (ssn[..DATEPART_LENGTH].ContainsAnyExcept(s_digits))
		{
			return false;
		}

		if (ssn.Slice(DATEPART_LENGTH, SEPARATOR_LENGTH).ContainsAnyExcept(s_separators))
		{
			return false;
		}

		if (ssn.Slice(DATEPART_LENGTH + SEPARATOR_LENGTH, ROLLING_NUMBER_LENGTH)
				  .ContainsAnyExcept(s_digits))
		{
			return false;
		}

		if (ssn.Slice(
				DATEPART_LENGTH + SEPARATOR_LENGTH + ROLLING_NUMBER_LENGTH, CHECKSUM_LENGTH)
					 .ContainsAnyExcept(s_checksumChars))
		{
			return false;
		}

		return true;
	}

	private static bool PassesChecksumCheck(ReadOnlySpan<char> ssn)
	{
		ReadOnlySpan<char> dateChars = ssn[..DATEPART_LENGTH];
		ReadOnlySpan<char> rollingNumberChars = ssn.Slice(DATEPART_LENGTH + SEPARATOR_LENGTH, ROLLING_NUMBER_LENGTH);

		ReadOnlySpan<char> checkNumberChars = string.Concat(dateChars, rollingNumberChars);

		if (!int.TryParse(checkNumberChars, out int checkNumber))
		{
			return false;
		}

		int checksumIndex = checkNumber % 31;

		if (checksumIndex > CHECKSUM_CHARS.Length - 1)
		{
			return false;
		}

		return CHECKSUM_CHARS[checksumIndex] == ssn[^CHECKSUM_LENGTH];
	}

	private static bool TryParseDateOfBirth(ReadOnlySpan<char> ssn, out DateOnly dateOfBirth)
	{
		dateOfBirth = default;

		ReadOnlySpan<char> dayChars = ssn[..2];

		if (!int.TryParse(dayChars, out int day))
		{
			return false;
		}

		if (day is < 1 or > 31)
		{
			return false;
		}

		ReadOnlySpan<char> monthChars = ssn.Slice(2, 2);

		if (!int.TryParse(monthChars, out int month))
		{
			return false;
		}

		if (month is < 1 or > 12)
		{
			return false;
		}

		int yearOfBirth = ParseYearOfBirth(ssn);

		// Checks for leap year as well
		if (day > DateTime.DaysInMonth(yearOfBirth, month))
		{
			return false;
		}

		dateOfBirth = new DateOnly(yearOfBirth, month, day);

		return true;
	}

	private static int ParseYearOfBirth(ReadOnlySpan<char> ssn)
	{
		ReadOnlySpan<char> lastTwoDigitsOfYear = ssn.Slice(4, 2);
		char separator = ssn.Slice(DATEPART_LENGTH, SEPARATOR_LENGTH)[0];

		string firstTwoDigitsOfYear = s_centuryMap[separator];

		ReadOnlySpan<char> yearChars = string.Concat(firstTwoDigitsOfYear, lastTwoDigitsOfYear);

		return int.Parse(yearChars);
	}

	private static Gender ParseGender(ReadOnlySpan<char> ssn)
	{
		ReadOnlySpan<char> rollingNumberChars = ssn.Slice(DATEPART_LENGTH + SEPARATOR_LENGTH, ROLLING_NUMBER_LENGTH);

		int rollingNumber = int.Parse(rollingNumberChars);

		return rollingNumber % 2 == 0 ? Gender.Female : Gender.Male;
	}

	public static implicit operator string(FinnishSSN ssn)
	{
		return ssn._ssn;
	}

	public static implicit operator FinnishSSN(string ssn)
	{
		return Parse(ssn);
	}

	public static bool operator ==(FinnishSSN left, FinnishSSN right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FinnishSSN left, FinnishSSN right)
	{
		return !(left == right);
	}

	public override readonly string ToString()
	{
		return this._ssn;
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is FinnishSSN ssn && this.Equals(ssn);
	}

	public readonly bool Equals(FinnishSSN other)
	{
		return this._ssn.Equals(other._ssn);
	}

	public override readonly int GetHashCode()
	{
		return this._ssn.GetHashCode();
	}

	public readonly int CompareTo(FinnishSSN other)
	{
		return this.DateOfBirth.CompareTo(other.DateOfBirth);
	}

	public static bool operator <(FinnishSSN left, FinnishSSN right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator <=(FinnishSSN left, FinnishSSN right)
	{
		return left.CompareTo(right) <= 0;
	}

	public static bool operator >(FinnishSSN left, FinnishSSN right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator >=(FinnishSSN left, FinnishSSN right)
	{
		return left.CompareTo(right) >= 0;
	}
}
