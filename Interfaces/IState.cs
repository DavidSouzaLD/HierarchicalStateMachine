namespace Radiopeaks.HSM
{
    public interface IState<TContext>
    {
        void Enter();
        void Exit();
        void Update();
        void FixedUpdate();
        void LateUpdate();
        
        void Initialize(TContext context, StateMachine<TContext> machine, IState<TContext> superState);
    }
}