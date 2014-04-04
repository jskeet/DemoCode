using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FunWithAwaiters
{
    public delegate YieldingAwaitable<bool> MethodLineAction([CallerMemberName] string method = null, [CallerLineNumber] int line = 0);
    public delegate YieldingAwaitable<bool> MethodGotoAction(string label);

    class GotoMethodExecutioner : Executioner
    {
        private readonly Dictionary<string, Action> labelToActionMap = new Dictionary<string, Action>();

        private readonly bool recording = true; // Urrrgh!

        public GotoMethodExecutioner(Action<MethodLineAction, MethodGotoAction> entryPoint,
            params Action<MethodLineAction, MethodGotoAction>[] actions)
            : base(exec => entryPoint(((GotoMethodExecutioner) exec).Line, ((GotoMethodExecutioner) exec).Goto))
        {
            foreach (var action in actions)
            {
                Execute(_ => action(Line, Goto));
            }
            recording = false;
        }

        public YieldingAwaitable<bool> Line([CallerMemberName] string method = null, [CallerLineNumber] int line = 0)
        {
            string label = method + ":" + line;
            return CreateYieldingAwaitable(stateMachine =>
                labelToActionMap[label] = stateMachine.GetActionForState(stateMachine.GetState()),
                recording);
        }

        public YieldingAwaitable<bool> Goto(string label)
        {
            return CreateYieldingAwaitable(stateMachine => labelToActionMap[label], false);
        }
    }
}
