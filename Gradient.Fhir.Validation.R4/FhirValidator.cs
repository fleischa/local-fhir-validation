using Firely.Fhir.Packages;
using Firely.Fhir.Validation;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Specification.Terminology;
using Microsoft.Extensions.Options;

namespace Gradient.Fhir.Validation.R4;

public class FhirValidator
{
	private readonly FhirValidatorOptions options;
	private readonly Validator validator;

	public FhirValidator(FhirValidatorOptions options)
	{
		this.options = options;
		this.validator = this.SetupValidator();
	}

	public FhirValidator(IOptions<FhirValidatorOptions> options)
	{
		this.options = options.Value;
		this.validator = this.SetupValidator();
	}

	public OperationOutcome Validate(Resource instance, string? profile = null)
	{
		return this.validator.Validate(instance, profile);
	}

	private Validator SetupValidator()
	{
		ModelInspector modelInspector = ModelInfo.ModelInspector;

		List<IAsyncResourceResolver> resolvers = new();

		if (this.options.RemotePackages != null)
		{
			foreach (RemotePackage remotePackage in this.options.RemotePackages)
			{
				resolvers.Add(new FhirPackageSource(modelInspector, remotePackage.Server, remotePackage.Packages.ToArray()));
			}
		}

		if (this.options.LocalPackages != null && this.options.LocalPackages.Any())
		{
			resolvers.Add(new FhirPackageSource(modelInspector, this.options.LocalPackages.ToArray()));
		}

		if (this.options.Directories != null)
		{
			DirectorySourceSettings settings = new()
			{
				IncludeSubDirectories = true,
				ExcludeSummariesForUnknownArtifacts = true,
				MultiThreaded = true
			};

			foreach (string profileDirectory in this.options.Directories)
			{
				resolvers.Add(new CommonDirectorySource(modelInspector, profileDirectory, settings));
			}
		}

		if (this.options.ZipFiles != null)
		{
			DirectorySourceSettings settings = new()
			{
				IncludeSubDirectories = true,
				ExcludeSummariesForUnknownArtifacts = true,
				MultiThreaded = true
			};

			foreach (string zipFile in this.options.ZipFiles)
			{
				FileInfo zipFileInfo = new(zipFile);

				if (zipFileInfo.Exists)
				{
					string cacheDirectory = string.IsNullOrEmpty(this.options.ZipCacheDirectory) ? "." : this.options.ZipCacheDirectory;
					resolvers.Add(new CommonZipSource(modelInspector, zipFile, cacheDirectory, settings));
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
		}

		if (!resolvers.Any())
		{
			throw new InvalidOperationException();
		}

		IAsyncResourceResolver sourceResolver;

		if (resolvers.Count > 1)
		{
			sourceResolver = new MultiResolver(resolvers);
		}
		else
		{
			sourceResolver = resolvers.Single();
		}

		SnapshotSource snapshotSource = new(sourceResolver);
		CachedResolver cachedResolver = new(snapshotSource);
		LocalTerminologyService terminologyService = new(cachedResolver);

		return new Validator(cachedResolver, terminologyService);
	}
}