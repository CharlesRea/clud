namespace Clud.Api.Features
{
    public class PersistentStorage
    {
        public int PersistentStorageId { get; private set; }
        public int ServiceId { get; private set; }
        public string Path { get; private set; }
    }
}
