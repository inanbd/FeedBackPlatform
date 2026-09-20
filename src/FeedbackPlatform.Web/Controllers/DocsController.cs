using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackPlatform.Web.Controllers;

[AllowAnonymous]
public sealed class DocsController : Controller
{
    public IActionResult Index() => View();
}
