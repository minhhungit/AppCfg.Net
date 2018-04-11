/**
 * Idea is from https://gist.github.com/IamNguele/bdc79d0d83a7895e693bc3b5cae543d7#file-typemixer-cs
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AppCfg
{
    internal class SettingTypeMixer<T>
    {
        private Dictionary<string, OptionAttribute> attributeBank = new Dictionary<string, OptionAttribute>();
        internal K ExtendWith<K>()
        {
            var assemblyName = new Guid().ToString();

            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
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

                var optAttr = v.GetCustomAttribute<OptionAttribute>();
                if (optAttr != null)
                {
                    var attrType = typeof(OptionAttribute);
                    var attributeBuilder = CreateAttributeBuilderFor(attrType, v.GetCustomAttribute(attrType));
                    if (attributeBuilder != null)
                    {
                        propertyBuilder.SetCustomAttribute(attributeBuilder);
                    }
                }                

                if (v.GetGetMethod() != null) typeBuilder.DefineMethodOverride(getter, v.GetGetMethod());
                if (v.GetSetMethod() != null) typeBuilder.DefineMethodOverride(setter, v.GetSetMethod());
            }

            return (K)Activator.CreateInstance(typeBuilder.CreateTypeInfo());
        }

        private CustomAttributeBuilder CreateAttributeBuilderFor(Type attrType, Attribute origialAttr)
        {
            var attrCtor = attrType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();

            if (attrType == typeof(OptionAttribute))
            {
                var origialAttrData = (OptionAttribute)origialAttr;
                var lstProperties = new PropertyInfo[4];

                var properties = attrType.GetProperties();

                lstProperties[0] = properties.FirstOrDefault(x => x.Name == "Alias");
                lstProperties[1] = properties.FirstOrDefault(x => x.Name == "DefaultValue");
                lstProperties[2] = properties.FirstOrDefault(x => x.Name == "InputFormat");
                lstProperties[3] = properties.FirstOrDefault(x => x.Name == "Separator");

                return new CustomAttributeBuilder(attrCtor, new object[] { }, lstProperties.ToArray(),
                    new object[] {
                        origialAttrData?.Alias,
                        origialAttrData?.DefaultValue,
                        origialAttrData?.InputFormat,
                        origialAttrData?.Separator
                    });
            }

            return null;
        }
    }
}
