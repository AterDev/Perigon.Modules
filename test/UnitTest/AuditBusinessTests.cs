using AuditMod.Models.AuditFlowDtos;
using AuditMod.Models.AuditRecordDtos;
using Entity.AuditMod;
using System.ComponentModel.DataAnnotations;

namespace UnitTest;

public sealed class AuditBusinessTests
{
    [Test]
    public async Task AuditEntities_ShouldRoundTripWorkflowStatusThroughPersistenceValues()
    {
        AuditRecord record = new() { Status = AuditRecordStatus.InProgress };
        AuditRecordDetail detail = new() { AuditModeSnapshot = AuditMode.All, Status = AuditStepStatus.Pending };
        AuditTask task = new() { Status = AuditTaskStatus.Approved };

        await Assert.That(record.StatusValue).IsEqualTo((int)AuditRecordStatus.InProgress);
        await Assert.That(detail.AuditModeSnapshotValue).IsEqualTo((int)AuditMode.All);
        await Assert.That(detail.StatusValue).IsEqualTo((int)AuditStepStatus.Pending);
        await Assert.That(task.StatusValue).IsEqualTo((int)AuditTaskStatus.Approved);

        record.Status = AuditRecordStatus.Approved;
        detail.Status = AuditStepStatus.Approved;
        task.Status = AuditTaskStatus.Rejected;

        await Assert.That(record.Status).IsEqualTo(AuditRecordStatus.Approved);
        await Assert.That(detail.Status).IsEqualTo(AuditStepStatus.Approved);
        await Assert.That(task.Status).IsEqualTo(AuditTaskStatus.Rejected);
    }

    [Test]
    public async Task AuditFlowDto_ShouldEnforceRequiredAndMaximumLengthFields()
    {
        AuditFlowAddDto invalid = new()
        {
            FlowKey = string.Empty,
            SceneCode = new string('s', 101),
            Name = "valid name",
            Steps = [new AuditFlowStepDto { Name = new string('x', 201) }],
        };

        List<ValidationResult> results = Validate(invalid);
        List<ValidationResult> stepResults = Validate(invalid.Steps[0]);

        await Assert.That(results).IsNotEmpty();
        await Assert.That(results.Any(result => result.MemberNames.Contains(nameof(AuditFlowAddDto.FlowKey)))).IsTrue();
        await Assert.That(results.Any(result => result.MemberNames.Contains(nameof(AuditFlowAddDto.SceneCode)))).IsTrue();
        await Assert.That(stepResults.Any(result => result.MemberNames.Contains(nameof(AuditFlowStepDto.Name)))).IsTrue();
    }

    [Test]
    public async Task AuditRecordSubmitDto_ShouldRejectMissingBusinessIdentity()
    {
        AuditRecordSubmitDto invalid = new();

        List<ValidationResult> results = Validate(invalid);

        await Assert.That(results).IsNotEmpty();
        await Assert.That(results.Any(result => result.MemberNames.Contains(nameof(AuditRecordSubmitDto.SceneCode)))).IsTrue();
        await Assert.That(results.Any(result => result.MemberNames.Contains(nameof(AuditRecordSubmitDto.BizId)))).IsTrue();
        await Assert.That(results.Any(result => result.MemberNames.Contains(nameof(AuditRecordSubmitDto.ApplyReason)))).IsTrue();
    }

    private static List<ValidationResult> Validate(object value)
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        return results;
    }
}
