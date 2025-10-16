using CrewManagerData;
using Microsoft.AspNetCore.Mvc;

namespace CrewManagerAPI.Controllers;

public class CMControllerBase: ControllerBase
{
    protected CMDBContext _context;

    public CMControllerBase(CMDBContext context)
    {
        _context = context;
    }
}