using Content.Packaging;
using Robust.Packaging;
using Robust.Packaging.AssetProcessing;
using Robust.Server.ServerStatus;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server.Acz;

public sealed class ContentMagicAczProvider : IMagicAczProvider
{
    private readonly IDependencyCollection _deps;

    public ContentMagicAczProvider(IDependencyCollection deps)
    {
        _deps = deps;
    }

    public async Task Package(AssetPass pass, IPackageLogger logger, CancellationToken cancel)
    {
        var contentDir = DefaultMagicAczProvider.FindContentRootPath(_deps);

        await ContentPackaging.WriteResources(contentDir, pass, logger, cancel);
    }
}
