namespace Clud.Api.Features
{
    public class EnvironmentVariable
    {
        public int EnvironmentVariableId { get; private set; }
        public int ServiceId { get; private set; }
        public string Name { get; private set; }
        public string Value { get; private set; }
    }
}
