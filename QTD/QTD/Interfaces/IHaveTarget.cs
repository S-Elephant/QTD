using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QTD
{
    public interface IHaveTarget
    {
        ITargetable Target { get; set; }
    }
}
