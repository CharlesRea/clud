using System;

namespace Clud.Api.Features
{
    public class ApplicationHistory
    {
        public int ApplicationHistoryId { get; private set; }
        public int ApplicationId { get; private set; }
        public string Message { get; private set; }
        public DateTimeOffset UpdatedDateTime { get; private set; }
        // TODO log the git commit hash?

        private ApplicationHistory() { }

        public ApplicationHistory(Application application, string message)
        {
            ApplicationId = application.ApplicationId;
            Message = message;
            UpdatedDateTime = DateTimeOffset.UtcNow;
        }
    }
}
