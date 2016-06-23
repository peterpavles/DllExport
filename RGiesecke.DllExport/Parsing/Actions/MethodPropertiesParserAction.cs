﻿// [Decompiled] Assembly: RGiesecke.DllExport, Version=1.2.2.23706, Culture=neutral, PublicKeyToken=ad5f9f4a55b5020b
// Author of original assembly (MIT-License): Robert Giesecke
// Use Readme & LICENSE files for details.

using System;

namespace RGiesecke.DllExport.Parsing.Actions
{
    [ParserStateAction(ParserState.MethodProperties)]
    public sealed class MethodPropertiesParserAction: IlParser.ParserStateAction
    {
        public override void Execute(ParserStateValues state, string trimmedLine)
        {
            if(trimmedLine.StartsWith(".custom instance void ", StringComparison.Ordinal) && trimmedLine.Contains(this.Parser.DllExportAttributeIlAsmFullName))
            {
                state.AddLine = false;
                state.State = ParserState.DeleteExportAttribute;
                this.Notifier.Notify(-1, "EXP0008", "Removing {0} from {1}", (object)Utilities.DllExportAttributeFullName, (object)(state.ClassNames.Peek() + "." + state.Method.Name));
            }
            else
            {
                if(!trimmedLine.StartsWith("// Code", StringComparison.Ordinal))
                {
                    return;
                }
                state.State = ParserState.Method;
                if(state.MethodPos == 0)
                {
                    return;
                }
                state.Result.Insert(state.MethodPos, state.Method.Declaration);
            }
        }
    }
}
