using System;
using System.Collections.Generic;

namespace SramComparer.Services
{
    public interface ICmdLineParser
    {
        IOptions Parse(IReadOnlyList<string> args);
    }
}