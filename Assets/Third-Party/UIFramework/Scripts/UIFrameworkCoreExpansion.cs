using System;

namespace Lowy.UIFramework
{
    public static class UIFrameworkCoreExpansion
    {
        public static int ValueToInt(this Enum e)
        {
            return Convert.ToInt32(e);
        }
    }
}