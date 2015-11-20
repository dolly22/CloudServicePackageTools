using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudServicePackageModule.Schema
{
    public interface IRoleSpecification
    {
        string Vmsize { get; set; }
        string Name { get; set; }
    }

    public partial class WebRole : IRoleSpecification
    {
    }

    public partial class WorkerRole : IRoleSpecification
    {
    }
}
