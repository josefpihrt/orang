// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Orang.CommandLine;

internal static class DelegateFactory
{
    public static bool TryCreateFromAssembly<TDelegate>(
        string path,
        Type returnType,
        Type parameterType,
        Logger logger,
        [NotNullWhen(true)] out TDelegate? result) where TDelegate : Delegate
    {
        return TryCreateFromAssembly(path, returnType, new Type[] { parameterType }, logger, out result);
    }

    public static bool TryCreateFromAssembly<TDelegate>(
        string path,
        Type returnType,
        Type[] parameters,
        Logger logger,
        [NotNullWhen(true)] out TDelegate? result) where TDelegate : Delegate
    {
        result = default;

        if (path == null)
            return false;

        int index = path.LastIndexOf(',');

        if (index <= 0
            || index >= path.Length - 1)
        {
            logger.WriteError($"Invalid value: {path}. "
                + "The expected format is \"MyLib.dll,MyNamespace.MyClass.MyMethod\".");

            return false;
        }

        string assemblyName = path.Substring(0, index);

        string methodFullName = path.Substring(index + 1);

        index = methodFullName.LastIndexOf('.');

        if (index < 0
            || index >= path.Length - 1)
        {
            logger.WriteError($"Invalid method full name: {methodFullName}. "
                + "The expected format is \"MyNamespace.MyClass.MyMethod\".");

            return false;
        }

        Assembly? assembly;

        try
        {
            assembly = Assembly.LoadFrom(assemblyName);
        }
        catch (Exception ex) when (ex is ArgumentException
            || ex is IOException
            || ex is BadImageFormatException)
        {
            logger.WriteError(ex);
            return false;
        }

        string typeName = methodFullName.Substring(0, index);

        string methodName = methodFullName.Substring(index + 1);

        result = CreateDelegateAndCatchIfThrows<TDelegate>(assembly, returnType, parameters, logger, typeName, methodName);

        return result != null;
    }

    private static TDelegate? CreateDelegateAndCatchIfThrows<TDelegate>(
        Assembly assembly,
        Type returnType,
        Type[] parameters,
        Logger logger,
        string? typeName = null,
        string? methodName = null) where TDelegate : Delegate
    {
        try
        {
            return CreateDelegate<TDelegate>(assembly, returnType, parameters, typeName, methodName);
        }
        catch (Exception ex) when (ex is ArgumentException
            || ex is AmbiguousMatchException
            || ex is TargetInvocationException
            || ex is MemberAccessException
            || ex is MissingMemberException
            || ex is TypeLoadException
            || ex is InvalidOperationException)
        {
            logger.WriteError(ex, $"Cannot create delegate: {ex.Message}");
            return null;
        }
    }

    public static TDelegate? CreateDelegate<TDelegate>(
        Assembly assembly,
        Type returnType,
        Type[] parameters,
        string? typeName = null,
        string? methodName = null) where TDelegate : Delegate
    {
        Type? type;
        MethodInfo? method;

        if (typeName != null)
        {
            type = assembly.GetType(typeName);

            if (type == null)
            {
                throw new InvalidOperationException($"Cannot find type '{typeName}' in assembly '{assembly.FullName}'");
            }

            if (methodName != null)
            {
                method = type.GetMethod(methodName, parameters);

                if (method == null)
                {
                    throw new InvalidOperationException($"Cannot find method '{methodName}' in type '{typeName}'");
                }
            }
            else
            {
                method = FindMethod(type, returnType, parameters);

                if (method == null)
                {
                    throw new InvalidOperationException("Cannot find public method with signature "
                        + $"'{returnType.Name} M({string.Join(", ", parameters.Select(f => f.Name))})'"
                        + $" in type '{typeName}'");
                }

                methodName = method.Name;
            }
        }
        else
        {
            method = FindMethod(assembly, returnType, parameters, methodName);

            if (method == null)
            {
                throw new InvalidOperationException("Cannot find public method with signature "
                    + $"'{returnType.Name} {methodName ?? "M"}({string.Join(", ", parameters.Select(f => f.Name))})'");
            }

            methodName ??= method.Name;

            type = method.DeclaringType;
        }

        if (method.IsStatic)
        {
            return (TDelegate)method.CreateDelegate(typeof(TDelegate));
        }
        else
        {
            object? typeInstance = Activator.CreateInstance(type!);

            if (typeInstance == null)
            {
                throw new InvalidOperationException($"Cannot create instance of '{typeName}'");
            }

            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), typeInstance, methodName);
        }
    }

    private static MethodInfo? FindMethod(
        Assembly assembly,
        Type returnType,
        Type[] parameters,
        string? methodName = null)
    {
        foreach (Type type in assembly.GetTypes())
        {
            MethodInfo? method = (methodName != null)
                ? type.GetMethod(methodName, parameters)
                : FindMethod(type, returnType, parameters);

            if (method != null)
                return method;
        }

        return null;
    }

    private static MethodInfo? FindMethod(Type type, Type returnType, Type[] parameters)
    {
        foreach (MethodInfo methodInfo in type.GetMethods())
        {
            if (!methodInfo.IsPublic)
                continue;

            if (methodInfo.ReturnType != returnType)
                continue;

            ParameterInfo[]? parameters2 = methodInfo.GetParameters();

            if (parameters2.Length != parameters.Length)
                continue;

            var areEqual = true;

            for (int i = 0; i < parameters2.Length; i++)
            {
                if (parameters2[i].ParameterType != parameters[i])
                {
                    areEqual = false;
                    break;
                }
            }

            if (areEqual)
                return methodInfo;
        }

        return null;
    }
}
