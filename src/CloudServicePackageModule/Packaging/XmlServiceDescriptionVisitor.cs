using CloudServicePackageModule.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CloudServicePackageModule.Packaging
{
    public abstract class XmlServiceDescriptionVisitor : ServiceDescriptionVisitor
    {
        protected override void UpdateServiceDescriptionPart(PackagePart serviceDescriptionPart)
        {
            var definitionSerializer = new XmlSerializer(typeof(ServiceDefinition));

            var packageStream = serviceDescriptionPart.GetStream(FileMode.Open, FileAccess.ReadWrite);
            var serviceDefinition = definitionSerializer.Deserialize(packageStream) as ServiceDefinition;

            UpdateServiceDefinition(serviceDefinition);

            // rewind package stream
            packageStream.Seek(0, SeekOrigin.Begin);
            packageStream.SetLength(0);

            var writerSettings = new XmlWriterSettings()
            {
                Indent = true,
                CloseOutput = false
            };

            // write back to output
            using (var writer = XmlWriter.Create(packageStream, writerSettings))
            {
                definitionSerializer.Serialize(writer, serviceDefinition);
            }
        }

        protected abstract void UpdateServiceDefinition(ServiceDefinition definition);
    }
}
