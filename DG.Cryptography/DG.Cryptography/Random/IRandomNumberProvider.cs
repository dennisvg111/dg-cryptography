namespace DG.Cryptography.Random
{
    /// <summary>
    /// Provides a way to generate random byte arrays.
    /// </summary>
    public interface IRandomNumberProvider
    {
        /// <summary>
        /// Generates an array of bytes with the given amount of randomly generated values
        /// </summary>
        /// <param name="count"></param>
        byte[] NextBytes(int count);
    }
}
