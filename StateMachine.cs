using System;
using System.Collections.Generic;
using UnityEngine;

namespace Radiopeaks.HSM
{
    public class StateMachine<TContext>
    {
        private readonly TContext _context;
        private readonly List<IState<TContext>> _stateHierarchy = new();
        private readonly Dictionary<Type, IState<TContext>> _stateCache = new();
        private bool _isTransitioning;

        public IState<TContext> CurrentState { get; private set; }

        public StateMachine(TContext context)
        {
            _context = context;
        }

        public void Start<T>() where T : IState<TContext>, new()
        {
            ChangeState(GetOrCreateState<T>());
        }

        public void Update()
        {
            _isTransitioning = false;
            for (int i = 0; i < _stateHierarchy.Count; i++)
            {
                if (_isTransitioning) break;
                _stateHierarchy[i]?.Update();
            }
        }

        public void FixedUpdate()
        {
            _isTransitioning = false;
            for (int i = 0; i < _stateHierarchy.Count; i++)
            {
                if (_isTransitioning) break;
                _stateHierarchy[i]?.FixedUpdate();
            }
        }

        public void LateUpdate()
        {
            _isTransitioning = false;
            for (int i = 0; i < _stateHierarchy.Count; i++)
            {
                if (_isTransitioning) break;
                _stateHierarchy[i]?.LateUpdate();
            }
        }

        public void ChangeState(IState<TContext> newState)
        {
            if (newState == null) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Initialize(_context, this, null);
            CurrentState.Enter();

            LoadHierarchy();
        }

        public void LoadHierarchy()
        {
            if (CurrentState == null) return;

            _stateHierarchy.Clear();
            var state = CurrentState;
            
            while (state != null)
            {
                _stateHierarchy.Add(state);
                state = (state as State<TContext>)?.SubState;
            }

            _isTransitioning = true;
        }

        public IState<TContext> GetOrCreateState<T>() where T : IState<TContext>, new()
        {
            var type = typeof(T);
            if (!_stateCache.TryGetValue(type, out var state))
            {
                state = new T();
                _stateCache[type] = state;
            }
            return state;
        }

#if UNITY_EDITOR
        public string HierarchyString()
        {
            var value = "";
            var i = 0;
            foreach (var t in _stateHierarchy)
            {
                var name = t.GetType().Name;
                for (var x = 0; x < i; x++) value += "       ";
                value += $" -> {name} \n";
                i++;
            }
            return value;
        }
#endif
    }
}