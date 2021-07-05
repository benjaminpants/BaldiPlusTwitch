using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace StolenYetHelpfulCode
{
    public static class CodeThatIsntMine
    {
        public static object InvokeMethod<T>(this T obj, string methodName, params object[] args) //thank you owen james: https://stackoverflow.com/users/2736798/owen-james
        {
            var type = typeof(T);
            var method = type.GetTypeInfo().GetDeclaredMethod(methodName);
            return method.Invoke(obj, args);
        }

    }
}
