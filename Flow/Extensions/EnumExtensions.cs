namespace Flow.Extensions
{
    //ToDo добавить тесты
    public static class EnumExtensions 
    {
        public static string FullName<TIndex>(this TIndex enumerator) where TIndex : struct
            => typeof(TIndex).Assembly.FullName.ToString() + enumerator.ToString();
    }
}
