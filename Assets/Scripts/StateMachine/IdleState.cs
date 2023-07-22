public class IdleState : State
{
    public override void Enter()
    {
        throw new System.NotImplementedException();
    }

    public override void Update(float deltaTime)
    {
        // Process Input
        stateController.inputManager.ProcessInput();
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }
}