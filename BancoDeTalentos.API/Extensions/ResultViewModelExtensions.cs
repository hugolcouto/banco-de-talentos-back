using BancoDeTalentos.Application.Exceptions;
using BancoDeTalentos.Application.Model;
using Microsoft.AspNetCore.Mvc;

namespace BancoDeTalentos.API.Extensions;

public static class ResultViewModelExtensions
{
    public static IActionResult ToActionResult<T>(this ResultViewModel<T> result, ControllerBase controller)
    {
        if (result.IsSuccess) return controller.Ok(result);

        return MapErrorToStatusCode(result, controller);
    }

    public static IActionResult ToActionResult(this ResultViewModel result, ControllerBase controller)
    {
        if (result.IsSuccess) return controller.Ok(result);

        return MapErrorToStatusCode(result, controller);
    }

    private static IActionResult MapErrorToStatusCode(ResultViewModel result, ControllerBase controller)
    {
        if (string.IsNullOrEmpty(result.Message))
            return controller.BadRequest(result);

        return result.ErrorCode switch
        {
            string code when code.Contains(ErrorCode.NOT_FOUND)
                => controller.NotFound(result),
            _ => controller.BadRequest(result)
        };
    }
}
