namespace Gradient.Fhir.Validation.R4;

public class FhirValidatorOptions
{
	public IEnumerable<RemotePackage>? RemotePackages { get; set; }

	public IEnumerable<string>? LocalPackages { get; set; }

	public IEnumerable<string>? Directories { get; set; }

	public IEnumerable<string>? ZipFiles { get; set; }

	public string? ZipCacheDirectory { get; set; }
}