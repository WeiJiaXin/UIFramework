using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lowy.Bind
{
    internal class Reflection : IReflection
    {
        public object GetInstance(Type t, params object[] args)
        {
            var constructors =
                t.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var constructor in constructors)
            {
                var pars = constructor.GetParameters();
                if (pars.Length != args.Length)
                    continue;
                bool same = true;
                for (int i = 0; i < args.Length; i++)
                {
                    if (pars[i].ParameterType != args[i].GetType())
                    {
                        same = false;
                        break;
                    }
                }

                if (!same)
                    continue;
                object o = Activator.CreateInstance(t,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,args,null);
                return o;
            }
            throw new InjectException("没有找到与参数匹配的构造函数 Type："+t.Name);
        }

        public FieldInfo[] GetFieldByAttribute<T>(object obj)where  T:Attribute
        {
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            List<FieldInfo> resFields = new List<FieldInfo>();
            foreach (var info in fields)
            {
                if(info.GetCustomAttribute<T>()!=null)
                    resFields.Add(info);
            }

            return resFields.ToArray();
        }
    }
}