using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace chatgptbot.Controllers.Base
{
    public class BaseController : ControllerBase
    {
        protected readonly ILogger _logger;

        public BaseController(ILogger logger)
        {
            _logger = logger;
        }
    }
}
