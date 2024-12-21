using UpdatingTable.Data;
using ILogger = Serilog.ILogger;

namespace UpdatingTable.Services
{
    public class ItemViewService : IDisposable
    {
        private readonly ILogger _logger;
        private readonly DataService _dataService;
        private readonly string _userId;

        public List<Item>? CurrentView { get; private set; }
        public event EventHandler? UpdatedView;

        public ItemViewService(
            ILogger logger
            , DataService dataService
            , IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger; 
            _dataService = dataService;

            // Niceness for logging to show who's being updated
            _userId = httpContextAccessor.HttpContext?.User.Identity?.Name
                ?? httpContextAccessor.HttpContext?.Connection.Id[^5..^0] // Last N characters
                ?? "Unknown";

            // Initial refresh
            RefreshView();

            // Subscribe this scoped service to the singleton data service
            _dataService.ItemChanged += HandleEvent;
        }

        private void HandleEvent(object? sender, ItemChangedEvent e)
        {
            _logger.Information("[{UserId}] Updating view from {EventType} event", _userId, e.NewState);
            RefreshView();

            // Push the event down to the component so it will rerender
            UpdatedView?.Invoke(this, EventArgs.Empty);
        }

        public void RefreshView()
        {
            CurrentView = _dataService.Get();
        }

        // On refreshing the page this service would be left hanging
        // The events would still fire, making it disposable and yeeting the
        // event sub fixes this
        public void Dispose()
        {
            _dataService.ItemChanged -= HandleEvent;
            GC.SuppressFinalize(this);
        }
    }
}
