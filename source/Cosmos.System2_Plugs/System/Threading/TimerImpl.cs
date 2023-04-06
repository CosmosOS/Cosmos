#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable IDE0060 // Remove unused parameter

using static Cosmos.HAL.Global;
using static Cosmos.HAL.PIT;
using IL2CPU.API.Attribs;

namespace Cosmos.System2_Plugs.System.Threading
{
	[Plug(Target = typeof(Timer))]
	public static class TimerImpl
	{
		public static void Ctor(Timer This, TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
		{
			Ctor(This, callback, state, (long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);
		}

		public static void Ctor(Timer This, TimerCallback callback, object? state, long dueTime, long period)
		{
			_CoreTimer = new(() => { callback.Invoke(state); }, (ulong)(dueTime * 1000000), true);

			// Run time config.
			Change(dueTime, period);
		}

		public static void Ctor(Timer This, TimerCallback callback, object? state, uint dueTime, uint period)
		{
			Ctor(This, callback, state, (long)dueTime, period);
		}

		public static void Ctor(Timer This, TimerCallback callback, object? state, int dueTime, int period)
		{
			Ctor(This, callback, state, (long)dueTime, period);
		}

		public static void Ctor(Timer This, TimerCallback callback)
		{
			Ctor(This, callback, null, (long)Timeout.Infinite, Timeout.Infinite);
		}

		#region Methods

		public static void Change(TimeSpan dueTime, TimeSpan period)
		{
			Change((long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);
		}

		public static void Change(long dueTime, long period)
		{
			// Check for issues.
			if (_CoreTimer == null)
			{
				throw new NotImplementedException($"An implementation issue has occurred with {typeof(Timer).FullName}!");
			}

			if (dueTime != Timeout.Infinite && period != Timeout.Infinite && _CoreTimer.TimerID == -1)
			{
				PIT.RegisterTimer(_CoreTimer);
			}
			if ((dueTime == Timeout.Infinite || period == Timeout.Infinite) && _CoreTimer.TimerID != -1)
			{
				PIT.UnregisterTimer(_CoreTimer.TimerID);
			}

			_CoreTimer.NanosecondsTimeout = (ulong)(dueTime * 1000000);
		}

		public static void Change(uint dueTime, uint period)
		{
			Change((long)dueTime, period);
		}

		public static void Change(int dueTime, int period)
		{
			Change((long)dueTime, period);
		}

		public static void Dispose()
		{
			// Check for issues.
			if (_CoreTimer == null)
			{
				throw new NotImplementedException($"An implementation issue has occurred with {typeof(Timer).FullName}!");
			}

			_CoreTimer.Dispose();
		}

        #endregion

        #region Fields

        private static PITTimer? _CoreTimer;
        
        #endregion
    }
}
