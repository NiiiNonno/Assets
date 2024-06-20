using System;
using System.Collections.Generic;
using System.Text;

namespace Nonno.Assets.Collections;
public delegate T Parser<T>(ReadOnlySpan<char> @string);
