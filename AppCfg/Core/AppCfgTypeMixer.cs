/**
 * Inspired by https://gist.github.com/IamNguele/bdc79d0d83a7895e693bc3b5cae543d7#file-typemixer-cs
 */

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AppCfg
{
    internal class AppCfgTypeMixer<T>
    {
        internal K ExtendWith<K>()
        {
            AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            ModuleBuilder module = assembly.DefineDynamicModule("AppCfgModule");

            var typeBuilder = module.DefineType(typeof(T).Name + "_" + typeof(K).Name, TypeAttributes.Public, typeof(T));
            typeBuilder.AddInterfaceImplementation(typeof(K));

            foreach (var v in typeof(K).GetProperties())
            {
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

                var cusAttData =  v.GetCustomAttributesData();
                foreach (var attr in v.GetCustomAttributes())
                {
                    var attributeBuilder = CreateAttributeBuilderFor(v.GetCustomAttribute(attr.GetType()), cusAttData.FirstOrDefault(x=>x.AttributeType == attr.GetType()));
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

        private CustomAttributeBuilder CreateAttributeBuilderFor(Attribute origialAttr, CustomAttributeData customAttributeData)
        {
            Type type = origialAttr.GetType();

            var constructor = type.GetConstructor(customAttributeData.ConstructorArguments.Select(x => x.ArgumentType).ToArray());
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            var propertyValues = from p in properties
                                 where p.CanWrite && p.CanRead
                                 select p.GetValue(origialAttr, null);

            var fieldValues = from f in fields
                              select f.GetValue(origialAttr);

            return new CustomAttributeBuilder(constructor, customAttributeData.ConstructorArguments.Select(x => x.Value).ToArray(),
                                             properties.Where(x => x.CanRead && x.CanWrite).ToArray(), propertyValues.ToArray(),
                                             fields, fieldValues.ToArray());
        }
    }
}
