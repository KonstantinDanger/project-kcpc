using VContainer;
using VContainer.Unity;

namespace ProjectKCPC
{
    public class SceneLifeTimeScope : BaseLifeTimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(SpawnPoint);
            builder.RegisterEntryPoint<SceneEntryPoint>(Lifetime.Scoped);
        }
    }
}
