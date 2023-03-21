using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.DesignPattern
{
    public interface IState
    {
        void Tick();
        void OnStateEnter();
        void OnStateExit();
    }

    public class StateMachine
    {
        public event Action<IState> stateChanged = null;
        private IState _currentState = null;
        public IState CurrentState => _currentState;

        private Dictionary<IState, List<Transition>> _transitions = new Dictionary<IState, List<Transition>>();
        private List<Transition> _currentTransitions = new List<Transition>();
        private List<Transition> _anyTransitions = new List<Transition>();

        private static List<Transition> EmptyTransitions = new List<Transition>(0);

        public void Tick()
        {
            var transition = GetTransition();
            if (transition != null)
                SetState(transition.To);

            _currentState?.Tick();
        }

        public void SetState(IState state)
        {
            if (state == _currentState)
                return;

            _currentState?.OnStateExit();

            _currentState = state;
            stateChanged?.Invoke(state);

            _transitions.TryGetValue(_currentState, out _currentTransitions);
            if (_currentTransitions == null)
                _currentTransitions = EmptyTransitions;

            _currentState.OnStateEnter();
        }

        public void AddTransition(IState from, IState to, Func<bool> predicate)
        {
            if (_transitions.TryGetValue(from, out var transitions) == false)
            {
                transitions = new List<Transition>();
                _transitions[from] = transitions;
            }

            transitions.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(IState state, Func<bool> predicate)
        {
            _anyTransitions.Add(new Transition(state, predicate));
        }

        private class Transition
        {
            public Func<bool> Condition { get; }
            public IState To { get; }

            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        private Transition GetTransition()
        {
            foreach (var transition in _anyTransitions)
                if (transition.Condition())
                    return transition;

            foreach (var transition in _currentTransitions)
                if (transition.Condition())
                    return transition;

            return null;
        }
    }
}