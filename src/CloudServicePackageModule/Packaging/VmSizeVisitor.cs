using CloudServicePackageModule.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudServicePackageModule.Packaging
{
    public class VmSizeVisitor : XmlServiceDescriptionVisitor
    {
        readonly IList<RoleSizeDefinition> roleSizeDefinitions;

        public VmSizeVisitor(IEnumerable<RoleSizeDefinition> roleSizeDefinitions)
        {
            if (roleSizeDefinitions == null)
                throw new ArgumentNullException(nameof(roleSizeDefinitions));

            this.roleSizeDefinitions = roleSizeDefinitions.ToList();
        }

        public int UpdatedRoles { get; private set; }

        protected override void UpdateServiceDefinition(ServiceDefinition definition)
        {
            UpdatedRoles = 0;

            // iterate all webroles
            UpdateVmSize(definition.WebRole);
            UpdateVmSize(definition.WorkerRole);
        }

        private void UpdateVmSize(IEnumerable<IRoleSpecification> roles)
        {           
            foreach (var role in roles)
            {
                var sizeDefinition = roleSizeDefinitions.FirstOrDefault(d => string.Equals(d.RoleName, role.Name, StringComparison.InvariantCultureIgnoreCase));
                if (sizeDefinition != null)
                {
                    // change role size
                    role.Vmsize = sizeDefinition.VmSize;
                    UpdatedRoles = UpdatedRoles + 1;
                }
            }
        }

        public class RoleSizeDefinition
        {
            public RoleSizeDefinition()
            {
            }

            public RoleSizeDefinition(string roleName, string vmSize)
            {
                this.RoleName = roleName;
                this.VmSize = vmSize;
            }

            public string RoleName { get; set; }
            public string VmSize { get; set; }
        }
    }
}
