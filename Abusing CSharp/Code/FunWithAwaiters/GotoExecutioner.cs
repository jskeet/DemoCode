// Copyright 2013 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FunWithAwaiters
{
    public delegate IAwaitable LineAction([CallerLineNumber] int line = 0);
    public delegate IAwaitable GotoAction(int line);

    public class GotoExecutioner : Executioner
    {
        private readonly Dictionary<int, int> lineToStateMap = new Dictionary<int,int>();

        public GotoExecutioner(Action<LineAction, GotoAction> entryPoint) : base(exec => entryPoint(((GotoExecutioner)exec).Line, ((GotoExecutioner)exec).Goto))
        {
        }

        public IAwaitable Line([CallerLineNumber] int line = 0)
        {
            return CreateAwaitable(stateMachine => lineToStateMap[line] = stateMachine.GetState());
        }

        public IAwaitable Goto(int line)
        {
            return CreateAwaitable(stateMachine => stateMachine.GetActionForState(lineToStateMap[line]));
        }
    }
}
