﻿//# Author of original code ([Decompiled] MIT-License): Copyright (c) 2009-2015  Robert Giesecke
//# Use Readme & LICENSE files for details.

//# Modifications: Copyright (c) 2016-2020  Denis Kuzmin < x-3F@outlook.com > GitHub/3F
//$ Distributed under the MIT License (MIT)

using Mono.Cecil;

namespace RGiesecke.DllExport
{
    internal static class Set
    {
        public static bool Contains(this ModuleAttributes input, ModuleAttributes set)
        {
            return (input & set) == input;
        }

        public static bool Contains(this TargetArchitecture input, TargetArchitecture set)
        {
            return (input & set) == input;
        }
    }
}
