namespace Clud.Api.Features
{
    public enum PortType
    {
        Tcp,
        Udp,
    }

    public class ServicePort
    {
        public int PortId { get; private set; }
        public int ServiceId { get; private set; }
        public PortType Type { get; private set; }
        public int TargetPort { get; private set; }
        public int ExposedPort { get; private set; }
    }
}
