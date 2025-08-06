namespace FiberCore.Controllers;

using Microsoft.AspNetCore.Mvc;
using BSFiberCore.Models;
using System.Web.Mvc;

public class CalculateController : Controller
{
    public CalculateController()
    {
        
    }

    [HttpPost]
    public ViewResult Calculate(Fiber fiber)
    {
        // Вызов метода RunCalc из класса Fiber
        string result = fiber.RunCalc();

        // Возврат результата в представление или в виде JSON
        return View();
    }
}


