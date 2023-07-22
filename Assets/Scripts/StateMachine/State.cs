public abstract class State
{
    public StateController stateController;
    
    public abstract void Enter();
    public abstract void  Update(float deltaTime);
    public abstract void  Exit();
}
