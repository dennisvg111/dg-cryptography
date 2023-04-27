namespace DG.Cryptography.Random
{
    /// <summary>
    /// This interface provides a way to generate integers between 0 and a given value.
    /// </summary>
    public interface IRandomNumberProvider
    {
        /// <summary>
        /// Generates an integer between 0 inclusive and <paramref name="maxValueExclusive"/> exclusive.
        /// </summary>
        /// <param name="maxValueExclusive"></param>
        /// <returns></returns>
        int Next(int maxValueExclusive);
    }
}
