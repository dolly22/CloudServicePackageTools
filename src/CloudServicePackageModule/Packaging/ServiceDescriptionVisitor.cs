using Microsoft.ServiceHosting.Tools.Packaging;
using Microsoft.WindowsAzure.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;

namespace CloudServicePackageModule.Packaging
{
    public abstract class ServiceDescriptionVisitor
    {
        public void ProcessPackage(string packagePath)
        {
            var packageFormat = PackageConverter.GetFormat(packagePath);
            if (packageFormat != PackageFormats.Legacy)
                throw new ArgumentException("Only legacy packages are supported", nameof(packagePath));

            var servicePackage = Package.Open(packagePath, FileMode.Open, FileAccess.ReadWrite);
            var serviceDescriptionPart = UpdateServiceDescription(servicePackage);

            // update outer package manifest
            UpdatePackageManifest(servicePackage, serviceDescriptionPart);
            servicePackage.Flush();
            servicePackage.Close();
        }

        private PackagePart UpdateServiceDescription(Package servicePackage)
        {
            var descRelation = servicePackage.GetRelationship("SERVICEDESCRIPTION");
            if (descRelation == null)
                throw new ArgumentException("SERVICEDESCRIPTION part not found inside package", nameof(servicePackage));

            var descPart = servicePackage.GetPart(descRelation.TargetUri);
            UpdateServiceDescriptionPackage(descPart);

            return descPart;
        }

        private void UpdateServiceDescriptionPackage(PackagePart servicePackageDescriptionPart)
        {
            // open service description package
            var descPackage = Package.Open(servicePackageDescriptionPart.GetStream(FileMode.Open, FileAccess.ReadWrite), FileMode.Open, FileAccess.ReadWrite);
            var descPackageRelation = descPackage.GetRelationship("SERVICEDESCRIPTION");
            var descPackagePart = descPackage.GetPart(descPackageRelation.TargetUri);

            // update service description content
            UpdateServiceDescriptionPart(descPackagePart);

            // update inner package manifest
            UpdatePackageManifest(descPackage, descPackagePart);
            descPackage.Flush();
            descPackage.Close();
        }

        protected abstract void UpdateServiceDescriptionPart(PackagePart serviceDescriptionPart);

        private void UpdatePackageManifest(Package package, PackagePart updatedPart)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));
            if (updatedPart == null)
                throw new ArgumentNullException(nameof(updatedPart));
            if (package.FileOpenAccess != FileAccess.ReadWrite)
                throw new InvalidOperationException("Package must be open for reading and writing");

            var manifestRelation = package.GetRelationship("MANIFEST");
            var manifestPart = package.GetPart(manifestRelation.TargetUri);

            // parse manifest
            var manifest = new PackageManifest(manifestPart, null);

            // rehash updated part
            var csDefPart = manifest.Items.FirstOrDefault(i => i.PartUri == updatedPart.Uri);
            if (csDefPart == null)
                throw new InvalidOperationException(string.Format("Unable to find part '{0}' in package manifest", updatedPart.Uri));

            csDefPart.Hash = manifest.HashAlgorithm.ComputeHash(updatedPart.GetStream(FileMode.Open, FileAccess.Read)); ;
            csDefPart.ModifiedDate = DateTime.UtcNow;

            var manifestStream = manifestPart.GetStream(FileMode.Open, FileAccess.Write);
            manifest.WriteToStream(manifestStream);
        }
    }
}
