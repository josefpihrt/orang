// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Orang.CommandLine.Tests
{
    internal static class CommandLineHelpers
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        //https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-commandlinetoargvw
        //https://www.pinvoke.net/default.aspx/shell32.commandlinetoargvw
        public static string[] SplitArgs(string command)
        {
            IntPtr pArgs = CommandLineToArgvW(command, out int argsCount);

            if (pArgs == IntPtr.Zero)
                throw new ArgumentException("Unable to split argument.", new Win32Exception());

            string[] args;

            try
            {
                args = new string[argsCount];

                for (int i = 0; i < argsCount; i++)
                {
                    args[i] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pArgs, i * IntPtr.Size));
                }

                return args;
            }
            finally
            {
                LocalFree(pArgs);
            }
        }
    }
}
