namespace Flow.Extensions
{
    public static class EnumExtensions 
    {
        public static string FullName<TIndex>(this TIndex enumerator) where TIndex : struct
            => $"{typeof(TIndex).FullName}.{enumerator}";
    }
}
