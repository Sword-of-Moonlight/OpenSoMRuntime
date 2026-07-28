using System.Runtime.CompilerServices;
using System.Text;

/// <summary>
/// HashThis allows quick and easy hashing of strings and byte buffers
/// </summary>
public static class HashThis
{
	// Default 64-bit hash implementation is FNV-1a, based on the implementation here: https://github.com/jslicer/FNV-1a/blob/master/Fnv1a/Fnv1a64.cs
	public const ulong FNV1A_64_OFFSET = 0xCBF29CE484222325;
	public const ulong FNV1A_64_PRIME  = 0x100000001B3;

	/// <summary>
	/// BaseHash64 is a replacable root function for hashing our various types of data.
	/// </summary>
	/// <param name="buffer">The data to hash</param>
	/// <returns>A 64-bit hash depending on implementation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static ulong BaseHash64(byte[] buffer, ulong hash)
    {	
		// I don't give a shit about overflows...
		unchecked
        {
			foreach (byte b in buffer)
			{
				hash ^= b;
				hash *= FNV1A_64_PRIME;
			}
		}

		return hash;
	}

	/// <summary>
	/// Gets a 64-bit hash of a buffer
	/// </summary>
	public static ulong BytesTo64(byte[] buffer) =>
		BaseHash64(buffer, FNV1A_64_OFFSET);

	/// <summary>
	/// Accumulates a 64-bit hash of a buffer by extending a previous hash
	/// </summary>
	public static ulong BytesTo64(byte[] buffer, ulong prevHash = FNV1A_64_OFFSET) =>
		BaseHash64(buffer, prevHash);

	/// <summary>
	/// Gets a 64-bit hash of a string
	/// </summary>
	/// <param name="stringToHash">The string you wish to hash</param>
	/// <returns>The hash dummy</returns>
	public static ulong StringTo64(string stringToHash) =>
		BaseHash64(Encoding.UTF8.GetBytes(stringToHash), FNV1A_64_OFFSET);
}
