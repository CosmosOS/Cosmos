using System;
using Indy.IL2CPU.Plugs;

[Plug(Target = typeof(System.TimeSpan))]
class TimeSpanImpl
{
    #region Static Methods
    static public TimeSpanImpl Parse(String str)
    {
        //TODO Implement
        return new TimeSpanImpl(0);
    }
    static public Boolean TryParse(String str, out TimeSpanImpl ris)
    {
        //TODO Implement
        ris = new TimeSpanImpl(0);
        return false;
    }

    static public Boolean Equals(TimeSpanImpl tm1, TimeSpanImpl tm2)
    {
        return tm1.Equals(tm2);
    }
    static public Int32 Compare(TimeSpanImpl tm1, TimeSpanImpl tm2)
    {
        return tm1.CompareTo(tm2);
    }
    static public TimeSpanImpl FromDays(Double Days)
    {
        return new TimeSpanImpl((long)(Days * TicksPerDay));
    }
    static public TimeSpanImpl FromHours(Double Hours)
    {
        return new TimeSpanImpl((long)(Hours * TicksPerHour));
    }
    static public TimeSpanImpl FromMinutes(Double Minutes)
    {
        return new TimeSpanImpl((long)(Minutes * TicksPerMinute));
    }
    static public TimeSpanImpl FromSeconds(Double Seconds)
    {
        return new TimeSpanImpl((long)(Seconds * TicksPerSecond));
    }
    static public TimeSpanImpl FromMilliseconds(Double Milliseconds)
    {
        return new TimeSpanImpl((long)(Milliseconds * TicksPerMillisecond));
    }
    static public TimeSpanImpl FromTicks(long Ticks)
    {
        return new TimeSpanImpl(Ticks);
    }
    #endregion
    #region Costants
    public const long TicksPerDay = 864000000000;
        public const long TicksPerHour = 36000000000;
        public const long TicksPerMinute = 600000000;
        public const long TicksPerSecond = 10000000;
        public const long TicksPerMillisecond = 10000;
        public const long MaxValue = Int64.MaxValue;
        public const long MinValue = Int64.MinValue;
    #endregion
    #region Operators
    public static TimeSpanImpl operator +(TimeSpanImpl tm, TimeSpanImpl tm2)
    {
        return tm.Add(tm2);
    }

    public static TimeSpanImpl operator -(TimeSpanImpl tm, TimeSpanImpl tm2)
    {
        return tm.Substract(tm2);
    }

    public static Boolean operator >(TimeSpanImpl tm, TimeSpanImpl tm2)
    {
        return (tm.Ticks > tm2.Ticks);
    }

    public static Boolean operator >=(TimeSpanImpl tm, TimeSpanImpl tm2)
    {
        return (tm.Ticks >= tm2.Ticks);
    }

    public static Boolean operator <(TimeSpanImpl tm, TimeSpanImpl tm2)
    {
        return (tm.Ticks < tm2.Ticks);
    }

    public static Boolean operator <=(TimeSpanImpl tm, TimeSpanImpl tm2)
    {
        return (tm.Ticks <= tm2.Ticks);
    }

    public static Boolean operator ==(TimeSpanImpl tm, TimeSpanImpl tm2)
    {
        return (tm.Ticks == tm2.Ticks);
    }

    public static Boolean operator !=(TimeSpanImpl tm, TimeSpanImpl tm2)
    {
        return (tm.Ticks != tm2.Ticks);
    }

    public static TimeSpanImpl operator !(TimeSpanImpl tm)
    {
        return tm.Negate();
    }

    public static TimeSpanImpl operator +(TimeSpanImpl tm)
    {
        return tm;
    }
    #endregion
    #region Methods
    public TimeSpanImpl Add(TimeSpanImpl tm)
    {
        return new TimeSpanImpl(Ticks + tm.Ticks);
    }

    public TimeSpanImpl Substract(TimeSpanImpl tm)
    {
        return new TimeSpanImpl(Ticks - tm.Ticks);
    }

    public TimeSpanImpl Negate()
    {
        long it = Ticks - Ticks - Ticks;

        return new TimeSpanImpl(it);
    }

    public TimeSpanImpl Duration()
    {
        if (Ticks < 0)
            return Negate();
        else
            return new TimeSpanImpl(Ticks);
    }

    public bool Equals(TimeSpanImpl tm)
    {
        //TODO Implement
        return false;
    }

    public Int32 CompareTo(TimeSpanImpl tm2)
    {
        //TODO Implement
        return -1;
    }
    public override String ToString()
    {
        //TODO Implement
        return "";
    }

    public override bool Equals(object obj)
    {
        //TODO Implement
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        //TODO Implement
        return base.GetHashCode();
    }

    #endregion
    #region Costructors
    public TimeSpanImpl()
    {
        Ticks = 0;
    }

    public TimeSpanImpl(long Ticks)
    {
        this.Ticks = Ticks;
    }

    public TimeSpanImpl(Int32 Hours, Int32 Minutes, Int32 Seconds)
    {
        this.Ticks = (Hours * 36000000000) + (Minutes * 600000000) + (Seconds * 10000000);
    }

    public TimeSpanImpl(Int32 Days, Int32 Hours, Int32 Minutes, Int32 Seconds)
    {
        this.Ticks = (Days * 864000000000) + (Hours * 36000000000) + (Minutes * 600000000) + (Seconds * 10000000);
    }

    public TimeSpanImpl(Int32 Days, Int32 Hours, Int32 Minutes, Int32 Seconds, Int32 Milliseconds)
    {
        this.Ticks = (Days * 864000000000) + (Hours * 36000000000) + (Minutes * 600000000) + (Seconds * 10000000) + (Milliseconds * 10000);
    }
    #endregion
    #region Fields
    public Int32 Milliseconds
    {
        get
        {
            Double temp = (Ticks - (Days * TicksPerDay) - (Hours * TicksPerHour) - (Minutes * TicksPerMinute) - (Seconds * TicksPerSecond));
            return (Int32)(temp / TicksPerMillisecond);
        }
    }
    public Int32 Seconds
    {
        get
        {
            Double temp = (Ticks - (Days * TicksPerDay) - (Hours * TicksPerHour) - (Minutes * TicksPerMinute));
            return (Int32)(temp / TicksPerSecond);
        }
    }
    public Int32 Minutes
    {
        get
        {
            Double temp = (Ticks - (Days * TicksPerDay) - (Hours * TicksPerHour));
            return (Int32)(temp / TicksPerMinute);
        }
    }
    public Int32 Hours
    {
        get
        {
            Double temp = (Ticks - (Days * TicksPerDay));
            return (Int32)(temp / TicksPerHour);
        }
    }
    public Int32 Days
    {
        get
        {
            return (Int32)(Ticks / TicksPerDay);
        }
    }

    public Double TotalMilliseconds
    {
        get
        {
            return (Ticks / TicksPerMillisecond);
        }
    }
    public Double TotalSeconds
    {
        get
        {
            return (Ticks / TicksPerSecond);
        }
    }
    public Double TotalMinutes
    {
        get
        {
            return (Ticks / TicksPerMinute);
        }
    }
    public Double TotalHours
    {
        get
        {
            return (Ticks / TicksPerHour);
        }
    }
    public Double TotalDays
    {
        get
        {
            return (Double)(Ticks / TicksPerDay);
        }
    }

    public long Ticks { get; private set; }

    public TimeSpanImpl Zero
    {
        get { return new TimeSpanImpl(0); }
    }
    #endregion
}