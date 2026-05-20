using System;

namespace Data.Repository.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime ToUtc(this DateTime source)
        {
            if (source.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(source, DateTimeKind.Utc);

            return source;
        }
    }
}
