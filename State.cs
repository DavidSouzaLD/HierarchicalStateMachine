using UnityEngine;

namespace Radiopeaks.HSM
{
    public abstract class State<TContext> : IState<TContext>
    {
        protected TContext Context { get; private set; }
        protected StateMachine<TContext> StateMachine { get; private set; }

        protected State<TContext> SuperState { get; private set; }
        public State<TContext> SubState { get; private set; }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void LateUpdate() { }

        public void Initialize(TContext context, StateMachine<TContext> machine, IState<TContext> superState)
        {
            Context = context;
            StateMachine = machine;
            SuperState = superState as State<TContext>;
        }

        protected void SetSubState<T>() where T : IState<TContext>, new()
        {
            ChangeSubState(StateMachine.GetOrCreateState<T>());
        }

        protected void SwitchState<T>() where T : IState<TContext>, new()
        {
            var newState = StateMachine.GetOrCreateState<T>();

            if (SuperState != null)
            {
                SuperState.ChangeSubState(newState);
            }
            else
            {
                StateMachine.ChangeState(newState);
            }
        }

        private void ChangeSubState(IState<TContext> subState)
        {
            if (subState == null) return;

            SubState?.Exit();

            SubState = subState as State<TContext>;

            SubState?.Initialize(Context, StateMachine, this);
            SubState?.Enter();

            StateMachine.LoadHierarchy();
        }
    }
}