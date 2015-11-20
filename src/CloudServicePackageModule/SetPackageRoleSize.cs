using CloudServicePackageModule.Packaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace CloudServicePackageTools
{
    [Cmdlet(VerbsCommon.Set, "PackageRoleSize")]
    public class SetPackageRoleSize : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 0
        )]
        public string Package { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1
        )]
        public Hashtable[] Sizes { get; set; }


        protected override void ProcessRecord()
        {
            var definitions = new List<VmSizeVisitor.RoleSizeDefinition>();

            // transform sizes
            foreach (var size in Sizes)
            {
                foreach (var key in size.Keys)
                {
                    var keyText = key.ToString();
                    var valueText = size[key].ToString();

                    definitions.Add(new VmSizeVisitor.RoleSizeDefinition(keyText, valueText));
                    this.WriteVerbose(string.Format("RoleSize '{0}' => '{1}'", keyText, valueText));
                }
            }

            ProviderInfo providerInfo;
            var paths = this.GetResolvedProviderPathFromPSPath(Package, out providerInfo);

            foreach (var path in paths)
            {
                var updater = new VmSizeVisitor(definitions);
                updater.ProcessPackage(path);
                this.WriteVerbose(string.Format("Updated {0} role size definitions", updater.UpdatedRoles));
            }
            base.ProcessRecord();
        }
    }
}
