using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets;
public struct LockToken
{
    volatile int _token;
    volatile int _next;

    public bool IsLocked => _token != 0;

    public int Token => _token;

    public bool Lock(out int token)
    {
        var next = Interlocked.Increment(ref _next);
        token = Interlocked.CompareExchange(ref _token, next, 0);
        return token == 0; 
    }

    public async ValueTask<int> ForceLock()
    {
        int s = 100;
        int token;
        while (!Lock(out token))
        {
            await Task.Delay(s);
            s = checked(s << 1);
        }
        return token;
    }

    public bool Unlock(int token)
    {
        var token_ = Interlocked.CompareExchange(ref _token, 0, token);
        return token_ == token;
    }

    public void ForceUnlock(int token)
    {
        if (!Unlock(token)) throw new Exception();
    }
}
