using System.Runtime.CompilerServices;

namespace Nonno.Assets;

public struct SimpleSpinLock
{
	const int TRUE = 1;
	const int FALSE = 0;
	volatile int _isLocked;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Enter()
	{
		if (Interlocked.Or(ref _isLocked, TRUE) == TRUE)
		{
			Spin();
		}
		return;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Exit() => _isLocked = FALSE;

	[MethodImpl(MethodImplOptions.NoInlining)]
	void Spin()
	{
		var sw = new SpinWait();
		do
		{
			sw.SpinOnce();
		}
		while (Interlocked.Or(ref _isLocked, TRUE) == TRUE);
	}
}
