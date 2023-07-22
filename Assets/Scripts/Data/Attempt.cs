namespace Data
{
    public struct Attempt
    {
        public float maxDistance;
        public bool success;

        public Attempt(float maxDistance, bool success)
        {
            this.maxDistance = maxDistance;
            this.success = success;
        }
    }
}