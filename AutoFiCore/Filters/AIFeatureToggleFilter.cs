using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AIFeatureToggleFilter : ActionFilterAttribute
{
    private readonly IConfiguration _config;

    public AIFeatureToggleFilter(IConfiguration config)
    {
        _config = config;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var isEnabled = _config.GetValue<bool>("FeatureFlags:AIEnabled");
        if (!isEnabled)
        {
            context.Result = new JsonResult(new
            {
                error = "AI functionality is currently disabled."
            })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
        }
    }
}
