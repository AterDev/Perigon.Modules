using Aspire.Hosting.Kubernetes.Resources;
using YamlDotNet.Serialization;

namespace AppHost;

public sealed class KubernetesJobResource() : BaseKubernetesResource("batch/v1", "Job")
{
    [YamlMember(Alias = "spec")]
    public KubernetesJobSpec Spec { get; set; } = new();
}

public sealed class KubernetesJobSpec
{
    [YamlMember(Alias = "backoffLimit")]
    public int BackoffLimit { get; set; } = 3;

    [YamlMember(Alias = "ttlSecondsAfterFinished")]
    public int TtlSecondsAfterFinished { get; set; } = 3600;

    [YamlMember(Alias = "template")]
    public PodTemplateSpecV1 Template { get; set; } = new();
}
