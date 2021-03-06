﻿using Righthand.Immutable;

namespace Sharp.Redux.Visualizer.Actions
{
    public class SelectedStepChangedAction: ReduxAction
    {
        public int? Key { get; }

        public SelectedStepChangedAction(int? key)
        {
            Key = key;
        }

        public SelectedStepChangedAction Clone(Param<int?>? key = null)
        {
            return new SelectedStepChangedAction(key.HasValue ? key.Value.Value : Key);
        }
    }
}
