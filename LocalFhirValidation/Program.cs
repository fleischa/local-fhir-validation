using System.Xml;
using Gradient.Fhir.Validation.R4;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace LocalFhirValidation;

internal static class Program
{
	private static void Main(string[] args)
	{
		/*
		FhirValidatorOptions options = new()
		{
			RemotePackages =
			[
				new RemotePackage
				{
					Server = "https://packages.simplifier.net",
					Packages = "kbv.ita.erp@1.1.2"
				}
			]
		};
		*/

		FhirValidatorOptions options = new()
		{
			Directories =
			[
				@"C:\Users\fabia\.fhir"
			],
			ZipFiles =
			[
				@"C:\temp\keytabs.zip"
			],
			ZipCacheDirectory = @"C:\temp\ZipCache"
		};

		FhirValidator validator = new(options);

		using StreamReader fileReader = File.OpenText(@"examples\0428d416-149e-48a4-977c-394887b3d85c.xml");
		using XmlReader xmlReader = XmlReader.Create(fileReader);
		FhirXmlPocoDeserializer deserializer = new();
		Resource erpBundle = deserializer.DeserializeResource(xmlReader);

		OperationOutcome operationOutcome = validator.Validate(erpBundle);
		Console.WriteLine(operationOutcome);
	}
}