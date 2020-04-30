namespace Clud.Api.Features
{
    public class Service
    {
        public int ServiceId { get; private set; }
        public int ApplicationId { get; private set; }
        public string Name { get; private set;  }

        private Service() { }

        public Service(string name)
        {
            Name = name;
        }
    }
}
