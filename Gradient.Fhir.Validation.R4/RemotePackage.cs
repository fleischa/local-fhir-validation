namespace Gradient.Fhir.Validation.R4;

public class RemotePackage
{
	public string Server { get; set; }

	public IEnumerable<string> Packages { get; set; }
}