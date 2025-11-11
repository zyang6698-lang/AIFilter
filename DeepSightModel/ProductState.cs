using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSightModel
{
    public enum ProductState
    {
        // 0: AVI OK
        // 1: AVI NG 但过滤后OK
        // 2: AVI NG 过滤后仍NG
        // 3: AVI NG 未过滤
        AVIOK=0,
        AVINGFilteredOK=1,
        AVINGFilteredNG=2,
        AVINGUnfiltered=3
    }
}
