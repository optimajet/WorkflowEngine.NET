using System;
using System.Globalization;


namespace Common.Utils
{
    public static class StringToNullableConverter
    {
        public static int? GetPositiveInt(string stringValue)
        {
            int? value = null;

            if (stringValue == null)
                return null;
            try
            {
                value = Convert.ToInt32(stringValue, CultureInfo.CurrentCulture);
                if (value < 0)
                    value = null;
            }
            catch (Exception)
            {
            }
            
            return value;
        }

        public static int? GetInt(string stringValue)
        {
            int? value = null;

            try
            {
                value = Convert.ToInt32(stringValue, CultureInfo.CurrentCulture);
            }
            catch (Exception)
            {
            }

            return value;
        }

        public static long? GetPositiveLong(string stringValue)
        {
            long? value = null;

            try
            {
                value = Convert.ToInt64(stringValue, CultureInfo.CurrentCulture);
                if (value < 0)
                    value = null;
            }
            catch (FormatException)
            {
            }
            return value;
        }

        public static decimal? GetPositiveDecimal(string stringValue)
        {
            decimal? value = null;

            try
            {
                value = Convert.ToDecimal(stringValue, CultureInfo.CurrentCulture);
                if (value < 0)
                    value = null;
            }
            catch (FormatException)
            {
            }
            return value;
        }

        public static short? GetPositiveShort(string stringValue)
        {
            short? value = null;

            try
            {
                value = Convert.ToInt16(stringValue, CultureInfo.CurrentCulture);
                if (value < 0)
                    value = null;
            }
            catch (FormatException)
            {
            }
            return value;
        }

        public static DateTime? GetDateTimeForDbServer (string stringValue)
        {
            DateTime? value = null;

            try
            {
                value = Convert.ToDateTime(stringValue, CultureInfo.CurrentCulture);
                //TODO Минимальная СКУЭЛЬ дата
                //if (value < )
                //    value = null;
            }
            catch (FormatException)
            {
            }
            return value;
        }

        public static Guid? GetGuid(string stringValue)
        {
            Guid? value = null;

            try
            {
                value = new Guid(stringValue);
            }
            catch (Exception)
            {
            }
            return value;
        }
    }
}
