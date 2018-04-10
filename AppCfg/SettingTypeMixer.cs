/**
 * Rewrite based on https://blog.iamnguele.com/2016/11/20/dynamic-interface-implementation-runtime/
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters.Binary;

namespace AppCfg
{
    internal static class SettingTypeMixer<T>
    {
        private static readonly BindingFlags visibilityFlags = BindingFlags.Public | BindingFlags.Instance;

        private static Dictionary<string, OptionAttribute> attributeBank = new Dictionary<string, OptionAttribute>();
        internal static K ExtendWith<K>()
        {
            var assemblyName = new Guid().ToString();

            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule("Module");
            var typeBuilder = module.DefineType(typeof(T).Name + "_" + typeof(K).Name, TypeAttributes.Public, typeof(T));
            var fieldsList = new List<string>();

            typeBuilder.AddInterfaceImplementation(typeof(K));

            foreach (var v in typeof(K).GetProperties())
            {
                attributeBank.Add(v.Name, v.GetCustomAttribute<OptionAttribute>());

                fieldsList.Add(v.Name);

                var field = typeBuilder.DefineField("_f_" + v.Name.ToLower(), v.PropertyType, FieldAttributes.Private);
                var propertyBuilder = typeBuilder.DefineProperty(v.Name, PropertyAttributes.None, v.PropertyType, new Type[0]);
                var getter = typeBuilder.DefineMethod("_get_" + v.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, v.PropertyType, new Type[0]);
                var setter = typeBuilder.DefineMethod("_set_" + v.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, null, new Type[] { v.PropertyType });

                var getGenerator = getter.GetILGenerator();
                var setGenerator = setter.GetILGenerator();

                getGenerator.Emit(OpCodes.Ldarg_0);
                getGenerator.Emit(OpCodes.Ldfld, field);
                getGenerator.Emit(OpCodes.Ret);

                setGenerator.Emit(OpCodes.Ldarg_0);
                setGenerator.Emit(OpCodes.Ldarg_1);
                setGenerator.Emit(OpCodes.Stfld, field);
                setGenerator.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getter);
                propertyBuilder.SetSetMethod(setter);

                var attrType = typeof(OptionAttribute);
                var ctorInfo = attrType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
                var lstProperties = new PropertyInfo[2];
                lstProperties[0] = attrType.GetProperties().FirstOrDefault(x => x.Name == "Alias");
                lstProperties[1] = attrType.GetProperties().FirstOrDefault(x => x.Name == "DefaultValue");

                var attributeBuilder = new CustomAttributeBuilder(ctorInfo, new object[] { }, lstProperties, new object[] { v.GetCustomAttribute<OptionAttribute>()?.Alias, v.GetCustomAttribute<OptionAttribute>()?.DefaultValue });

                propertyBuilder.SetCustomAttribute(attributeBuilder);


                if (v.GetGetMethod() != null) typeBuilder.DefineMethodOverride(getter, v.GetGetMethod());
                if (v.GetSetMethod() != null) typeBuilder.DefineMethodOverride(setter, v.GetSetMethod());
            }

            var newObject = (K)Activator.CreateInstance(typeBuilder.CreateType());
            foreach (PropertyInfo prop in newObject.GetType().GetProperties(visibilityFlags))
            {
                if (prop != null && prop.CanWrite)
                {
                    var propAttr = attributeBank[prop.Name];
                    prop.SetValue(newObject, propAttr?.DefaultValue ?? prop.GetValue(newObject), null);
                }
            }

            return newObject;
        }

        static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        private static K CopyValues<K>(T source, K destination)
        {
            foreach (PropertyInfo property in source.GetType().GetProperties(visibilityFlags))
            {
                var prop = destination.GetType().GetProperty(property.Name, visibilityFlags);
                if (prop != null && prop.CanWrite) {
                    var optAttr = property.GetCustomAttribute<OptionAttribute>();
                    prop.SetValue(destination, optAttr?.DefaultValue ?? property.GetValue(source), null);
                }
            }

            return destination;
        }
    }
}
